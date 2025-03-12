using Raggle.Connectors.Anthropic.Base;

namespace Raggle.Connectors.Anthropic.Extensions;

internal static class AnthropicModelExtensions
{
    internal static bool IsChatCompletion(this AnthropicModel model)
    {
        return model.ID.Contains("claude-3");
    }
}
