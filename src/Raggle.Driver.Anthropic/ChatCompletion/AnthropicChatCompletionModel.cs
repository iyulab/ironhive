using Raggle.Driver.Anthropic.Base;

namespace Raggle.Driver.Anthropic.ChatCompletion;

internal class AnthropicChatCompletionModel : AnthropicModel
{
    internal bool IsSupportImage
    {
        get
        {
            return ID.Contains("claude-3");
        }
    }

    internal bool IsSupportTool
    {
        get
        {
            return ID.Contains("opus") || ID.Contains("sonnet");
        }
    }

    internal bool IsLegacy
    {
        get
        {
            return ID.Contains("claude-2")
                || ID.Contains("claude-instant-1.2");
        }
    }
}
