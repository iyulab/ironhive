using IronHive.Abstractions.Messages.Content;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 도구 실행 동작 설정입니다.
/// MessageRequest.ToolOptions에 지정합니다.
/// </summary>
public class ToolOptions
{
    /// <summary>
    /// 병렬로 동시에 실행할 최대 도구 수입니다.
    /// </summary>
    public int MaxParallel { get; set; } = 3;

    /// <summary>
    /// 개별 도구 실행의 최대 허용 시간입니다.
    /// null이면 제한 없음입니다.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// 도구 실행 직전에 호출됩니다. content(Input/Name 등)를 직접 수정할 수 있습니다.
    /// 이 델리게이트 안에서 content.Output을 채우면 실제 도구 실행을 스킵합니다.
    /// 병렬로 여러 도구에 대해 동시에 호출될 수 있으므로 상태를 공유하지 않아야 합니다.
    /// </summary>
    public Func<ToolMessageContent, CancellationToken, Task>? OnBeforeInvoke { get; set; }

    /// <summary>
    /// 도구 실행 직후(성공/실패 모두)에 호출됩니다. content.Output을 직접 수정할 수 있습니다.
    /// 병렬로 여러 도구에 대해 동시에 호출될 수 있으므로 상태를 공유하지 않아야 합니다.
    /// 그래서 이 훅은 결과 하나만 다룹니다 — 도구 루프 전체의 결과 합계를 제한하려면
    /// IronHive.Core의 <c>ToolResultBudgetMiddleware</c>(메시지 미들웨어)를 등록하세요.
    /// </summary>
    public Func<ToolMessageContent, CancellationToken, Task>? OnAfterInvoke { get; set; }
}
