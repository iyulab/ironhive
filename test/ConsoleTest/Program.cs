using IronHive.Abstractions;
using IronHive.Abstractions.Memory;
using IronHive.Connectors.Anthropic;
using IronHive.Connectors.OpenAI;
using IronHive.Core;
using IronHive.Core.Storages;
using IronHive.Storages.Qdrant;
using IronHive.Storages.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;

var hive = Create();
//var session = hive.

return;

IHiveMind Create()
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

    var services = new ServiceCollection();
    services.AddLogging();

    var mind = new HiveServiceBuilder(services)
        .AddDefaultFileStorages()
        .AddDefaultFileDecoders()
        .AddDefaultPipelineHandlers()
        .AddOpenAIConnectors("openai", o_config)
        .AddAnthropicConnectors("anthropic", a_config)
        .AddOpenAIConnectors("gemini", g_config)
        .AddOpenAIConnectors("iyulab", l_config)
        .BuildHiveMind();

    return mind;
}
