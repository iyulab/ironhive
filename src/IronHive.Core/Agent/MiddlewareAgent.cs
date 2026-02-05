using System.Runtime.CompilerServices;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Tools;

namespace IronHive.Core.Agent;

/// <summary>
/// 미들웨어 체인을 통해 에이전트를 실행하는 래퍼입니다.
/// </summary>
public class MiddlewareAgent : IAgent
{
    private readonly IAgent _inner;
    private readonly IReadOnlyList<IAgentMiddleware> _middlewares;

    public string Provider { get => _inner.Provider; set => _inner.Provider = value; }
    public string Model { get => _inner.Model; set => _inner.Model = value; }
    public string Name { get => _inner.Name; set => _inner.Name = value; }
    public string Description { get => _inner.Description; set => _inner.Description = value; }
    public string? Instructions { get => _inner.Instructions; set => _inner.Instructions = value; }
    public IEnumerable<ToolItem>? Tools { get => _inner.Tools; set => _inner.Tools = value; }
    public MessageGenerationParameters? Parameters { get => _inner.Parameters; set => _inner.Parameters = value; }

    public MiddlewareAgent(IAgent inner, IReadOnlyList<IAgentMiddleware> middlewares)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _middlewares = middlewares ?? throw new ArgumentNullException(nameof(middlewares));
    }

    /// <inheritdoc />
    public Task<MessageResponse> InvokeAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default)
    {
        if (_middlewares.Count == 0)
            return _inner.InvokeAsync(messages, cancellationToken);

        // 미들웨어 체인 구성: 마지막 미들웨어부터 역순으로 래핑
        Func<IEnumerable<Message>, Task<MessageResponse>> pipeline =
            msgs => _inner.InvokeAsync(msgs, cancellationToken);

        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var next = pipeline;
            pipeline = msgs => middleware.InvokeAsync(_inner, msgs, next, cancellationToken);
        }

        return pipeline(messages);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IEnumerable<Message> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // IStreamingAgentMiddleware를 구현한 미들웨어만 필터링
        var streamingMiddlewares = _middlewares
            .OfType<IStreamingAgentMiddleware>()
            .ToList();

        if (streamingMiddlewares.Count == 0)
            return _inner.InvokeStreamingAsync(messages, cancellationToken);

        // 스트리밍 미들웨어 체인 구성
        Func<IEnumerable<Message>, IAsyncEnumerable<StreamingMessageResponse>> pipeline =
            msgs => _inner.InvokeStreamingAsync(msgs, cancellationToken);

        for (var i = streamingMiddlewares.Count - 1; i >= 0; i--)
        {
            var middleware = streamingMiddlewares[i];
            var next = pipeline;
            pipeline = msgs => middleware.InvokeStreamingAsync(_inner, msgs, next, cancellationToken);
        }

        return pipeline(messages);
    }
}
