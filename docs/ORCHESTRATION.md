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
    // Description은 프롬프트에 그대로 실려 모델이 전환 대상을 고르는 근거가 된다
    .AddAgent(triageAgent,
        new HandoffTarget { AgentName = "billing", Description = "결제 관련 문의" },
        new HandoffTarget { AgentName = "support", Description = "기술 지원 문의" })
    .AddAgent(billingAgent,
        new HandoffTarget { AgentName = "triage", Description = "다른 문의로 돌아가기" })
    .AddAgent(supportAgent,
        new HandoffTarget { AgentName = "triage", Description = "다른 문의로 돌아가기" })
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
    // 또는: .WithLlmManager(managerAgent)       // LLM 기반 선택
    .TerminateAfterRounds(10)                  // 최대 10라운드
    // 또는: .TerminateOnKeyword("APPROVED")    // 키워드 종료
    .SetMaxRounds(50);                         // 안전 상한선 (기본값: 50)

var orch = builder.Build();
```

두 빌더(Handoff · GroupChat)는 `OrchestratorOptions` 의 공통 옵션을 전부 `Set{옵션}` 으로 받는다 — `SetStopOnAgentFailure` ·
`SetAgentMiddlewares` · `SetContextScope` · `SetResultDistiller`/`SetResultDistillationOptions` · `SetApprovalHandler` ·
`SetCheckpointStore` 등(0.31.0 부터 — 그 전엔 앞의 다섯을 빌더로 줄 수 없었다). 실패한 스텝은 `TerminateAfterRounds` 에 세지 않으므로
`SetStopOnAgentFailure(false)` 로 계속 도는 대화는 `SetMaxRounds` 가 끝낸다.

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

// LLM 기반 선택 — 매니저 에이전트가 다음 발언자를 고른다
.WithLlmManager(managerAgent)

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
var allOf = new CompositeTermination(requireAll: true,
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
orch.SetHubAgent(coordinatorAgent);
orch.AddSpokeAgent(researchAgent);
orch.AddSpokeAgent(writerAgent);
orch.AddSpokeAgent(reviewerAgent);
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
    // 조건부 엣지: 조건은 소스 노드("start")의 AgentStepResult를 받는다
    .AddEdge("start", "billing", step =>
        step.Response?.Message?.Content.OfType<TextMessageContent>()
            .Any(t => t.Value.Contains("billing", StringComparison.OrdinalIgnoreCase)) == true)
    .AddEdge("start", "general")   // 조건 없음 = 항상 진행
    // Fan-In — 노드는 인입 엣지의 소스가 모두 실행되고 조건이 모두 참일 때만 실행된다.
    // billing이 건너뛰어지면 merge도 건너뛰고, 최종 출력은 마지막으로 성공한 단계(general)의 출력이 된다.
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

### 사이클 미지원 — 반복 루프가 필요하면

`GraphOrchestratorBuilder.Build()`는 그래프에 사이클이 있으면 예외를 던집니다
(`Graph contains a cycle. Only DAG (Directed Acyclic Graph) is supported.`, 대안 안내 포함).
이는 누락이 아니라 의도된 설계입니다 — DAG 제약 덕분에 GraphOrchestrator는 "항상 종료한다"는
성질을 구조적으로 보장합니다.

"생성 → 검증 → 실패 시 재시도" 같은 반복 루프가 필요하면, Graph 안에 사이클을 만드는 대신
아래 두 오케스트레이터를 쓰세요 — 둘 다 이미 반복·조건부 종료를 1급으로 지원합니다:

- **HubSpokeOrchestrator** — `MaxRounds`로 허브↔스포크 라운드를 여러 번 반복 (위 HubSpokeOrchestrator 절 참조).
- **GroupChatOrchestrator** — `ITerminationCondition`으로 조건이 충족될 때까지 반복 (위 종료 조건 절 참조).

Graph 노드 합성 안에서 직접 반복 루프를 표현하는 것(예: 검증 노드가 실패 시 생성 노드로 되돌아가는
엣지)은 현재 지원하지 않습니다 — 종료 보장을 잃지 않는 형태의 설계가 아직 없기 때문입니다.

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
| `ApprovalRequired` | 승인 대기 — `ApprovalHandler` 를 기다리기 **전에** 방출(Sequential · Graph 스트림, 핸들러가 적용되는 에이전트만) |
| `ApprovalGranted` | 승인됨 — 핸들러가 true 를 돌려준 직후, 해당 에이전트의 `AgentStarted` 전(Sequential · Graph 스트림) |
| `ApprovalDenied` | 승인 거부됨 — 체크포인트 저장 뒤, `Failed` 앞(Sequential · Graph 스트림) |
| `HumanInputRequired` | 사람 입력 필요 |
| `Completed` | 오케스트레이션 완료 |
| `Failed` | 오케스트레이션 실패 |

---

## 체크포인트 & 재개

```csharp
// 인메모리 체크포인트 (테스트용)
ICheckpointStore store = new InMemoryCheckpointStore();

// 파일 기반 체크포인트 (영속성)
store = new FileCheckpointStore("./checkpoints");

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
options.ContextScope = new LastNMessagesScope(maxMessages: 5);

// 대화 이력을 구조화된 요약(목표·도구 작업·파일 경로·오류 코드) + 현재 작업으로 압축 (LLM 호출 없음)
options.ContextScope = new SummaryContextScope(new SummaryContextScopeOptions
{
    MinMessagesForSummary = 4,   // 이 개수 이하면 그대로 전달 (기본값 4)
    MaxGoalLength = 200,         // 요약 속 목표 최대 길이 (기본값 200)
});

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

> `AsAgent()`로 래핑한 에이전트는 per-request `AgentInvokeOptions`를 지원하지 않습니다 —
> non-null 옵션 전달 시 `NotSupportedException`. 멤버 에이전트/오케스트레이터 옵션으로 구성하세요.

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
