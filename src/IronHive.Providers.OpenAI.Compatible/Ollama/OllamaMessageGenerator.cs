using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI.Payloads.Responses;

namespace IronHive.Providers.OpenAI.Compatible.Ollama;

/// <summary>
/// Ollama 서비스를 위한 메시지 생성기입니다.
/// </summary>
public class OllamaMessageGenerator : OpenAIResponseMessageGenerator
{
    private readonly OllamaConfig _config;

    public OllamaMessageGenerator(OllamaConfig config) : base(config.ToOpenAI())
    {
        _config = config;
    }

    protected override ResponsesRequest OnBeforeSend(
        MessageGenerationRequest source,
        ResponsesRequest request)
    {
        // Ollama에서 미지원하는 파라미터 제거
        request.TopLogProbs = null;
        request.StreamOptions = null;

        return request;
    }
}
