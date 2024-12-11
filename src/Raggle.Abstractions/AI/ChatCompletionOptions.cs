namespace Raggle.Abstractions.AI;

public class ChatCompletionOptions
{
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
}
