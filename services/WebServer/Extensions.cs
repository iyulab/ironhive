using dotenv.net;
using IronHive.Connectors.Anthropic;
using IronHive.Connectors.OpenAI;
using System.Diagnostics;

namespace WebServer;

public static class Extensions
{
    /// <summary>
    /// 임시 메서드
    /// </summary>
    public static void AddMainSerivces(this IServiceCollection services)
    {
        DotEnv.Load(options: new DotEnvOptions(envFilePaths: [".env"], trimValues: true));
        var env =  DotEnv.Read();

        var o_config = new OpenAIConfig
        {
            ApiKey = env.TryGetValue("OPENAI", out var oValue) ? oValue : string.Empty,
        };
        var a_config = new AnthropicConfig
        {
            ApiKey = env.TryGetValue("ANTHROPIC", out var aValue) ? aValue : string.Empty,
        };
        var g_config = new OpenAIConfig
        {
            BaseUrl = "https://generativelanguage.googleapis.com/v1beta/openai/",
            ApiKey = env.TryGetValue("GOOGLE", out var gValue) ? gValue : string.Empty,
        };
        var x_config = new AnthropicConfig
        {
            BaseUrl = "https://api.x.ai/v1/",
            ApiKey = env.TryGetValue("XAI", out var xValue) ? xValue : string.Empty,
        };
        var l_config = new OpenAIConfig
        {
            BaseUrl = "http://172.30.1.53:8080/v1-openai/",
            ApiKey = env.TryGetValue("LOCAL", out var lValue) ? lValue : string.Empty,
        };

        services.AddHiveServiceCore()
            .AddOpenAIConnectors("openai", o_config)
            .AddAnthropicConnectors("anthropic", a_config)
            .AddOpenAIConnectors("google", g_config)
            .AddAnthropicConnectors("xai", x_config)
            .AddOpenAIConnectors("local", l_config);
    }
}
