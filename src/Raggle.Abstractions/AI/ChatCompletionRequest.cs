using Raggle.Abstractions.Messages;
using Raggle.Abstractions.Tools;

namespace Raggle.Abstractions.AI;

/// <summary>
/// Represents a request for chat completion.
/// </summary>
public class ChatCompletionRequest : ChatCompletionOptions
{
    /// <summary>
    /// Gets or sets the chat completion model name.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the instructions to the model.
    /// </summary>
    public string? System { get; set; }

    /// <summary>
    /// Gets or sets the chat history to use for completion.
    /// </summary>
    public MessageCollection Messages { get; set; } = new();

    /// <summary>
    /// Gets or sets the tools to use in the model.
    /// </summary>
    public FunctionToolCollection? Tools { get; set; }
}
