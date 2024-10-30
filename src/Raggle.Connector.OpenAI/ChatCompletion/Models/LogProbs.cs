using System.Text.Json.Serialization;

namespace Raggle.Connector.OpenAI.ChatCompletion.Models;

internal class LogProbs
{
    [JsonPropertyName("content")]
    internal LogProbTokens[]? Content { get; set; }

    [JsonPropertyName("refusal")]
    internal LogProbTokens[]? Refusal { get; set; }
}

internal class LogProbTokens
{
    [JsonPropertyName("token")]
    internal string? Token { get; set; }

    [JsonPropertyName("logprob")]
    internal double? LogProb { get; set; }

    [JsonPropertyName("bytes")]
    internal int[]? Bytes { get; set; }

    [JsonPropertyName("top_logprobs")]
    internal LogProbToken[]? TopLogProbs { get; set; }
}

internal class LogProbToken
{
    [JsonPropertyName("token")]
    internal string? Token { get; set; }

    [JsonPropertyName("logprob")]
    internal double? LogProb { get; set; }

    [JsonPropertyName("bytes")]
    internal int[]? Bytes { get; set; }
}