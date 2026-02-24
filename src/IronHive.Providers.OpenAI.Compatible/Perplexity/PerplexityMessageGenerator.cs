using System.Text.Json;
using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI.Payloads.Responses;

namespace IronHive.Providers.OpenAI.Compatible.Perplexity;

/// <summary>
/// Perplexity 서비스를 위한 메시지 생성기입니다.
/// </summary>
public class PerplexityMessageGenerator : OpenAIResponseMessageGenerator
{
    private readonly PerplexityConfig _config;

    public PerplexityMessageGenerator(PerplexityConfig config) : base(config.ToOpenAI())
    {
        _config = config;
    }

    protected override ResponsesRequest OnBeforeGenerate(
        MessageGenerationRequest source,
        ResponsesRequest request)
    {
        // 미지원 파라미터 제거
        request.Background = null;
        request.Conversation = null;
        request.ToolChoice = null;
        request.TopLogProbs = null;

        return request;
    }
}
