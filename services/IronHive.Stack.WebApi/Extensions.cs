using IronHive.Connectors.Anthropic;
using IronHive.Connectors.OpenAI;
using IronHive.Stack.WebApi.Tools;

namespace IronHive.Stack.WebApi;

public static class Extensions
{
    /// <summary>
    /// 임시 메서드
    /// </summary>
    public static void AddHiveSerivces(this IServiceCollection services)
    {
        var o_config = new OpenAIConfig
        {
            ApiKey = "",
        };
        var a_config = new AnthropicConfig
        {
            ApiKey = "",
        };
        var g_config = new OpenAIConfig
        {
            BaseUrl = "https://generativelanguage.googleapis.com/v1beta/openai/",
            ApiKey = ""
        };
        var l_config = new OpenAIConfig
        {
            BaseUrl = "http://172.30.1.53:8080/v1-openai/",
            ApiKey = ""
        };

        services.AddHiveServiceCore()
            .AddChatCompletionConnector("openai", new OpenAIChatCompletionConnector(o_config))
            .AddEmbeddingConnector("openai", new OpenAIEmbeddingConnector(o_config))
            .AddChatCompletionConnector("anthropic", new AnthropicChatCompletionConnector(a_config))
            .AddChatCompletionConnector("gemini", new OpenAIChatCompletionConnector(g_config))
            .AddChatCompletionConnector("iyulab", new OpenAIChatCompletionConnector(l_config))
            .AddToolHandler<TestTool>("test", ServiceLifetime.Scoped);
    }
}
