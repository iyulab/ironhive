using System.Text.Json.Serialization;

namespace IronHive.Connectors.Anthropic.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextDeltaMessageContent), "text_delta")]
[JsonDerivedType(typeof(ToolUseDeltaMessageContent), "input_json_delta")]
[JsonDerivedType(typeof(ThinkingDeltaMessageContent), "thinking_delta")]
[JsonDerivedType(typeof(SignatureDeltaMessageContent), "signature_delta")]
internal interface IMessageDeltaContent
{ }

internal class TextDeltaMessageContent : IMessageDeltaContent
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

internal class ToolUseDeltaMessageContent : IMessageDeltaContent
{
    [JsonPropertyName("partial_json")]
    public required string PartialJson { get; set; }
}

internal class ThinkingDeltaMessageContent : IMessageDeltaContent
{
    [JsonPropertyName("thinking")]
    public required string Thinking { get; set; }
}

internal class SignatureDeltaMessageContent : IMessageDeltaContent
{
    [JsonPropertyName("signature")]
    public required string Signature { get; set; }
}
