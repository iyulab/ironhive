using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Raggle.Abstractions;
using Raggle.Abstractions.ChatCompletion;
using Raggle.Abstractions.ChatCompletion.Messages;
using Raggle.Abstractions.Embedding;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Raggle.Stack.WebApi.Controllers;

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
            var context = new MessageContext(request.Messages);

            if (request.Stream)
            {
                Response.ContentType = "application/stream+json";
                await foreach (var result in _chat.InvokeStreamingAsync(context, request, cancellationToken))
                {
                    var json = JsonSerializer.Serialize(result, _jsonOptions);
                    var data = Encoding.UTF8.GetBytes(json + "\n");

                    await Response.Body.WriteAsync(data, cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
            }
            else
            {
                var result = await _chat.InvokeAsync(context, request, cancellationToken);
                await Response.WriteAsJsonAsync(result, _jsonOptions, cancellationToken);
            }
        }
        catch(Exception ex)
        {
            Debug.Write(ex.Message);
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
            var result = await _embedding.EmbedBatchAsync(request, cancellationToken);
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
