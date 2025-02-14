using Raggle.Abstractions.Messages;
using Raggle.Abstractions.Tools;

namespace Raggle.Abstractions.AI;

/// <summary>
/// Represents a request for chat completion.
/// </summary>
public class MessageContext
{
    /// <summary>
    /// chat completion model name.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// the chat history to use for completion.
    /// </summary>
    public MessageCollection Messages { get; set; } = new();

    /// <summary>
    /// the tool list to use in the model.
    /// </summary>
    public ToolCollection? Tools { get; set; }

    /// <summary>
    /// the chat completion parameters.
    /// </summary>
    public ChatCompletionParameters? Parameters { get; set; }

    /// <summary>
    /// Message management options.
    /// </summary>
    public MessageOptions? MessagesOptions { get; set; }

    /// <summary>
    /// Tool execution options.
    /// </summary>
    public ToolExecutionOptions? ToolOptions { get; set; }
}
