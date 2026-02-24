using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI.Payloads.Responses;

namespace IronHive.Providers.OpenAI.Compatible.Groq;

/// <summary>
/// Groq 서비스를 위한 메시지 생성기입니다.
/// </summary>
public class GroqMessageGenerator : OpenAIResponseMessageGenerator
{
    private readonly GroqConfig _config;

    public GroqMessageGenerator(GroqConfig config) : base(config.ToOpenAI())
    {
        _config = config;
    }

    protected override ResponsesRequest OnBeforeGenerate(
        MessageGenerationRequest source,
        ResponsesRequest request)
    {
        // 미지원 파라미터 제거
        request.TopLogProbs = null;

        return request;
    }
}
