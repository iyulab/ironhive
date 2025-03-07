using Raggle.Abstractions;
using Raggle.Connectors.OpenAI;
using Raggle.Connectors.OpenAI.Configurations;
using Raggle.Core;

namespace Raggle.Stack.WebApi;

public static class Extensions
{
    /// <summary>
    /// 임시 메서드
    /// </summary>
    public static void AddHiveMind(
        this WebApplicationBuilder builder)
    {
        var h_builder = new HiveMindBuilder(builder.Services);
        var o_config = new OpenAIConfig
        {
            ApiKey = "",
        };
        h_builder.AddChatCompletionConnector("openai", new OpenAIChatCompletionConnector(o_config));
        h_builder.AddEmbeddingConnector("openai", new OpenAIEmbeddingConnector(o_config));
        var hive = h_builder.Build();

        builder.Services.AddSingleton<IHiveMind>(hive);
    }
}
