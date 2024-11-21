using Microsoft.AspNetCore.Mvc;
using Raggle.Abstractions;
using Raggle.Abstractions.AI;
using Raggle.Server.WebApi.Models;

namespace Raggle.Server.WebApi.Controllers;

[ApiController]
[Route("/v1/models")]
public class ModelController : ControllerBase
{
    private readonly IRaggle _raggle;
    private readonly RaggleServiceKeys[] modelServiceKeys =
    [
        RaggleServiceKeys.OpenAI,
        RaggleServiceKeys.Anthropic,
        RaggleServiceKeys.Ollama
    ];

    public ModelController(IRaggle raggle)
    {
        _raggle = raggle;
    }

    [HttpGet("chat")]
    public async Task<ActionResult> GetChatCompletionModelsAsync()
    {
        var models = new Dictionary<string, string[]>();

        foreach (var key in modelServiceKeys)
        {
            var service = _raggle.Services.GetKeyedService<IChatCompletionService>(key);
            if (service != null)
            {
                var chatModels = await service.GetChatCompletionModelsAsync();
                models[key.ToString()] = chatModels.Select(m => m.Model).ToArray();
            }
        }

        return Ok(models);
    }

    [HttpGet("embedding")]
    public async Task<ActionResult> GetEmbeddingModelsAsync()
    {
        var models = new Dictionary<string, string[]>();

        foreach (var key in modelServiceKeys)
        {
            var service = _raggle.Services.GetKeyedService<IEmbeddingService>(key);
            if (service != null)
            {
                var embeddingModels = await service.GetEmbeddingModelsAsync();
                models[key.ToString()] = embeddingModels.Select(m => m.Model).ToArray();
            }
        }

        return Ok(models);
    }
}
