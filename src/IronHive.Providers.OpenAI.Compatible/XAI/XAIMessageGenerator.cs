using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI.Payloads.Responses;

namespace IronHive.Providers.OpenAI.Compatible.XAI;

/// <summary>
/// xAI (Grok) 서비스를 위한 메시지 생성기입니다.
/// </summary>
public class XAIMessageGenerator : OpenAIResponseMessageGenerator
{
    private readonly XAIConfig _config;

    public XAIMessageGenerator(XAIConfig config) : base(config.ToOpenAI())
    {
        _config = config;
    }

    protected override ResponsesRequest OnBeforeSend(
        MessageGenerationRequest source,
        ResponsesRequest request)
    {
        // 미지원 파라미터 제거
        request.Background = null;
        request.Metadata = null;
        request.ServiceTier = null;
        request.Truncation = null;

        // instructions 대신 system/developer 역할 메시지 사용
        if (!string.IsNullOrWhiteSpace(request.Instructions))
        {
            var systemMessage = new ResponsesMessageItem
            {
                Role = ResponsesMessageRole.Developer,
                Content =
                [
                    new ResponsesInputTextContent
                    {
                        Text = request.Instructions
                    }
                ]
            };

            var newInput = new List<ResponsesItem> { systemMessage };
            newInput.AddRange(request.Input);
            request.Input = newInput;
            request.Instructions = null;
        }

        // reasoning_effort는 grok-3-mini에서만 지원
        bool isReasoningModel = request.Model.Contains("grok-3-mini", StringComparison.OrdinalIgnoreCase);
        if (!isReasoningModel)
        {
            request.Reasoning = null;
            request.Include = null;
        }

        // include는 reasoning.encrypted_content만 지원
        if (request.Include is { Count: > 0 })
        {
            var filtered = request.Include
                .Where(i => i == "reasoning.encrypted_content")
                .ToList();
            request.Include = filtered.Count > 0 ? filtered : null;
        }

        return request;
    }
}
