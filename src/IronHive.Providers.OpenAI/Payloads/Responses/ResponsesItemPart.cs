using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ResponsesTextItemPart), "output_text")]
[JsonDerivedType(typeof(ResponsesRefusalItemPart), "refusal")]
[JsonDerivedType(typeof(ResponsesReasoningItemPart), "reasoning_text")]
[JsonDerivedType(typeof(ResponsesSummaryReasoningItemPart), "summary_text")]
public abstract class ResponsesItemPart
{ }

public class ResponsesTextItemPart : ResponsesItemPart
{
    [JsonPropertyName("annotations")]
    public ICollection<ResponsesAnnotation>? Annotations { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; }

    [JsonPropertyName("logprobs")]
    public IEnumerable<ResponsesLogProb>? Logprobs { get; set; }
}

public class ResponsesRefusalItemPart : ResponsesItemPart
{
    [JsonPropertyName("refusal")]
    public required string Refusal { get; set; }
}

public class ResponsesReasoningItemPart : ResponsesItemPart
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public class ResponsesSummaryReasoningItemPart : ResponsesItemPart
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}
