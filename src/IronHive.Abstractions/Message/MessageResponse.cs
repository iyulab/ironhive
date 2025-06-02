using IronHive.Abstractions.Message.Roles;

namespace IronHive.Abstractions.Message;

/// <summary>
/// Represents the result of message response
/// </summary>
public class MessageResponse
{
    /// <summary>
    /// the response ended reason.
    /// </summary>
    public MessageDoneReason? DoneReason { get; set; }

    /// <summary>
    /// the data of the response.
    /// </summary>
    public required AssistantMessage Message { get; set; }

    /// <summary>
    /// the usage of tokens in the request.
    /// </summary>
    public MessageTokenUsage? TokenUsage { get; set; }
}
