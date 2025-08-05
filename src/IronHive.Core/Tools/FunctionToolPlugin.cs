using System.Reflection;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Tools;

namespace IronHive.Core.Tools;

/// <summary>
/// .NET 메서드를 툴로 제공하는 플러그인입니다.
/// </summary>
public class FunctionToolPlugin<T> : IToolPlugin where T : class
{
    private readonly IServiceProvider? _provider;
    private readonly IReadOnlyCollection<ToolDescriptor> _tools;
    private readonly IReadOnlyDictionary<string, MethodInfo> _methods;

    public FunctionToolPlugin(IServiceProvider? provider = null)
    {
        _provider = provider;
        (_tools, _methods)= ExtractToolsAndMethods();
    }

    /// <inheritdoc />
    public required string PluginName { get; init; }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public Task<IEnumerable<ToolDescriptor>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var result = _tools.Select(t => new ToolDescriptor
        {
            Name = t.Name,
            Description = t.Description,
            Parameters = t.Parameters,
            RequiresApproval = t.RequiresApproval,
        });
        return Task.FromResult(result);
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
                return ToolOutput.NotFound(name);

            var instance = _provider != null
                ? ActivatorUtilities.CreateInstance<T>(_provider)
                : Activator.CreateInstance<T>();

            var parameters = method.GetParameters();
            var arguments = BuildArguments(parameters, input, cancellationToken);

            // [TODO] 타임아웃 테스트 요구!!
            var invokeTask = Task.Run(() => method.Invoke(instance, arguments), cancellationToken);
            var timeoutTask = Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            var completedTask = await Task.WhenAny(invokeTask, timeoutTask);
            if (completedTask == timeoutTask)
                return ToolOutput.Failure("The function execution timed out after 5 minutes. Please try again later.");

            cancellationToken.ThrowIfCancellationRequested();
            var result = invokeTask.Result;

            var output = await HandleDynamicResultAsync(result, method.ReturnType, cancellationToken);
            var json = JsonSerializer.Serialize(output, JsonDefaultOptions.Options);

            // 결과가 너무 큰 경우
            return json.Length > 30_000
                ? ToolOutput.TooMuch()
                : ToolOutput.Success(json);
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
    /// 지정된 클래스 타입의 인스턴스에서 툴과 메서드를 추출합니다.
    /// </summary>
    private static (IReadOnlyCollection<ToolDescriptor>, IReadOnlyDictionary<string, MethodInfo>) ExtractToolsAndMethods()
    {
        var tools = new List<ToolDescriptor>();
        var methods = new Dictionary<string, MethodInfo>(StringComparer.Ordinal);

        foreach (var method in typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
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
            methods[descriptor.Name] = method;
        }

        return (tools, methods);
    }

    /// <summary>
    /// 툴 메서드의 입력 JSON 스키마를 빌드합니다.
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
