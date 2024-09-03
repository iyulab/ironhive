using Raggle.Engines.OpenAI.Configurations;

namespace Raggle.Engines.OpenAI.ChatCompletion;

public class OpenAIChatClient : OpenAIClientBase
{
    public OpenAIChatClient(string apiKey) : base(apiKey) { }

    public OpenAIChatClient(OpenAIConfig config) : base(config) { }

    public async Task<IEnumerable<OpenAIModel>> GetChatModelsAsync() =>
        await GetModelsAsync([OpenAIModelType.GPT]);

}
