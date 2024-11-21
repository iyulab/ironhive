using Microsoft.AspNetCore.Mvc;
using Raggle.Server.WebApi.Models;
using Raggle.Server.WebApi.Services;

namespace Raggle.Server.WebApi.Controllers;

[ApiController]
[Route("/v1/memory")]
public class MemoryController : ControllerBase
{
    private readonly ILogger<MemoryController> _logger;
    private readonly MemoryService _service;

    public MemoryController(
        ILogger<MemoryController> logger,
        MemoryService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost("{collectionId:guid}/upload")]
    public async Task<ActionResult> UploadedDocumentAsync(
        [FromRoute] Guid collectionId,
        [FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No file uploaded.");

        var file = files.FirstOrDefault();
        var filePath = $@"C:\temp\{file.FileName}";

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok(new { message = "File uploaded successfully." });
    }

    [HttpGet]
    public async Task<ActionResult> FindCollectionsAsync(
        [FromQuery(Name = "name")] string? name = null,
        [FromQuery(Name = "limit")] int limit = 10,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "order")] string order = "desc")
    {
        var collections = await _service.FindCollectionsAsync(name, limit, skip, order);
        return Ok(collections);
    }

    [HttpGet("{collectionId:guid}/documents")]
    public async Task<ActionResult> FindDocumentsAsync(
        [FromRoute] Guid collectionId,
        [FromQuery(Name = "name")] string? name = null,
        [FromQuery(Name = "limit")] int limit = 10,
        [FromQuery(Name = "order")] string order = "desc")
    {
        await Task.CompletedTask;
        return Ok();
    }

    [HttpGet("{collectionId:guid}/documents/{documentId:guid}")]
    public async Task<ActionResult> FindDocumentFilesAsync(
        [FromRoute] Guid collectionId,
        [FromRoute] Guid documentId)
    {
        await Task.CompletedTask;
        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult> UpsertCollectionAsync(
        [FromBody] CollectionModel collection)
    {
        var result = await _service.UpsertCollectionAsync(collection);
        return Ok(result);
    }

    [HttpPost("{collectionId:guid}/documents")]
    public async Task<ActionResult> UploadDocumentAsync(
        [FromRoute] Guid collectionId,
        [FromForm] FormFile file)
    {
        await Task.CompletedTask;
        return Ok();
    }

    [HttpDelete("{collectionId:guid}")]
    public async Task<ActionResult> DeleteCollectionAsync(
        [FromRoute] Guid collectionId)
    {
        await _service.DeleteCollectionAsync(collectionId);
        return Ok();
    }

    [HttpDelete("{collectionId:guid}/documents/{documentId:guid}")]
    public async Task<ActionResult> DeleteDocumentAsync(
        [FromRoute] Guid collectionId,
        [FromRoute] Guid documentId)
    {
        await Task.CompletedTask;
        return Ok();
    }
}
