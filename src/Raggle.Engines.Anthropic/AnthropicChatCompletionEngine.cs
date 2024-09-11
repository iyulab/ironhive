using Raggle.Abstractions.Engines;
using Raggle.Engines.Anthropic.ChatCompletion;
using Raggle.Engines.Anthropic.Configurations;

namespace Raggle.Engines.Anthropic;

public class AnthropicChatCompletionEngine : IChatCompletionEngine
{
    private readonly AnthropicChatCompletionClient _client;

    public AnthropicChatCompletionEngine(AnthropicConfig config)
    {
        _client = new AnthropicChatCompletionClient(config);
    }

    public AnthropicChatCompletionEngine(string apiKey)
    {
        _client = new AnthropicChatCompletionClient(apiKey);
    }

    public Task<IEnumerable<ChatCompletionModel>> GetChatCompletionModelsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ChatCompletionResponse>> ChatCompletionAsync()
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<StreamingChatCompletionResponse> StreamingChatCompletionAsync()
    {
        throw new NotImplementedException();
    }
}
