using Microsoft.AspNetCore.Mvc;
using Raggle.Abstractions.ChatCompletion;
using Raggle.Abstractions.Embedding;

namespace Raggle.Stack.WebApi.Controllers;

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
        var service = _services.GetRequiredService<IChatCompletionService>();
        var models = await service.GetModelsAsync();

        return Ok(models);
    }

    [HttpGet("embedding")]
    public async Task<ActionResult> GetEmbeddingModelsAsync()
    {
        var service = _services.GetRequiredService<IEmbeddingService>();
        var models = await service.GetModelsAsync();

        return Ok(models);
    }
}
