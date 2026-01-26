namespace IronHive.Providers.OpenAI.Payloads.ChatCompletion;

public enum ChatReasoningEffort
{
    /// <summary>
    /// No reasoning effort
    /// </summary>
    None,

    /// <summary>
    /// Very low reasoning effort
    /// </summary>
    Minimal,

    /// <summary>
    /// Low reasoning effort
    /// </summary>
    Low,
    
    /// <summary>
    /// Medium reasoning effort
    /// </summary>
    Medium,

    /// <summary>
    /// High reasoning effort
    /// </summary>
    High,

    /// <summary>
    /// Very high reasoning effort
    /// </summary>
    Xhigh
}
