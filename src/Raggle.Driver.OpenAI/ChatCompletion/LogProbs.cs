using System.Text.Json.Serialization;

namespace Raggle.Driver.OpenAI.ChatCompletion;

internal class LogProbs
{
    [JsonPropertyName("content")]
    public LogProbTokens[]? Content { get; set; }

    [JsonPropertyName("refusal")]
    public LogProbTokens[]? Refusal { get; set; }
}

internal class LogProbTokens
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("logprob")]
    public float? LogProb { get; set; }

    [JsonPropertyName("bytes")]
    public int[]? Bytes { get; set; }

    [JsonPropertyName("top_logprobs")]
    public LogProbToken[]? TopLogProbs { get; set; }
}

internal class LogProbToken
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("logprob")]
    public float? LogProb { get; set; }

    [JsonPropertyName("bytes")]
    public int[]? Bytes { get; set; }
}