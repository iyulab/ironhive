using Raggle.Abstractions.Messages;
using Raggle.Abstractions.Tools;

namespace Raggle.Abstractions.AI;

/// <summary>
/// Represents a request for chat completion.
/// </summary>
public class MessageContext
{
    private int _count = 0;

    public void TryCount()
    {
        _count++;
    }

    /// <summary>
    /// chat completion model name.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// the instructions to the model.
    /// </summary>
    public string? System { get; set; }

    /// <summary>
    /// the chat history to use for completion.
    /// </summary>
    public MessageCollection Messages { get; set; } = new();

    /// <summary>
    /// the chat completion parameters.
    /// </summary>
    public ChatCompletionParameters? Parameters { get; set; }

    /// <summary>
    /// the tool list to use in the model.
    /// </summary>
    public FunctionToolCollection? Tools { get; set; }

    /// <summary>
    /// the maximum number of tries to generate.
    /// </summary>
    public int MaxTryCount => _count;
}
