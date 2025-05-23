using System.Reflection;
using System.ComponentModel;
using System.Text.Json;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Tools;

namespace IronHive.Core.Tools;

public class FunctionToolPlugin<T> : IToolPlugin where T : class
{
    private readonly IServiceProvider _provider;
    private readonly List<ToolDescriptor> _tools;
    private readonly Dictionary<string, string> _mapper;

    public FunctionToolPlugin(IServiceProvider serviceProvider)
    {
        _provider = serviceProvider;
        (_tools, _mapper) = ExtractToolDescriptors();
    }

    /// <inheritdoc />
    public required string PluginName { get; init; }

    /// <inheritdoc />
    public void Dispose() => GC.SuppressFinalize(this);

    /// <inheritdoc />
    public Task<IEnumerable<ToolDescriptor>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var list = _tools.Select(t => new ToolDescriptor
        {
            Name = t.Name,
            Description = t.Description,
            Parameters = t.Parameters,
            RequiresApproval = t.RequiresApproval,
        });
        return Task.FromResult(list);
    }

    /// <inheritdoc />
    public async Task<ToolOutput> InvokeAsync(
        string name, 
        ToolInput input, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var instance = ActivatorUtilities.CreateInstance<T>(_provider);
            if (!_mapper.TryGetValue(name, out var methodName))
                return ToolOutput.Failure($"Tool '{name}' not found.");

            var method = typeof(T).GetMethod(methodName, 
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (method == null)
                return ToolOutput.Failure($"Method '{methodName}' not found.");

            var parameters = method.GetParameters();
            var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
            var returnType = method.ReturnType;
            var functionType = returnType == typeof(void)
                ? Expression.GetActionType(parameterTypes)
                : Expression.GetFuncType([.. parameterTypes, returnType]);
            var function = method.CreateDelegate(functionType, instance);
            var arguments = BuildArguments(parameters, input, cancellationToken);

            var result = function.DynamicInvoke(arguments);
            cancellationToken.ThrowIfCancellationRequested();
            var output = await HandleDynamicResultAsync(result, function.Method.ReturnType, cancellationToken);
            var json = JsonSerializer.Serialize(output, JsonDefaultOptions.Options);

            return json.Length < 30_000
                ? ToolOutput.Success(json)
                : ToolOutput.ExcessiveResult();
        }
        catch (OperationCanceledException)
        {
            throw; // Cancellation is handled by the caller
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            return ToolOutput.Failure(ex.InnerException.Message);
        }
        catch (Exception ex)
        {
            return ToolOutput.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Delegate의 인자를 빌드합니다.
    /// </summary>
    private static object?[]? BuildArguments(ParameterInfo[] parameters, ToolInput input, CancellationToken cancellationToken)
    {
        if (parameters.Length == 0) return null;
        var args = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var name = param.Name ?? throw new InvalidOperationException("Parameter name cannot be null.");

            // 특별한 타입의 경우
            if (param.ParameterType == typeof(CancellationToken))
                args[i] = cancellationToken;
            // 인자가 존재하는 경우
            else if (input.TryGetValue(name, out var value))
                args[i] = value.ConvertTo(param.ParameterType);
            // 인자가 존재하지 않고, 기본값이 있는 경우
            else if (param.HasDefaultValue)
                args[i] = param.DefaultValue;
            // 인자가 존재하지 않고, 선택적 인자인 경우
            else if (param.IsOptional)
                args[i] = null;
            // 인자가 존재하지 않고, 필수 인자인 경우
            else
                throw new ArgumentException($"Parameter '{name}' is required");
        }

        return args;
    }

    /// <summary>
    /// 비동기 메서드를 적절히 처리합니다.
    /// </summary>
    private static async Task<object?> HandleDynamicResultAsync(object? result, Type returnType, CancellationToken cancellationToken)
    {
        // Void 인 경우
        if (result == null || returnType == typeof(void))
        {
            return "completed function";
        }

        // Task 인 경우
        if (returnType == typeof(Task))
        {
            await ((Task)result).ConfigureAwait(false);
            return "completed function";
        }

        // ValueTask 인 경우
        if (returnType == typeof(ValueTask))
        {
            await ((ValueTask)result).ConfigureAwait(false);
            return "completed function";
        }

        if (returnType.IsGenericType)
        {
            var def = returnType.GetGenericTypeDefinition();

            // Task<T>인 경우
            if (def == typeof(Task<>))
            {
                dynamic dynTask = result;
                return await dynTask.ConfigureAwait(false);
            }

            // ValueTask<T>인 경우
            if (def == typeof(ValueTask<>))
            {
                dynamic dynTask = result;
                return await dynTask.ConfigureAwait(false);
            }

            // IAsyncEnumerable<T>인 경우
            if (def == typeof(IAsyncEnumerable<>))
            {
                var list = new List<object?>();
                var enumerator = ((IAsyncEnumerable<object?>)result).GetAsyncEnumerator(cancellationToken);
                while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    list.Add(enumerator.Current);
                }
                return list;
            }
        }

        // 커스텀 awaitable 인 경우
        if (returnType.GetMethod("GetAwaiter") != null)
        {
            dynamic dynTask = result;
            return await dynTask;
        }

        // 이외 의 경우 그대로 반환
        return result;
    }


    /// <summary>
    /// Get all tools from the class.
    /// </summary>
    private static (List<ToolDescriptor>, Dictionary<string, string>) ExtractToolDescriptors()
    {
        var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        var tools = new List<ToolDescriptor>();
        var mapper = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<FunctionToolAttribute>();
            if (attr == null) continue;

            var descriptor = new ToolDescriptor
            {
                Name = attr.Name ?? method.Name,
                Description = attr.Description ?? method.GetCustomAttribute<DescriptionAttribute>()?.Description,
                Parameters = method.GetInputJsonSchema(),
                RequiresApproval = attr.RequiresApproval,
            };
            tools.Add(descriptor);
            mapper[descriptor.Name] = method.Name;
        }

        return (tools, mapper);
    }
}
