using Raggle.Connector.OpenAI.Base;

namespace Raggle.Connector.OpenAI.ChatCompletion;

internal class OpenAIChatCompletionModel : OpenAIModel
{
    /// <summary>
    /// <see href="https://platform.openai.com/docs/guides/vision"/>
    /// </summary>
    internal bool IsSupportImage
    {
        get
        {
            return ID.Contains("gpt-4o") || ID.Contains("gpt-4-turbo");
        }
    }

    /// <summary>
    /// <see href="https://platform.openai.com/docs/guides/function-calling"/>
    /// </summary>
    internal bool IsSupportTool
    {
        get
        {
            return ID.Contains("gpt-4o");
        }
    }

    internal bool IsLegacy
    {
        get
        {
            return ID.Contains("babbage") || ID.Contains("davinci");
        }
    }
}
