using Raggle.Abstractions.Engines;
using Raggle.Engines.OpenAI.ChatCompletion;
using Raggle.Engines.OpenAI.Configurations;
using ChatCompletionResponse = Raggle.Abstractions.Engines.ChatCompletionResponse;
using StreamingChatCompletionResponse = Raggle.Abstractions.Engines.StreamingChatCompletionResponse;

namespace Raggle.Engines.OpenAI;

public class OpenAIChatCompletionEngine : IChatCompletionEngine
{
    private readonly OpenAIChatCompletionClient _client;

    public OpenAIChatCompletionEngine(OpenAIConfig config)
    {
        _client = new OpenAIChatCompletionClient(config);
    }

    public OpenAIChatCompletionEngine(string apiKey)
    {
        _client = new OpenAIChatCompletionClient(apiKey);
    }

    public async Task<IEnumerable<ChatCompletionModel>> GetChatCompletionModelsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<ChatCompletionResponse>> ChatCompletionAsync()
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<StreamingChatCompletionResponse> StreamingChatCompletionAsync()
    {
        yield break;
        throw new NotImplementedException();
    }
}
