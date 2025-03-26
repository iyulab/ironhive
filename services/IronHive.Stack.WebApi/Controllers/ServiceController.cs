using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.ComponentModel;
using Tavily;
using System.Threading.Tasks;
using IronHive.Stack.WebApi.Tools;
using IronHive.Abstractions.Messages;

namespace IronHive.Stack.WebApi.Controllers;

[ApiController]
[Route("/v1")]
public class ServiceController : ControllerBase
{
    private readonly IChatCompletionService _chat;
    private readonly IEmbeddingService _embedding;
    private readonly JsonSerializerOptions _jsonOptions;

    public ServiceController(
        IChatCompletionService chat,
        IEmbeddingService embedding,
        IOptions<JsonOptions> jsonOptions)
    {
        _chat = chat;
        _embedding = embedding;
        _jsonOptions = jsonOptions.Value.JsonSerializerOptions;
    }

    [HttpGet("chat/models")]
    public async Task<ActionResult> GetChatCompletionModelsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var models = await _chat.GetModelsAsync(cancellationToken);
            return Ok(models);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("chat/completion")]
    public async Task ChatCompletionAsync(
        [FromBody] ChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.Stream)
            {
                Response.ContentType = "application/stream+json";
                await foreach (var result in _chat.ExecuteStreamingAsync(request.Messages, request, cancellationToken))
                {
                    var json = JsonSerializer.Serialize(result, _jsonOptions);
                    var data = Encoding.UTF8.GetBytes(json + "\n");

                    await Response.Body.WriteAsync(data, cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
            }
            else
            {
                var result = await _chat.ExecuteAsync(request.Messages, request, cancellationToken);
                await Response.WriteAsJsonAsync(result, _jsonOptions, cancellationToken);
            }
        }
        catch(OperationCanceledException)
        {
            // When Canceled
            Debug.WriteLine("Canceled");
        }
        catch(Exception ex)
        {
            throw;
        }
        finally
        {
            await Response.CompleteAsync();
        }
    }

    [HttpGet("embedding/models")]
    public async Task<ActionResult> GetEmbeddingModelsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var models = await _embedding.GetModelsAsync(cancellationToken);
            return Ok(models);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("embedding/batch")]
    public async Task<ActionResult> EmbeddingBatchAsync(
        [FromBody] EmbeddingsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _embedding.EmbedBatchAsync(
                request.Provider,
                request.Model, 
                request.Input, 
                cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}

public class ChatCompletionRequest : ChatCompletionOptions
{
    public MessageCollection Messages { get; set; } = new();

    public bool Stream { get; set; } = false;
}

public class EmbeddingsRequest
{
    public required string Provider { get; set; }

    public required string Model { get; set; }

    public required IEnumerable<string> Input { get; set; }
}

public class ChatCompletionStreamResponse
{
    public enum Status
    {
        Begin,
        Continue,
        End
    }

    public Status State { get; set; }
    public IMessageContent? Content { get; set; }
    public int? TotalTokens { get; set; }
    public string? SystemMessage { get; set; }
}
