using System.Text.Json;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;

namespace IronHive.Providers.OpenAI.Compatible.Fireworks;

/// <summary>
/// Fireworks AI 서비스를 위한 메시지 생성기입니다.
/// </summary>
internal class FireworksMessageGenerator : CompatibleChatMessageGenerator
{
    private readonly FireworksConfig _fireworksConfig;

    public FireworksMessageGenerator(FireworksConfig config) : base(config)
    {
        _fireworksConfig = config;
    }

    protected override T PostProcessRequest<T>(ChatCompletionRequest request)
    {
        // 미지원 파라미터 제거
        request.LogitBias = null;
        request.WebSearchOptions = null;
        request.ReasoningEffort = null;

        // Fireworks AI 전용 파라미터 주입
        request.AdditionalProperties ??= [];

        if (_fireworksConfig.TopK.HasValue)
            request.AdditionalProperties["top_k"] =
                JsonSerializer.SerializeToElement(_fireworksConfig.TopK.Value);

        if (_fireworksConfig.MinP.HasValue)
            request.AdditionalProperties["min_p"] =
                JsonSerializer.SerializeToElement(_fireworksConfig.MinP.Value);

        if (_fireworksConfig.RepetitionPenalty.HasValue)
            request.AdditionalProperties["repetition_penalty"] =
                JsonSerializer.SerializeToElement(_fireworksConfig.RepetitionPenalty.Value);

        if (_fireworksConfig.PromptTruncateLen.HasValue)
            request.AdditionalProperties["prompt_truncate_len"] =
                JsonSerializer.SerializeToElement(_fireworksConfig.PromptTruncateLen.Value);

        if (!string.IsNullOrEmpty(_fireworksConfig.ContextLengthExceededBehavior))
            request.AdditionalProperties["context_length_exceeded_behavior"] =
                JsonSerializer.SerializeToElement(_fireworksConfig.ContextLengthExceededBehavior);

        return (T)request;
    }
}
