namespace IronHive.Abstractions.Messages;

/// <summary>
/// <see cref="MessageToolChoice"/>가 나타내는 모드입니다.
/// </summary>
public enum MessageToolChoiceMode
{
    /// <summary>
    /// 모델이 도구 호출 여부를 자유롭게 결정합니다.
    /// </summary>
    Auto,

    /// <summary>
    /// 도구 목록이 있어도 도구 호출을 억제합니다.
    /// </summary>
    None,

    /// <summary>
    /// 최소 한 개 이상의 도구 호출을 강제합니다.
    /// </summary>
    Required,

    /// <summary>
    /// 지정된 이름의 특정 도구 호출을 강제합니다.
    /// </summary>
    Function
}

/// <summary>
/// 모델이 도구를 호출할지, 어떤 도구를 호출할지를 제어합니다.
/// <see cref="MessageGenerationRequest.Tools"/>가 비어있지 않을 때만 의미가 있습니다.
/// </summary>
public sealed class MessageToolChoice
{
    /// <summary>
    /// 모델이 도구 호출 여부를 자유롭게 결정합니다. 값을 지정하지 않은 것과 동일합니다.
    /// </summary>
    public static readonly MessageToolChoice Auto = new(MessageToolChoiceMode.Auto, null);

    /// <summary>
    /// 도구 목록이 있어도 도구 호출을 억제합니다.
    /// </summary>
    public static readonly MessageToolChoice None = new(MessageToolChoiceMode.None, null);

    /// <summary>
    /// 최소 한 개 이상의 도구 호출을 강제합니다.
    /// </summary>
    public static readonly MessageToolChoice Required = new(MessageToolChoiceMode.Required, null);

    /// <summary>
    /// 지정된 이름의 특정 도구 호출을 강제합니다.
    /// </summary>
    /// <param name="name">강제할 도구의 이름입니다.</param>
    public static MessageToolChoice Function(string name) =>
        new(MessageToolChoiceMode.Function, name ?? throw new ArgumentNullException(nameof(name)));

    /// <summary>
    /// 이 값이 나타내는 모드입니다.
    /// </summary>
    public MessageToolChoiceMode Mode { get; }

    /// <summary>
    /// <see cref="Mode"/>가 <see cref="MessageToolChoiceMode.Function"/>일 때 강제할 도구 이름입니다.
    /// 그 외의 모드에서는 null입니다.
    /// </summary>
    public string? FunctionName { get; }

    private MessageToolChoice(MessageToolChoiceMode mode, string? functionName)
    {
        Mode = mode;
        FunctionName = functionName;
    }
}
