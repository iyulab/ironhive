using System.Text.Json.Serialization;

namespace Raggle.Engines.OpenAI.ChatCompletion;

public class LogProbs
{
    [JsonPropertyName("content")]
    public ICollection<LogProbTokens>? Content { get; set; }

    [JsonPropertyName("refusal")]
    public ICollection<LogProbTokens>? Refusal { get; set; }
}

public class LogProbTokens
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("logprob")]
    public double? LogProb { get; set; }

    [JsonPropertyName("bytes")]
    public ICollection<int>? Bytes { get; set; }

    [JsonPropertyName("top_logprobs")]
    public ICollection<LogProbToken>? TopLogProbs { get; set; }
}

public class LogProbToken
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("logprob")]
    public double? LogProb { get; set; }

    [JsonPropertyName("bytes")]
    public ICollection<int>? Bytes { get; set; }
}