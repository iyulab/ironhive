using System.Text.Json.Serialization;

namespace IronHive.Providers.Ollama.Payloads;

/// <summary>
/// Represents configuration options for the language model.
/// </summary>
internal class OllamaModelOptions
{
    /// <summary>
    /// Enables Mirostat sampling for controlling perplexity.
    /// 0 = disabled, 1 = Mirostat, 2 = Mirostat 2.0. Default is 0.
    /// </summary>
    [JsonPropertyName("mirostat")]
    public int? Mirostat { get; set; }

    /// <summary>
    /// Influences how quickly the algorithm responds to feedback from the generated text.
    /// A lower learning rate results in slower adjustments, while a higher learning rate makes the algorithm more responsive.
    /// Default is 0.1.
    /// </summary>
    [JsonPropertyName("mirostat_eta")]
    public float? MirostatEta { get; set; }

    /// <summary>
    /// Controls the balance between coherence and diversity of the output.
    /// A lower value results in more focused and coherent text.
    /// Default is 5.0.
    /// </summary>
    [JsonPropertyName("mirostat_tau")]
    public float? MirostatTau { get; set; }

    /// <summary>
    /// Sets the size of the context window used to generate the next token.
    /// Default is 2048.
    /// </summary>
    [JsonPropertyName("num_ctx")]
    public int? NumCtx { get; set; }

    /// <summary>
    /// Sets how far back for the model to look back to prevent repetition.
    /// 0 = disabled, -1 = num_ctx. Default is 64.
    /// </summary>
    [JsonPropertyName("repeat_last_n")]
    public int? RepeatLastN { get; set; }

    /// <summary>
    /// Sets how strongly to penalize repetitions.
    /// A higher value (e.g., 1.5) penalizes repetitions more strongly,
    /// while a lower value (e.g., 0.9) is more lenient.
    /// Default is 1.1.
    /// </summary>
    [JsonPropertyName("repeat_penalty")]
    public float? RepeatPenalty { get; set; }

    /// <summary>
    /// The temperature of the model.
    /// Increasing the temperature makes the model answer more creatively.
    /// Default is 0.8.
    /// </summary>
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    /// <summary>
    /// Sets the random number seed to use for generation.
    /// Setting this to a specific number will make the model generate the same text for the same prompt.
    /// Default is 0.
    /// </summary>
    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    /// <summary>
    /// Sets the stop sequences to use.
    /// When this pattern is encountered, the LLM will stop generating text and return.
    /// Default is "AI assistant:".
    /// </summary>
    [JsonPropertyName("stop")]
    public string? Stop { get; set; }

    /// <summary>
    /// Tail free sampling is used to reduce the impact of less probable tokens from the output.
    /// A higher value (e.g., 2.0) reduces the impact more,
    /// while a value of 1.0 disables this setting.
    /// Default is 1.0.
    /// </summary>
    [JsonPropertyName("tfs_z")]
    public float? TfsZ { get; set; }

    /// <summary>
    /// Maximum number of tokens to predict when generating text.
    /// -1 = infinite generation, -2 = fill context. Default is 128.
    /// </summary>
    [JsonPropertyName("num_predict")]
    public int? NumPredict { get; set; }

    /// <summary>
    /// Reduces the probability of generating nonsense.
    /// A higher value (e.g., 100) gives more diverse answers,
    /// while a lower value (e.g., 10) is more conservative.
    /// Default is 40.
    /// </summary>
    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }

    /// <summary>
    /// Works together with TopK.
    /// A higher value (e.g., 0.95) leads to more diverse text,
    /// while a lower value (e.g., 0.5) generates more focused and conservative text.
    /// Default is 0.9.
    /// </summary>
    [JsonPropertyName("top_p")]
    public float? TopP { get; set; }

    /// <summary>
    /// Alternative to TopP, aiming to ensure a balance of quality and variety.
    /// The parameter p represents the minimum probability for a token to be considered,
    /// relative to the probability of the most likely token.
    /// For example, with p=0.05 and the most likely token having a probability of 0.9,
    /// logits with a value less than 0.045 are filtered out.
    /// Default is 0.0.
    /// </summary>
    [JsonPropertyName("min_p")]
    public float? MinP { get; set; }
}
