using System.Reflection;
using IronHive.Abstractions.Tools;

namespace IronHive.Core.Tools;

/// <summary>
/// Represents a tool that invokes a dotnet function.
/// </summary>
public class FunctionTool : ITool
{
    private readonly Delegate _function;

    /// <inheritdoc />
    public required string Name { get; set; }

    /// <inheritdoc />
    public string? Description { get; set; }

    /// <inheritdoc />
    public ToolParameters? Parameters { get; set; }

    /// <inheritdoc />
    public required bool RequiresApproval { get; set; }

    public FunctionTool(Delegate function)
    {
        _function = function;
        Parameters = _function.Method.GetParameters() is var parameters && parameters.Length > 0
            ? new ToolParameters(parameters)
            : null;
    }

    /// <inheritdoc />
    public async Task<ToolResult> InvokeAsync(object? args, CancellationToken cancellationToken = default)
    {
        try
        {
            var arguments = Parameters?.BuildArguments(args);
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
