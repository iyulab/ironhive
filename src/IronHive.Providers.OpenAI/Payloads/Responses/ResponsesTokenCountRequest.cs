using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

public class ResponsesTokenCountRequest
{
    [JsonPropertyName("conversation")]
    public ResponsesConversation? Conversation { get; set; }

    [JsonPropertyName("input")]
    public required ICollection<ResponsesItem> Input { get; set; }

    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("parallel_tool_calls")]
    public bool? ParallelToolCalls { get; set; }

    [JsonPropertyName("previous_response_id")]
    public string? PreviousResponseId { get; set; }

    [JsonPropertyName("reasoning")]
    public ResponsesReasoning? Reasoning { get; set; }

    [JsonPropertyName("text")]
    public ResponsesText? Text { get; set; }

    [JsonPropertyName("tool_choice")]
    public ResponsesToolChoice? ToolChoice { get; set; }

    [JsonPropertyName("tools")]
    public IEnumerable<ResponsesTool>? Tools { get; set; }

    [JsonPropertyName("truncation")]
    public string? Truncation { get; set; }
}
