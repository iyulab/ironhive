using System.Text.Json;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;

namespace IronHive.Providers.OpenAI.Compatible.Groq;

/// <summary>
/// Groq 서비스를 위한 메시지 생성기입니다.
/// </summary>
internal class GroqMessageGenerator : CompatibleChatMessageGenerator
{
    private readonly GroqConfig _groqConfig;

    public GroqMessageGenerator(GroqConfig config) : base(config)
    {
        _groqConfig = config;
    }

    protected override T PostProcessRequest<T>(ChatCompletionRequest request)
    {
        // n=1 강제 (Groq는 1만 지원)
        request.N = 1;

        // 미지원 파라미터 제거
        request.PresencePenalty = null;
        request.LogitBias = null;
        request.LogProbs = null;
        request.TopLogProbs = null;
        request.WebSearchOptions = null;

        // reasoning_format 주입
        if (!string.IsNullOrEmpty(_groqConfig.ReasoningFormat))
        {
            request.AdditionalProperties ??= [];
            request.AdditionalProperties["reasoning_format"] =
                JsonSerializer.SerializeToElement(_groqConfig.ReasoningFormat);
        }

        return (T)request;
    }
}
