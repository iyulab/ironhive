using Raggle.Driver.Anthropic.Base;

namespace Raggle.Driver.Anthropic.ChatCompletion;

internal class AnthropicChatCompletionModel : AnthropicModel
{
    internal bool IsSupportImage
    {
        get
        {
            return ModelId.Contains("claude-3");
        }
    }

    internal bool IsSupportTool
    {
        get
        {
            return ModelId.Contains("opus") || ModelId.Contains("sonnet");
        }
    }

    internal bool IsLegacy
    {
        get
        {
            return ModelId.Contains("claude-2")
                || ModelId.Contains("claude-instant-1.2");
        }
    }
}
