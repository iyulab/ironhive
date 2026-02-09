using System.Text.Json;
using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;

namespace IronHive.Providers.OpenAI.Compatible.DeepSeek;

/// <summary>
/// DeepSeek 서비스를 위한 메시지 생성기입니다.
/// </summary>
public class DeepSeekMessageGenerator : OpenAIChatMessageGenerator
{
    private readonly DeepSeekConfig _config;

    public DeepSeekMessageGenerator(DeepSeekConfig config) : base(config.ToOpenAI())
    {
        _config = config;
    }

    protected override ChatCompletionRequest OnBeforeSend(
        MessageGenerationRequest source,
        ChatCompletionRequest request)
    {
        // 미지원 파라미터 제거
        request.Temperature = null;
        request.TopP = null;
        request.PresencePenalty = null;
        request.FrequencyPenalty = null;
        request.LogProbs = null;
        request.TopLogProbs = null;

        // Thinking 모드 활성화
        if (source.ThinkingEffort != null && source.ThinkingEffort != MessageThinkingEffort.None)
        {
            request.AdditionalProperties = new Dictionary<string, JsonElement>
            {
                ["thinking"] = JsonSerializer.SerializeToElement(new Dictionary<string, object>
                {
                    ["type"] = "enabled",
                })
            };
        }

        return request;
    }
}
