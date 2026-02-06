using System.Text.Json;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;

namespace IronHive.Providers.OpenAI.Compatible.DeepSeek;

/// <summary>
/// DeepSeek 서비스를 위한 메시지 생성기입니다.
/// </summary>
internal class DeepSeekMessageGenerator : CompatibleChatMessageGenerator
{
    private readonly DeepSeekConfig _deepSeekConfig;

    public DeepSeekMessageGenerator(DeepSeekConfig config) : base(config)
    {
        _deepSeekConfig = config;
    }

    protected override T PostProcessRequest<T>(ChatCompletionRequest request)
    {
        // n=1 강제 (DeepSeek은 1만 지원)
        request.N = 1;

        // 미지원 파라미터 제거
        request.LogitBias = null;
        request.LogProbs = null;
        request.TopLogProbs = null;

        // Thinking 모드 활성화
        if (_deepSeekConfig.EnableThinking)
        {
            request.AdditionalProperties ??= [];
            var thinking = new Dictionary<string, object> { ["type"] = "enabled" };
            if (_deepSeekConfig.ThinkingBudgetTokens.HasValue)
            {
                thinking["budget_tokens"] = _deepSeekConfig.ThinkingBudgetTokens.Value;
            }
            request.AdditionalProperties["thinking"] =
                JsonSerializer.SerializeToElement(thinking);

            // 추론 모드에서 무시되는 파라미터 제거
            request.Temperature = null;
            request.TopP = null;
            request.PresencePenalty = null;
            request.FrequencyPenalty = null;
        }

        return (T)request;
    }
}
