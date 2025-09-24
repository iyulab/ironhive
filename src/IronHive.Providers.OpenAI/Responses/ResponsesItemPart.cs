using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ResponsesTextItemPart), "output_text")]
[JsonDerivedType(typeof(ResponsesRefusalItemPart), "refusal")]
[JsonDerivedType(typeof(ResponsesReasoningItemPart), "reasoning_text")]
[JsonDerivedType(typeof(ResponsesSummaryReasoningItemPart), "summary_text")]
internal abstract class ResponsesItemPart
{ }

internal class ResponsesTextItemPart : ResponsesItemPart
{
    [JsonPropertyName("annotations")]
    public ICollection<object>? Annotations { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; }

    [JsonPropertyName("logprobs")]
    public required ICollection<ResponsesLogProbs> Logprobs { get; set; }
}

internal class ResponsesRefusalItemPart : ResponsesItemPart
{
    [JsonPropertyName("refusal")]
    public required string Refusal { get; set; }
}

internal class ResponsesReasoningItemPart : ResponsesItemPart
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

internal class ResponsesSummaryReasoningItemPart : ResponsesItemPart
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}
