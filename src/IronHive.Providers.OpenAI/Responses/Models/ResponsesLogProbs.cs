using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses.Models;

public class ResponsesLogProb
{
    [JsonPropertyName("bytes")]
    public required int[] Bytes { get; set; }

    [JsonPropertyName("logprob")]
    public required float Logprob { get; set; }

    [JsonPropertyName("token")]
    public required string Token { get; set; }

    [JsonPropertyName("top_logprobs")]
    public IEnumerable<ResponsesLogProb>? TopLogprobs { get; set; }
}
