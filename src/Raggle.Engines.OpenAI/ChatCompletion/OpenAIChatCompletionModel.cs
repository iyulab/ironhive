using Raggle.Engines.OpenAI.Abstractions;

namespace Raggle.Engines.OpenAI.ChatCompletion;

public class OpenAIChatCompletionModel : OpenAIModel
{
    /// <summary>
    /// <see href="https://platform.openai.com/docs/guides/vision"/>
    /// </summary>
    public bool IsSupportImage
    {
        get
        {
            return ID.Contains("gpt-4o") || ID.Contains("gpt-4-turbo");
        }
    }

    /// <summary>
    /// <see href="https://platform.openai.com/docs/guides/function-calling"/>
    /// </summary>
    public bool IsSupportTool
    {
        get
        {
            return ID.Contains("gpt-4o");
        }
    }

    public bool IsLegacy
    {
        get
        {
            return ID.Contains("babbage") || ID.Contains("davinci");
        }
    }
}
