using System.Text.Json;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;

namespace IronHive.Providers.OpenAI.Compatible.TogetherAI;

/// <summary>
/// Together AI 서비스를 위한 메시지 생성기입니다.
/// </summary>
internal class TogetherAIMessageGenerator : CompatibleChatMessageGenerator
{
    private readonly TogetherAIConfig _togetherConfig;

    public TogetherAIMessageGenerator(TogetherAIConfig config) : base(config)
    {
        _togetherConfig = config;
    }

    protected override T PostProcessRequest<T>(ChatCompletionRequest request)
    {
        // 미지원 파라미터 제거
        request.LogitBias = null;
        request.WebSearchOptions = null;
        request.ReasoningEffort = null;

        // Together AI 전용 파라미터 주입
        request.AdditionalProperties ??= [];

        if (_togetherConfig.TopK.HasValue)
            request.AdditionalProperties["top_k"] =
                JsonSerializer.SerializeToElement(_togetherConfig.TopK.Value);

        if (_togetherConfig.RepetitionPenalty.HasValue)
            request.AdditionalProperties["repetition_penalty"] =
                JsonSerializer.SerializeToElement(_togetherConfig.RepetitionPenalty.Value);

        if (_togetherConfig.MinP.HasValue)
            request.AdditionalProperties["min_p"] =
                JsonSerializer.SerializeToElement(_togetherConfig.MinP.Value);

        if (!string.IsNullOrEmpty(_togetherConfig.SafetyModel))
            request.AdditionalProperties["safety_model"] =
                JsonSerializer.SerializeToElement(_togetherConfig.SafetyModel);

        return (T)request;
    }
}
