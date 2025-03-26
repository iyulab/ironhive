using IronHive.Abstractions.ChatCompletion;
using IronHive.Connectors.Ollama.Base;
using IronHive.Connectors.Ollama.ChatCompletion;
using System.Runtime.CompilerServices;
using Message = IronHive.Abstractions.Messages.Message;
using OllamaMessage = IronHive.Connectors.Ollama.ChatCompletion.Message;
using MessageRole = IronHive.Abstractions.Messages.MessageRole;
using OllamaMessageRole = IronHive.Connectors.Ollama.ChatCompletion.Message;
using System.Text.Json;
using IronHive.Abstractions.Messages;

namespace IronHive.Connectors.Ollama;

public class OllamaChatCompletionConnector : IChatCompletionConnector
{
    private readonly OllamaChatCompletionClient _client;

    public OllamaChatCompletionConnector(OllamaConfig? config = null)
    {
        _client = new OllamaChatCompletionClient(config);
    }

    public OllamaChatCompletionConnector(string baseUrl)
    {
        _client = new OllamaChatCompletionClient(baseUrl);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatCompletionModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetModelsAsync(cancellationToken);
        return models.Select(m => new ChatCompletionModel
        {
            Model = m.Name,
            CreatedAt = m.ModifiedAt,
        });
    }

    /// <inheritdoc />
    public async Task<ChatCompletionModel> GetModelAsync(
        string model,
        CancellationToken cancellationToken = default)
    {
        var models = await GetModelsAsync(cancellationToken);
        return models.First(m => m.Model == model);
    }

    /// <inheritdoc />
    public async Task<ChatCompletionResponse<Message>> GenerateMessageAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var req = ConvertRequest(request);
        var res = await _client.PostChatAsync(req, cancellationToken);
        var message = new Message(MessageRole.Assistant);
        
        // 텍스트 생성
        var text = res.Message?.Content;
        if (text != null)
        {
            message.Content.AddText(text);
        }

        // 도구 호출
        var tools = res.Message?.ToolCalls;
        if (tools != null)
        {
            foreach (var t in tools)
            {
                message.Content.AddTool(null, t.Function?.Name, JsonSerializer.Serialize(t.Function?.Arguments), null);
            }
        }

        return new ChatCompletionResponse<Message>
        {
            EndReason = res.DoneReason switch
            {
                DoneReason.Stop => EndReason.EndTurn,
                _ => null
            },
            TokenUsage = null,
            Data = message,
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionResponse<IMessageContent>> GenerateStreamingMessageAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = ConvertRequest(request);

        await foreach (var res in _client.PostSteamingChatAsync(req, cancellationToken))
        {
            // 텍스트 생성
            var text = res.Message?.Content;
            if (text != null)
            {
                yield return new ChatCompletionResponse<IMessageContent>
                {
                    Data = new TextContent
                    {
                        Value = text
                    },
                };
            }

            // 종료 메시지
            if (res.DoneReason != null)
            {
                yield return new ChatCompletionResponse<IMessageContent>
                {
                    EndReason = res.DoneReason switch
                    {
                        DoneReason.Stop => EndReason.EndTurn,
                        _ => null
                    },
                };
            }
        }
    }

    private static ChatRequest ConvertRequest(ChatCompletionRequest request)
    {
        var _req = new ChatRequest
        {
            Model = request.Model,
            Messages = request.Messages.ToOllama(request?.System),
            Options = new ModelOptions
            {
                NumPredict = request?.MaxTokens,
                Temperature = request?.Temperature,
                TopP = request?.TopP,
                TopK = request?.TopK,
                Stop = request?.StopSequences != null
                    ? string.Join(" ", request.StopSequences)
                    : null,
            },
        };

        // 동작하지 않는 모델 존재
        _req.Tools = request?.Tools.Select(t =>
        {
            return new Tool
            {
                Function = new FunctionTool
                {
                    Name = t.Name,
                    Description = t.Description,
                    Parameters = new ParametersSchema
                    {
                        Properties = t.Parameters?.Properties,
                        Required = t.Parameters?.Required,
                    }
                }
            };
        });

        return _req;
    }
}
