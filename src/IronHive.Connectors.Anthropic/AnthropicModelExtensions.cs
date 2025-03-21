using IronHive.Connectors.Anthropic.Base;

namespace IronHive.Connectors.Anthropic;

internal static class AnthropicModelExtensions
{
    internal static bool IsChatCompletion(this AnthropicModel model)
    {
        return model.Id.Contains("claude-3");
    }
}
