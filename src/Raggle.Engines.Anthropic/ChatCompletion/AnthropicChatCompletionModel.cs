using Raggle.Engines.Anthropic.Abstractions;

namespace Raggle.Engines.Anthropic.ChatCompletion;

public class AnthropicChatCompletionModel : AnthropicModel
{
    public bool IsSupportImage
    {
        get
        {
            return ModelId.Contains("claude-3");
        }
    }

    public bool IsSupportTool
    {
        get
        {
            return ModelId.Contains("opus") || ModelId.Contains("sonnet");
        }
    }

    public bool IsLegacy
    {
        get
        {
            return ModelId.Contains("claude-2")
                || ModelId.Contains("claude-instant-1.2");
        }
    }
}
