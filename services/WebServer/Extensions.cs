using IronHive.Connectors.Anthropic;
using IronHive.Connectors.OpenAI;

namespace WebServer;

public static class Extensions
{
    /// <summary>
    /// 임시 메서드
    /// </summary>
    public static void AddMainSerivces(this IServiceCollection services)
    {
        var o_config = new OpenAIConfig
        {
            ApiKey = "",
        };
        var a_config = new AnthropicConfig
        {
            ApiKey = "",
        };
        //var g_config = new OpenAIConfig
        //{
        //    BaseUrl = "https://generativelanguage.googleapis.com/v1beta/openai/",
        //    ApiKey = ""
        //};
        //var l_config = new OpenAIConfig
        //{
        //    BaseUrl = "http://172.30.1.53:8080/v1-openai/",
        //    ApiKey = ""
        //};

        services.AddHiveServiceCore()
            .AddOpenAIConnectors("openai", o_config)
            .AddAnthropicConnectors("anthropic", a_config);
    }
}
