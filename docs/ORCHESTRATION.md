# 오케스트레이션

IronHive의 멀티에이전트 오케스트레이션 패턴에 대한 상세 문서입니다.

## 개요

오케스트레이터는 여러 에이전트를 조합하여 복잡한 워크플로우를 구성합니다.

```
IOrchestrator (인터페이스)
    │
    ├── SequentialOrchestrator   # 순차 실행
    ├── ParallelOrchestrator     # 병렬 실행
    ├── GraphOrchestrator        # DAG 기반 실행
    ├── HandoffOrchestrator      # 에이전트 간 전달
    ├── GroupChatOrchestrator    # 그룹 토론
    └── HubSpokeOrchestrator     # 허브 중심 분배
```

---

## SequentialOrchestrator

에이전트를 순서대로 실행하며, 이전 에이전트의 출력이 다음 에이전트의 컨텍스트가 됩니다.

### 기본 사용법

```csharp
var orch = new SequentialOrchestratorBuilder()
    .AddAgent(translatorAgent)
    .AddAgent(summarizerAgent)
    .AddAgent(formatterAgent)
    .Build();

var result = await orch.ExecuteAsync(new[]
{
    new UserMessage { Content = [new TextMessageContent { Value = "Hello world" }] }
});

// result.FinalOutput: 최종 에이전트의 출력
// result.Steps: 각 에이전트의 실행 결과
```

### 옵션

```csharp
var orch = new SequentialOrchestratorBuilder()
    .AddAgent(agent1)
    .AddAgent(agent2)
    .WithHistoryMode(HistoryAccumulationMode.Accumulate)  // 이전 메시지 누적
    .WithErrorHandling(ErrorHandlingMode.ContinueOnFailure)
    .WithCheckpointStore(checkpointStore)
    .Build();
```

### HistoryAccumulationMode

| 모드 | 설명 |
|------|------|
| `None` | 각 에이전트에 원본 메시지만 전달 |
| `Accumulate` | 이전 에이전트의 응답을 메시지에 누적 |
| `LastOnly` | 마지막 에이전트의 응답만 전달 |

---

## ParallelOrchestrator

여러 에이전트를 동시에 실행합니다.

### 기본 사용법

```csharp
var orch = new ParallelOrchestratorBuilder()
    .AddAgent(analystA)
    .AddAgent(analystB)
    .AddAgent(analystC)
    .Build();

var result = await orch.ExecuteAsync(messages);
// 모든 에이전트의 결과가 Steps에 포함됨
```

### 결과 집계

```csharp
var orch = new ParallelOrchestratorBuilder()
    .AddAgent(agent1)
    .AddAgent(agent2)
    .AddAgent(agent3)
    .WithResultAggregation(ParallelResultAggregation.Merge)  // 결과 병합
    .Build();
```

| 집계 방식 | 설명 |
|----------|------|
| `Merge` | 모든 결과를 하나로 병합 (기본값) |
| `FirstSuccess` | 첫 번째 성공한 결과만 반환 |
| `Fastest` | 가장 빨리 완료된 결과 반환 |

### 동시성 제한

```csharp
var orch = new ParallelOrchestratorBuilder()
    .AddAgent(agent1)
    .AddAgent(agent2)
    .AddAgent(agent3)
    .AddAgent(agent4)
    .WithMaxConcurrency(2)  // 동시 실행 2개로 제한
    .Build();
```

---

## GraphOrchestrator

DAG(Directed Acyclic Graph) 기반으로 조건부 분기를 포함한 복잡한 워크플로우를 구성합니다.

### 기본 사용법

```csharp
var orch = new GraphOrchestratorBuilder()
    .AddNode("start", startAgent)
    .AddNode("process", processAgent)
    .AddNode("review", reviewAgent)
    .AddEdge("start", "process")
    .AddEdge("process", "review")
    .SetStartNode("start")
    .Build();

var result = await orch.ExecuteAsync(messages);
```

### 조건부 엣지

```csharp
var orch = new GraphOrchestratorBuilder()
    .AddNode("router", routerAgent)
    .AddNode("path-a", pathAAgent)
    .AddNode("path-b", pathBAgent)
    .AddNode("merge", mergeAgent)
    .AddEdge("router", "path-a", ctx => ctx.LastOutput.Contains("A"))
    .AddEdge("router", "path-b", ctx => ctx.LastOutput.Contains("B"))
    .AddEdge("path-a", "merge")
    .AddEdge("path-b", "merge")
    .SetStartNode("router")
    .Build();
```

### 다이아몬드 패턴 (Fan-out/Fan-in)

```csharp
//     A
//    / \
//   B   C
//    \ /
//     D

var orch = new GraphOrchestratorBuilder()
    .AddNode("A", agentA)
    .AddNode("B", agentB)
    .AddNode("C", agentC)
    .AddNode("D", agentD)
    .AddEdge("A", "B")
    .AddEdge("A", "C")
    .AddEdge("B", "D")
    .AddEdge("C", "D")
    .SetStartNode("A")
    .Build();
```

---

## HandoffOrchestrator

에이전트 간에 대화를 전달합니다. 에이전트가 JSON 형식으로 핸드오프를 요청하면 다른 에이전트로 전환됩니다.

### 기본 사용법

```csharp
var orch = new HandoffOrchestratorBuilder()
    .AddAgent(triageAgent,
        new HandoffTarget { AgentName = "billing", Description = "결제 관련 문의" },
        new HandoffTarget { AgentName = "support", Description = "기술 지원 문의" })
    .AddAgent(billingAgent,
        new HandoffTarget { AgentName = "triage", Description = "다른 문의로 돌아가기" })
    .AddAgent(supportAgent,
        new HandoffTarget { AgentName = "triage", Description = "다른 문의로 돌아가기" })
    .SetInitialAgent("triage")
    .Build();
```

### 핸드오프 JSON 형식

에이전트가 다음 형식의 JSON을 출력하면 핸드오프가 발생합니다:

```json
{"handoff_to": "billing", "context": "고객이 결제 문의를 원합니다"}
```

지원되는 키:
- `handoff_to`, `transfer_to`, `delegate_to`, `agent`
- `context`, `message`, `reason`

### 핸드오프 제한

```csharp
var orch = new HandoffOrchestratorBuilder()
    .AddAgent(agent1, targets...)
    .AddAgent(agent2, targets...)
    .SetInitialAgent("agent1")
    .SetMaxTransitions(5)  // 최대 5회 핸드오프
    .Build();
```

### 핸드오프 핸들러

핸드오프가 발생하지 않을 때의 처리:

```csharp
var orch = new HandoffOrchestratorBuilder()
    .AddAgent(triageAgent, targets...)
    .SetInitialAgent("triage")
    .OnNoHandoff((agent, response) =>
    {
        Console.WriteLine($"{agent.Name}가 핸드오프 없이 응답했습니다.");
    })
    .Build();
```

---

## GroupChatOrchestrator

여러 에이전트가 그룹 대화에 참여합니다. 발화자 선택 전략에 따라 다음 발화자가 결정됩니다.

### 기본 사용법

```csharp
var orch = new GroupChatOrchestratorBuilder()
    .AddAgent(expertA)
    .AddAgent(expertB)
    .AddAgent(moderator)
    .WithRoundRobin()  // 순환 발화
    .SetMaxRounds(6)
    .Build();
```

### 발화자 선택 전략

```csharp
// 순환 (Round Robin)
.WithRoundRobin()

// LLM 기반 선택
.WithLlmSpeakerSelection(selectorAgent)

// 커스텀 선택
.WithSpeakerSelector(async (agents, history) =>
{
    // 컨텍스트에 따라 다음 발화자 선택
    return agents.First(a => a.Name == "expert");
})
```

### 종료 조건

```csharp
// 키워드 종료
.WithTerminationCondition(new KeywordTermination("APPROVED"))

// 최대 라운드
.WithTerminationCondition(new MaxRoundsTermination(10))

// 복합 조건 (OR)
.WithTerminationCondition(new CompositeTermination(
    requireAll: false,  // 하나라도 만족하면 종료
    new KeywordTermination("DONE"),
    new MaxRoundsTermination(5)))

// 복합 조건 (AND)
.WithTerminationCondition(new CompositeTermination(
    requireAll: true,  // 모두 만족해야 종료
    new KeywordTermination("APPROVED"),
    new KeywordTermination("VERIFIED")))
```

---

## HubSpokeOrchestrator

중앙 허브 에이전트가 작업을 분석하고 스포크 에이전트들에게 분배합니다.

### 기본 사용법

```csharp
var orch = new HubSpokeOrchestratorBuilder()
    .SetHubAgent(coordinatorAgent)
    .AddSpokeAgent(researchAgent)
    .AddSpokeAgent(writerAgent)
    .AddSpokeAgent(reviewerAgent)
    .Build();
```

### 작동 방식

1. 허브가 사용자 요청을 분석
2. 적절한 스포크 에이전트에게 작업 위임 (JSON 형식)
3. 스포크가 작업 수행 후 결과 반환
4. 허브가 결과를 종합하여 최종 응답 생성

---

## 오케스트레이터 조합

오케스트레이터를 중첩하여 복잡한 워크플로우를 구성합니다.

### 예시: 순차 + 병렬

```csharp
// 내부 병렬 오케스트레이터
var parallelReview = new ParallelOrchestratorBuilder()
    .AddAgent(reviewer1)
    .AddAgent(reviewer2)
    .Build();

// 외부 순차 오케스트레이터
var pipeline = new SequentialOrchestratorBuilder()
    .AddAgent(writer)
    .AddAgent(new OrchestratorAgent(parallelReview))  // 오케스트레이터를 에이전트로 래핑
    .AddAgent(finalizer)
    .Build();
```

---

## 스트리밍

### 이벤트 수신

```csharp
await foreach (var evt in orch.ExecuteStreamingAsync(messages))
{
    Console.WriteLine($"[{evt.EventType}] Agent: {evt.AgentName}");

    if (evt.StreamingResponse != null)
    {
        Console.Write(evt.StreamingResponse.Delta);
    }
}
```

### 이벤트 타입

| 이벤트 | 발생 시점 |
|--------|----------|
| `OrchestrationStarted` | 오케스트레이션 시작 |
| `AgentStarted` | 에이전트 실행 시작 |
| `AgentStreaming` | 에이전트 스트리밍 청크 수신 |
| `AgentCompleted` | 에이전트 실행 완료 |
| `Handoff` | 핸드오프 발생 |
| `SpeakerSelected` | 그룹챗 발화자 선택 |
| `OrchestrationCompleted` | 오케스트레이션 완료 |
| `Error` | 에러 발생 |

---

## 체크포인트 & 복구

### 체크포인트 저장소

```csharp
// 인메모리 (테스트용)
var store = new InMemoryCheckpointStore();

// 커스텀 구현
public class RedisCheckpointStore : ICheckpointStore
{
    public Task SaveAsync(string sessionId, OrchestrationCheckpoint checkpoint, CancellationToken ct) { ... }
    public Task<OrchestrationCheckpoint?> LoadAsync(string sessionId, CancellationToken ct) { ... }
    public Task DeleteAsync(string sessionId, CancellationToken ct) { ... }
}
```

### 사용법

```csharp
var orch = new SequentialOrchestratorBuilder()
    .AddAgent(agent1)
    .AddAgent(agent2)
    .AddAgent(agent3)
    .WithCheckpointStore(store)
    .Build();

// 세션 ID로 실행 (체크포인트 활성화)
var result = await orch.ExecuteAsync(messages, new ExecuteOptions
{
    SessionId = "user-session-123"
});
```

---

## 미들웨어와 함께 사용

오케스트레이터 자체에 미들웨어를 적용하거나, 개별 에이전트에 적용할 수 있습니다.

### 오케스트레이터 미들웨어

```csharp
var orch = new SequentialOrchestratorBuilder()
    .AddAgent(agent1)
    .AddAgent(agent2)
    .WithMiddleware(new LoggingMiddleware())  // 전체 오케스트레이션에 적용
    .Build();
```

### 개별 에이전트 미들웨어

```csharp
var agent1WithRetry = agent1.WithMiddleware(new RetryMiddleware(maxRetries: 3));

var orch = new SequentialOrchestratorBuilder()
    .AddAgent(agent1WithRetry)
    .AddAgent(agent2)
    .Build();
```

---

## 관련 문서

- [AGENTS.md](AGENTS.md) - 에이전트 시스템 개요
- [MIDDLEWARE.md](MIDDLEWARE.md) - 미들웨어 시스템
