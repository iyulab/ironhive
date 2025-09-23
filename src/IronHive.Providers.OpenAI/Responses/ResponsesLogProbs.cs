using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses;

public class ResponsesLogProbs
{
    [JsonPropertyName("logprob")]
    public required float Logprob { get; set; }

    [JsonPropertyName("token")]
    public required string Token { get; set; }

    [JsonPropertyName("top_logprobs")]
    public IEnumerable<ResponsesLogProb>? TopLogprobs { get; set; }
}

public class ResponsesLogProb
{
    [JsonPropertyName("logprob")]
    public required float Logprob { get; set; }

    [JsonPropertyName("token")]
    public required string Token { get; set; }
}
