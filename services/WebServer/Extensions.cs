using dotenv.net;
using IronHive.Connectors.Anthropic;
using IronHive.Connectors.OpenAI;
using System.Diagnostics;
using WebServer.Tools;

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
            ApiKey = Environment.GetEnvironmentVariable("OPENAI_KEY") ?? string.Empty,
        };
        var a_config = new AnthropicConfig
        {
            ApiKey = Environment.GetEnvironmentVariable("ANTHROPIC_KEY") ?? string.Empty,
        };
        var g_config = new OpenAIConfig
        {
            BaseUrl = "https://generativelanguage.googleapis.com/v1beta/openai/",
            ApiKey = Environment.GetEnvironmentVariable("GOOGLE_KEY") ?? string.Empty
        };
        var x_config = new AnthropicConfig
        {
            BaseUrl = "https://api.x.ai/v1/",
            ApiKey = Environment.GetEnvironmentVariable("XAI_KEY") ?? string.Empty
        };
        var l_config = new OpenAIConfig
        {
            BaseUrl = "http://labs.iyulab.com:10150/v1-openai/",
            ApiKey = Environment.GetEnvironmentVariable("GPUSTACK_KEY") ?? string.Empty
        };

        services.AddHiveServiceCore()
            .AddOpenAIConnectors("openai", o_config)
            .AddAnthropicConnectors("anthropic", a_config)
            .AddOpenAIConnectors("google", g_config)
            .AddAnthropicConnectors("xai", x_config)
            .AddOpenAIConnectors("gpustack", l_config)
            .AddFunctionTools<TestTool>("function");
    }
}
