using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ResponsesMessageItem), "message")]
[JsonDerivedType(typeof(ResponsesReasoningItem), "reasoning")]
[JsonDerivedType(typeof(ResponsesFunctionToolCallItem), "function_call")]
[JsonDerivedType(typeof(ResponsesFunctionToolOutputItem), "function_call_output")]
[JsonDerivedType(typeof(ResponsesCustomToolCallItem), "custom_call")]
[JsonDerivedType(typeof(ResponsesCustomToolOutputItem), "custom_call_output")]
[JsonDerivedType(typeof(ResponsesReferenceItem), "item_reference")]
internal abstract class ResponsesItem
{ }

/// <summary>
/// role이 "assistant"인 경우 모든 prop은 필수입니다.
/// </summary>
internal class ResponsesMessageItem : ResponsesItem
{
    [JsonPropertyName("content")]
    public required ICollection<ResponsesMessageContent> Content { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("role")]
    public required ResponsesMessageRole Role { get; set; }

    [JsonPropertyName("status")]
    public ResponsesItemStatus? Status { get; set; }
}

internal class ResponsesReasoningItem : ResponsesItem
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

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

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("status")]
    public ResponsesItemStatus? Status { get; set; }
}

internal class ResponsesFunctionToolOutputItem : ResponsesItem
{
    [JsonPropertyName("call_id")]
    public required string CallId { get; set; }

    [JsonPropertyName("output")]
    public required string Output { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

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

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

internal class ResponsesCustomToolOutputItem : ResponsesItem
{
    [JsonPropertyName("call_id")]
    public required string CallId { get; set; }

    [JsonPropertyName("output")]
    public required string Output { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

internal class ResponsesReferenceItem : ResponsesItem
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
}