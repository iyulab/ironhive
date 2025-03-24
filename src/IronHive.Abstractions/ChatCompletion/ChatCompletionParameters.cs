namespace IronHive.Abstractions.ChatCompletion;

/// <summary>
/// Inference parameters for chat completion model.
/// </summary>
public class ChatCompletionParameters
{
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
    public ICollection<string>? StopSequences { get; set; }
}
