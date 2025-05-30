using System.Reflection;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Tools;

namespace IronHive.Core.Tools;

public class FunctionToolPlugin<T> : IToolPlugin where T : class
{
    private readonly IServiceProvider _provider;
    private readonly IReadOnlyCollection<ToolDescriptor> _tools;
    private readonly IReadOnlyDictionary<string, MethodInfo> _methods;

    public FunctionToolPlugin(IServiceProvider serviceProvider)
    {
        _provider = serviceProvider;
        (_tools, _methods)= ExtractMethods();
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
            if(!_methods.TryGetValue(name, out var method))
                return ToolOutput.ToolNotFound(name);

            var instance = ActivatorUtilities.CreateInstance<T>(_provider);
            var parameters = method.GetParameters();
            var arguments = BuildArguments(parameters, input, cancellationToken);

            var invokeTask = Task.Run(() =>
            {
                return method.Invoke(instance, arguments);
            }, cancellationToken);
            var timeoutTask = Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            var completedTask = await Task.WhenAny(invokeTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                return ToolOutput.Failure("메서드 실행이 타임아웃되었습니다.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            var result = invokeTask.Result;

            var output = await HandleDynamicResultAsync(result, method.ReturnType, cancellationToken);
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
    private static object?[]? BuildArguments(
        ParameterInfo[] parameters, 
        ToolInput input, 
        CancellationToken cancellationToken)
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
    private static async Task<object?> HandleDynamicResultAsync(
        object? result, 
        Type resultType, 
        CancellationToken cancellationToken)
    {
        // Void 인 경우
        if (result == null || resultType == typeof(void))
        {
            return "completed function";
        }
        // Task 인 경우
        if (resultType == typeof(Task))
        {
            await ((Task)result).ConfigureAwait(false);
            return "completed function";
        }
        // ValueTask 인 경우
        if (resultType == typeof(ValueTask))
        {
            await ((ValueTask)result).ConfigureAwait(false);
            return "completed function";
        }
        if (resultType.IsGenericType)
        {
            var def = resultType.GetGenericTypeDefinition();

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
        if (resultType.GetMethod("GetAwaiter") != null)
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
    private static (IReadOnlyCollection<ToolDescriptor>, IReadOnlyDictionary<string, MethodInfo>) ExtractMethods()
    {
        var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        var tools = new List<ToolDescriptor>();
        var mappers = new Dictionary<string, MethodInfo>(StringComparer.Ordinal);

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<FunctionToolAttribute>();
            if (attr == null) continue;

            var descriptor = new ToolDescriptor
            {
                Name = attr.Name ?? method.Name,
                Description = attr.Description ?? method.GetCustomAttribute<DescriptionAttribute>()?.Description,
                Parameters = BuildInputJsonSchema(method),
                RequiresApproval = attr.RequiresApproval,
            };
            tools.Add(descriptor);
            mappers[descriptor.Name] = method;
        }

        return (tools, mappers);
    }

    /// <summary>
    /// Get the input JSON schema for the method.
    /// </summary>
    private static ObjectJsonSchema? BuildInputJsonSchema(MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length == 0)
        {
            return null;
        }

        var properties = new Dictionary<string, JsonSchema>();
        var required = new List<string>();

        foreach (var param in parameters)
        {
            // 이름이 없거나 out 매개변수인 경우
            if (string.IsNullOrEmpty(param.Name) || param.IsOut)
                continue;
            // 특정 타입의 경우
            if (param.ParameterType == typeof(CancellationToken))
                continue;
            // 특정 서비스의 경우
            if (param.GetCustomAttribute<FromKeyedServicesAttribute>() != null)
                continue;

            var description = param.GetCustomAttribute<DescriptionAttribute>()?.Description;
            var schema = JsonSchemaFactory.CreateFrom(param.ParameterType, description);
            properties.Add(param.Name, schema);

            // 필수 속성인지 확인
            if (!param.IsOptional)
                required.Add(param.Name);
        }

        return new ObjectJsonSchema
        {
            Properties = properties,
            Required = required.Count != 0 ? required : null,
        };
    }
}
