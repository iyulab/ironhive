using Microsoft.Extensions.AI;

namespace IronHive.Extensions.AI;

/// <summary>
/// 스트리밍 중인 도구 호출 인자의 한 조각입니다 — provider가 인자를 쓰는 동안 보낸 원문 조각 그대로
/// (Chat Completions <c>tool_calls[].function.arguments</c>, Responses <c>function_call_arguments.delta</c>,
/// Anthropic <c>input_json_delta</c>). 요청의 <see cref="ChatOptions.AdditionalProperties"/>에
/// <see cref="ChatClientAdapter.StreamToolArgumentsKey"/> = <c>true</c>가 있을 때만 <see cref="ChatClientAdapter"/>가 냅니다.
/// </summary>
/// <remarks>
/// 같은 <see cref="CallId"/>의 조각을 이어 붙이면 그 호출의 인자 원문이 됩니다. 조각은 부분 JSON이라 해석하지 않습니다.
/// 호출이 끝나면 같은 <see cref="CallId"/>의 완전한 <see cref="FunctionCallContent"/>가 따로 옵니다 — 도구는 그것으로
/// 실행되고, 이 조각들은 진행 표시용입니다. 히스토리에 섞여 돌아와도 어댑터는 provider에 보내지 않습니다.
/// Google provider는 호출을 한 번에 보내므로 조각이 없습니다.
/// </remarks>
public sealed class FunctionCallDeltaContent : AIContent
{
    /// <summary>
    /// 새 조각을 만듭니다.
    /// </summary>
    /// <param name="callId">호출 id — 뒤따르는 <c>FunctionCallContent.CallId</c>와 같습니다.</param>
    /// <param name="argumentsFragment">인자 원문의 한 조각(빈 문자열일 수 있습니다).</param>
    /// <param name="name">도구 이름 — 호출의 첫 조각에만 실립니다.</param>
    public FunctionCallDeltaContent(string callId, string argumentsFragment, string? name = null)
    {
        CallId = callId ?? throw new ArgumentNullException(nameof(callId));
        ArgumentsFragment = argumentsFragment ?? string.Empty;
        Name = name;
    }

    /// <summary>호출 id — 뒤따르는 완전한 <c>FunctionCallContent.CallId</c>와 같습니다.</summary>
    public string CallId { get; }

    /// <summary>도구 이름. 호출의 첫 조각에만 있고, 이후 조각은 null입니다.</summary>
    public string? Name { get; }

    /// <summary>인자 원문의 한 조각 — 부분 JSON입니다.</summary>
    public string ArgumentsFragment { get; }
}
