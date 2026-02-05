using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;

namespace IronHive.Core.Agent;

/// <summary>
/// 동일한 입력에 대한 응답을 캐싱하는 미들웨어입니다.
/// 비결정적인 LLM 응답을 캐싱할 때 주의가 필요합니다.
/// </summary>
public class CachingMiddleware : IAgentMiddleware
{
    private readonly CachingMiddlewareOptions _options;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

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
    /// 현재 캐시된 항목 수를 반환합니다.
    /// </summary>
    public int CacheCount => _cache.Count;

    /// <summary>
    /// 캐시를 모두 제거합니다.
    /// </summary>
    public void ClearCache() => _cache.Clear();

    public async Task<MessageResponse> InvokeAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        Func<IEnumerable<Message>, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        var messageList = messages.ToList();
        var cacheKey = ComputeCacheKey(agent, messageList);

        // 캐시 히트 확인
        if (_cache.TryGetValue(cacheKey, out var entry))
        {
            if (!IsExpired(entry))
            {
                _options.OnCacheHit?.Invoke(agent.Name, cacheKey);
                return entry.Response;
            }

            // 만료된 항목 제거
            _cache.TryRemove(cacheKey, out _);
        }

        // 캐시 미스 - 실제 호출
        _options.OnCacheMiss?.Invoke(agent.Name, cacheKey);
        var response = await next(messageList).ConfigureAwait(false);

        // 캐시 저장
        if (ShouldCache(response))
        {
            CleanupExpiredEntries();

            if (_cache.Count < _options.MaxCacheSize)
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

    private string ComputeCacheKey(IAgent agent, IReadOnlyList<Message> messages)
    {
        var keyBuilder = new StringBuilder();

        // 에이전트 식별 정보
        keyBuilder.Append($"agent:{agent.Name}:{agent.Model}:");

        // Instructions가 캐시 키에 영향을 주는지 여부
        if (_options.IncludeInstructionsInKey && !string.IsNullOrEmpty(agent.Instructions))
        {
            keyBuilder.Append($"instructions:{agent.Instructions}:");
        }

        // 메시지 내용
        foreach (var message in messages)
        {
            var (role, content) = message switch
            {
                UserMessage um => ("user", ExtractTextContent(um.Content)),
                AssistantMessage am => ("assistant", ExtractTextContent(am.Content)),
                _ => ("unknown", "")
            };
            keyBuilder.Append($"[{role}:{content}]");
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

    private bool IsExpired(CacheEntry entry)
    {
        if (_options.Expiration == TimeSpan.Zero)
            return false; // 만료 없음

        return DateTime.UtcNow - entry.CreatedAt > _options.Expiration;
    }

    private bool ShouldCache(MessageResponse response)
    {
        // 완료된 응답만 캐싱
        return response.DoneReason == MessageDoneReason.EndTurn
            || response.DoneReason == MessageDoneReason.MaxTokens;
    }

    private void CleanupExpiredEntries()
    {
        if (_options.Expiration == TimeSpan.Zero)
            return;

        var now = DateTime.UtcNow;
        var keysToRemove = _cache
            .Where(kvp => now - kvp.Value.CreatedAt > _options.Expiration)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.TryRemove(key, out _);
        }
    }

    private class CacheEntry
    {
        public required MessageResponse Response { get; init; }
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
    /// 최대 캐시 크기 (기본값: 1000)
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
