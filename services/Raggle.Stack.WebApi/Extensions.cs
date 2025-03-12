using Microsoft.Extensions.Azure;
using Raggle.Abstractions;
using Raggle.Abstractions.ChatCompletion;
using Raggle.Abstractions.Embedding;
using Raggle.Connectors.Anthropic;
using Raggle.Connectors.Anthropic.Configurations;
using Raggle.Connectors.OpenAI;
using Raggle.Connectors.OpenAI.Configurations;
using Raggle.Core;

namespace Raggle.Stack.WebApi;

public static class Extensions
{
    /// <summary>
    /// 임시 메서드
    /// </summary>
    public static void AddHiveMind(this WebApplicationBuilder builder)
    {
        builder.Services.AddHiveServices();

        var container = new HiveServiceContainer();
        var o_config = new OpenAIConfig
        {
            ApiKey = "",
        };
        container.RegisterKeyedService<IChatCompletionConnector>("openai", new OpenAIChatCompletionConnector(o_config));
        container.RegisterKeyedService<IEmbeddingConnector>("openai", new OpenAIEmbeddingConnector(o_config));

        var a_config = new AnthropicConfig
        {
            ApiKey = "",
        };
        container.RegisterKeyedService<IChatCompletionConnector>("anthropic", new AnthropicChatCompletionConnector(a_config));

        builder.Services.AddSingleton<IHiveServiceContainer>(container);
    }
}
