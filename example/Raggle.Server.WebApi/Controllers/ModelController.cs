using Microsoft.AspNetCore.Mvc;
using Raggle.Abstractions;
using Raggle.Abstractions.AI;
using Raggle.Driver.Anthropic;
using Raggle.Driver.Ollama;
using Raggle.Driver.OpenAI;
using Raggle.Server.Configurations;

namespace Raggle.Server.WebApi.Controllers;

[ApiController]
[Route("/v1/models")]
public class ModelController : ControllerBase
{
    private readonly IServiceProvider _services;

    public ModelController(IServiceProvider provider)
    {
        _services = provider;
    }

    [HttpGet("chat")]
    public async Task<ActionResult> GetChatCompletionModelsAsync()
    {
        var models = new Dictionary<string, string[]>();
        var services = _services.GetKeyedServices<IChatCompletionService>(KeyedService.AnyKey);

        var serviceKeyMap = new Dictionary<Type, string>
        {
            { typeof(OpenAIChatCompletionService), DefaultServiceKeys.OpenAI },
            { typeof(AnthropicChatCompletionService), DefaultServiceKeys.Anthrophic },
            { typeof(OllamaChatCompletionService), DefaultServiceKeys.Ollama }
        };

        foreach (var service in services)
        {
            if (serviceKeyMap.TryGetValue(service.GetType(), out var key))
            {
                try
                {
                    var _models = await service.GetModelsAsync();
                    models[key] = _models.Select(m => m.Model).ToArray();
                }
                catch
                {
                    models[key] = [];
                }
            }
        }

        return Ok(models);
    }

    [HttpGet("embedding")]
    public async Task<ActionResult> GetEmbeddingModelsAsync()
    {
        var models = new Dictionary<string, string[]>();
        var services = _services.GetKeyedServices<IEmbeddingService>(KeyedService.AnyKey);

        var serviceKeyMap = new Dictionary<Type, string>
        {
            { typeof(OpenAIEmbeddingService), DefaultServiceKeys.OpenAI },
            { typeof(OllamaEmbeddingService), DefaultServiceKeys.Ollama }
        };

        foreach (var service in services)
        {
            if (serviceKeyMap.TryGetValue(service.GetType(), out var key))
            {
                try
                {
                    var _models = await service.GetModelsAsync();
                    models[key] = _models.Select(m => m.Model).ToArray();
                }
                catch
                {
                    models[key] = [];
                }
            }
        }

        return Ok(models);
    }
}
