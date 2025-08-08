using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion;

public class ChatLogProbs
{
    [JsonPropertyName("content")]
    public IEnumerable<LogProbTokens>? Content { get; set; }

    [JsonPropertyName("refusal")]
    public IEnumerable<LogProbTokens>? Refusal { get; set; }
}

public class LogProbTokens : LogProbToken
{
    [JsonPropertyName("top_logprobs")]
    public IEnumerable<LogProbToken>? TopLogProbs { get; set; }
}

public class LogProbToken
{
    [JsonPropertyName("bytes")]
    public IEnumerable<int>? Bytes { get; set; }

    [JsonPropertyName("logprob")]
    public float? LogProb { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }
}