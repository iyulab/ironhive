using Raggle.Abstractions.Tools;

namespace Raggle.Abstractions.AI;

public class ChatCompletionOptions
{
    /// <summary>
    /// The Chat Completion Model ID.
    /// </summary>
    public required string ModelId { get; set; }

    /// <summary>
    /// Instructions to the model.
    /// </summary>
    public string? System { get; set; }

    /// <summary>
    /// Maximum number of tokens to generate.
    /// </summary>
    public int MaxTokens { get; set; } = 2048;

    /// <summary>
    /// Tools to use in the model.
    /// </summary>
    public FunctionTool[]? Tools { get; set; }

    /// <summary>
    /// 0.0 to 1.0
    /// </summary>
    public double? Temperature { get; set; }

    /// <summary>
    /// 0 to 100
    /// </summary>
    public int? TopK { get; set; }

    /// <summary>
    /// 0.0 to 1.0
    /// </summary>
    public double? TopP { get; set; }

    /// <summary>
    /// Stop Text generation at specified sequences.
    /// </summary>
    public string[]? StopSequences { get; set; }
}
