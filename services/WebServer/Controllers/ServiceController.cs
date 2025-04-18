using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Models;

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
        try
        {
            Response.ContentType = "text/event-stream; charset=utf-8";
            Response.Headers.CacheControl = "no-cache";
            Response.Headers.Connection = "keep-alive";

            await foreach (var result in _chat.GenerateStreamingMessageAsync(request.Messages, request, cancellationToken))
            {
                var json = JsonSerializer.Serialize(result, _jsonOptions);

                var type = $"event: delta\n";
                var data = $"data: {json}\n\n";
                var bytes = Encoding.UTF8.GetBytes(type + data);

                await Response.Body.WriteAsync(bytes, cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }

            var done = Encoding.UTF8.GetBytes($"data: [DONE]\n\n");
            await Response.Body.WriteAsync(done, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
        catch(OperationCanceledException)
        {
            // When Canceled
            Debug.WriteLine("Canceled");

            Response.StatusCode = 499; // Client Closed Request
            var problem = new ProblemDetails
            {
                Title = "Client Closed Request",
                Status = 499,
                Detail = "Client closed request.",
                Instance = HttpContext.Request.Path
            };

            await Response.WriteAsJsonAsync(problem, _jsonOptions, cancellationToken);
        }
        catch(Exception ex)
        {
            // When Error
            Debug.WriteLine(ex.ToString());

            Response.StatusCode = 500;
            var problem = new ProblemDetails
            {
                Title = "Internal Server Error",
                Status = 500,
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            };

            await Response.WriteAsJsonAsync(problem, _jsonOptions, cancellationToken);
        }
        finally
        {
            await Response.CompleteAsync();
        }
    }
}

public class ConversationRequest : ChatCompletionOptions
{
    public MessageCollection Messages { get; set; } = new();
}
