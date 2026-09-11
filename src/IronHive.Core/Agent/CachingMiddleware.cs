using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;

namespace IronHive.Core.Agent;

/// <summary>
/// 동일한 입력에 대한 응답을 캐싱하는 미들웨어입니다.
/// 비결정적인 LLM 응답을 캐싱할 때 주의가 필요합니다.
/// 스트리밍과 비스트리밍 모두 지원합니다 — 스트리밍 호출은 프레임을 그대로 흘려보내면서 모아 두었다가,
/// 스트림이 정상 종료(<see cref="MessageDoneReason.EndTurn"/> · <see cref="MessageDoneReason.MaxTokens"/>)로
/// 끝까지 읽혔을 때만 저장하고, 같은 입력의 다음 스트리밍 호출에 같은 프레임을 재생합니다. 버퍼드 응답과
/// 스트림은 따로 저장되므로 한쪽으로 채운 캐시가 다른 쪽 호출에 적중하지 않습니다.
/// </summary>
public class CachingMiddleware : IAgentMiddleware, IStreamingAgentMiddleware
{
    private readonly CachingMiddlewareOptions _options;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly ConcurrentDictionary<string, StreamCacheEntry> _streamCache = new();

    public CachingMiddleware(CachingMiddlewareOptions? options = null)
    {
        _options = options ?? new CachingMiddlewareOptions();
    }

    /// <summary>
    /// 캐시 만료 시간만 지정하여 생성합니다.
    /// </summary>
    public CachingMiddleware(TimeSpan expiration)
        : this(new CachingMiddlewareOptions { Expiration = expiration })
    {
    }

    /// <summary>
    /// 현재 캐시된 항목 수를 반환합니다(버퍼드 응답과 스트림을 합한 수).
    /// </summary>
    public int CacheCount => _cache.Count + _streamCache.Count;

    /// <summary>
    /// 캐시를 모두 제거합니다.
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
        _streamCache.Clear();
    }

    public async Task<MessageResponse> InvokeAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        AgentInvokeOptions? options,
        Func<IEnumerable<Message>, AgentInvokeOptions?, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        var messageList = messages.ToList();
        var cacheKey = ComputeCacheKey(agent, messageList, options);

        // 캐시 히트 확인
        if (_cache.TryGetValue(cacheKey, out var entry))
        {
            if (!IsExpired(entry.CreatedAt))
            {
                _options.OnCacheHit?.Invoke(agent.Name, cacheKey);
                return entry.Response;
            }

            // 만료된 항목 제거
            _cache.TryRemove(cacheKey, out _);
        }

        // 캐시 미스 - 실제 호출
        _options.OnCacheMiss?.Invoke(agent.Name, cacheKey);
        var response = await next(messageList, options).ConfigureAwait(false);

        // 캐시 저장
        if (ShouldCache(response.DoneReason))
        {
            CleanupExpiredEntries();

            if (CacheCount < _options.MaxCacheSize)
            {
                _cache[cacheKey] = new CacheEntry
                {
                    Response = response,
                    CreatedAt = DateTime.UtcNow
                };
            }
        }

        return response;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        AgentInvokeOptions? options,
        Func<IEnumerable<Message>, AgentInvokeOptions?, IAsyncEnumerable<StreamingMessageResponse>> next,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messageList = messages.ToList();
        var cacheKey = ComputeCacheKey(agent, messageList, options);

        if (_streamCache.TryGetValue(cacheKey, out var entry))
        {
            if (!IsExpired(entry.CreatedAt))
            {
                _options.OnCacheHit?.Invoke(agent.Name, cacheKey);
                foreach (var frame in entry.Frames)
                {
                    yield return frame;
                }

                yield break;
            }

            _streamCache.TryRemove(cacheKey, out _);
        }

        _options.OnCacheMiss?.Invoke(agent.Name, cacheKey);
        var frames = new List<StreamingMessageResponse>();
        await foreach (var frame in next(messageList, options).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            frames.Add(frame);
            yield return frame;
        }

        // Reached only when the caller read the stream to its end: a stream abandoned half-way is
        // never stored, since replaying it would hand the next caller a truncated answer.
        if (frames is [.., StreamingMessageDoneResponse done] && ShouldCache(done.DoneReason))
        {
            CleanupExpiredEntries();

            if (CacheCount < _options.MaxCacheSize)
            {
                _streamCache[cacheKey] = new StreamCacheEntry
                {
                    Frames = frames,
                    CreatedAt = DateTime.UtcNow
                };
            }
        }
    }

    private string ComputeCacheKey(IAgent agent, IReadOnlyList<Message> messages, AgentInvokeOptions? options)
    {
        var keyBuilder = new StringBuilder();

        // 에이전트 식별 정보
        keyBuilder.Append(CultureInfo.InvariantCulture, $"agent:{agent.Name}:{agent.Model}:");

        // Instructions가 캐시 키에 영향을 주는지 여부
        if (_options.IncludeInstructionsInKey && !string.IsNullOrEmpty(agent.Instructions))
        {
            keyBuilder.Append(CultureInfo.InvariantCulture, $"instructions:{agent.Instructions}:");
        }

        // per-request 옵션 — 같은 입력이라도 옵션이 다르면 응답이 달라지므로 키에 포함.
        // blanket JSON 직렬화는 델리게이트(ToolOptions 콜백)에서 throw하므로 명시 필드만 반영한다.
        // 델리게이트·Items의 값은 키에 표현 불가 — 기존 키가 메시지의 텍스트 콘텐츠만 반영하는 것과
        // 같은 근사 키 정책 (Items는 키 목록까지만 구분).
        if (options is not null)
        {
            keyBuilder.Append(CultureInfo.InvariantCulture,
                $"options:{options.PreviousId}:{options.ThinkingEffort}:{options.MaxTokens}:{options.MaxTurns}:");

            if (options.ToolOptions is { } toolOptions)
            {
                keyBuilder.Append(CultureInfo.InvariantCulture,
                    $"tools:{toolOptions.MaxParallel}:{toolOptions.Timeout}:");
            }

            if (options.OutputFormat is { } outputFormat)
            {
                keyBuilder.Append(CultureInfo.InvariantCulture,
                    $"format:{outputFormat.Schema.ToJsonString()}:");
            }

            if (options.Suggestions is { } suggestions)
            {
                keyBuilder.Append(CultureInfo.InvariantCulture,
                    $"suggestions:{suggestions.Mode}:{suggestions.MaxCount}:{suggestions.MinItems}:{suggestions.MaxItems}:");
            }

            if (options.Items is { Count: > 0 } items)
            {
                keyBuilder.Append(CultureInfo.InvariantCulture,
                    $"items:{string.Join(",", items.Keys.Order(StringComparer.Ordinal))}:");
            }
        }

        // 메시지 내용
        foreach (var message in messages)
        {
            var role = message.Role == MessageRole.User ? "user" : "assistant";
            var content = ExtractTextContent(message.Content);
            keyBuilder.Append(CultureInfo.InvariantCulture, $"[{role}:{content}]");
        }

        // SHA256 해시로 키 생성
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(keyBuilder.ToString()));
        return Convert.ToHexStringLower(hash);
    }

    private static string ExtractTextContent(IEnumerable<MessageContent>? content)
    {
        if (content == null)
            return "";

        return string.Join("", content.OfType<TextMessageContent>().Select(c => c.Value));
    }

    private bool IsExpired(DateTime createdAt)
    {
        if (_options.Expiration == TimeSpan.Zero)
            return false; // 만료 없음

        return DateTime.UtcNow - createdAt > _options.Expiration;
    }

    private static bool ShouldCache(MessageDoneReason? doneReason)
    {
        // 완료된 응답만 캐싱
        return doneReason == MessageDoneReason.EndTurn
            || doneReason == MessageDoneReason.MaxTokens;
    }

    private void CleanupExpiredEntries()
    {
        if (_options.Expiration == TimeSpan.Zero)
            return;

        foreach (var key in _cache.Where(kvp => IsExpired(kvp.Value.CreatedAt)).Select(kvp => kvp.Key).ToList())
        {
            _cache.TryRemove(key, out _);
        }

        foreach (var key in _streamCache.Where(kvp => IsExpired(kvp.Value.CreatedAt)).Select(kvp => kvp.Key).ToList())
        {
            _streamCache.TryRemove(key, out _);
        }
    }

    private sealed class CacheEntry
    {
        public required MessageResponse Response { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    private sealed class StreamCacheEntry
    {
        public required IReadOnlyList<StreamingMessageResponse> Frames { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}

/// <summary>
/// CachingMiddleware 설정 옵션
/// </summary>
public class CachingMiddlewareOptions
{
    /// <summary>
    /// 캐시 만료 시간 (기본값: 5분)
    /// TimeSpan.Zero면 만료 없음
    /// </summary>
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 최대 캐시 크기 (기본값: 1000). 버퍼드 응답과 스트림을 합한 항목 수입니다.
    /// </summary>
    public int MaxCacheSize { get; set; } = 1000;

    /// <summary>
    /// Instructions를 캐시 키에 포함할지 여부 (기본값: true)
    /// </summary>
    public bool IncludeInstructionsInKey { get; set; } = true;

    /// <summary>
    /// 캐시 히트 시 호출되는 콜백 (agentName, cacheKey)
    /// </summary>
    public Action<string, string>? OnCacheHit { get; set; }

    /// <summary>
    /// 캐시 미스 시 호출되는 콜백 (agentName, cacheKey)
    /// </summary>
    public Action<string, string>? OnCacheMiss { get; set; }
}
