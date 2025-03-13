using IronHive.Abstractions.ChatCompletion.Tools;

namespace IronHive.Abstractions.ChatCompletion;

public class ChatCompletionOptions
{
    /// <summary>
    /// chat completion model name.
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// system message to generate a response to.
    /// </summary>
    public string? System { get; set; }

    /// <summary>
    /// the tool list to use in the model.
    /// </summary>
    public ToolCollection? Tools { get; set; }

    /// <summary>
    /// Tool execution options.
    /// </summary>
    public IToolChoice? ToolChoice { get; set; }

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
