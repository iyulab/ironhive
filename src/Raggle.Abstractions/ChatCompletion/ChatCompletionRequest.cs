using Raggle.Abstractions.ChatCompletion.Messages;
using Raggle.Abstractions.ChatCompletion.Tools;

namespace Raggle.Abstractions.ChatCompletion;

/// <summary>
/// Represents a request for chat completion.
/// </summary>
public class ChatCompletionRequest : ChatCompletionParameters
{
    /// <summary>
    /// chat completion model name.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// system message to generate a response to.
    /// </summary>
    public string? System { get; set; }

    /// <summary>
    /// the chat history to use for completion.
    /// </summary>
    public MessageCollection Messages { get; set; } = new();

    /// <summary>
    /// the tool list to use in the model.
    /// </summary>
    public ToolCollection? Tools { get; set; }

    /// <summary>
    /// Tool execution options.
    /// </summary>
    public IToolChoice? ToolChoice { get; set; }
}
