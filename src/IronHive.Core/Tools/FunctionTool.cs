using System.ComponentModel;
using System.Reflection;
using IronHive.Abstractions.Tools;

namespace IronHive.Core.Tools;

/// <summary>
/// Represents a tool that invokes a dotnet function.
/// </summary>
public class FunctionTool : ITool
{
    private readonly Delegate _function;

    public FunctionTool(Delegate function)
    {
        _function = function;
        Name = function.Method.GetCustomAttribute<FunctionToolAttribute>()?.Name ?? function.Method.Name;
        Description = function.Method.GetCustomAttribute<DescriptionAttribute>()?.Description;
        InputSchema = _function.Method.GetInputJsonSchema();
    }

    /// <inheritdoc />
    public ToolPermission Permission { get; set; } = ToolPermission.Auto;

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public string? Description { get; set; }

    /// <inheritdoc />
    public object? InputSchema { get; set; }

    /// <inheritdoc />
    public async Task<ToolResult> InvokeAsync(object? args, CancellationToken cancellationToken = default)
    {
        try
        {
            var arguments = BuildArguments(args);
            var result = _function.DynamicInvoke(arguments);
            cancellationToken.ThrowIfCancellationRequested();

            result = await HandleResultAsync(result, _function.Method.ReturnType, cancellationToken);

            return ToolResult.Success(result);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            return ToolResult.Failed(ex.InnerException.Message);
        }
        catch (Exception ex)
        {
            return ToolResult.Failed(ex.Message);
        }
    }

    // delegate의 인자들을 빌드합니다.
    private object?[]? BuildArguments(object? args)
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

            if (dictionary.TryGetValue(name, out var value))
            {
                // 인자가 존재하는 경우
                arguments[i] = value.ConvertTo(param.ParameterType);
            }
            else if (param.HasDefaultValue)
            {
                // 인자가 존재하지 않고, 기본값이 있는 경우
                arguments[i] = param.DefaultValue;
            }
            else if (param.IsOptional)
            {
                // 인자가 존재하지 않고, 선택적 인자인 경우
                arguments[i] = null;
            }
            else
            {
                // 인자가 존재하지 않고, 필수 인자인 경우
                throw new ArgumentException($"Parameter '{name}' is required");
            }
        }

        return arguments;
    }

    // 비동기 메서드를 적절히 처리합니다.
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
