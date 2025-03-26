namespace IronHive.Abstractions.Tools;

/// <summary>
/// 툴 실행 핸들러의 기본 추상 클래스입니다.
/// </summary>
public abstract class ToolHandlerBase : IToolHandler
{
    /// <inheritdoc />
    public virtual Task HandleInitializedAsync(object? options)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task<string> HandleSetInstructionsAsync(object? options)
    {
        return Task.FromResult(string.Empty);
    }
}
