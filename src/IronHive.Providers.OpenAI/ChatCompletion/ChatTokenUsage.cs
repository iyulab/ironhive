using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion;

public class ChatTokenUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }

    [JsonPropertyName("prompt_tokens_details")]
    public PromptTokensDetails? PromptTokensDetails { get; set; }

    [JsonPropertyName("completion_tokens_details")]
    public CompletionTokensDetails? CompletionTokensDetails { get; set; }
}

public class CompletionTokensDetails
{
    [JsonPropertyName("accepted_prediction_tokens")]
    public int AcceptedTokens { get; set; }

    [JsonPropertyName("audio_tokens")]
    public int AudioTokens { get; set; }

    [JsonPropertyName("reasoning_tokens")]
    public int ReasoningTokens { get; set; }

    [JsonPropertyName("rejected_prediction_tokens")]
    public int RejectedTokens { get; set; }
}

public class PromptTokensDetails
{
    [JsonPropertyName("audio_tokens")]
    public int AudioTokens { get; set; }

    [JsonPropertyName("cached_tokens")]
    public int CachedTokens { get; set; }
}