# 오케스트레이션

멀티에이전트 오케스트레이션 패턴에 대한 상세 문서입니다.

## 개요

```
IAgentOrchestrator (인터페이스)
    │
    ├── SequentialOrchestrator   # 순차 실행
    ├── ParallelOrchestrator     # 병렬 실행
    ├── HandoffOrchestrator      # 에이전트 간 전달
    ├── GroupChatOrchestrator    # 그룹 토론
    ├── HubSpokeOrchestrator     # 허브 중심 분배
    └── GraphOrchestrator        # DAG 기반 조건부 실행
```

### IAgentOrchestrator 인터페이스

```csharp
public interface IAgentOrchestrator
{
    string Name { get; }
    IReadOnlyList<IAgent> Agents { get; }
    bool SupportsRealTimeStreaming { get; }   // 실시간 스트리밍 지원 여부

    void AddAgent(IAgent agent);
    void AddAgents(IEnumerable<IAgent> agents);

    Task<OrchestrationResult> ExecuteAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<OrchestrationStreamEvent> ExecuteStreamingAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default);
}
```

---

## OrchestrationResult

```csharp
public sealed class OrchestrationResult
{
    public bool IsSuccess { get; init; }
    public Message? FinalOutput { get; init; }         // 최종 출력
    public IReadOnlyList<AgentStepResult> Steps { get; init; }  // 단계별 결과
    public TimeSpan TotalDuration { get; init; }
    public TokenUsageSummary? TokenUsage { get; init; }
    public string? Error { get; init; }
}

public sealed class AgentStepResult
{
    public required string AgentName { get; init; }
    public IReadOnlyList<Message> Input { get; init; }
    public MessageResponse? Response { get; init; }
    public TimeSpan Duration { get; init; }
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
}
```

---

## 공통 옵션

모든 오케스트레이터는 `OrchestratorOptions`를 기반으로 합니다:

```csharp
public class OrchestratorOptions
{
    public string? Name { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);      // 전체 타임아웃
    public TimeSpan AgentTimeout { get; set; } = TimeSpan.FromMinutes(2); // 개별 에이전트 타임아웃
    public bool StopOnAgentFailure { get; set; } = true;
    public ICheckpointStore? CheckpointStore { get; set; }
    public string? OrchestrationId { get; set; }
    public Func<string, AgentStepResult?, Task<bool>>? ApprovalHandler { get; set; }
    public HashSet<string>? RequireApprovalForAgents { get; set; }
    public IList<IAgentMiddleware>? AgentMiddlewares { get; set; }
    public IContextScope? ContextScope { get; set; }
    public IResultDistiller? ResultDistiller { get; set; }
}
```

---

## SequentialOrchestrator

에이전트를 순서대로 실행합니다. 이전 에이전트 출력이 다음 에이전트의 입력이 됩니다.
`SupportsRealTimeStreaming = true`.

```csharp
var options = new SequentialOrchestratorOptions
{
    PassOutputAsInput = true,    // 이전 출력 → 다음 입력 (기본값: true)
    AccumulateHistory = false,   // 모든 메시지 히스토리 누적 (기본값: false)
};

var orch = new SequentialOrchestrator(options);
orch.AddAgent(translatorAgent);
orch.AddAgent(summarizerAgent);
orch.AddAgent(formatterAgent);

var result = await orch.ExecuteAsync(messages);
// result.FinalOutput: 마지막 에이전트의 출력
// result.Steps: 각 에이전트 실행 결과
```

---

## ParallelOrchestrator

여러 에이전트를 동시에 같은 입력으로 실행합니다.

```csharp
var options = new ParallelOrchestratorOptions
{
    MaxConcurrency = null,  // null이면 무제한 (기본값)
    ResultAggregation = ParallelResultAggregation.All,  // 기본값
    RequireAllSuccess = false,
};

var orch = new ParallelOrchestrator(options);
orch.AddAgent(analystA);
orch.AddAgent(analystB);
orch.AddAgent(analystC);

var result = await orch.ExecuteAsync(messages);
```

### ParallelResultAggregation 옵션

| 값 | 설명 |
|----|------|
| `All` | 모든 결과를 개별 Steps으로 반환 (기본값) |
| `FirstSuccess` | 첫 번째 성공 결과만 반환 |
| `Fastest` | 가장 빨리 완료된 결과 반환 |
| `Merge` | 모든 결과를 하나의 메시지로 병합 |

---

## HandoffOrchestrator

에이전트가 JSON 형식으로 핸드오프를 요청하면 다른 에이전트로 전환합니다.

```csharp
var builder = new HandoffOrchestratorBuilder();
builder
    .AddAgent(triageAgent,
        new HandoffTarget { AgentName = "billing", Condition = "결제 관련 문의" },
        new HandoffTarget { AgentName = "support", Condition = "기술 지원 문의" })
    .AddAgent(billingAgent,
        new HandoffTarget { AgentName = "triage", Condition = "다른 문의로 돌아가기" })
    .AddAgent(supportAgent,
        new HandoffTarget { AgentName = "triage", Condition = "다른 문의로 돌아가기" })
    .SetInitialAgent("triage")
    .SetMaxTransitions(20);

var orch = builder.Build();
```

### 핸드오프 JSON 형식

에이전트가 다음 형식의 JSON을 출력하면 핸드오프가 발생합니다:

```json
{"handoff_to": "billing", "context": "고객이 결제 문의를 원합니다"}
```

지원되는 키: `handoff_to`, `transfer_to`, `delegate_to`, `agent` / `context`, `message`, `reason`

### HandoffOrchestratorOptions

```csharp
public class HandoffOrchestratorOptions : OrchestratorOptions
{
    public required string InitialAgentName { get; set; }
    public int MaxTransitions { get; set; } = 20;
    public Func<string, AgentStepResult, Task<Message?>>? NoHandoffHandler { get; set; }
}
```

---

## GroupChatOrchestrator

여러 에이전트가 그룹 대화에 참여합니다. `ISpeakerSelector`가 다음 발언자를 결정하고, `ITerminationCondition`이 종료를 결정합니다.

```csharp
var builder = new GroupChatOrchestratorBuilder();
builder
    .AddAgent(expertA)
    .AddAgent(expertB)
    .AddAgent(moderator)
    .WithRoundRobin()                          // 순환 발언
    // 또는: .WithLlmSpeakerSelection(selector)  // LLM 기반 선택
    .TerminateAfterRounds(10)                  // 최대 10라운드
    // 또는: .TerminateOnKeyword("APPROVED")    // 키워드 종료
    .SetMaxRounds(50);                         // 안전 상한선 (기본값: 50)

var orch = builder.Build();
```

### GroupChatOrchestratorOptions

```csharp
public class GroupChatOrchestratorOptions : OrchestratorOptions
{
    public required ISpeakerSelector SpeakerSelector { get; set; }
    public required ITerminationCondition TerminationCondition { get; set; }
    public int MaxRounds { get; set; } = 50;
}
```

### 발언자 선택 전략

```csharp
// 순환 (Round Robin)
.WithRoundRobin()

// LLM 기반 선택
.WithLlmSpeakerSelection(selectorAgent)

// 랜덤 선택
.WithRandom()
```

### 종료 조건

```csharp
// 키워드 종료
.TerminateOnKeyword("APPROVED")

// 최대 라운드 종료
.TerminateAfterRounds(10)

// 복합 조건 (OR — 하나라도 만족하면 종료)
var condition = new CompositeTermination(requireAll: false,
    new KeywordTermination("DONE"),
    new MaxRoundsTermination(5));

// 복합 조건 (AND — 모두 만족해야 종료)
var condition = new CompositeTermination(requireAll: true,
    new KeywordTermination("APPROVED"),
    new KeywordTermination("VERIFIED"));
```

---

## HubSpokeOrchestrator

중앙 허브 에이전트가 작업을 분석하고 스포크 에이전트에게 위임합니다.

```csharp
var options = new HubSpokeOrchestratorOptions
{
    MaxRounds = 10,           // 기본값: 10
    ParallelSpokes = false,   // 스포크 병렬 실행 여부
    MaxConcurrentSpokes = null,
};

var orch = new HubSpokeOrchestrator(options);
orch.AddHub(coordinatorAgent);
orch.AddSpoke(researchAgent);
orch.AddSpoke(writerAgent);
orch.AddSpoke(reviewerAgent);
```

작동 방식:
1. Hub가 사용자 요청을 분석
2. Spoke 에이전트에 작업 위임 (JSON 형식)
3. Spoke가 작업 수행 후 결과 반환
4. Hub가 결과를 종합하여 최종 응답 생성

---

## GraphOrchestrator

DAG(Directed Acyclic Graph) 기반으로 조건부 분기를 포함한 복잡한 워크플로우를 구성합니다.
`SupportsRealTimeStreaming = true`.

```csharp
var builder = new GraphOrchestratorBuilder();
builder
    .AddNode("start", startAgent)
    .AddNode("billing", billingAgent)
    .AddNode("general", generalAgent)
    .AddNode("merge", mergeAgent)
    // 조건부 엣지: ctx.LastOutput 기반
    .AddEdge("start", "billing", ctx => ctx.LastOutput.Contains("billing"))
    .AddEdge("start", "general")
    // Fan-In
    .AddEdge("billing", "merge")
    .AddEdge("general", "merge")
    .SetStartNode("start")
    .SetOutputNode("merge");

var orch = builder.Build();
```

### Fan-Out / Fan-In 패턴

```
      A
     / \
    B   C      — Fan-Out
     \ /
      D        — Fan-In

builder
    .AddNode("A", agentA).AddNode("B", agentB)
    .AddNode("C", agentC).AddNode("D", agentD)
    .AddEdge("A", "B").AddEdge("A", "C")
    .AddEdge("B", "D").AddEdge("C", "D")
    .SetStartNode("A");
```

---

## 스트리밍

```csharp
await foreach (var evt in orch.ExecuteStreamingAsync(messages))
{
    switch (evt.EventType)
    {
        case OrchestrationEventType.AgentStarted:
            Console.WriteLine($"시작: {evt.AgentName}");
            break;
        case OrchestrationEventType.MessageDelta:
            if (evt.StreamingResponse is StreamingContentDeltaResponse delta
                && delta.Delta is TextDeltaContent text)
                Console.Write(text.Value);
            break;
        case OrchestrationEventType.AgentCompleted:
            Console.WriteLine($"\n완료: {evt.AgentName}");
            break;
        case OrchestrationEventType.Handoff:
            Console.WriteLine($"핸드오프: {evt.AgentName} → ...");
            break;
        case OrchestrationEventType.Completed:
            Console.WriteLine($"전체 완료: {evt.Result?.IsSuccess}");
            break;
        case OrchestrationEventType.Failed:
            Console.WriteLine($"실패: {evt.Error}");
            break;
    }
}
```

### OrchestrationEventType 전체 목록

| 이벤트 | 설명 |
|--------|------|
| `Started` | 오케스트레이션 시작 |
| `AgentStarted` | 에이전트 실행 시작 |
| `MessageDelta` | 에이전트 스트리밍 청크 |
| `AgentCompleted` | 에이전트 실행 완료 |
| `AgentFailed` | 에이전트 실행 실패 |
| `Handoff` | 핸드오프 발생 |
| `SpeakerSelected` | GroupChat 발언자 선택 |
| `ApprovalRequired` | 승인 대기 |
| `ApprovalGranted` | 승인됨 |
| `ApprovalDenied` | 승인 거부됨 |
| `HumanInputRequired` | 사람 입력 필요 |
| `Completed` | 오케스트레이션 완료 |
| `Failed` | 오케스트레이션 실패 |

---

## 체크포인트 & 재개

```csharp
// 인메모리 체크포인트 (테스트용)
var store = new InMemoryCheckpointStore();

// 파일 기반 체크포인트 (영속성)
var store = new FileCheckpointStore("./checkpoints");

var options = new SequentialOrchestratorOptions
{
    CheckpointStore = store,
    OrchestrationId = "session-user-123"  // null이면 자동 생성
};
var orch = new SequentialOrchestrator(options);
// 중단 후 같은 OrchestrationId로 재실행하면 체크포인트에서 재개
```

---

## Human-in-the-Loop (HITL)

```csharp
var options = new SequentialOrchestratorOptions
{
    ApprovalHandler = async (agentName, previousStep) =>
    {
        Console.WriteLine($"에이전트 '{agentName}' 실행을 승인하시겠습니까? (y/n)");
        return Console.ReadLine() == "y";
    },
    RequireApprovalForAgents = new HashSet<string> { "dangerous-agent" }  // null이면 모든 에이전트에 적용
};
```

---

## 컨텍스트 범위 (IContextScope)

서브에이전트에 전달할 메시지 범위를 제한합니다:

```csharp
// 마지막 N개 메시지만 전달
options.ContextScope = new LastNMessagesScope(n: 5);

// 요약 프롬프트로 컨텍스트 압축
options.ContextScope = new SummaryContextScope(summaryAgent);

// 현재 작업만 전달
options.ContextScope = new TaskOnlyScope();
```

---

## 오케스트레이터 중첩

```csharp
// 내부 병렬 오케스트레이터
var parallelReview = new ParallelOrchestrator();
parallelReview.AddAgent(reviewer1);
parallelReview.AddAgent(reviewer2);

// 오케스트레이터를 IAgent로 래핑
var reviewAgent = parallelReview.AsAgent(name: "ParallelReviewer");

// 외부 순차 오케스트레이터에 중첩
var pipeline = new SequentialOrchestrator();
pipeline.AddAgent(writer);
pipeline.AddAgent(reviewAgent);
pipeline.AddAgent(finalizer);
```

---

## TypedPipeline (타입 안전 파이프라인)

컴파일타임 타입 안전성을 보장하는 파이프라인:

```csharp
// ITypedExecutor<TIn, TOut> 체이닝
var pipeline = TypedPipeline
    .Start(analysisExecutor)    // ITypedExecutor<string, Analysis>
    .Then(summaryExecutor)      // ITypedExecutor<Analysis, Summary>
    .Build();                   // ITypedExecutor<string, Summary>

var result = await pipeline.ExecuteAsync("input text");
```

`AgentExecutor<TIn, TOut>`는 `IAgent`를 `ITypedExecutor`로 래핑합니다.

---

## 관련 문서

- [AGENTS.md](AGENTS.md) — 에이전트 시스템
- [MIDDLEWARE.md](MIDDLEWARE.md) — 미들웨어 시스템
