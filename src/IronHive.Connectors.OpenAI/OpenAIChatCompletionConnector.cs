using IronHive.Connectors.OpenAI.Configurations;
using IronHive.Connectors.OpenAI.Extensions;
using System.Runtime.CompilerServices;
using TokenUsage = IronHive.Abstractions.ChatCompletion.TokenUsage;
using IronHive.Connectors.OpenAI.ChatCompletion;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.ChatCompletion.Messages;
using IronHive.Abstractions.ChatCompletion.Tools;
using Message = IronHive.Abstractions.ChatCompletion.Messages.Message;
using OpenAIMessage = IronHive.Connectors.OpenAI.ChatCompletion.Message;

namespace IronHive.Connectors.OpenAI;

public class OpenAIChatCompletionConnector : IChatCompletionConnector
{
    private readonly OpenAIChatCompletionClient _client;

    public OpenAIChatCompletionConnector(OpenAIConfig config)
    {
        _client = new OpenAIChatCompletionClient(config);
    }

    public OpenAIChatCompletionConnector(string apiKey)
    {
        _client = new OpenAIChatCompletionClient(apiKey);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatCompletionModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetModelsAsync(cancellationToken);
        return models.Where(m => m.IsChatCompletion())
                    .Select(m => new ChatCompletionModel
                    {
                        Model = m.Id,
                        Owner = m.OwnedBy,
                        CreatedAt = m.Created,
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
    public async Task<ChatCompletionResult<Message>> GenerateMessageAsync(
        MessageCollection messages,
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default)
    {
        var req = BuildRequest(messages, options);
        var res = await _client.PostChatCompletionAsync(req, cancellationToken);
        var choice = res.Choices?.FirstOrDefault();
        var message = new Message(MessageRole.Assistant);

        // 툴 사용
        var tools = choice?.Message?.ToolCalls;
        if (tools != null && tools.Count > 0)
        {
            
            foreach (var t in tools)
            {
                message.Content.AddTool(t.Id, t.Function?.Name, t.Function?.Arguments, null);
            }
        }

        // 텍스트 생성
        var text = choice?.Message?.Content;
        if (!string.IsNullOrWhiteSpace(text))
        {
            message.Content.AddText(text);
        }

        return new ChatCompletionResult<Message>
        {
            MessageId = res.Id,
            EndReason = choice?.FinishReason switch
            {
                FinishReason.ToolCalls => EndReason.ToolCall,
                FinishReason.Stop => EndReason.EndTurn,
                FinishReason.Length => EndReason.MaxTokens,
                FinishReason.ContentFilter => EndReason.ContentFilter,
                _ => null
            },
            TokenUsage = new TokenUsage
            {
                InputTokens = res.Usage?.PromptTokens,
                OutputTokens = res.Usage?.CompletionTokens
            },
            Data = message
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionResult<IMessageContent>> GenerateStreamingMessageAsync(
        MessageCollection messages,
        ChatCompletionOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = BuildRequest(messages, options);

        string? id = null;
        EndReason? reason = null;
        TokenUsage? usage = null;
        await foreach (var res in _client.PostStreamingChatCompletionAsync(req, cancellationToken))
        {
            // id 설정
            id ??= res.Id;

            // 토큰 사용량(FinishReason 다음 호출)
            if (res.Usage != null)
            {
                usage = new TokenUsage
                {
                    InputTokens = res.Usage.PromptTokens,
                    OutputTokens = res.Usage.CompletionTokens
                };
            }

            // 메시지 확인 및 건너뛰기
            var choice = res.Choices?.FirstOrDefault();
            if (choice == null)
            {
                continue;
            }

            // 종료 메시지
            if (choice.FinishReason != null)
            {
                reason = choice.FinishReason switch
                {
                    FinishReason.ToolCalls => EndReason.ToolCall,
                    FinishReason.Stop => EndReason.EndTurn,
                    FinishReason.Length => EndReason.MaxTokens,
                    FinishReason.ContentFilter => EndReason.ContentFilter,
                    _ => null
                };
            }

            // 툴 사용
            var tool = choice.Delta?.ToolCalls?.FirstOrDefault();
            if (tool != null)
            {
                yield return new ChatCompletionResult<IMessageContent>
                {
                    MessageId = id,
                    Data = new ToolContent
                    {
                        Id = tool.Id,
                        Index = tool.Index,
                        Name = tool.Function?.Name,
                        Arguments = tool.Function?.Arguments
                    }
                };
            }

            // 텍스트 생성
            var text = choice.Delta?.Content;
            if (text != null)
            {
                yield return new ChatCompletionResult<IMessageContent>
                {
                    MessageId = id,
                    Data = new TextContent
                    {
                        Index = null,
                        Value = text
                    }
                };
            }
        }

        // 종료
        yield return new ChatCompletionResult<IMessageContent>
        {
            MessageId = id,
            EndReason = reason,
            TokenUsage = usage,
        };
    }

    private static ChatCompletionRequest BuildRequest(MessageCollection messages, ChatCompletionOptions options)
    {
        // Reasoning Models, 일부 파라미터 작동 안함
        var reason = options.Model.Contains("o1") || options.Model.Contains("o3");

        var request = new ChatCompletionRequest
        {
            Model = options.Model,
            Messages = messages.ToOpenAI(options.System),
            MaxCompletionTokens = options.MaxTokens,
            Temperature = reason ? null : options.Temperature,
            TopP = reason ? null : options.TopP,
            Stop = options.StopSequences,
        };

        if (options.Tools != null && options.Tools.Count > 0)
        {
            request.Tools = options.Tools.Select(t => new Tool
            {
                Function = new Function
                {
                    Name = t.Name,
                    Description = t.Description,
                    Parameters = t.Parameters != null ? new FunctionParameters
                    {
                        Properties = t.Parameters.Properties,
                        Required = t.Parameters.Required,
                    }: null
                }
            });
        }

        return request;
    }
}
