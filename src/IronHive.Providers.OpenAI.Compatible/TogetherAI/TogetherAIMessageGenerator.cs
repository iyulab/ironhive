using System.Text.Json;
using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;

namespace IronHive.Providers.OpenAI.Compatible.TogetherAI;

/// <summary>
/// Together AI 서비스를 위한 메시지 생성기입니다.
/// </summary>
public class TogetherAIMessageGenerator : OpenAIChatMessageGenerator
{
    private readonly TogetherAIConfig _config;

    public TogetherAIMessageGenerator(TogetherAIConfig config) : base(config.ToOpenAI())
    {
        _config = config;
    }

    protected override ChatCompletionRequest OnBeforeSend(
        MessageGenerationRequest source,
        ChatCompletionRequest request)
    {
        // 미지원 파라미터 제거
        request.LogitBias = null;
        request.WebSearchOptions = null;
        request.ReasoningEffort = null;

        // Together AI 전용 파라미터 주입
        request.AdditionalProperties ??= [];

        if (!string.IsNullOrEmpty(_config.SafetyModel))
            request.AdditionalProperties["safety_model"] =
                JsonSerializer.SerializeToElement(_config.SafetyModel);

        return request;
    }
}
