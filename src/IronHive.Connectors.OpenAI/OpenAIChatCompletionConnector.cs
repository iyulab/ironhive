using System.Runtime.CompilerServices;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Connectors.OpenAI.Clients;
using IronHive.Connectors.OpenAI.ChatCompletion;
using ChatCompletionRequest = IronHive.Abstractions.ChatCompletion.ChatCompletionRequest;
using OpenAIChatCompletionRequest = IronHive.Connectors.OpenAI.ChatCompletion.ChatCompletionRequest;
using AssistantMessage = IronHive.Abstractions.Messages.AssistantMessage;
using TokenUsage = IronHive.Abstractions.ChatCompletion.TokenUsage;

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
    public async Task<ChatCompletionResponse<AssistantMessage>> GenerateMessageAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var req = ConvertRequest(request);
        var res = await _client.PostChatCompletionAsync(req, cancellationToken);
        var choice = res.Choices?.FirstOrDefault();
        var message = new AssistantMessage();

        // 텍스트 생성
        var text = choice?.Message?.Content;
        if (!string.IsNullOrWhiteSpace(text))
        {
            message.Content.Add(new AssistantTextContent
            {
                Value = text
            });
        }

        // 툴 사용
        var tools = choice?.Message?.ToolCalls;
        if (tools != null && tools.Count > 0)
        {
            foreach (var t in tools)
            {
                message.Content.Add(new AssistantToolContent
                {
                    Id = t.Id,
                    Name = t.Function?.Name,
                    Arguments = t.Function?.Arguments,
                });
            }
        }

        return new ChatCompletionResponse<AssistantMessage>
        {
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
    public async IAsyncEnumerable<ChatCompletionResponse<IAssistantContent>> GenerateStreamingMessageAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = ConvertRequest(request);

        // index 지정 용도: OpenAI는 텍스트컨텐츠에 인덱스 속성이 없음, 임의로 생성
        // tool의 index를 정확히 하여 찾을 수 있게 하기 위함
        bool txtgen = false;
        EndReason? reason = null;
        TokenUsage? usage = null;
        await foreach (var res in _client.PostStreamingChatCompletionAsync(req, cancellationToken))
        {
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
            var tools = choice.Delta?.ToolCalls;
            if (tools != null)
            {
                foreach(var tool in tools)
                {
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = new AssistantToolContent
                        {
                            Id = tool.Id,
                            Index = txtgen ? tool.Index + 1 : tool.Index,
                            Name = tool.Function?.Name,
                            Arguments = tool.Function?.Arguments
                        }
                    };
                }
            }

            // 텍스트 생성
            var text = choice.Delta?.Content;
            if (text != null)
            {
                txtgen = true;
                yield return new ChatCompletionResponse<IAssistantContent>
                {
                    Data = new AssistantTextContent
                    {
                        Index = 0,
                        Value = text
                    }
                };
            }
        }

        // 종료
        yield return new ChatCompletionResponse<IAssistantContent>
        {
            EndReason = reason,
            TokenUsage = usage,
        };
    }

    private static OpenAIChatCompletionRequest ConvertRequest(ChatCompletionRequest request)
    {
        // 추론모델의 경우 일부 파라미터 작동 안함
        var isReasoning = IsReasoningModel(request.Model);

        var _req = new OpenAIChatCompletionRequest
        {
            Model = request.Model,
            Messages = request.Messages.ToOpenAI(request.System),
            MaxCompletionTokens = request.MaxTokens,
            Temperature = isReasoning ? null : request.Temperature,
            TopP = isReasoning ? null : request.TopP,
            Stop = request.StopSequences,
            Tools = request.Tools.Select(t => new Tool
            {
                Function = new Function
                {
                    Name = t.Name,
                    Description = t.Description,
                    Parameters = t.InputSchema
                }
            })
        };

        return _req;
    }

    private static bool IsReasoningModel(string model)
    {
        // 처음 시작이 o와 숫자로 시작하는 모델
        return model.StartsWith('o') && char.IsDigit(model[1]);
    }
}
