# Multi-Agent Orchestration

All orchestrators implement `IAgentOrchestrator` and can be wrapped as `IAgent` via `AsAgent()`.

## Orchestrator Overview

| Pattern | Class | When to Use |
|---------|-------|-------------|
| Sequential | `SequentialOrchestrator` | Chain agents in order, output passes to next |
| Parallel | `ParallelOrchestrator` | Run agents concurrently, collect all results |
| Handoff | `HandoffOrchestrator` | Dynamic routing — agent decides who handles next |
| GroupChat | `GroupChatOrchestrator` | Multi-turn discussion with speaker selection |
| HubSpoke | `HubSpokeOrchestrator` | Central coordinator dispatches to specialist agents |
| Graph (DAG) | `GraphOrchestrator` | Conditional branching, complex workflows |

## Core Interface

```csharp
public interface IAgentOrchestrator
{
    IAsyncEnumerable<OrchestrationStreamEvent> ExecuteAsync(
        IEnumerable<Message> messages,
        CancellationToken ct = default);
}

// Wrap as IAgent
IAgent agent = orchestrator.AsAgent();
```

## OrchestrationStreamEvent

```csharp
public class OrchestrationStreamEvent
{
    public OrchestrationEventType Type { get; }
    public string? AgentName { get; }
    public IEnumerable<MessageContent>? Content { get; }
    public OrchestrationResult? Result { get; }     // on Completed
}

public enum OrchestrationEventType
{
    Started,
    AgentStarted,
    MessageDelta,         // streaming text chunk from agent
    AgentCompleted,
    AgentFailed,
    Handoff,
    SpeakerSelected,
    ApprovalRequired,
    ApprovalGranted,
    ApprovalDenied,
    HumanInputRequired,
    Completed,
    Failed
}
```

## Sequential Orchestrator

```csharp
var orchestrator = new SequentialOrchestrator(
    new[] { researchAgent, summaryAgent, reviewAgent },
    new SequentialOrchestratorOptions
    {
        PassResultToNext = true    // each agent receives previous output
    });

// Stream results
await foreach (var evt in orchestrator.ExecuteAsync(messages))
{
    if (evt.Type == OrchestrationEventType.MessageDelta)
        Console.Write(evt.Content?.OfType<TextMessageContent>().FirstOrDefault()?.Value);
}
```

## Parallel Orchestrator

```csharp
var orchestrator = new ParallelOrchestrator(
    new[] { agentA, agentB, agentC },
    new ParallelOrchestratorOptions
    {
        MaxConcurrency = 3
    });

await foreach (var evt in orchestrator.ExecuteAsync(messages))
{
    if (evt.Type == OrchestrationEventType.AgentCompleted)
        Console.WriteLine($"{evt.AgentName} finished");
}
```

## Handoff Orchestrator

```csharp
var orchestrator = new HandoffOrchestratorBuilder()
    .AddAgent(triageAgent,
        new HandoffTarget { Name = "Billing", Condition = "billing-related question" },
        new HandoffTarget { Name = "Technical", Condition = "technical issue" })
    .AddAgent(billingAgent)
    .AddAgent(technicalAgent)
    .SetInitialAgent("Triage")
    .WithOptions(new HandoffOrchestratorOptions
    {
        MaxTransitions     = 20,
        ApprovalHandler    = async (agent, msgs, ct) => true,   // auto-approve
        ContextScope       = new LastNMessagesScope(20)
    })
    .Build();
```

### HandoffOrchestratorOptions

```csharp
public class HandoffOrchestratorOptions
{
    public int MaxTransitions { get; set; } = 10;
    public Func<IAgent, IEnumerable<Message>, CancellationToken, Task<bool>>? ApprovalHandler { get; set; }
    public IContextScope? ContextScope { get; set; }
    public IResultDistiller? ResultDistiller { get; set; }
}
```

## GroupChat Orchestrator

```csharp
var orchestrator = new GroupChatOrchestratorBuilder()
    .AddAgent(agentA)
    .AddAgent(agentB)
    .AddAgent(agentC)
    .WithOptions(new GroupChatOrchestratorOptions
    {
        SpeakerSelector      = new LlmSpeakerSelector(managerAgent),   // or RoundRobinSpeakerSelector / RandomSpeakerSelector
        TerminationCondition = new KeywordTerminationCondition("TERMINATE"),  // or MaxRoundsTerminationCondition(10)
        MaxRounds            = 20,
        ContextScope         = new LastNMessagesScope(30)
    })
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
new KeywordTerminationCondition("TERMINATE")   // stop when keyword appears
new MaxRoundsTerminationCondition(10)          // stop after N rounds
```

## Graph Orchestrator (DAG)

```csharp
var orchestrator = new GraphOrchestratorBuilder()
    .AddNode("classify", classifyAgent)
    .AddNode("billing",  billingAgent)
    .AddNode("general",  generalAgent)
    .AddEdge("classify", "billing", result =>
        result.Text?.Contains("billing", StringComparison.OrdinalIgnoreCase) == true)
    .AddEdge("classify", "general")   // default (no condition = fallthrough)
    .SetStartNode("classify")
    .SetOutputNode("billing", "general")   // terminal nodes
    .Build();
```

## HubSpoke Orchestrator

```csharp
var orchestrator = new HubSpokeOrchestrator(
    hubAgent,
    new[] { specialistA, specialistB, specialistC },
    new HubSpokeOrchestratorOptions
    {
        MaxIterations = 5
    });
```

## Checkpoint / Resume

```csharp
// In-memory checkpoint (survives within process)
var store = new InMemoryCheckpointStore();

// File-based checkpoint (survives restarts)
var store = new FileCheckpointStore("./checkpoints");

var orchestrator = new HandoffOrchestratorBuilder()
    // ...
    .WithCheckpointStore(store, orchestrationId: "session-abc-123")
    .Build();
```

## Context Scopes

Controls how much conversation history each agent receives:

```csharp
new LastNMessagesScope(20)       // last N messages only
new SummaryContextScope(agent)   // summarize older history via agent
new TaskOnlyScope()              // only the original task message
```

## OrchestrationResult

```csharp
public class OrchestrationResult
{
    public IEnumerable<AgentStepResult> Steps { get; }  // per-agent results
    public TokenUsageSummary Usage { get; }             // aggregated token usage
    public Message? FinalMessage { get; }               // last agent output
}

public class AgentStepResult
{
    public string AgentName { get; }
    public MessageResponse Response { get; }
    public TimeSpan Duration { get; }
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
var outer = new SequentialOrchestrator(new[] { orchestratorAgent, reviewAgent });
```
