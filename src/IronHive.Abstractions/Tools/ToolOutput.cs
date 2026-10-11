using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;

namespace IronHive.Abstractions.Tools;

/// <summary>
/// 도구(툴) 실행의 결과를 나타내는 클래스입니다.
/// 성공 여부와 결과 데이터(또는 오류 메시지)를 포함합니다.
/// </summary>
public class ToolOutput
{
    /// <summary>
    /// 도구 실행이 성공했는지를 나타냅니다.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// LLM에게 반환할 도구 실행에 대한 결과 콘텐츠입니다.
    /// 성공한 경우, 결과 콘텐츠 블록들을 포함합니다.
    /// 실패한 경우, 오류 메시지를 담은 텍스트 콘텐츠를 포함합니다.
    /// </summary>
    public IReadOnlyList<MessageContent> Content { get; set; } = [];

    /// <summary>
    /// 기본 생성자입니다.
    /// </summary>
    public ToolOutput()
    { }

    /// <summary>
    /// 결과 상태와 콘텐츠를 지정하여 객체를 생성합니다.
    /// </summary>
    /// <param name="isSuccess">성공 여부</param>
    /// <param name="content">결과 콘텐츠 블록들</param>
    public ToolOutput(bool isSuccess, IReadOnlyList<MessageContent> content)
    {
        IsSuccess = isSuccess;
        Content = content;
    }

    /// <summary>
    /// 도구 실행이 성공했을 때의 결과를 생성합니다.
    /// </summary>
    /// <param name="content">실행 결과 콘텐츠 블록들</param>
    /// <returns>성공한 <see cref="ToolOutput"/> 객체</returns>
    public static ToolOutput Success(IEnumerable<MessageContent> content)
        => new(true, content as IReadOnlyList<MessageContent> ?? content.ToArray());

    /// <summary>
    /// 도구 실행이 성공했을 때의 결과를 텍스트로 생성합니다.
    /// </summary>
    /// <param name="text">실행 결과 텍스트</param>
    /// <returns>성공한 <see cref="ToolOutput"/> 객체</returns>
    public static ToolOutput Success(string? text)
        => new(true, text is null ? [] : [new TextMessageContent { Value = text }]);

    /// <summary>
    /// 도구 실행이 실패했을 때의 결과를 생성합니다.
    /// </summary>
    /// <param name="error">오류 메시지</param>
    /// <returns>실패한 <see cref="ToolOutput"/> 객체</returns>
    public static ToolOutput Failure(string? error)
        => new(false, error is null ? [] : [new TextMessageContent { Value = error }]);

    /// <summary>
    /// 결과 콘텐츠를 하나의 문자열로 평탄화합니다 — 텍스트만 나르는 자리(문자열 하나만 받는 provider의 도구 결과 wire,
    /// 텍스트 스트림 소비자)를 위한 형태입니다. 텍스트 블록은 그대로 줄바꿈으로 잇고, 텍스트가 아닌 블록은
    /// <paramref name="describeNonText"/>가 돌려준 문자열로 대신합니다.
    /// </summary>
    /// <param name="describeNonText">
    /// 텍스트가 아닌 블록을 대신할 문자열. 생략하면 블록 종류를 이름으로 밝히는 <c>[image content omitted]</c> 형태입니다.
    /// </param>
    /// <returns>평탄화된 텍스트. 콘텐츠가 없으면 빈 문자열입니다.</returns>
    public string ToText(Func<MessageContent, string>? describeNonText = null)
    {
        if (Content.Count == 0)
            return string.Empty;

        describeNonText ??= DescribeOmitted;
        return string.Join("\n", Content.Select(c => c is TextMessageContent text ? text.Value ?? string.Empty : describeNonText(c)));
    }

    private static string DescribeOmitted(MessageContent content)
    {
        var kind = content.GetType().Name;
        if (kind.EndsWith(nameof(MessageContent), StringComparison.Ordinal) && kind.Length > nameof(MessageContent).Length)
            kind = kind[..^nameof(MessageContent).Length];
        return $"[{kind.ToLowerInvariant()} content omitted]";
    }
}