using Microsoft.AspNetCore.Mvc;
using Raggle.Server.WebApi.Models;
using Raggle.Server.WebApi.Services;

namespace Raggle.Server.WebApi.Controllers;

public class SearchQuery
{
    public required string Query { get; set; }
}

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

    #region Collection

    [HttpGet]
    public async Task<ActionResult> FindCollectionsAsync(
        [FromQuery(Name = "name")] string? name = null,
        [FromQuery(Name = "limit")] int limit = 10,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "order")] string order = "desc")
    {
        try
        {
            var collections = await _service.FindCollectionsAsync(name, limit, skip, order);
            return Ok(collections);
        }
        catch(Exception ex)
        {
            return StatusCode(500, ex);
        }
    }

    [HttpPost]
    public async Task<ActionResult> UpsertCollectionAsync(
        [FromBody] CollectionModel collection)
    {
        try
        {
            var result = await _service.UpsertCollectionAsync(collection);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex);
        }
    }

    [HttpDelete("{collectionId}")]
    public async Task<ActionResult> DeleteCollectionAsync(
        [FromRoute] string collectionId)
    {
        try
        {
            await _service.DeleteCollectionAsync(collectionId);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex);
        }
    }

    [HttpPost("{collectionId}/search")]
    public async Task<ActionResult> SearchDocumentAsync(
        [FromRoute] string collectionId,
        [FromBody] SearchQuery request)
    {
        try
        {
            var result = await _service.SearchDocumentAsync(collectionId, request.Query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex);
        }
    }

    #endregion

    #region Document

    [HttpGet("{collectionId}/documents")]
    public async Task<ActionResult> FindDocumentsAsync(
        [FromRoute] string collectionId,
        [FromQuery(Name = "name")] string? name = null,
        [FromQuery(Name = "limit")] int limit = 10,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "order")] string order = "desc")
    {
        try
        {
            var documents = await _service.FindDocumentsAsync(collectionId, name, limit, skip, order);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex);
        }
    }

    [RequestSizeLimit(10_737_418_240)]
    [HttpPost("{collectionId}/documents")]
    public async Task<ActionResult> UploadDocumentAsync(
        [FromRoute] string collectionId,
        IFormFile file)
    {
        try
        {
            var document = new DocumentModel
            {
                CollectionId = collectionId,
                FileName = file.FileName,
                FileSize = file.Length,
                ContentType = file.ContentType,
                Tags = []
            };
            using (var data = file.OpenReadStream())
            {
                await _service.UploadDocumentAsync(collectionId, document, data);
            }
            return Ok(document);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex);
        }
    }

    [HttpDelete("{collectionId}/documents/{documentId}")]
    public async Task<ActionResult> DeleteDocumentAsync(
        [FromRoute] string collectionId,
        [FromRoute] string documentId)
    {
        try
        {
            await _service.DeleteDocumentAsync(collectionId, documentId);
            return Ok($"deleted");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex);
        }
    }

    #endregion
}
