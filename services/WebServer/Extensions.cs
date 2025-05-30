using dotenv.net;
using IronHive.Providers.Anthropic;
using IronHive.Providers.OpenAI;
using IronHive.Core;
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
            .AddOpenAIProviders("openai", o_config)
            .AddAnthropicProviders("anthropic", a_config)
            .AddOpenAIProviders("google", g_config)
            .AddAnthropicProviders("xai", x_config)
            .AddOpenAIProviders("gpustack", l_config)
            .AddFunctionTools<TestTool>("builtin");
    }
}
