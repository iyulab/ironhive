using Microsoft.AspNetCore.Mvc;
using Raggle.Server.API.Models;
using Raggle.Server.Web.Repositories;
using System.Text.Json;

namespace Raggle.Server.Web.Controllers;

[ApiController]
[Route("/source")]
public class SourceController : ControllerBase
{
    private readonly SourceRepository _source;

    public SourceController(SourceRepository sourceRepository)
    {
        _source = sourceRepository;
    }

    [HttpGet("/sources/{userId:guid}")]
    public async Task<IActionResult> GetSourcesAsync(Guid userId)
    {
        var sources = await _source.GetAllAsync(userId);
        return Ok(sources);
    }

    [HttpGet("{sourceId:guid}")]
    public async Task<IActionResult> GetSourceAsync(Guid sourceId)
    {
        var source = await _source.GetAsync(sourceId);
        return Ok(source);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSourceAsync([FromBody] DataSource source)
    {
        var newSource = await _source.InsertAsync(source);
        return Ok(newSource);
    }

    [HttpPatch("{sourceId:guid}")]
    public async Task<IActionResult> UpdateSourceAsync(Guid sourceId, [FromBody] JsonElement updates)
    {
        var updateSource = await _source.UpdateAsync(sourceId, updates);
        return Ok(updateSource);
    }

    [HttpDelete("{sourceId:guid}")]
    public async Task<IActionResult> DeleteSourceAsync(Guid sourceId)
    {
        await _source.DeleteAsync(sourceId);
        return Ok();
    }
}
