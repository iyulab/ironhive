using System.Runtime.CompilerServices;
using TokenUsage = IronHive.Abstractions.ChatCompletion.TokenUsage;
using IronHive.Connectors.OpenAI.ChatCompletion;
using IronHive.Abstractions.ChatCompletion;
using ChatCompletionRequest = IronHive.Abstractions.ChatCompletion.ChatCompletionRequest;
using OpenAIChatCompletionRequest = IronHive.Connectors.OpenAI.ChatCompletion.ChatCompletionRequest;
using IMessage = IronHive.Abstractions.Messages.IMessage;
using OpenAIMessage = IronHive.Connectors.OpenAI.ChatCompletion.Message;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Embedding;
using System.Reflection;
using IronHive.Abstractions.Messages;
using AssistantMessage = IronHive.Abstractions.Messages.AssistantMessage;
using OpenAIAssistantMessage = IronHive.Connectors.OpenAI.ChatCompletion.AssistantMessage;

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
        if (_client.Client.BaseAddress?.ToString() == OpenAIConstants.DefaultBaseUrl)
        {
            // OpenAI 모델을 호출하는 경우 내장 리소스를 사용
            var assembly = Assembly.GetExecutingAssembly();
            var resource = await JsonResourceLoader.LoadAsync<IEnumerable<ChatCompletionModel>>(
                assembly: assembly,
                resourceName: $"{assembly.GetName().Name}.Resources.OpenAIChatModels.json",
                options: _client.JsonOptions,
                cancellationToken: cancellationToken);
            if (resource.Data == null)
                throw new InvalidOperationException("Failed to load OpenAI models.");

            return resource.Data;
        }
        else
        {
            // 다른 OpenAI 서버를 호출하는 경우 API를 사용
            var models = await _client.GetModelsAsync(cancellationToken);
            return models.Where(m => m.IsChatCompletion())
                        .Select(m => new ChatCompletionModel
                        {
                            Model = m.Id,
                            CreatedAt = m.Created,
                        });
        }
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
            message.Content.AddText(text);
        }

        // 툴 사용
        var tools = choice?.Message?.ToolCalls;
        if (tools != null && tools.Count > 0)
        {
            
            foreach (var t in tools)
            {
                message.Content.AddTool(t.Id, t.Function?.Name, t.Function?.Arguments, null);
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
            var tool = choice.Delta?.ToolCalls?.FirstOrDefault();
            if (tool != null)
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
        // Reasoning Models, 일부 파라미터 작동 안함
        var reason = request.Model.Contains("o1") || request.Model.Contains("o3");

        var _req = new OpenAIChatCompletionRequest
        {
            Model = request.Model,
            Messages = request.Messages.ToOpenAI(request.System),
            MaxCompletionTokens = request.MaxTokens,
            Temperature = reason ? null : request.Temperature,
            TopP = reason ? null : request.TopP,
            Stop = request.StopSequences,
        };

        _req.Tools = request.Tools.Select(t => new Tool
        {
            Function = new Function
            {
                Name = t.Name,
                Description = t.Description,
                Parameters = t.Parameters != null 
                ? new FunctionParameters
                {
                    Properties = t.Parameters.JsonSchema.Properties,
                    Required = t.Parameters.JsonSchema.Required,
                }
                : null
            }
        });

        return _req;
    }
}
