using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Tools;

namespace IronHive.Core.Tools;

/// <summary>
/// Built-in dotnet function tool.
/// </summary>
public class FunctionTool : ITool
{
    private readonly Delegate _function;

    public FunctionTool(Delegate function)
    {
        var attr = function.Method.GetCustomAttribute<FunctionToolAttribute>();
        Name = attr?.Name ?? function.Method.Name;
        Description = attr?.Description ?? function.Method.GetCustomAttribute<DescriptionAttribute>()?.Description;
        RequiresApproval = attr?.RequiresApproval ?? false;
        InputSchema = function.Method.GetInputJsonSchema();
        _function = function;
    }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public string? Description { get; set; }

    /// <inheritdoc />
    public object? InputSchema { get; set; }

    /// <inheritdoc />
    public bool RequiresApproval { get; set; } = false;

    /// <inheritdoc />
    public async Task<ToolResult> InvokeAsync(object? args, CancellationToken cancellationToken = default)
    {
        try
        {
            var arguments = BuildArguments(args, cancellationToken);
            var obj = _function.DynamicInvoke(arguments);
            cancellationToken.ThrowIfCancellationRequested();

            obj = await HandleResultAsync(obj, _function.Method.ReturnType, cancellationToken);
            var str = JsonSerializer.Serialize(obj, JsonDefaultOptions.Options);

            if (str.Length > 30_000)
            {
                return ToolResult.ExcessiveResult();
            }
            return ToolResult.Success(str);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            return ToolResult.Failure(ex.InnerException.Message);
        }
        catch (Exception ex)
        {
            return ToolResult.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Delegate의 인자를 빌드합니다.
    /// </summary>
    private object?[]? BuildArguments(object? args, CancellationToken cancellationToken)
    {
        var parameters = _function.Method.GetParameters();
        if (parameters.Length == 0 || args == null)
            return null;

        var dictionary = args.ConvertTo<IDictionary<string, object?>>()
            ?? new Dictionary<string, object?>();
        var arguments = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var name = param.Name ?? throw new InvalidOperationException("Parameter name cannot be null.");

            // 특별한 타입의 경우
            if (param.ParameterType == typeof(CancellationToken))
                arguments[i] = cancellationToken;
            // 인자가 존재하는 경우
            else if (dictionary.TryGetValue(name, out var value))
                arguments[i] = value.ConvertTo(param.ParameterType);
            // 인자가 존재하지 않고, 기본값이 있는 경우
            else if (param.HasDefaultValue)
                arguments[i] = param.DefaultValue;
            // 인자가 존재하지 않고, 선택적 인자인 경우
            else if (param.IsOptional)
                arguments[i] = null;
            // 인자가 존재하지 않고, 필수 인자인 경우
            else
                throw new ArgumentException($"Parameter '{name}' is required");
        }

        return arguments;
    }

    /// <summary>
    /// 비동기 메서드를 적절히 처리합니다.
    /// </summary>
    private static async Task<object?> HandleResultAsync(object? result, Type returnType, CancellationToken cancellationToken)
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
        else if (returnType == typeof(ValueTask))
        {
            await ((ValueTask)result).ConfigureAwait(false);
            return "completed function";
        }
        // Task<T>인 경우
        else if (returnType.IsGenericType == true &&
            returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            dynamic dynTask = result;
            var taskResult = await dynTask.ConfigureAwait(false);
            return taskResult;
        }
        // ValueTask<T>인 경우
        else if (returnType.IsGenericType == true &&
            returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            dynamic dynValueTask = result;
            var valueTaskResult = await dynValueTask.ConfigureAwait(false);
            return valueTaskResult;
        }
        // IAsyncEnumerable<T>인 경우
        else if (returnType.IsGenericType == true &&
            returnType.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
        {
            var enumerableResult = new List<object?>();
            var enumerator = ((IAsyncEnumerable<object?>)result).GetAsyncEnumerator(cancellationToken);
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                enumerableResult.Add(enumerator.Current);
            }
            return enumerableResult;
        }
        // 커스텀 awaitable 인 경우
        else if (returnType.GetMethod("GetAwaiter") != null)
        {
            dynamic dynResult = result;
            var awaited = await dynResult;
            return awaited;
        }
        // 이외 의 경우 그대로 반환
        else
        {
            return result;
        }
    }
}
