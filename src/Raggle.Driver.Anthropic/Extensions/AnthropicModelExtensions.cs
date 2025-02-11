using Raggle.Driver.Anthropic.Base;

namespace Raggle.Driver.Anthropic.Extensions;

internal static class AnthropicModelExtensions
{
    internal static bool IsChatCompletion(this AnthropicModel model)
    {
        return model.ID.Contains("claude-3-5");
    }
}
