using System.Runtime.CompilerServices;
using System.Text.Json;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Messages;
using IronHive.Connectors.Ollama.ChatCompletion;
using IronHive.Connectors.Ollama.Clients;

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
    public async Task<ChatCompletionResponse<AssistantMessage>> GenerateMessageAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var req = ConvertRequest(request);
        var res = await _client.PostChatAsync(req, cancellationToken);
        var message = new AssistantMessage();
        
        // 텍스트 생성
        var text = res.Message?.Content;
        if (text != null)
        {
            message.Content.Add(new AssistantTextContent
            {
                Value = text
            });
        }

        // 도구 호출
        var tools = res.Message?.ToolCalls;
        if (tools != null)
        {
            foreach (var t in tools)
            {
                message.Content.Add(new AssistantToolContent
                {
                    Name = t.Function?.Name,
                    Arguments = JsonSerializer.Serialize(t.Function?.Arguments)
                });
            }
        }

        return new ChatCompletionResponse<AssistantMessage>
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
    public async IAsyncEnumerable<ChatCompletionResponse<IAssistantContent>> GenerateStreamingMessageAsync(
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
                yield return new ChatCompletionResponse<IAssistantContent>
                {
                    Data = new AssistantTextContent
                    {
                        Value = text
                    },
                };
            }

            // 종료 메시지
            if (res.DoneReason != null)
            {
                yield return new ChatCompletionResponse<IAssistantContent>
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
            Options = new OllamaModelOptions
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
                    Parameters = t.InputSchema
                }
            };
        });

        return _req;
    }
}
