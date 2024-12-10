using Raggle.Abstractions.Messages;
using Raggle.Abstractions.Tools;

namespace Raggle.Abstractions.AI;

/// <summary>
/// Represents a request for chat completion.
/// </summary>
public class ChatCompletionRequest
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
    /// Gets or sets the maximum number of tokens to generate.
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Gets or sets the temperature for the model. Value ranges from 0.0 to 1.0.
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// Gets or sets the top K value for the model. Value ranges from 0 to 100.
    /// </summary>
    public int? TopK { get; set; }

    /// <summary>
    /// Gets or sets the top P value for the model. Value ranges from 0.0 to 1.0.
    /// </summary>
    public float? TopP { get; set; }

    /// <summary>
    /// Gets or sets the stop sequences to stop text generation at specified sequences.
    /// </summary>
    public string[]? StopSequences { get; set; }

    /// <summary>
    /// Gets or sets the tools to use in the model.
    /// </summary>
    public FunctionToolCollection? Tools { get; set; }
}
