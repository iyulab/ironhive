using Raggle.Abstractions.Messages;
using Raggle.Abstractions.Tools;

namespace Raggle.Abstractions.AI;

/// <summary>
/// Represents a request for chat completion.
/// </summary>
public class ChatCompletionRequest
{
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
    /// the maximum number of tokens to generate.
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Value ranges from 0.0 to 1.0.
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// Value ranges from 0 to 100.
    /// </summary>
    public int? TopK { get; set; }

    /// <summary>
    /// Value ranges from 0.0 to 1.0.
    /// </summary>
    public float? TopP { get; set; }

    /// <summary>
    /// the sequences to stop text generation
    /// </summary>
    public string[]? StopSequences { get; set; }

    /// <summary>
    /// the tool list to use in the model.
    /// </summary>
    public FunctionToolCollection? Tools { get; set; }



}
