using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Payloads.Messages;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EnabledThinkingConfig), "enabled")]
[JsonDerivedType(typeof(DisabledThinkingConfig), "disabled")]
internal abstract class ThinkingConfig
{ }

internal class EnabledThinkingConfig : ThinkingConfig
{
    [JsonPropertyName("budget_tokens")]
    public required int BudgetTokens { get; set; }
}

internal class DisabledThinkingConfig : ThinkingConfig
{ }
