# Multi-Agent Cycle 5: Middleware 확장

## 범위

RetryMiddleware와 LoggingMiddleware 구현 — 실용적 가치 높은 미들웨어 확장

## 신규 구현체

### RetryMiddleware

| 파일 | 설명 |
|------|------|
| `src/IronHive.Core/Agent/RetryMiddleware.cs` | 지수 백오프 기반 재시도 미들웨어 |

**주요 기능:**
- 지수 백오프 (exponential backoff) 재시도
- 지터 (jitter) 적용으로 thundering herd 방지
- 재시도 가능 여부 커스터마이징 (`ShouldRetry` predicate)
- 콜백 훅: `OnRetry`, `OnRetryFailed`
- 취소 토큰 존중 (cancellation-aware)

**설정 옵션:**
```csharp
new RetryMiddlewareOptions
{
    MaxRetries = 3,
    InitialDelay = TimeSpan.FromSeconds(1),
    MaxDelay = TimeSpan.FromSeconds(30),
    BackoffMultiplier = 2.0,
    JitterFactor = 0.2,
    ShouldRetry = ex => ex is not ArgumentException,
    OnRetry = (name, attempt, ex, delay) => Console.WriteLine($"Retry {attempt}"),
    OnRetryFailed = (name, attempts, ex) => Console.WriteLine("All retries failed")
}
```

### LoggingMiddleware

| 파일 | 설명 |
|------|------|
| `src/IronHive.Core/Agent/LoggingMiddleware.cs` | 에이전트 호출 로깅 미들웨어 |

**주요 기능:**
- 시작/완료/실패 로깅
- 토큰 사용량 로깅
- 메시지/응답 미리보기
- 소요 시간 측정
- `Action<string>` 기반 유연한 출력

**설정 옵션:**
```csharp
new LoggingMiddlewareOptions
{
    LogAction = Console.WriteLine,
    LogPrefix = "Agent",
    IncludeMessagePreview = true,
    IncludeResponsePreview = true,
    MaxPreviewLength = 100
}
```

## 사용 예시

```csharp
// 개별 에이전트에 적용
var agent = baseAgent.WithMiddleware(
    new LoggingMiddleware(Console.WriteLine),
    new RetryMiddleware(3)
);

// 오케스트레이터 레벨에서 적용 (모든 에이전트에 자동 적용)
var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
{
    AgentMiddlewares = [
        new LoggingMiddleware(Console.WriteLine),
        new RetryMiddleware(new RetryMiddlewareOptions { MaxRetries = 2 })
    ]
});
```

## 테스트 결과

### CI 테스트 (tests/IronHive.Tests/)

| 카테고리 | 결과 |
|----------|------|
| 기존 Middleware 테스트 | 5 PASS |
| 신규 RetryMiddleware 테스트 | 5 PASS |
| 신규 LoggingMiddleware 테스트 | 5 PASS |
| **전체** | 168 PASS |

### 로컬 테스트 (tests/multi-agent-test/)

| 카테고리 | 결과 |
|----------|------|
| Mock 테스트 (기존) | 45 PASS |
| 신규 Middleware 테스트 | 6 PASS |
| LLM 통합 테스트 | 4 PASS, 1 FAIL* |
| **합계** | 55 PASS, 1 FAIL |

*`llm-streaming-handoff` 실패는 LLM 비결정성 문제 (기존 P2 이슈)

### 신규 테스트 목록

**CI 테스트:**
- `RetryMiddleware_ShouldSucceedOnFirstAttempt`
- `RetryMiddleware_ShouldRetryOnFailure`
- `RetryMiddleware_ShouldThrowAfterMaxRetries`
- `RetryMiddleware_ShouldRespectShouldRetryPredicate`
- `RetryMiddleware_ShouldNotRetryOnCancellation`
- `LoggingMiddleware_ShouldLogStartAndComplete`
- `LoggingMiddleware_ShouldLogError`
- `LoggingMiddleware_ShouldIncludeTokenUsage`
- `LoggingMiddleware_ShouldRespectOptions`
- `LoggingMiddleware_WithNullLogAction_ShouldNotThrow`

**로컬 테스트:**
- `retry-first-success`
- `retry-on-failure`
- `retry-max-exceeded`
- `logging-basic`
- `logging-error`
- `retry-logging-combined`

## 검증

```bash
# 빌드: 0 Error
dotnet build IronHive.slnx

# CI 테스트: 168 PASS
dotnet test tests/IronHive.Tests/IronHive.Tests.csproj

# 로컬 테스트: 55 PASS
dotnet run --project tests/multi-agent-test/MultiAgentTest.csproj
```

## 변경된 파일

| 파일 | 변경 내용 |
|------|----------|
| `src/IronHive.Core/Agent/RetryMiddleware.cs` | 신규 |
| `src/IronHive.Core/Agent/LoggingMiddleware.cs` | 신규 |
| `tests/IronHive.Tests/Agent/Orchestration/MiddlewareTests.cs` | 10개 테스트 추가 |
| `tests/multi-agent-test/Program.cs` | 6개 테스트 추가, FailAfterMiddleware 헬퍼 |

## 설계 결정

### ILogger 대신 Action<string> 사용

- **이유**: Microsoft.Extensions.Logging 의존성 추가 없이 유연성 제공
- **장점**: 사용자가 원하는 로깅 시스템에 쉽게 연결 가능
- **대안**: 향후 ILogger 어댑터 추가 고려

### 스트리밍 미지원

- **현재 상태**: MiddlewareAgent가 스트리밍은 inner에 직접 위임
- **이유**: v0.x에서 스트리밍 미들웨어 설계 확정 필요
- **영향**: RetryMiddleware, LoggingMiddleware 모두 InvokeAsync만 인터셉트

## 잔여 이슈

### P2: LLM Handoff JSON 출력 불확실성 (기존)

- `llm-streaming-handoff` 테스트 변동성의 원인
- 가능한 개선: Structured output, Few-shot examples, 재시도

### P3: 스트리밍 미들웨어 설계 (신규)

- 현재 스트리밍은 미들웨어 바이패스
- 스트리밍 + retry 조합 설계 필요

## 다음 사이클 제안

1. **스트리밍 미들웨어 지원** — `IAsyncEnumerable` 인터셉션 설계
2. **CachingMiddleware** — 동일 입력에 대한 응답 캐싱
3. **P2 Handoff JSON 개선** — 더 명확한 프롬프트, 재시도
4. **Cycle 1/2 잔여 항목** — HubSpoke, checkpoint resume 등

## 프로젝트 철학 정렬

- **커스텀 자유도 최대화**: Action<string> 기반으로 어떤 로깅 시스템과도 통합 가능
- **v0.x 과감한 리팩토링**: 스트리밍 미지원은 명시적 결정, 향후 Breaking Change 가능
- **실용적 가치**: RetryMiddleware와 LoggingMiddleware는 실제 프로덕션에서 즉시 활용 가능
