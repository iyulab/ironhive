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
    private readonly IRaggle _raggle;

    public ModelController(IRaggle raggle, IServiceProvider provider)
    {
        _raggle = raggle;
    }

    [HttpGet("chat")]
    public async Task<ActionResult> GetChatCompletionModelsAsync()
    {
        var models = new Dictionary<string, string[]>();
        var services = _raggle.Services.GetKeyedServices<IChatCompletionService>(KeyedService.AnyKey);

        var serviceKeyMap = new Dictionary<Type, string>
        {
            { typeof(OpenAIChatCompletionService), RaggleServiceKeys.OpenAI },
            { typeof(AnthropicChatCompletionService), RaggleServiceKeys.Anthrophic },
            { typeof(OllamaChatCompletionService), RaggleServiceKeys.Ollama }
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
        var services = _raggle.Services.GetKeyedServices<IEmbeddingService>(KeyedService.AnyKey);

        var serviceKeyMap = new Dictionary<Type, string>
        {
            { typeof(OpenAIEmbeddingService), RaggleServiceKeys.OpenAI },
            { typeof(OllamaEmbeddingService), RaggleServiceKeys.Ollama }
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
