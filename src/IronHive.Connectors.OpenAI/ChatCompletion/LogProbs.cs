using System.Text.Json.Serialization;

namespace IronHive.Connectors.OpenAI.ChatCompletion;

internal class LogProbs
{
    [JsonPropertyName("content")]
    public IEnumerable<LogProbTokens>? Content { get; set; }

    [JsonPropertyName("refusal")]
    public IEnumerable<LogProbTokens>? Refusal { get; set; }
}

internal class LogProbTokens
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("logprob")]
    public float? LogProb { get; set; }

    [JsonPropertyName("bytes")]
    public IEnumerable<int>? Bytes { get; set; }

    [JsonPropertyName("top_logprobs")]
    public IEnumerable<LogProbToken>? TopLogProbs { get; set; }
}

internal class LogProbToken
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("logprob")]
    public float? LogProb { get; set; }

    [JsonPropertyName("bytes")]
    public IEnumerable<int>? Bytes { get; set; }
}