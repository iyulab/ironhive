using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.ChatCompletion.Messages;
using IronHive.Connectors.Ollama.Base;
using IronHive.Connectors.Ollama.ChatCompletion;
using IronHive.Connectors.Ollama.Configurations;
using IronHive.Connectors.Ollama.Extensions;
using System.Runtime.CompilerServices;
using Message = IronHive.Abstractions.ChatCompletion.Messages.Message;
using OllamaMessage = IronHive.Connectors.Ollama.ChatCompletion.Message;
using MessageRole = IronHive.Abstractions.ChatCompletion.Messages.MessageRole;
using OllamaMessageRole = IronHive.Connectors.Ollama.ChatCompletion.Message;
using System.Text.Json;

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
            Owner = null,
            CreatedAt = null,
            ModifiedAt = m.ModifiedAt,
        });
    }

    /// <inheritdoc />
    public async Task<ChatCompletionResult<Message>> GenerateMessageAsync(
        MessageCollection messages,
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default)
    {
        var req = BuildRequest(messages, options);
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

        return new ChatCompletionResult<Message>
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
    public async IAsyncEnumerable<ChatCompletionResult<IMessageContent>> GenerateStreamingMessageAsync(
        MessageCollection messages,
        ChatCompletionOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = BuildRequest(messages, options);

        await foreach (var res in _client.PostSteamingChatAsync(req, cancellationToken))
        {
            // 텍스트 생성
            var text = res.Message?.Content;
            if (text != null)
            {
                yield return new ChatCompletionResult<IMessageContent>
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
                yield return new ChatCompletionResult<IMessageContent>
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

    private static ChatRequest BuildRequest(MessageCollection messages, ChatCompletionOptions options)
    {
        var request = new ChatRequest
        {
            Model = options.Model,
            Messages = messages.ToOllama(options?.System),
            Options = new ModelOptions
            {
                NumPredict = options?.MaxTokens,
                Temperature = options?.Temperature,
                TopP = options?.TopP,
                TopK = options?.TopK,
                Stop = options?.StopSequences != null
                    ? string.Join(" ", options.StopSequences)
                    : null,
            },
        };

        // 동작하지 않는 모델 존재함
        //if (options?.Tools != null && options.Tools.Count > 0)
        //{
        //    request.Tools = options.Tools.Select(t =>
        //    {
        //        return new Tool
        //        {
        //            Function = new FunctionTool
        //            {
        //                Name = t.Name,
        //                Description = t.Description,
        //                Parameters = new ParametersSchema
        //                {
        //                    Properties = t.Parameters,
        //                    Required = t.Required,
        //                }
        //            }
        //        };
        //    });
        //}

        return request;
    }
}
