# 에이전트 시스템

IronHive의 에이전트 시스템은 LLM 기반의 자율적 대화 및 작업 수행을 위한 프레임워크입니다.

## 개요

```
IAgent (인터페이스)
    │
    ├── BasicAgent          # 기본 에이전트
    ├── MiddlewareAgent     # 미들웨어 래핑 에이전트
    └── Orchestrator        # 멀티에이전트 오케스트레이터
         ├── SequentialOrchestrator
         ├── ParallelOrchestrator
         ├── GraphOrchestrator
         ├── HandoffOrchestrator
         ├── GroupChatOrchestrator
         └── HubSpokeOrchestrator
```

## IAgent 인터페이스

모든 에이전트의 기본 인터페이스입니다.

```csharp
public interface IAgent
{
    string Provider { get; }           // 프로바이더 이름 (예: "openai")
    string Model { get; }              // 모델 ID (예: "gpt-4o-mini")
    string Name { get; }               // 에이전트 이름
    string Description { get; }        // 에이전트 설명
    string? Instructions { get; }      // 시스템 프롬프트
    IEnumerable<ToolItem>? Tools { get; }              // 사용 가능한 도구
    MessageGenerationParameters? Parameters { get; }   // 생성 파라미터

    Task<MessageResponse> InvokeAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default);
}
```

## BasicAgent

가장 기본적인 에이전트 구현체입니다.

### 생성

```csharp
// IMessageService 주입
var agent = new BasicAgent(messageService)
{
    Provider = "openai",
    Model = "gpt-4o-mini",
    Name = "Assistant",
    Description = "일반 도우미 에이전트",
    Instructions = "당신은 친절한 도우미입니다.",
    Parameters = new MessageGenerationParameters
    {
        MaxTokens = 4096,
        Temperature = 0.7f
    }
};
```

### 동기 호출

```csharp
var response = await agent.InvokeAsync(new[]
{
    new UserMessage { Content = [new TextMessageContent { Value = "안녕하세요" }] }
});

Console.WriteLine(response.Message?.Content?.FirstOrDefault());
```

### 스트리밍 호출

```csharp
await foreach (var chunk in agent.InvokeStreamingAsync(messages))
{
    switch (chunk)
    {
        case StreamingMessageBeginResponse begin:
            Console.WriteLine($"시작: {begin.Id}");
            break;
        case StreamingMessageDeltaResponse delta:
            Console.Write(delta.Delta);
            break;
        case StreamingMessageEndResponse end:
            Console.WriteLine($"\n완료: {end.DoneReason}");
            break;
    }
}
```

## 미들웨어 시스템

에이전트 호출을 가로채어 추가 로직을 적용합니다.

### 미들웨어 적용

```csharp
// 단일 미들웨어
var wrapped = agent.WithMiddleware(new RetryMiddleware(maxRetries: 3));

// 다중 미들웨어 (실행 순서: 왼쪽 → 오른쪽)
var wrapped = agent.WithMiddleware(
    new LoggingMiddleware(),
    new RetryMiddleware(maxRetries: 3),
    new TimeoutMiddleware(TimeSpan.FromSeconds(30))
);
```

### 미들웨어 종류

| 미들웨어 | 설명 |
|----------|------|
| `RetryMiddleware` | 실패 시 자동 재시도 (지수 백오프 지원) |
| `TimeoutMiddleware` | 호출 타임아웃 적용 |
| `RateLimitMiddleware` | 슬라이딩 윈도우 방식 호출 빈도 제한 |
| `CircuitBreakerMiddleware` | 연속 실패 시 회로 차단 |
| `BulkheadMiddleware` | 동시 실행 수 제한 |
| `CachingMiddleware` | 응답 캐싱 |
| `LoggingMiddleware` | 호출 로깅 |
| `FallbackMiddleware` | 실패 시 대체 에이전트로 폴백 |
| `CompositeMiddleware` | 여러 미들웨어를 하나로 조합 |

자세한 내용은 [MIDDLEWARE.md](MIDDLEWARE.md)를 참조하세요.

## 멀티에이전트 오케스트레이션

여러 에이전트를 조합하여 복잡한 워크플로우를 구성합니다.

### 오케스트레이터 종류

| 오케스트레이터 | 패턴 | 설명 |
|---------------|------|------|
| `SequentialOrchestrator` | 순차 | 에이전트를 순서대로 실행 |
| `ParallelOrchestrator` | 병렬 | 에이전트를 동시에 실행 |
| `GraphOrchestrator` | DAG | 조건부 분기가 있는 그래프 실행 |
| `HandoffOrchestrator` | 핸드오프 | 에이전트 간 대화 전달 |
| `GroupChatOrchestrator` | 그룹챗 | 여러 에이전트가 토론 |
| `HubSpokeOrchestrator` | 허브스포크 | 중앙 허브가 작업 분배 |

### 빠른 예제

```csharp
// 순차 오케스트레이터
var orch = new SequentialOrchestratorBuilder()
    .AddAgent(agent1)
    .AddAgent(agent2)
    .Build();

var result = await orch.ExecuteAsync(messages);

// 핸드오프 오케스트레이터
var orch = new HandoffOrchestratorBuilder()
    .AddAgent(triageAgent,
        new HandoffTarget { AgentName = "billing", Description = "결제 문의" },
        new HandoffTarget { AgentName = "support", Description = "기술 지원" })
    .AddAgent(billingAgent)
    .AddAgent(supportAgent)
    .SetInitialAgent("triage")
    .Build();
```

자세한 내용은 [ORCHESTRATION.md](ORCHESTRATION.md)를 참조하세요.

## 체크포인트

오케스트레이션 중간 상태를 저장하고 복구합니다.

### ICheckpointStore

```csharp
public interface ICheckpointStore
{
    Task SaveAsync(string sessionId, OrchestrationCheckpoint checkpoint, CancellationToken ct);
    Task<OrchestrationCheckpoint?> LoadAsync(string sessionId, CancellationToken ct);
    Task DeleteAsync(string sessionId, CancellationToken ct);
}
```

### 사용법

```csharp
var store = new InMemoryCheckpointStore();

var orch = new SequentialOrchestratorBuilder()
    .AddAgent(agent1)
    .AddAgent(agent2)
    .WithCheckpointStore(store)
    .Build();

// 체크포인트 저장 활성화된 실행
var result = await orch.ExecuteAsync(messages, new ExecuteOptions
{
    SessionId = "session-123"
});
```

## Human-in-the-Loop (HITL)

사람의 승인을 받아 에이전트 실행을 제어합니다.

### IApprovalHandler

```csharp
public interface IApprovalHandler
{
    Task<bool> RequestApprovalAsync(IAgent agent, IEnumerable<Message> messages, CancellationToken ct);
}
```

### 사용법

```csharp
var approvalHandler = new CustomApprovalHandler();

var orch = new SequentialOrchestratorBuilder()
    .AddAgent(safeAgent)
    .AddAgent(riskyAgent, requiresApproval: true)
    .WithApprovalHandler(approvalHandler)
    .Build();
```

## 스트리밍

오케스트레이션 이벤트를 실시간으로 수신합니다.

### 이벤트 타입

| 이벤트 | 설명 |
|--------|------|
| `OrchestrationStarted` | 오케스트레이션 시작 |
| `AgentStarted` | 에이전트 실행 시작 |
| `AgentStreaming` | 에이전트 스트리밍 청크 |
| `AgentCompleted` | 에이전트 실행 완료 |
| `Handoff` | 핸드오프 발생 |
| `SpeakerSelected` | 그룹챗 발화자 선택 |
| `OrchestrationCompleted` | 오케스트레이션 완료 |

### 사용법

```csharp
await foreach (var evt in orch.ExecuteStreamingAsync(messages))
{
    switch (evt.EventType)
    {
        case OrchestrationEventType.AgentStreaming:
            Console.Write(evt.StreamingResponse?.Delta);
            break;
        case OrchestrationEventType.Handoff:
            Console.WriteLine($"핸드오프: {evt.FromAgent} → {evt.ToAgent}");
            break;
    }
}
```

## 에러 처리

### ErrorHandlingMode

```csharp
public enum ErrorHandlingMode
{
    StopOnFailure,    // 실패 시 중단 (기본값)
    ContinueOnFailure // 실패해도 계속 진행
}
```

### 사용법

```csharp
var orch = new SequentialOrchestratorBuilder()
    .AddAgent(agent1)
    .AddAgent(agent2)
    .WithErrorHandling(ErrorHandlingMode.ContinueOnFailure)
    .Build();
```

## 관련 문서

- [ORCHESTRATION.md](ORCHESTRATION.md) - 오케스트레이션 패턴 상세
- [MIDDLEWARE.md](MIDDLEWARE.md) - 미들웨어 시스템 상세
- [CORE-COMPONENTS.md](CORE-COMPONENTS.md) - 핵심 컴포넌트
