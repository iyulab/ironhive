using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ResponsesMessageItem), "message")]
[JsonDerivedType(typeof(ResponsesReasoningItem), "reasoning")]
[JsonDerivedType(typeof(ResponsesFunctionToolCallItem), "function_call")]
[JsonDerivedType(typeof(ResponsesFunctionToolOutputItem), "function_call_output")]
[JsonDerivedType(typeof(ResponsesCustomToolCallItem), "custom_call")]
[JsonDerivedType(typeof(ResponsesCustomToolOutputItem), "custom_call_output")]
[JsonDerivedType(typeof(ResponsesReferenceItem), "item_reference")]
internal abstract class ResponsesItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

internal class ResponsesMessageItem : ResponsesItem
{
    [JsonPropertyName("content")]
    public required ICollection<ResponsesMessageContent> Content { get; set; }

    [JsonPropertyName("role")]
    public required ResponsesMessageRole Role { get; set; }

    [JsonPropertyName("status")]
    public ResponsesItemStatus? Status { get; set; }
}

internal class ResponsesReasoningItem : ResponsesItem
{
    /// <summary>
    /// type "summary" only
    /// </summary>
    [JsonPropertyName("summary")]
    public required ICollection<ResponsesReasoningContent> Summary { get; set; }

    /// <summary>
    /// type "text" only
    /// </summary>
    [JsonPropertyName("content")]
    public ICollection<ResponsesReasoningContent>? Content { get; set; }

    /// <summary>
    /// store: false일 경우, encrypted_content를 포함시켜야 합니다.
    /// 요청에 include: "reasoning.encrypted_content"가 포함되어 있어야 응답을 받을 수 있습니다.
    /// </summary>
    [JsonPropertyName("encrypted_content")]
    public string? EncryptedContent { get; set; }

    [JsonPropertyName("status")]
    public ResponsesItemStatus? Status { get; set; }
}

internal class ResponsesFunctionToolCallItem : ResponsesItem
{
    [JsonPropertyName("arguments")]
    public required string Arguments { get; set; }

    [JsonPropertyName("call_id")]
    public required string CallId { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("status")]
    public ResponsesItemStatus? Status { get; set; }
}

internal class ResponsesFunctionToolOutputItem : ResponsesItem
{
    [JsonPropertyName("call_id")]
    public required string CallId { get; set; }

    [JsonPropertyName("output")]
    public required string Output { get; set; }

    [JsonPropertyName("status")]
    public ResponsesItemStatus? Status { get; set; }
}

internal class ResponsesCustomToolCallItem : ResponsesItem
{
    [JsonPropertyName("call_id")]
    public required string CallId { get; set; }

    [JsonPropertyName("input")]
    public required string Input { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }
}

internal class ResponsesCustomToolOutputItem : ResponsesItem
{
    [JsonPropertyName("call_id")]
    public required string CallId { get; set; }

    [JsonPropertyName("output")]
    public required string Output { get; set; }
}

internal class ResponsesReferenceItem : ResponsesItem
{ }
