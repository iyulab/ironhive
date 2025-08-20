using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.StaticFiles;
using IronHive.Abstractions.Tools;
using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Message;

namespace WebServer.Controllers;

[ApiController]
[Route("/api")]
public class ServiceController : ControllerBase
{
    private readonly IModelCatalogService _model;
    private readonly IMessageGenerationService _service;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public ServiceController(
        IModelCatalogService model,
        IMessageGenerationService service,
        IOptions<JsonOptions> jsonOptions)
    {
        _model = model;
        _service = service;
        _jsonOptions = jsonOptions.Value.JsonSerializerOptions;
    }

    [RequestSizeLimit(10_737_418_240)] // 10GB
    [HttpPost("upload")]
    public async Task<ActionResult> UploadAsync(
        [FromForm] IFormFile[] files,
        CancellationToken cancellationToken)
    {
        var dic = @"C:\\temp";
        var fileInfos = new List<object>();
        foreach (var file in files) 
        {
            var filePath = Path.Combine(dic, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }
            fileInfos.Add(new
            {
                Name = file.FileName,
                Type = file.ContentType,
                Size = file.Length,
            });
        }
        return Ok(fileInfos);
    }

    [HttpGet("models")]
    public async Task<ActionResult> GetModelsAsync(
        [FromQuery(Name = "provider")] string? provider,
        CancellationToken cancellationToken)
    {
        var models = await _model.ListModelsAsync(provider, cancellationToken);
        return Ok(models);
    }

    [HttpPost("conversation")]
    public async Task ConversationAsync(
        [FromBody] MessageGenerationRequest request,
        CancellationToken cancellationToken)
    {
        request.System = $"Current UTC Time: {DateTime.UtcNow}\n" + request.System;
        request.Tools = _service.Tools.ToList();

        try
        {
            Response.Headers.Connection = "keep-alive";
            Response.Headers.CacheControl = "no-cache";
            Response.ContentType = "text/event-stream; charset=utf-8";
            
            await foreach (var result in _service.GenerateStreamingMessageAsync(request, cancellationToken))
            {
                await WriteEventAsync(result, "delta");
            }
        }
        catch(OperationCanceledException ex)
        {
            await WriteEventAsync(new StreamingMessageErrorResponse
            {
                Code = 499,
                Message = ex.Message,
            }, "delta");
        }
        catch(Exception ex)
        {
            await WriteEventAsync(new StreamingMessageErrorResponse
            {
                Code = 500,
                Message = ex.Message,
            }, "delta");
        }
        finally
        {
            await Response.CompleteAsync();
        }
    }

    private async Task WriteEventAsync(object data, string? type = null)
    {
        if (data is not string str)
            str = JsonSerializer.Serialize(data, _jsonOptions);

        var sb = new StringBuilder();
        if (type != null)
            sb.Append("event: ").Append(type).Append('\n');
        
        sb.Append("data: ").Append(str).Append('\n');
        sb.Append('\n'); // 빈 줄로 이벤트 종료
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());

        await Response.Body.WriteAsync(bytes);
        await Response.Body.FlushAsync();
    }
}
