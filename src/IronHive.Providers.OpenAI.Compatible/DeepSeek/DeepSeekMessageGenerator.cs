using System.Text.Json.Nodes;
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

    protected override ChatCompletionRequest OnBeforeGenerate(
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
            request.ExtraBody = new()
            {
                ["thinking"] = new JsonObject { ["type"] = "enabled" }
            };
        }

        return request;
    }
}
