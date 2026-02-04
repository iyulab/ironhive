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
        var caps = ModelCapabilityResolver.Resolve(request.Model, _config.Compatibility);
        IMessageGenerator generator = caps.UseResponsesApi ? _responseGenerator : _chatGenerator;
        return generator.GenerateMessageAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var caps = ModelCapabilityResolver.Resolve(request.Model, _config.Compatibility);
        IMessageGenerator generator = caps.UseResponsesApi ? _responseGenerator : _chatGenerator;
        return generator.GenerateStreamingMessageAsync(request, cancellationToken);
    }
}
