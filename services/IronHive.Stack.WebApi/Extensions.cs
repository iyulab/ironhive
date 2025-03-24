using IronHive.Connectors.Anthropic;
using IronHive.Connectors.OpenAI;
using IronHive.Core.ChatCompletion;
using IronHive.Core.Embedding;
using IronHive.Stack.WebApi.Tools;

namespace IronHive.Stack.WebApi;

public static class Extensions
{
    /// <summary>
    /// 임시 메서드
    /// </summary>
    public static void AddHiveSerivces(this IServiceCollection services)
    {
        var cb = new ChatCompletionServiceBuilder();
        var eb = new EmbeddingServiceBuilder();
        var o_config = new OpenAIConfig
        {
            ApiKey = "",
        };
        cb.AddConnector("openai", new OpenAIChatCompletionConnector(o_config));
        eb.AddConnector("openai", new OpenAIEmbeddingConnector(o_config));

        var a_config = new AnthropicConfig
        {
            ApiKey = "",
        };
        cb.AddConnector("anthropic", new AnthropicChatCompletionConnector(a_config));

        var g_config = new OpenAIConfig
        {
            BaseUrl = "https://generativelanguage.googleapis.com/v1beta/openai/",
            ApiKey = ""
        };
        cb.AddConnector("gemini", new OpenAIChatCompletionConnector(g_config));

        var l_config = new OpenAIConfig
        {
            BaseUrl = "http://172.30.1.53:8080/v1-openai/",
            ApiKey = ""
        };
        cb.AddConnector("iyulab", new OpenAIChatCompletionConnector(l_config));

        #region ============= TestTool =============
        cb.AddTool("test", new TestTool());
        #endregion

        var cs = cb.Build();
        var es = eb.Build();

        services.AddSingleton(cs);
        services.AddSingleton(es);
    }
}
