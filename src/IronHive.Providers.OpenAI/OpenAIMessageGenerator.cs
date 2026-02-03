using System.Runtime.CompilerServices;
using IronHive.Abstractions.Messages;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// OpenAI 통합 메시지 생성기.
/// 모델명에 따라 적절한 API (Chat Completions 또는 Responses)를 자동 선택합니다.
/// </summary>
public class OpenAIMessageGenerator : IMessageGenerator
{
    private readonly OpenAIConfig _config;
    private readonly OpenAIChatMessageGenerator _chatGenerator;
    private readonly OpenAIResponseMessageGenerator _responseGenerator;

    public OpenAIMessageGenerator(string apiKey)
        : this(new OpenAIConfig { ApiKey = apiKey })
    { }

    public OpenAIMessageGenerator(OpenAIConfig config)
    {
        _config = config;
        _chatGenerator = new OpenAIChatMessageGenerator(config);
        _responseGenerator = new OpenAIResponseMessageGenerator(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _chatGenerator.Dispose();
        _responseGenerator.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public Task<MessageResponse> GenerateMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var generator = SelectGenerator(request.Model);
        return generator.GenerateMessageAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var generator = SelectGenerator(request.Model);
        return generator.GenerateStreamingMessageAsync(request, cancellationToken);
    }

    /// <summary>
    /// 모델명에 따라 적절한 Generator를 선택합니다.
    /// Responses API를 사용하는 모델:
    /// - OpenAI: o-series (o1, o3, o4) 및 gpt-5 이상
    ///
    /// 참고: xAI grok-4는 reasoning을 API에서 노출하지 않음 (내부 reasoning만 수행)
    /// grok-3-mini만 Chat Completions에서 reasoning_content를 반환
    /// </summary>
    private IMessageGenerator SelectGenerator(string model)
    {
        var modelLower = model.ToLowerInvariant();

        // OpenAI o-series (o1, o3, o4 등) 및 gpt-5 이상
        if (modelLower.StartsWith("o1") ||
            modelLower.StartsWith("o3") ||
            modelLower.StartsWith("o4") ||
            modelLower.StartsWith("gpt-5"))
        {
            return _responseGenerator;
        }

        return _chatGenerator;
    }
}
