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
}