using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Models;
using Microsoft.AspNetCore.StaticFiles;
using IronHive.Core.Tools;
using IronHive.Core.Mcp;
using WebServer.Tools;

namespace WebServer.Controllers;

[ApiController]
[Route("/api")]
public class ServiceController : ControllerBase
{
    private readonly IModelService _model;
    private readonly IChatCompletionService _chat;
    private readonly JsonSerializerOptions _jsonOptions;

    public ServiceController(
        IModelService model,
        IChatCompletionService chat,
        IOptions<JsonOptions> jsonOptions)
    {
        _model = model;
        _chat = chat;
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

    [HttpGet("download/{fileName}")]
    public async Task<ActionResult> DownloadAsync(
        [FromRoute] string fileName,
        CancellationToken cancellationToken)
    {
        var dic = @"C:\\temp";
        var filePath = Path.Combine(dic, fileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound($"파일을 찾을 수 없습니다: {fileName}");

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(fileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(stream, contentType, fileName);
    }

    [HttpGet("models")]
    public async Task<ActionResult> GetModelsAsync(
        CancellationToken cancellationToken)
    {
        var models = await _model.ListModelsAsync(cancellationToken);
        return Ok(models);
    }

    [HttpPost("conversation")]
    public async Task ConversationAsync(
        [FromBody] ConversationRequest request,
        CancellationToken cancellationToken)
    {
        request.Instructions = $"Current UTC Time: {DateTime.UtcNow}\n" + request.Instructions;
        request.Tools = FunctionToolFactory.CreateFromObject<TestTool>();

        try
        {
            Response.ContentType = "text/event-stream; charset=utf-8";
            Response.Headers.CacheControl = "no-cache";
            Response.Headers.Connection = "keep-alive";

            await WriteEventAsync("[Begin]");
            await foreach (var result in _chat.GenerateStreamingMessageAsync(request.Messages, request, cancellationToken))
            {
                await WriteEventAsync(result, "delta");
            }
            await WriteEventAsync("[DONE]");
        }
        catch(OperationCanceledException ex)
        {
            await WriteEventAsync(new
            {
                Source = ex.Source,
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                Data = ex.Data,
                HelpLink = ex.HelpLink,
            }, "cancelled");
        }
        catch(Exception ex)
        {
            await WriteEventAsync(new
            {
                Source = ex.Source,
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                Data = ex.Data,
                HelpLink = ex.HelpLink,
            }, "error");
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

public class ConversationRequest : ChatCompletionOptions
{
    public MessageCollection Messages { get; set; } = new();
}
