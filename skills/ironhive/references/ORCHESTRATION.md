# Multi-Agent Orchestration

All orchestrators implement `IAgentOrchestrator` and can be wrapped as `IAgent` via `AsAgent()`.

## Orchestrator Overview

| Pattern | Class | When to Use |
|---------|-------|-------------|
| Sequential | `SequentialOrchestrator` | Chain agents in order, output passes to next |
| Parallel | `ParallelOrchestrator` | Run agents concurrently, collect all results |
| Handoff | `HandoffOrchestrator` (via `HandoffOrchestratorBuilder`) | Dynamic routing — agent decides who handles next |
| GroupChat | `GroupChatOrchestrator` (via `GroupChatOrchestratorBuilder`) | Multi-turn discussion with speaker selection |
| HubSpoke | `HubSpokeOrchestrator` | Central coordinator dispatches to specialist agents |
| Graph (DAG) | `GraphOrchestrator` (via `GraphOrchestratorBuilder`) | Conditional branching, complex workflows |

## Core Interface

```csharp
public interface IAgentOrchestrator
{
    string Name { get; }
    IReadOnlyList<IAgent> Agents { get; }
    bool SupportsRealTimeStreaming { get; }

    void AddAgent(IAgent agent);
    void AddAgents(IEnumerable<IAgent> agents);

    Task<OrchestrationResult> ExecuteAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<OrchestrationStreamEvent> ExecuteStreamingAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default);
}

// Wrap as IAgent
IAgent agent = orchestrator.AsAgent();
```

## OrchestrationStreamEvent

```csharp
public sealed class OrchestrationStreamEvent
{
    public required OrchestrationEventType EventType { get; init; }
    public string? AgentName { get; init; }
    public StreamingMessageResponse? StreamingResponse { get; init; }  // on MessageDelta
    public MessageResponse? CompletedResponse { get; init; }           // on AgentCompleted
    public OrchestrationResult? Result { get; init; }                  // on Completed
    public string? Error { get; init; }
}

public enum OrchestrationEventType
{
    Started,
    AgentStarted,
    MessageDelta,         // streaming chunk from agent
    AgentCompleted,
    AgentFailed,
    Completed,
    Failed,
    ApprovalRequired,
    ApprovalGranted,
    ApprovalDenied,
    Handoff,
    SpeakerSelected,
    HumanInputRequired
}
```

## Common Options

Every orchestrator's options derive from `OrchestratorOptions`: `Name`, `Timeout` (default 5 min),
`AgentTimeout` (default 2 min), `StopOnAgentFailure` (default true), `CheckpointStore`, `OrchestrationId`,
`ApprovalHandler`, `RequireApprovalForAgents`, `AgentMiddlewares`, `ContextScope`, `ResultDistiller`,
`ResultDistillationOptions`.

## Sequential Orchestrator

```csharp
var orchestrator = new SequentialOrchestrator(new SequentialOrchestratorOptions
{
    PassOutputAsInput = true,    // each agent receives previous output (default: true)
    AccumulateHistory = false    // pass the full accumulated history instead (default: false)
});
orchestrator.AddAgents([researchAgent, summaryAgent, reviewAgent]);

// Stream results
await foreach (var evt in orchestrator.ExecuteStreamingAsync(messages))
{
    if (evt.EventType == OrchestrationEventType.MessageDelta
        && evt.StreamingResponse is StreamingContentDeltaResponse { Delta: TextDeltaContent text })
        Console.Write(text.Value);
}
```

## Parallel Orchestrator

```csharp
var orchestrator = new ParallelOrchestrator(new ParallelOrchestratorOptions
{
    MaxConcurrency    = 3,                                // null = unlimited (default)
    ResultAggregation = ParallelResultAggregation.All     // All | FirstSuccess | Fastest | Merge
});
orchestrator.AddAgents([agentA, agentB, agentC]);

await foreach (var evt in orchestrator.ExecuteStreamingAsync(messages))
{
    if (evt.EventType == OrchestrationEventType.AgentCompleted)
        Console.WriteLine($"{evt.AgentName} finished");
}
```

## Handoff Orchestrator

An agent hands off by emitting JSON such as `{"handoff_to": "billing", "context": "..."}`.

```csharp
var orchestrator = new HandoffOrchestratorBuilder()
    // Description is put into the prompt; the model picks a target from it
    .AddAgent(triageAgent,
        new HandoffTarget { AgentName = "billing",   Description = "billing-related question" },
        new HandoffTarget { AgentName = "technical", Description = "technical issue" })
    .AddAgent(billingAgent)
    .AddAgent(technicalAgent)
    .SetInitialAgent("triage")
    .SetMaxTransitions(20)
    .SetApprovalHandler((agentName, previousStep) => Task.FromResult(true))   // auto-approve
    .SetContextScope(new LastNMessagesScope(20))
    .Build();
```

### HandoffOrchestratorOptions

```csharp
public class HandoffOrchestratorOptions : OrchestratorOptions
{
    public required string InitialAgentName { get; set; }
    public int MaxTransitions { get; set; } = 20;
    public Func<string, AgentStepResult, Task<Message?>>? NoHandoffHandler { get; set; }
}
```

## GroupChat Orchestrator

```csharp
var orchestrator = new GroupChatOrchestratorBuilder()
    .AddAgent(agentA)
    .AddAgent(agentB)
    .AddAgent(agentC)
    .WithLlmManager(managerAgent)          // or .WithRoundRobin() / .WithRandom() / .WithSpeakerSelector(...)
    .TerminateOnKeyword("TERMINATE")       // or .TerminateAfterRounds(10) / .TerminateOnTokenBudget(...) / .WithTerminationCondition(...)
    .SetMaxRounds(20)                      // safety cap (default: 50)
    .SetContextScope(new LastNMessagesScope(30))
    .Build();
```

### Speaker Selectors

```csharp
new LlmSpeakerSelector(managerAgent)      // LLM picks next speaker
new RoundRobinSpeakerSelector()           // rotate in order
new RandomSpeakerSelector()               // random
```

### Termination Conditions

```csharp
new KeywordTermination("TERMINATE")       // stop when keyword appears
new MaxRoundsTermination(10)              // stop after N rounds
new TokenBudgetTermination(50_000)        // stop after a token budget
new CompositeTermination(requireAll: false,
    new KeywordTermination("DONE"),
    new MaxRoundsTermination(5))          // OR (requireAll: true = AND)
```

## Graph Orchestrator (DAG)

```csharp
var orchestrator = new GraphOrchestratorBuilder()
    .AddNode("classify", classifyAgent)
    .AddNode("billing",  billingAgent)
    .AddNode("general",  generalAgent)
    .AddEdge("classify", "billing", step =>
        step.Response?.Message?.Content.OfType<TextMessageContent>()
            .Any(t => t.Value.Contains("billing", StringComparison.OrdinalIgnoreCase)) == true)
    .AddEdge("classify", "general")   // no condition = always taken
    .SetStartNode("classify")
    .SetOutputNode("billing")
    .Build();
```

Cycles are rejected at `Build()` — for retry loops use HubSpoke (`MaxRounds`) or GroupChat (termination conditions).

## HubSpoke Orchestrator

```csharp
var orchestrator = new HubSpokeOrchestrator(new HubSpokeOrchestratorOptions
{
    MaxRounds      = 5,        // default: 10
    ParallelSpokes = false
});
orchestrator.SetHubAgent(hubAgent);
orchestrator.AddSpokeAgent(specialistA);
orchestrator.AddSpokeAgent(specialistB);
orchestrator.AddSpokeAgent(specialistC);
```

## Checkpoint / Resume

```csharp
// In-memory checkpoint (survives within process)
var store = new InMemoryCheckpointStore();

// File-based checkpoint (survives restarts)
var store = new FileCheckpointStore("./checkpoints");

var orchestrator = new HandoffOrchestratorBuilder()
    // ...
    .SetCheckpointStore(store)
    .SetOrchestrationId("session-abc-123")   // re-running with the same id resumes from the checkpoint
    .Build();

// Options-based orchestrators take the same two settings as options
var sequential = new SequentialOrchestrator(new SequentialOrchestratorOptions
{
    CheckpointStore = store,
    OrchestrationId = "session-abc-123"
});
```

## Context Scopes

Controls how much conversation history each agent receives:

```csharp
new LastNMessagesScope(20)          // last N messages only (default 5)
new SummaryContextScope()           // compress older history into a structured summary (SummaryContextScopeOptions)
new TaskOnlyScope()                 // only the original task message
```

## OrchestrationResult

```csharp
public sealed class OrchestrationResult
{
    public bool IsSuccess { get; init; }
    public Message? FinalOutput { get; init; }                  // last agent output
    public IReadOnlyList<AgentStepResult> Steps { get; init; }  // per-agent results
    public TimeSpan TotalDuration { get; init; }
    public TokenUsageSummary? TokenUsage { get; init; }         // aggregated token usage
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

## Wrap Orchestrator as IAgent

```csharp
// Any orchestrator can be used wherever IAgent is expected
IAgent orchestratorAgent = orchestrator.AsAgent(
    name: "Pipeline",
    description: "Multi-agent research pipeline"
);

// Can then be used in another orchestrator
var outer = new SequentialOrchestrator();
outer.AddAgent(orchestratorAgent);
outer.AddAgent(reviewAgent);
```

Orchestrator-wrapped agents do not support per-request `AgentInvokeOptions` —
passing a non-null options throws `NotSupportedException`. Configure member
agents or orchestrator options instead.
