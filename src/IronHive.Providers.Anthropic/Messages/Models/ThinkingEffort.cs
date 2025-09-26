using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Messages.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EnabledThinkingEffort), "enabled")]
[JsonDerivedType(typeof(DisabledThinkingEffort), "disabled")]
internal abstract class ThinkingEffort
{ }

internal class EnabledThinkingEffort : ThinkingEffort
{
    [JsonPropertyName("budget_tokens")]
    public required int BudgetTokens { get; set; }
}

internal class DisabledThinkingEffort : ThinkingEffort
{ }
