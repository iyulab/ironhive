# 미들웨어 시스템

IronHive의 에이전트 미들웨어 시스템에 대한 상세 문서입니다.

## 개요

미들웨어는 에이전트 호출을 가로채어 추가 로직(재시도, 타임아웃, 로깅 등)을 적용합니다.

```
IAgentMiddleware (인터페이스)
    │
    ├── RetryMiddleware           # 실패 시 재시도
    ├── TimeoutMiddleware         # 타임아웃 적용
    ├── RateLimitMiddleware       # 호출 빈도 제한
    ├── CircuitBreakerMiddleware  # 회로 차단기
    ├── BulkheadMiddleware        # 동시 실행 제한
    ├── CachingMiddleware         # 응답 캐싱
    ├── LoggingMiddleware         # 로깅
    ├── FallbackMiddleware        # 실패 시 대체 에이전트
    └── CompositeMiddleware       # 미들웨어 조합
```

---

## 미들웨어 적용

### 기본 사용법

```csharp
// 단일 미들웨어
var wrapped = agent.WithMiddleware(new RetryMiddleware(maxRetries: 3));

// 다중 미들웨어 (실행 순서: 왼쪽 → 오른쪽)
var wrapped = agent.WithMiddleware(
    new LoggingMiddleware(Console.WriteLine),
    new RetryMiddleware(maxRetries: 3),
    new TimeoutMiddleware(TimeSpan.FromSeconds(30))
);
```

### 실행 순서

미들웨어는 적용된 순서대로 실행됩니다. 위 예제에서 실행 흐름은:

```
LoggingMiddleware → RetryMiddleware → TimeoutMiddleware → Agent → TimeoutMiddleware → RetryMiddleware → LoggingMiddleware
```

---

## RetryMiddleware

에이전트 호출 실패 시 지수 백오프(exponential backoff)로 재시도합니다.

### 기본 사용법

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

### 옵션

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

에이전트 호출에 타임아웃을 적용합니다.

### 기본 사용법

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

### 옵션

| 옵션 | 기본값 | 설명 |
|------|--------|------|
| `Timeout` | 30초 | 타임아웃 시간 |
| `OnTimeout` | null | 타임아웃 발생 시 콜백 |

### 예외

타임아웃 발생 시 `TimeoutException`이 throw됩니다.

---

## RateLimitMiddleware

슬라이딩 윈도우 방식으로 API 호출 빈도를 제한합니다.

### 기본 사용법

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

### 옵션

| 옵션 | 기본값 | 설명 |
|------|--------|------|
| `MaxRequests` | 60 | 윈도우 내 최대 요청 수 |
| `Window` | 1분 | 슬라이딩 윈도우 시간 |
| `OnRateLimited` | null | Rate limit 대기 시 콜백 |

### 속성

- `CurrentRequestCount`: 현재 윈도우 내 요청 수

---

## CircuitBreakerMiddleware

연속 실패 시 회로를 차단하여 시스템을 보호합니다. Circuit Breaker 패턴을 구현합니다.

### 상태 전이

```
Closed (정상) ──실패 임계값 도달──→ Open (차단)
    ↑                                  │
    │                                  │
    └───── 성공 ────── HalfOpen ←──── 차단 시간 경과
                       (테스트)
```

| 상태 | 설명 |
|------|------|
| `Closed` | 정상 상태. 요청 허용 |
| `Open` | 차단 상태. 모든 요청 즉시 거부 |
| `HalfOpen` | 테스트 상태. 한 번의 요청으로 복구 여부 결정 |

### 기본 사용법

```csharp
// 간단한 사용 (5회 실패, 30초 차단)
var agent = baseAgent.WithMiddleware(new CircuitBreakerMiddleware(5, TimeSpan.FromSeconds(30)));

// 상세 설정
var circuitBreaker = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
{
    FailureThreshold = 5,
    FailureWindow = TimeSpan.FromMinutes(1),
    BreakDuration = TimeSpan.FromSeconds(30),
    OnBreak = (name, ex, duration) =>
        Console.WriteLine($"Circuit opened for {duration.TotalSeconds}s"),
    OnStateChanged = (oldState, newState) =>
        Console.WriteLine($"Circuit: {oldState} → {newState}")
});

var agent = baseAgent.WithMiddleware(circuitBreaker);

// 상태 확인
Console.WriteLine($"Current state: {circuitBreaker.State}");

// 수동 리셋
circuitBreaker.Reset();
```

### 옵션

| 옵션 | 기본값 | 설명 |
|------|--------|------|
| `FailureThreshold` | 5 | 회로 Open에 필요한 연속 실패 횟수 |
| `FailureWindow` | 1분 | 실패 카운트가 유지되는 윈도우 |
| `BreakDuration` | 30초 | 회로 Open 유지 시간 |
| `OnBreak` | null | 회로 Open 시 콜백 |
| `OnStateChanged` | null | 상태 변경 시 콜백 |
| `OnRejected` | null | 요청 거부 시 콜백 |

### 예외

회로가 Open 상태일 때 `CircuitBreakerOpenException`이 throw됩니다.

---

## BulkheadMiddleware

격리된 실행 풀로 동시 실행 수를 제한합니다. Bulkhead 패턴을 구현합니다.

### 기본 사용법

```csharp
// 동시 실행 5개 제한
var agent = baseAgent.WithMiddleware(new BulkheadMiddleware(5));

// 대기열 포함
var agent = baseAgent.WithMiddleware(new BulkheadMiddleware(5, maxQueueSize: 10));

// 상세 설정
var bulkhead = new BulkheadMiddleware(new BulkheadMiddlewareOptions
{
    MaxConcurrency = 5,
    MaxQueueSize = 10,
    QueueTimeout = TimeSpan.FromSeconds(30),
    OnQueued = (name, executing, queued) =>
        Console.WriteLine($"Queued. Executing: {executing}, Queued: {queued}"),
    OnRejected = (name, executing, queued) =>
        Console.WriteLine($"Rejected! Queue full.")
});

var agent = baseAgent.WithMiddleware(bulkhead);

// 상태 확인
Console.WriteLine($"Executing: {bulkhead.CurrentExecuting}");
Console.WriteLine($"Queued: {bulkhead.CurrentQueued}");
Console.WriteLine($"Available: {bulkhead.AvailableSlots}");
```

### 옵션

| 옵션 | 기본값 | 설명 |
|------|--------|------|
| `MaxConcurrency` | 10 | 최대 동시 실행 수 |
| `MaxQueueSize` | 0 (무제한) | 대기열 최대 크기 |
| `QueueTimeout` | 무한대 | 대기열 타임아웃 |
| `OnQueued` | null | 대기열 진입 시 콜백 |
| `OnRejected` | null | 요청 거부 시 콜백 |

### 예외

대기열이 가득 차거나 타임아웃 시 `BulkheadRejectedException`이 throw됩니다.

---

## CachingMiddleware

동일한 입력에 대한 응답을 캐싱합니다.

> **주의**: LLM 응답은 비결정적이므로 캐싱 사용 시 주의가 필요합니다.

### 기본 사용법

```csharp
// 5분 캐시
var agent = baseAgent.WithMiddleware(new CachingMiddleware(TimeSpan.FromMinutes(5)));

// 상세 설정
var caching = new CachingMiddleware(new CachingMiddlewareOptions
{
    Expiration = TimeSpan.FromMinutes(5),
    MaxCacheSize = 1000,
    IncludeInstructionsInKey = true,
    OnCacheHit = (name, key) => Console.WriteLine("Cache hit!"),
    OnCacheMiss = (name, key) => Console.WriteLine("Cache miss")
});

var agent = baseAgent.WithMiddleware(caching);

// 캐시 정보
Console.WriteLine($"Cached items: {caching.CacheCount}");

// 캐시 클리어
caching.ClearCache();
```

### 옵션

| 옵션 | 기본값 | 설명 |
|------|--------|------|
| `Expiration` | 5분 | 캐시 만료 시간 (`TimeSpan.Zero`면 만료 없음) |
| `MaxCacheSize` | 1000 | 최대 캐시 항목 수 |
| `IncludeInstructionsInKey` | true | Instructions를 캐시 키에 포함 |
| `OnCacheHit` | null | 캐시 히트 시 콜백 |
| `OnCacheMiss` | null | 캐시 미스 시 콜백 |

### 캐시 키 구성

캐시 키는 다음 요소로 구성됩니다:
- 에이전트 이름
- 모델 ID
- Instructions (옵션)
- 메시지 내용 (역할 + 텍스트)

---

## LoggingMiddleware

에이전트 호출을 로깅합니다. 스트리밍과 비스트리밍 모두 지원합니다.

### 기본 사용법

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

### 출력 예시

```
[MyAgent] Agent 'Assistant' starting (invoke) (messages: 3, model: gpt-4o-mini)
  Last message: 안녕하세요, 오늘 날씨가 어떤가요?
[MyAgent] Agent 'Assistant' completed (1234ms, tokens: 50+150)
  Response: 안녕하세요! 오늘 서울의 날씨는...
```

### 옵션

| 옵션 | 기본값 | 설명 |
|------|--------|------|
| `LogAction` | null | 로그 출력 함수 |
| `LogPrefix` | "Agent" | 로그 접두사 |
| `IncludeMessagePreview` | true | 입력 메시지 미리보기 포함 |
| `IncludeResponsePreview` | true | 응답 미리보기 포함 |
| `MaxPreviewLength` | 100 | 미리보기 최대 길이 |

---

## FallbackMiddleware

에이전트 실패 시 대체 에이전트로 폴백합니다.

### 기본 사용법

```csharp
// 간단한 사용
var agent = primaryAgent.WithMiddleware(new FallbackMiddleware(fallbackAgent));

// 동적 폴백
var agent = primaryAgent.WithMiddleware(new FallbackMiddleware(new FallbackMiddlewareOptions
{
    FallbackFactory = primary =>
    {
        // primary 에이전트 정보를 참조하여 적절한 폴백 선택
        return new BasicAgent(messageService)
        {
            Provider = "ollama",  // 로컬 폴백
            Model = "llama3",
            Name = $"{primary.Name}-fallback"
        };
    },
    ShouldFallback = ex => ex is HttpRequestException,  // HTTP 오류만 폴백
    ResponseValidator = response => response.Message != null,  // 응답 검증
    OnFallback = (name, ex, reason) =>
        Console.WriteLine($"Falling back from {name}: {reason}")
}));
```

### 옵션

| 옵션 | 기본값 | 설명 |
|------|--------|------|
| `FallbackAgent` | null | 대체 에이전트 |
| `FallbackFactory` | null | 대체 에이전트 동적 생성 함수 |
| `ShouldFallback` | null (모든 예외) | 폴백 조건 함수 |
| `ResponseValidator` | null | 응답 검증 함수 (false면 폴백) |
| `OnFallback` | null | 폴백 발생 시 콜백 |
| `OnFallbackFailed` | null | 폴백도 실패 시 콜백 |

> `FallbackAgent` 또는 `FallbackFactory` 중 하나는 반드시 제공해야 합니다.

### 예외

폴백 에이전트도 실패하면 `FallbackFailedException`이 throw됩니다.

---

## CompositeMiddleware

여러 미들웨어를 하나로 조합하여 재사용 가능한 팩을 생성합니다.

### 기본 사용법

```csharp
// 커스텀 팩 생성
var myPack = new CompositeMiddleware("my-pack",
    new LoggingMiddleware(Console.WriteLine),
    new RetryMiddleware(3),
    new TimeoutMiddleware(TimeSpan.FromSeconds(30)));

var agent = baseAgent.WithMiddleware(myPack);

// 정보 확인
Console.WriteLine($"Pack name: {myPack.Name}");
Console.WriteLine($"Middleware count: {myPack.Count}");
```

### 불변 확장

```csharp
// 새 미들웨어 추가 (뒤에)
var extended = myPack.With(new CachingMiddleware(TimeSpan.FromMinutes(5)));

// 새 미들웨어 추가 (앞에)
var prepended = myPack.Prepend(new BulkheadMiddleware(10));

// 원본은 변경되지 않음
Console.WriteLine(myPack.Count);      // 3
Console.WriteLine(extended.Count);    // 4
Console.WriteLine(prepended.Count);   // 4
```

### 속성

| 속성 | 설명 |
|------|------|
| `Name` | 팩 이름 |
| `Count` | 포함된 미들웨어 수 |
| `Middlewares` | 미들웨어 목록 (읽기 전용) |

---

## MiddlewarePacks

자주 사용되는 미들웨어 조합을 미리 정의한 팩토리입니다.

### Resilience

기본 복원력: 재시도 + 타임아웃

```csharp
var pack = MiddlewarePacks.Resilience(
    maxRetries: 3,
    timeout: TimeSpan.FromSeconds(30),
    retryDelay: TimeSpan.FromMilliseconds(500));

var agent = baseAgent.WithMiddleware(pack);
```

### AdvancedResilience

고급 복원력: 회로 차단기 + 재시도 + 타임아웃

```csharp
var pack = MiddlewarePacks.AdvancedResilience(
    maxRetries: 3,
    timeout: TimeSpan.FromSeconds(30),
    circuitBreakerThreshold: 5,
    breakDuration: TimeSpan.FromSeconds(30));
```

### Observability

관측성: 로깅

```csharp
var pack = MiddlewarePacks.Observability(Console.WriteLine);
```

### ResourceProtection

리소스 보호: Bulkhead + Rate Limit

```csharp
var pack = MiddlewarePacks.ResourceProtection(
    maxConcurrency: 10,
    maxRequestsPerMinute: 60);
```

### Production

프로덕션 환경용: 로깅 + Rate Limit + 회로 차단기 + 재시도 + 타임아웃

```csharp
var pack = MiddlewarePacks.Production(
    maxRetries: 3,
    timeout: TimeSpan.FromSeconds(30),
    circuitBreakerThreshold: 5,
    maxRequestsPerMinute: 60,
    logAction: Console.WriteLine);

var agent = baseAgent.WithMiddleware(pack);
```

### 팩 구성 비교

| 팩 | 로깅 | Rate Limit | Bulkhead | 회로 차단기 | 재시도 | 타임아웃 |
|------|:---:|:---:|:---:|:---:|:---:|:---:|
| Resilience | | | | | ✓ | ✓ |
| AdvancedResilience | | | | ✓ | ✓ | ✓ |
| Observability | ✓ | | | | | |
| ResourceProtection | | ✓ | ✓ | | | |
| Production | ✓ | ✓ | | ✓ | ✓ | ✓ |

---

## 스트리밍 지원

`IStreamingAgentMiddleware` 인터페이스를 구현한 미들웨어만 스트리밍을 지원합니다.

현재 스트리밍 지원 미들웨어:
- `LoggingMiddleware`
- `CompositeMiddleware` (스트리밍 미들웨어 자동 필터링)

스트리밍을 지원하지 않는 미들웨어는 스트리밍 호출 시 건너뜁니다.

---

## 커스텀 미들웨어 구현

### IAgentMiddleware

```csharp
public class MyMiddleware : IAgentMiddleware
{
    public async Task<MessageResponse> InvokeAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        Func<IEnumerable<Message>, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        // 전처리
        Console.WriteLine("Before agent call");

        // 다음 미들웨어 또는 에이전트 호출
        var response = await next(messages);

        // 후처리
        Console.WriteLine("After agent call");

        return response;
    }
}
```

### IStreamingAgentMiddleware

```csharp
public class MyStreamingMiddleware : IAgentMiddleware, IStreamingAgentMiddleware
{
    public Task<MessageResponse> InvokeAsync(...) { ... }

    public async IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        Func<IEnumerable<Message>, IAsyncEnumerable<StreamingMessageResponse>> next,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Streaming started");

        await foreach (var chunk in next(messages).WithCancellation(cancellationToken))
        {
            // 청크 처리
            yield return chunk;
        }

        Console.WriteLine("Streaming completed");
    }
}
```

---

## 관련 문서

- [AGENTS.md](AGENTS.md) - 에이전트 시스템 개요
- [ORCHESTRATION.md](ORCHESTRATION.md) - 오케스트레이션 패턴
