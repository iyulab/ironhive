# 미들웨어 시스템

에이전트 호출을 가로채어 재시도, 타임아웃, 로깅 등 횡단 관심사를 적용합니다.

## 개요

```
IAgentMiddleware
    │
    ├── RetryMiddleware           # 실패 시 재시도 (지수 백오프)
    ├── TimeoutMiddleware         # 타임아웃 적용
    ├── RateLimitMiddleware       # 호출 빈도 제한 (슬라이딩 윈도우)
    ├── CircuitBreakerMiddleware  # 회로 차단기
    ├── BulkheadMiddleware        # 동시 실행 수 제한
    ├── CachingMiddleware         # 응답 캐싱
    ├── LoggingMiddleware         # 호출 로깅
    ├── FallbackMiddleware        # 실패 시 대체 에이전트
    └── CompositeMiddleware       # 미들웨어 조합
```

---

## 미들웨어 적용

```csharp
// 단일 미들웨어
var agent = baseAgent.WithMiddleware(new RetryMiddleware(maxRetries: 3));

// 다중 미들웨어 (실행 순서: 왼쪽 → 오른쪽)
var agent = baseAgent.WithMiddleware(
    new LoggingMiddleware(Console.WriteLine),
    new RetryMiddleware(maxRetries: 3),
    new TimeoutMiddleware(TimeSpan.FromSeconds(30))
);
```

실행 흐름:
```
LoggingMiddleware → RetryMiddleware → TimeoutMiddleware → Agent
                                                            ↓
LoggingMiddleware ← RetryMiddleware ← TimeoutMiddleware ← 응답
```

---

## RetryMiddleware

실패 시 지수 백오프(exponential backoff)로 재시도합니다.

```csharp
// 간단한 사용
var agent = baseAgent.WithMiddleware(new RetryMiddleware(maxRetries: 3));

// 상세 설정
var agent = baseAgent.WithMiddleware(new RetryMiddleware(new RetryMiddlewareOptions
{
    MaxRetries = 3,
    InitialDelay = TimeSpan.FromSeconds(1),
    MaxDelay = TimeSpan.FromSeconds(30),
    BackoffMultiplier = 2.0,
    JitterFactor = 0.2,
    ShouldRetry = ex => ex is HttpRequestException,
    OnRetry = (name, attempt, ex, delay) =>
        Console.WriteLine($"Retry {attempt}: {ex.Message}")
}));
```

| 옵션 | 기본값 | 설명 |
|------|--------|------|
| `MaxRetries` | 3 | 최대 재시도 횟수 |
| `InitialDelay` | 1초 | 초기 대기 시간 |
| `MaxDelay` | 30초 | 최대 대기 시간 |
| `BackoffMultiplier` | 2.0 | 백오프 배수 |
| `JitterFactor` | 0.2 | 지터 비율 (0~1) |
| `ShouldRetry` | `_ => true` | 재시도 조건 함수 |
| `OnRetry` | null | 재시도 시 콜백 |
| `OnRetryFailed` | null | 모든 재시도 실패 시 콜백 |

---

## TimeoutMiddleware

에이전트 호출에 타임아웃을 적용합니다. 초과 시 `TimeoutException` 발생.

```csharp
// 간단한 사용
var agent = baseAgent.WithMiddleware(new TimeoutMiddleware(TimeSpan.FromSeconds(30)));

// 상세 설정
var agent = baseAgent.WithMiddleware(new TimeoutMiddleware(new TimeoutMiddlewareOptions
{
    Timeout = TimeSpan.FromSeconds(30),
    OnTimeout = (name, timeout) =>
        Console.WriteLine($"Agent '{name}' timed out after {timeout.TotalSeconds}s")
}));
```

---

## RateLimitMiddleware

슬라이딩 윈도우 방식으로 호출 빈도를 제한합니다.

```csharp
// 분당 60회 제한
var agent = baseAgent.WithMiddleware(new RateLimitMiddleware(60, TimeSpan.FromMinutes(1)));

// 상세 설정
var agent = baseAgent.WithMiddleware(new RateLimitMiddleware(new RateLimitMiddlewareOptions
{
    MaxRequests = 60,
    Window = TimeSpan.FromMinutes(1),
    OnRateLimited = (name, waitTime) =>
        Console.WriteLine($"Rate limited. Waiting {waitTime.TotalSeconds}s")
}));
```

| 옵션 | 기본값 | 설명 |
|------|--------|------|
| `MaxRequests` | 60 | 윈도우 내 최대 요청 수 |
| `Window` | 1분 | 슬라이딩 윈도우 시간 |
| `OnRateLimited` | null | Rate limit 대기 시 콜백 |

속성: `CurrentRequestCount` — 현재 윈도우 내 요청 수

---

## CircuitBreakerMiddleware

연속 실패 시 회로를 차단하여 시스템을 보호합니다.

### 상태 전이

```
Closed (정상) ──실패 임계값 도달──→ Open (차단)
    ↑                                  │
    └───── 성공 ────── HalfOpen ←──── 차단 시간 경과
                       (테스트)
```

```csharp
// 간단한 사용 (5회 실패, 30초 차단)
var agent = baseAgent.WithMiddleware(new CircuitBreakerMiddleware(5, TimeSpan.FromSeconds(30)));

// 상세 설정
var cb = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
{
    FailureThreshold = 5,
    FailureWindow = TimeSpan.FromMinutes(1),
    BreakDuration = TimeSpan.FromSeconds(30),
    OnBreak = (name, ex, duration) =>
        Console.WriteLine($"Circuit opened for {duration.TotalSeconds}s"),
    OnStateChanged = (oldState, newState) =>
        Console.WriteLine($"Circuit: {oldState} → {newState}")
});

// 상태 확인
Console.WriteLine(cb.State); // Closed / Open / HalfOpen

// 수동 리셋
cb.Reset();
```

| 옵션 | 기본값 | 설명 |
|------|--------|------|
| `FailureThreshold` | 5 | 회로 Open에 필요한 연속 실패 횟수 |
| `FailureWindow` | 1분 | 실패 카운트 유지 시간 |
| `BreakDuration` | 30초 | 회로 Open 유지 시간 |

회로가 Open 상태일 때 `CircuitBreakerOpenException` 발생.

---

## BulkheadMiddleware

격리된 실행 풀로 동시 실행 수를 제한합니다.

```csharp
// 동시 실행 5개 제한
var agent = baseAgent.WithMiddleware(new BulkheadMiddleware(5));

// 대기열 포함
var agent = baseAgent.WithMiddleware(new BulkheadMiddleware(5, maxQueueSize: 10));

// 상세 설정
var bh = new BulkheadMiddleware(new BulkheadMiddlewareOptions
{
    MaxConcurrency = 5,
    MaxQueueSize = 10,
    QueueTimeout = TimeSpan.FromSeconds(30),
    OnQueued = (name, executing, queued) =>
        Console.WriteLine($"Queued. Executing: {executing}, Queued: {queued}"),
    OnRejected = (name, executing, queued) =>
        Console.WriteLine("Queue full!")
});

// 상태 확인
Console.WriteLine(bh.CurrentExecuting);
Console.WriteLine(bh.CurrentQueued);
Console.WriteLine(bh.AvailableSlots);
```

| 옵션 | 기본값 | 설명 |
|------|--------|------|
| `MaxConcurrency` | 10 | 최대 동시 실행 수 |
| `MaxQueueSize` | 0 (무제한) | 대기열 최대 크기 |
| `QueueTimeout` | 무한대 | 대기열 타임아웃 |

대기열이 가득 차거나 타임아웃 시 `BulkheadRejectedException` 발생.

---

## CachingMiddleware

동일한 입력에 대한 응답을 캐싱합니다.

> **주의**: LLM 응답은 비결정적이므로 캐싱 사용 시 주의가 필요합니다.

```csharp
// 5분 캐시
var agent = baseAgent.WithMiddleware(new CachingMiddleware(TimeSpan.FromMinutes(5)));

// 상세 설정
var cache = new CachingMiddleware(new CachingMiddlewareOptions
{
    Expiration = TimeSpan.FromMinutes(5),
    MaxCacheSize = 1000,
    IncludeInstructionsInKey = true,
    OnCacheHit = (name, key) => Console.WriteLine("Cache hit!"),
    OnCacheMiss = (name, key) => Console.WriteLine("Cache miss")
});

Console.WriteLine(cache.CacheCount);
cache.ClearCache();
```

캐시 키 구성 요소: 에이전트 이름 + 모델 ID + Instructions (옵션) + 메시지 내용

---

## LoggingMiddleware

에이전트 호출을 로깅합니다. 스트리밍 지원.

```csharp
// 간단한 사용
var agent = baseAgent.WithMiddleware(new LoggingMiddleware(Console.WriteLine));

// 상세 설정
var agent = baseAgent.WithMiddleware(new LoggingMiddleware(new LoggingMiddlewareOptions
{
    LogAction = Console.WriteLine,
    LogPrefix = "MyAgent",
    IncludeMessagePreview = true,
    IncludeResponsePreview = true,
    MaxPreviewLength = 100
}));
```

출력 예시:
```
[MyAgent] Agent 'Assistant' starting (invoke) (messages: 3, model: gpt-4o-mini)
  Last message: 안녕하세요, 오늘 날씨가 어떤가요?
[MyAgent] Agent 'Assistant' completed (1234ms, tokens: 50+150)
  Response: 안녕하세요! 오늘 서울의 날씨는...
```

---

## FallbackMiddleware

실패 시 대체 에이전트로 폴백합니다.

```csharp
// 단순 폴백
var agent = primaryAgent.WithMiddleware(new FallbackMiddleware(fallbackAgent));

// 동적 폴백
var agent = primaryAgent.WithMiddleware(new FallbackMiddleware(new FallbackMiddlewareOptions
{
    FallbackFactory = primary => new BasicAgent(messageService)
    {
        Provider = "ollama",
        Model = "llama3",
        Name = $"{primary.Name}-fallback"
    },
    ShouldFallback = ex => ex is HttpRequestException,
    ResponseValidator = response => response.Message != null,
    OnFallback = (name, ex, reason) =>
        Console.WriteLine($"Falling back from {name}: {reason}")
}));
```

폴백 에이전트도 실패하면 `FallbackFailedException` 발생.

---

## CompositeMiddleware

여러 미들웨어를 재사용 가능한 팩으로 조합합니다.

```csharp
var myPack = new CompositeMiddleware("my-pack",
    new LoggingMiddleware(Console.WriteLine),
    new RetryMiddleware(3),
    new TimeoutMiddleware(TimeSpan.FromSeconds(30)));

var agent = baseAgent.WithMiddleware(myPack);

// 불변 확장
var extended = myPack.With(new CachingMiddleware(TimeSpan.FromMinutes(5)));
var prepended = myPack.Prepend(new BulkheadMiddleware(10));
// 원본 변경 없음: myPack.Count == 3, extended.Count == 4
```

---

## 스트리밍 지원

`IStreamingAgentMiddleware`를 구현한 미들웨어만 스트리밍 호출에 참여합니다.

스트리밍 지원: `LoggingMiddleware`, `CompositeMiddleware` (스트리밍 미들웨어 자동 필터링)

---

## 커스텀 미들웨어 구현

```csharp
// 비스트리밍 미들웨어
public class MyMiddleware : IAgentMiddleware
{
    public async Task<MessageResponse> InvokeAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        Func<IEnumerable<Message>, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        // 전처리
        var response = await next(messages);
        // 후처리
        return response;
    }
}

// 스트리밍 지원 미들웨어
public class MyStreamingMiddleware : IAgentMiddleware, IStreamingAgentMiddleware
{
    public Task<MessageResponse> InvokeAsync(...) { ... }

    public async IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        Func<IEnumerable<Message>, IAsyncEnumerable<StreamingMessageResponse>> next,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var chunk in next(messages).WithCancellation(cancellationToken))
        {
            yield return chunk;
        }
    }
}
```

---

## 관련 문서

- [AGENTS.md](AGENTS.md) — 에이전트 시스템
- [ORCHESTRATION.md](ORCHESTRATION.md) — 오케스트레이션 패턴
