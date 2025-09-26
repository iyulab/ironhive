using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Messages.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextDeltaMessageContent), "text_delta")]
[JsonDerivedType(typeof(ToolUseDeltaMessageContent), "input_json_delta")]
[JsonDerivedType(typeof(ThinkingDeltaMessageContent), "thinking_delta")]
[JsonDerivedType(typeof(SignatureDeltaMessageContent), "signature_delta")]
internal abstract class MessageDeltaContent
{ }

internal class TextDeltaMessageContent : MessageDeltaContent
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

internal class ToolUseDeltaMessageContent : MessageDeltaContent
{
    [JsonPropertyName("partial_json")]
    public required string PartialJson { get; set; }
}

internal class ThinkingDeltaMessageContent : MessageDeltaContent
{
    [JsonPropertyName("thinking")]
    public required string Thinking { get; set; }
}

internal class SignatureDeltaMessageContent : MessageDeltaContent
{
    [JsonPropertyName("signature")]
    public required string Signature { get; set; }
}
