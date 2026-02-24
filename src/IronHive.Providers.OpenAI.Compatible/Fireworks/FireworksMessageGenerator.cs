using System.Text.Json;
using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;
using IronHive.Providers.OpenAI.Payloads.Responses;

namespace IronHive.Providers.OpenAI.Compatible.Fireworks;

/// <summary>
/// Fireworks AI 서비스를 위한 메시지 생성기입니다.
/// </summary>
public class FireworksMessageGenerator : OpenAIResponseMessageGenerator
{
    private readonly FireworksConfig _config;

    public FireworksMessageGenerator(FireworksConfig config) : base(config.ToOpenAI())
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
        request.Include = null;
        request.TopLogProbs = null;

        return request;
    }
}
