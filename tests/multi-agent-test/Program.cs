// =============================================================================
// IronHive Multi-Agent Orchestration Tests
// =============================================================================
// Tests orchestrator functionality: Sequential, Parallel, HubSpoke, Graph,
// Checkpointing, Human-in-the-Loop, TypedPipeline.
// Uses mock agents — no API keys or LLM calls required.
// =============================================================================

using System.Runtime.CompilerServices;
using System.Text;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Tools;
using IronHive.Core.Agent.Orchestration;

Console.WriteLine("=============================================================");
Console.WriteLine("IronHive Multi-Agent Orchestration Tests");
Console.WriteLine("=============================================================\n");

var results = new List<(string Category, string Scenario, TestResult Result)>();

// ---- Sequential Orchestrator ----
results.AddRange(await RunCategory("sequential", RunSequentialTests));

// ---- Parallel Orchestrator ----
results.AddRange(await RunCategory("parallel", RunParallelTests));

// ---- Graph Orchestrator ----
results.AddRange(await RunCategory("graph", RunGraphTests));

// ---- Checkpointing ----
results.AddRange(await RunCategory("checkpoint", RunCheckpointTests));

// ---- Human-in-the-Loop ----
results.AddRange(await RunCategory("hitl", RunHITLTests));

// ---- TypedPipeline ----
results.AddRange(await RunCategory("typed-pipeline", RunTypedPipelineTests));

// ---- Streaming ----
results.AddRange(await RunCategory("streaming", RunStreamingTests));

// ---- Error Handling ----
results.AddRange(await RunCategory("error", RunErrorTests));

// Summary
Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("SUMMARY");
Console.WriteLine(new string('=', 60));

var passed = results.Count(r => r.Result.Success);
var failed = results.Count(r => !r.Result.Success);

Console.WriteLine($"  Passed:  {passed}");
Console.WriteLine($"  Failed:  {failed}");
Console.WriteLine($"  Total:   {results.Count}");

if (failed > 0)
{
    Console.WriteLine("\nFailed tests:");
    foreach (var (category, scenario, result) in results.Where(r => !r.Result.Success))
        Console.WriteLine($"  - {category}/{scenario}: {result.Message}");
}

return failed > 0 ? 1 : 0;

// =============================================================================
// Test Runner Helper
// =============================================================================

async Task<List<(string, string, TestResult)>> RunCategory(
    string category,
    Func<Task<List<(string, TestResult)>>> testFunc)
{
    Console.WriteLine($"\n  [{category}]");
    var categoryResults = new List<(string, string, TestResult)>();
    try
    {
        var scenarioResults = await testFunc();
        foreach (var (scenario, result) in scenarioResults)
        {
            categoryResults.Add((category, scenario, result));
            PrintResult(scenario, result);
        }
    }
    catch (Exception ex)
    {
        var result = TestResult.Error(ex.Message);
        categoryResults.Add((category, "setup", result));
        PrintResult("setup", result);
    }
    return categoryResults;
}

void PrintResult(string scenario, TestResult result)
{
    var status = result.Success ? "[PASS]" : "[FAIL]";
    var color = result.Success ? ConsoleColor.Green : ConsoleColor.Red;

    Console.ForegroundColor = color;
    Console.Write($"{"",4}{status}");
    Console.ResetColor();
    Console.Write($" {scenario}");
    if (!string.IsNullOrEmpty(result.Message))
        Console.Write($": {result.Message}");
    Console.WriteLine();
}

// =============================================================================
// 1. Sequential Orchestrator Tests
// =============================================================================

async Task<List<(string, TestResult)>> RunSequentialTests()
{
    var results = new List<(string, TestResult)>();

    // 1-1: Basic sequential — 2 agents chained, output→input
    try
    {
        var agent1 = new MockAgent("translator", "mock", "Translates to uppercase")
        {
            ResponseFunc = msgs => $"TRANSLATED: {GetLastUserText(msgs)}"
        };
        var agent2 = new MockAgent("summarizer", "mock", "Adds summary prefix")
        {
            ResponseFunc = msgs => $"SUMMARY of '{GetLastAssistantText(msgs)}'"
        };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            PassOutputAsInput = true
        });
        orch.AddAgents([agent1, agent2]);

        var result = await orch.ExecuteAsync(MakeUserMessages("hello world"));

        var ok = result.IsSuccess
            && result.Steps.Count == 2
            && result.Steps[0].AgentName == "translator"
            && result.Steps[1].AgentName == "summarizer"
            && GetTextFromMessage(result.FinalOutput).Contains("SUMMARY")
            && GetTextFromMessage(result.FinalOutput).Contains("TRANSLATED");

        results.Add(("basic-chain", ok
            ? TestResult.Pass($"steps={result.Steps.Count}, output contains chained result")
            : TestResult.Error($"Unexpected result: {GetTextFromMessage(result.FinalOutput)}")));
    }
    catch (Exception ex) { results.Add(("basic-chain", TestResult.Error(ex.Message))); }

    // 1-2: AccumulateHistory — messages accumulate across agents
    try
    {
        var callCount = 0;
        var agent1 = new MockAgent("a1", "mock", "First") { ResponseFunc = _ => { callCount++; return "response-1"; } };
        var agent2 = new MockAgent("a2", "mock", "Second")
        {
            ResponseFunc = msgs =>
            {
                callCount++;
                var msgCount = msgs.Count();
                return $"received-{msgCount}-messages";
            }
        };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            PassOutputAsInput = true,
            AccumulateHistory = true
        });
        orch.AddAgents([agent1, agent2]);

        var result = await orch.ExecuteAsync(MakeUserMessages("start"));

        // AccumulateHistory=true: agent2 receives original user message + agent1's assistant response = 2 messages
        var outputText = GetTextFromMessage(result.FinalOutput);
        var ok = result.IsSuccess && outputText.Contains("received-2-messages");

        results.Add(("accumulate-history", ok
            ? TestResult.Pass($"output={outputText}")
            : TestResult.Error($"Expected 2 messages accumulated, got: {outputText}")));
    }
    catch (Exception ex) { results.Add(("accumulate-history", TestResult.Error(ex.Message))); }

    // 1-3: 3 agents sequential — verify ordering
    try
    {
        var order = new List<string>();
        var agents = Enumerable.Range(1, 3).Select(i =>
        {
            var agent = new MockAgent($"step-{i}", "mock", $"Step {i}");
            agent.ResponseFunc = msgs => { order.Add($"step-{i}"); return $"output-{i}"; };
            return (IAgent)agent;
        }).ToArray();

        var orch = new SequentialOrchestrator();
        orch.AddAgents(agents);

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        var ok = result.IsSuccess
            && result.Steps.Count == 3
            && string.Join(",", order) == "step-1,step-2,step-3";

        results.Add(("ordering", ok
            ? TestResult.Pass($"order={string.Join("→", order)}")
            : TestResult.Error($"Wrong order: {string.Join(",", order)}")));
    }
    catch (Exception ex) { results.Add(("ordering", TestResult.Error(ex.Message))); }

    return results;
}

// =============================================================================
// 2. Parallel Orchestrator Tests
// =============================================================================

async Task<List<(string, TestResult)>> RunParallelTests()
{
    var results = new List<(string, TestResult)>();

    // 2-1: Basic parallel — all agents execute
    try
    {
        var agents = Enumerable.Range(1, 3).Select(i =>
        {
            var agent = new MockAgent($"worker-{i}", "mock", $"Worker {i}");
            agent.ResponseFunc = _ => $"result-{i}";
            return (IAgent)agent;
        }).ToArray();

        var orch = new ParallelOrchestrator();
        orch.AddAgents(agents);

        var result = await orch.ExecuteAsync(MakeUserMessages("process"));

        var ok = result.IsSuccess && result.Steps.Count == 3
            && result.Steps.All(s => s.IsSuccess);

        results.Add(("basic-parallel", ok
            ? TestResult.Pass($"steps={result.Steps.Count}, all success")
            : TestResult.Error($"steps={result.Steps.Count}, success={result.Steps.Count(s => s.IsSuccess)}")));
    }
    catch (Exception ex) { results.Add(("basic-parallel", TestResult.Error(ex.Message))); }

    // 2-2: Merge aggregation
    try
    {
        var agent1 = new MockAgent("analyst-A", "mock", "A") { ResponseFunc = _ => "Analysis A" };
        var agent2 = new MockAgent("analyst-B", "mock", "B") { ResponseFunc = _ => "Analysis B" };

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            ResultAggregation = ParallelResultAggregation.Merge
        });
        orch.AddAgents([agent1, agent2]);

        var result = await orch.ExecuteAsync(MakeUserMessages("analyze"));
        var output = GetTextFromMessage(result.FinalOutput);

        var ok = result.IsSuccess
            && output.Contains("analyst-A")
            && output.Contains("analyst-B")
            && output.Contains("Analysis A")
            && output.Contains("Analysis B");

        results.Add(("merge-aggregation", ok
            ? TestResult.Pass($"merged output contains both agents")
            : TestResult.Error($"Merge failed: {output}")));
    }
    catch (Exception ex) { results.Add(("merge-aggregation", TestResult.Error(ex.Message))); }

    // 2-3: MaxConcurrency limit
    try
    {
        var concurrent = 0;
        var maxConcurrent = 0;
        var lockObj = new object();

        var agents = Enumerable.Range(1, 4).Select(i =>
        {
            var agent = new MockAgent($"w-{i}", "mock", $"Worker {i}");
            agent.ResponseFuncAsync = async _ =>
            {
                lock (lockObj) { concurrent++; maxConcurrent = Math.Max(maxConcurrent, concurrent); }
                await Task.Delay(50);
                lock (lockObj) { concurrent--; }
                return $"done-{i}";
            };
            return (IAgent)agent;
        }).ToArray();

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            MaxConcurrency = 2
        });
        orch.AddAgents(agents);

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        var ok = result.IsSuccess && maxConcurrent <= 2;

        results.Add(("max-concurrency", ok
            ? TestResult.Pass($"maxConcurrent={maxConcurrent} (limit=2)")
            : TestResult.Error($"maxConcurrent={maxConcurrent} exceeded limit=2")));
    }
    catch (Exception ex) { results.Add(("max-concurrency", TestResult.Error(ex.Message))); }

    return results;
}

// =============================================================================
// 3. Graph Orchestrator Tests
// =============================================================================

async Task<List<(string, TestResult)>> RunGraphTests()
{
    var results = new List<(string, TestResult)>();

    // 3-1: Linear graph A→B→C
    try
    {
        var order = new List<string>();
        var agentA = new MockAgent("A", "mock", "Node A")
        { ResponseFunc = _ => { order.Add("A"); return "output-A"; } };
        var agentB = new MockAgent("B", "mock", "Node B")
        { ResponseFunc = _ => { order.Add("B"); return "output-B"; } };
        var agentC = new MockAgent("C", "mock", "Node C")
        { ResponseFunc = _ => { order.Add("C"); return "output-C"; } };

        var orch = new GraphOrchestratorBuilder()
            .AddNode("a", agentA)
            .AddNode("b", agentB)
            .AddNode("c", agentC)
            .AddEdge("a", "b")
            .AddEdge("b", "c")
            .SetStartNode("a")
            .SetOutputNode("c")
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("start"));

        var ok = result.IsSuccess
            && result.Steps.Count == 3
            && string.Join(",", order) == "A,B,C"
            && GetTextFromMessage(result.FinalOutput) == "output-C";

        results.Add(("linear-abc", ok
            ? TestResult.Pass($"order={string.Join("→", order)}, output=output-C")
            : TestResult.Error($"order={string.Join(",", order)}, output={GetTextFromMessage(result.FinalOutput)}")));
    }
    catch (Exception ex) { results.Add(("linear-abc", TestResult.Error(ex.Message))); }

    // 3-2: Fan-out A→{B,C}→D (diamond graph)
    try
    {
        var order = new List<string>();
        var agentA = new MockAgent("A", "mock", "Start") { ResponseFunc = _ => { order.Add("A"); return "from-A"; } };
        var agentB = new MockAgent("B", "mock", "Branch1") { ResponseFunc = _ => { order.Add("B"); return "from-B"; } };
        var agentC = new MockAgent("C", "mock", "Branch2") { ResponseFunc = _ => { order.Add("C"); return "from-C"; } };
        var agentD = new MockAgent("D", "mock", "Merge")
        {
            ResponseFunc = msgs =>
            {
                order.Add("D");
                // D receives Fan-In messages from B and C
                var count = msgs.Count();
                return $"merged-{count}-inputs";
            }
        };

        var orch = new GraphOrchestratorBuilder()
            .AddNode("a", agentA)
            .AddNode("b", agentB)
            .AddNode("c", agentC)
            .AddNode("d", agentD)
            .AddEdge("a", "b")
            .AddEdge("a", "c")
            .AddEdge("b", "d")
            .AddEdge("c", "d")
            .SetStartNode("a")
            .SetOutputNode("d")
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("start"));

        // A first, then B and C (any order), then D
        var aIdx = order.IndexOf("A");
        var dIdx = order.IndexOf("D");
        var ok = result.IsSuccess
            && result.Steps.Count == 4
            && aIdx == 0
            && dIdx == 3
            && GetTextFromMessage(result.FinalOutput).Contains("merged");

        results.Add(("diamond-fan", ok
            ? TestResult.Pass($"order={string.Join("→", order)}, output={GetTextFromMessage(result.FinalOutput)}")
            : TestResult.Error($"order={string.Join(",", order)}, output={GetTextFromMessage(result.FinalOutput)}")));
    }
    catch (Exception ex) { results.Add(("diamond-fan", TestResult.Error(ex.Message))); }

    // 3-3: Conditional edge — edge skipped when condition is false
    try
    {
        var executed = new List<string>();
        var agentA = new MockAgent("A", "mock", "Start")
        { ResponseFunc = _ => { executed.Add("A"); return "low-score"; } };
        var agentB = new MockAgent("B", "mock", "Conditional")
        { ResponseFunc = _ => { executed.Add("B"); return "B-executed"; } };

        var orch = new GraphOrchestratorBuilder()
            .AddNode("a", agentA)
            .AddNode("b", agentB)
            .AddEdge("a", "b", condition: step =>
            {
                // Only execute B if A's output contains "high-score"
                var text = GetTextFromStepResult(step);
                return text.Contains("high-score");
            })
            .SetStartNode("a")
            .SetOutputNode("b")
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("start"));

        // B should not execute since condition is false, A's output is "low-score"
        var ok = !executed.Contains("B") && executed.Contains("A");

        results.Add(("conditional-edge", ok
            ? TestResult.Pass($"B skipped (condition false), executed=[{string.Join(",", executed)}]")
            : TestResult.Error($"executed=[{string.Join(",", executed)}]")));
    }
    catch (Exception ex) { results.Add(("conditional-edge", TestResult.Error(ex.Message))); }

    // 3-4: Cycle detection — builder should throw
    try
    {
        var threw = false;
        try
        {
            var agent = new MockAgent("X", "mock", "Node");
            new GraphOrchestratorBuilder()
                .AddNode("a", agent)
                .AddNode("b", agent)
                .AddEdge("a", "b")
                .AddEdge("b", "a")
                .SetStartNode("a")
                .SetOutputNode("b")
                .Build();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("cycle"))
        {
            threw = true;
        }

        results.Add(("cycle-detection", threw
            ? TestResult.Pass("InvalidOperationException thrown for cycle")
            : TestResult.Error("No exception thrown for cyclic graph")));
    }
    catch (Exception ex) { results.Add(("cycle-detection", TestResult.Error(ex.Message))); }

    // 3-5: Builder validation — missing start node
    try
    {
        var threw = false;
        try
        {
            var agent = new MockAgent("X", "mock", "Node");
            new GraphOrchestratorBuilder()
                .AddNode("a", agent)
                .SetOutputNode("a")
                .Build();
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }

        results.Add(("builder-validation", threw
            ? TestResult.Pass("InvalidOperationException for missing start node")
            : TestResult.Error("No exception for missing start node")));
    }
    catch (Exception ex) { results.Add(("builder-validation", TestResult.Error(ex.Message))); }

    return results;
}

// =============================================================================
// 4. Checkpoint Tests
// =============================================================================

async Task<List<(string, TestResult)>> RunCheckpointTests()
{
    var results = new List<(string, TestResult)>();

    // 4-1: InMemoryCheckpointStore CRUD
    try
    {
        var store = new InMemoryCheckpointStore();

        var checkpoint = new OrchestrationCheckpoint
        {
            OrchestrationId = "test-1",
            OrchestratorName = "TestOrch",
            CompletedStepCount = 2,
            CompletedSteps = [],
            CurrentMessages = []
        };

        await store.SaveAsync("test-1", checkpoint);
        var loaded = await store.LoadAsync("test-1");
        var exists = loaded != null && loaded.OrchestrationId == "test-1" && loaded.CompletedStepCount == 2;

        await store.DeleteAsync("test-1");
        var deleted = await store.LoadAsync("test-1");
        var notExists = deleted == null;

        var ok = exists && notExists;
        results.Add(("store-crud", ok
            ? TestResult.Pass("Save/Load/Delete verified")
            : TestResult.Error($"exists={exists}, deleted={notExists}")));
    }
    catch (Exception ex) { results.Add(("store-crud", TestResult.Error(ex.Message))); }

    // 4-2: Sequential orchestrator saves checkpoints during execution
    try
    {
        var store = new InMemoryCheckpointStore();
        var orchId = "orch-seq-ckpt";

        var agent1 = new MockAgent("a1", "mock", "First") { ResponseFunc = _ => "done-1" };
        var agent2 = new MockAgent("a2", "mock", "Second") { ResponseFunc = _ => "done-2" };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            CheckpointStore = store,
            OrchestrationId = orchId
        });
        orch.AddAgents([agent1, agent2]);

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        // After successful completion, checkpoint should be deleted
        var ckpt = await store.LoadAsync(orchId);
        var ok = result.IsSuccess && ckpt == null;

        results.Add(("seq-checkpoint-cleanup", ok
            ? TestResult.Pass("Checkpoint deleted after successful completion")
            : TestResult.Error($"IsSuccess={result.IsSuccess}, ckpt={ckpt != null}")));
    }
    catch (Exception ex) { results.Add(("seq-checkpoint-cleanup", TestResult.Error(ex.Message))); }

    // 4-3: Checkpoint persists on failure
    try
    {
        var store = new InMemoryCheckpointStore();
        var orchId = "orch-fail-ckpt";

        var agent1 = new MockAgent("a1", "mock", "First") { ResponseFunc = _ => "done-1" };
        var agent2 = new MockAgent("a2", "mock", "Failing")
        {
            ResponseFuncAsync = _ => throw new InvalidOperationException("Simulated failure")
        };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            CheckpointStore = store,
            OrchestrationId = orchId,
            StopOnAgentFailure = true
        });
        orch.AddAgents([agent1, agent2]);

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        // On failure, checkpoint should persist with completed step count = 1 (agent1) + 1 (failed agent2)
        var ckpt = await store.LoadAsync(orchId);
        var ok = !result.IsSuccess && ckpt != null && ckpt.CompletedStepCount >= 1;

        results.Add(("checkpoint-on-failure", ok
            ? TestResult.Pass($"Checkpoint saved: steps={ckpt?.CompletedStepCount}")
            : TestResult.Error($"IsSuccess={result.IsSuccess}, ckpt={ckpt != null}, steps={ckpt?.CompletedStepCount}")));
    }
    catch (Exception ex) { results.Add(("checkpoint-on-failure", TestResult.Error(ex.Message))); }

    return results;
}

// =============================================================================
// 5. Human-in-the-Loop Tests
// =============================================================================

async Task<List<(string, TestResult)>> RunHITLTests()
{
    var results = new List<(string, TestResult)>();

    // 5-1: Approval granted — all agents execute
    try
    {
        var approvalLog = new List<string>();

        var agent1 = new MockAgent("writer", "mock", "Writer") { ResponseFunc = _ => "draft" };
        var agent2 = new MockAgent("reviewer", "mock", "Reviewer") { ResponseFunc = _ => "approved" };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            ApprovalHandler = async (name, prev) =>
            {
                approvalLog.Add($"approve:{name}");
                return true; // always approve
            }
        });
        orch.AddAgents([agent1, agent2]);

        var result = await orch.ExecuteAsync(MakeUserMessages("write"));

        var ok = result.IsSuccess
            && result.Steps.Count == 2
            && approvalLog.Count == 2
            && approvalLog[0] == "approve:writer"
            && approvalLog[1] == "approve:reviewer";

        results.Add(("approval-granted", ok
            ? TestResult.Pass($"Both agents approved and executed")
            : TestResult.Error($"steps={result.Steps.Count}, approvals={string.Join(",", approvalLog)}")));
    }
    catch (Exception ex) { results.Add(("approval-granted", TestResult.Error(ex.Message))); }

    // 5-2: Approval denied — orchestration stops
    try
    {
        var executed = new List<string>();

        var agent1 = new MockAgent("safe", "mock", "Safe")
        { ResponseFunc = _ => { executed.Add("safe"); return "ok"; } };
        var agent2 = new MockAgent("risky", "mock", "Risky")
        { ResponseFunc = _ => { executed.Add("risky"); return "dangerous"; } };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            ApprovalHandler = async (name, prev) => name != "risky" // deny "risky"
        });
        orch.AddAgents([agent1, agent2]);

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        var ok = !result.IsSuccess
            && result.Error!.Contains("Approval denied")
            && executed.Contains("safe")
            && !executed.Contains("risky");

        results.Add(("approval-denied", ok
            ? TestResult.Pass($"Risky agent denied, executed=[{string.Join(",", executed)}]")
            : TestResult.Error($"IsSuccess={result.IsSuccess}, executed=[{string.Join(",", executed)}]")));
    }
    catch (Exception ex) { results.Add(("approval-denied", TestResult.Error(ex.Message))); }

    // 5-3: RequireApprovalForAgents — selective approval
    try
    {
        var approvalChecked = new List<string>();

        var agent1 = new MockAgent("auto", "mock", "Auto") { ResponseFunc = _ => "auto-result" };
        var agent2 = new MockAgent("manual", "mock", "Manual") { ResponseFunc = _ => "manual-result" };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            ApprovalHandler = async (name, prev) =>
            {
                approvalChecked.Add(name);
                return true;
            },
            RequireApprovalForAgents = new HashSet<string> { "manual" }
        });
        orch.AddAgents([agent1, agent2]);

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        // Only "manual" should have been checked
        var ok = result.IsSuccess
            && approvalChecked.Count == 1
            && approvalChecked[0] == "manual";

        results.Add(("selective-approval", ok
            ? TestResult.Pass($"Only 'manual' checked: [{string.Join(",", approvalChecked)}]")
            : TestResult.Error($"Checked: [{string.Join(",", approvalChecked)}]")));
    }
    catch (Exception ex) { results.Add(("selective-approval", TestResult.Error(ex.Message))); }

    return results;
}

// =============================================================================
// 6. TypedPipeline Tests
// =============================================================================

async Task<List<(string, TestResult)>> RunTypedPipelineTests()
{
    var results = new List<(string, TestResult)>();

    // 6-1: Simple string→int pipeline
    try
    {
        var step1 = new SimpleExecutor<string, int>("parser", s => s.Length);
        var step2 = new SimpleExecutor<int, string>("formatter", n => $"length={n}");

        var pipeline = TypedPipeline
            .Start(step1)
            .Then(step2)
            .Build();

        var output = await pipeline.ExecuteAsync("hello");

        var ok = output == "length=5" && pipeline.Name == "parser -> formatter";

        results.Add(("string-int-chain", ok
            ? TestResult.Pass($"output='{output}', name='{pipeline.Name}'")
            : TestResult.Error($"output='{output}', name='{pipeline.Name}'")));
    }
    catch (Exception ex) { results.Add(("string-int-chain", TestResult.Error(ex.Message))); }

    // 6-2: 3-stage pipeline
    try
    {
        var s1 = new SimpleExecutor<string, string>("upper", s => s.ToUpperInvariant());
        var s2 = new SimpleExecutor<string, string>("bracket", s => $"[{s}]");
        var s3 = new SimpleExecutor<string, int>("length", s => s.Length);

        var pipeline = TypedPipeline
            .Start(s1)
            .Then(s2)
            .Then(s3)
            .Build();

        var output = await pipeline.ExecuteAsync("abc");
        // "abc" → "ABC" → "[ABC]" → 5

        var ok = output == 5;
        results.Add(("three-stage", ok
            ? TestResult.Pass($"output={output}")
            : TestResult.Error($"Expected 5, got {output}")));
    }
    catch (Exception ex) { results.Add(("three-stage", TestResult.Error(ex.Message))); }

    // 6-3: AgentExecutor wrapping MockAgent
    try
    {
        var mockAgent = new MockAgent("processor", "mock", "Processes text")
        {
            ResponseFunc = msgs => $"processed: {GetLastUserText(msgs)}"
        };

        var executor = new AgentExecutor<string, string>(
            mockAgent,
            input => MakeUserMessages(input),
            response => GetTextFromResponse(response)
        );

        var output = await executor.ExecuteAsync("test-input");

        var ok = output == "processed: test-input" && executor.Name == "processor";

        results.Add(("agent-executor", ok
            ? TestResult.Pass($"output='{output}'")
            : TestResult.Error($"output='{output}'")));
    }
    catch (Exception ex) { results.Add(("agent-executor", TestResult.Error(ex.Message))); }

    return results;
}

// =============================================================================
// 7. Streaming Tests
// =============================================================================

async Task<List<(string, TestResult)>> RunStreamingTests()
{
    var results = new List<(string, TestResult)>();

    // 7-1: Sequential streaming events
    try
    {
        var agent1 = new MockAgent("s1", "mock", "Agent 1") { ResponseFunc = _ => "streaming-1" };
        var agent2 = new MockAgent("s2", "mock", "Agent 2") { ResponseFunc = _ => "streaming-2" };

        var orch = new SequentialOrchestrator();
        orch.AddAgents([agent1, agent2]);

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("go")))
        {
            events.Add(evt);
        }

        var eventTypes = events.Select(e => e.EventType).ToList();
        var hasStarted = eventTypes.Contains(OrchestrationEventType.Started);
        var hasCompleted = eventTypes.Contains(OrchestrationEventType.Completed);
        var agentStartCount = eventTypes.Count(t => t == OrchestrationEventType.AgentStarted);
        var agentCompletedCount = eventTypes.Count(t => t == OrchestrationEventType.AgentCompleted);

        var ok = hasStarted && hasCompleted && agentStartCount == 2 && agentCompletedCount == 2;

        results.Add(("seq-streaming-events", ok
            ? TestResult.Pass($"events: Started={hasStarted}, AgentStarted={agentStartCount}, AgentCompleted={agentCompletedCount}, Completed={hasCompleted}")
            : TestResult.Error($"events: {string.Join(",", eventTypes)}")));
    }
    catch (Exception ex) { results.Add(("seq-streaming-events", TestResult.Error(ex.Message))); }

    // 7-2: Parallel streaming events
    try
    {
        var agents = Enumerable.Range(1, 3).Select(i =>
        {
            var agent = new MockAgent($"p-{i}", "mock", $"Agent {i}");
            agent.ResponseFunc = _ => $"result-{i}";
            return (IAgent)agent;
        }).ToArray();

        var orch = new ParallelOrchestrator();
        orch.AddAgents(agents);

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("go")))
        {
            events.Add(evt);
        }

        var eventTypes = events.Select(e => e.EventType).ToList();
        var hasStarted = eventTypes.Contains(OrchestrationEventType.Started);
        var hasCompleted = eventTypes.Contains(OrchestrationEventType.Completed);
        var completedCount = eventTypes.Count(t => t == OrchestrationEventType.AgentCompleted);

        var ok = hasStarted && hasCompleted && completedCount == 3;

        results.Add(("parallel-streaming-events", ok
            ? TestResult.Pass($"Started+Completed, AgentCompleted={completedCount}")
            : TestResult.Error($"events: {string.Join(",", eventTypes)}")));
    }
    catch (Exception ex) { results.Add(("parallel-streaming-events", TestResult.Error(ex.Message))); }

    // 7-3: Graph streaming events
    try
    {
        var agentA = new MockAgent("GA", "mock", "A") { ResponseFunc = _ => "a-out" };
        var agentB = new MockAgent("GB", "mock", "B") { ResponseFunc = _ => "b-out" };

        var orch = new GraphOrchestratorBuilder()
            .AddNode("a", agentA)
            .AddNode("b", agentB)
            .AddEdge("a", "b")
            .SetStartNode("a")
            .SetOutputNode("b")
            .Build();

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("go")))
        {
            events.Add(evt);
        }

        var eventTypes = events.Select(e => e.EventType).ToList();
        var hasStarted = eventTypes.Contains(OrchestrationEventType.Started);
        var hasCompleted = eventTypes.Contains(OrchestrationEventType.Completed);
        var agentEvents = events.Where(e => e.AgentName != null).Select(e => e.AgentName).Distinct().ToList();

        var ok = hasStarted && hasCompleted && agentEvents.Contains("GA") && agentEvents.Contains("GB");

        results.Add(("graph-streaming-events", ok
            ? TestResult.Pass($"agents=[{string.Join(",", agentEvents)}]")
            : TestResult.Error($"events: {string.Join(",", eventTypes)}, agents: {string.Join(",", agentEvents)}")));
    }
    catch (Exception ex) { results.Add(("graph-streaming-events", TestResult.Error(ex.Message))); }

    return results;
}

// =============================================================================
// 8. Error Handling Tests
// =============================================================================

async Task<List<(string, TestResult)>> RunErrorTests()
{
    var results = new List<(string, TestResult)>();

    // 8-1: No agents registered
    try
    {
        var orch = new SequentialOrchestrator();
        var result = await orch.ExecuteAsync(MakeUserMessages("hello"));

        var ok = !result.IsSuccess && result.Error!.Contains("No agents");

        results.Add(("no-agents", ok
            ? TestResult.Pass($"Error: {result.Error}")
            : TestResult.Error($"IsSuccess={result.IsSuccess}, Error={result.Error}")));
    }
    catch (Exception ex) { results.Add(("no-agents", TestResult.Error(ex.Message))); }

    // 8-2: Agent failure with StopOnAgentFailure=true
    try
    {
        var executed = new List<string>();
        var agent1 = new MockAgent("fail", "mock", "Failing")
        {
            ResponseFuncAsync = _ => { executed.Add("fail"); throw new Exception("boom"); }
        };
        var agent2 = new MockAgent("after", "mock", "After")
        {
            ResponseFunc = _ => { executed.Add("after"); return "after-result"; }
        };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            StopOnAgentFailure = true
        });
        orch.AddAgents([agent1, agent2]);

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        var ok = !result.IsSuccess && executed.Contains("fail") && !executed.Contains("after");

        results.Add(("stop-on-failure", ok
            ? TestResult.Pass($"Stopped after failure, executed=[{string.Join(",", executed)}]")
            : TestResult.Error($"executed=[{string.Join(",", executed)}], IsSuccess={result.IsSuccess}")));
    }
    catch (Exception ex) { results.Add(("stop-on-failure", TestResult.Error(ex.Message))); }

    // 8-3: Agent failure with StopOnAgentFailure=false (continue)
    try
    {
        var executed = new List<string>();
        var agent1 = new MockAgent("fail", "mock", "Failing")
        {
            ResponseFuncAsync = _ => { executed.Add("fail"); throw new Exception("boom"); }
        };
        var agent2 = new MockAgent("continue", "mock", "Continue")
        {
            ResponseFunc = _ => { executed.Add("continue"); return "continued"; }
        };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            StopOnAgentFailure = false
        });
        orch.AddAgents([agent1, agent2]);

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        var ok = result.IsSuccess && executed.Contains("fail") && executed.Contains("continue");

        results.Add(("continue-on-failure", ok
            ? TestResult.Pass($"Continued after failure, executed=[{string.Join(",", executed)}]")
            : TestResult.Error($"executed=[{string.Join(",", executed)}], IsSuccess={result.IsSuccess}")));
    }
    catch (Exception ex) { results.Add(("continue-on-failure", TestResult.Error(ex.Message))); }

    // 8-4: Parallel — all fail
    try
    {
        var agents = Enumerable.Range(1, 3).Select(i =>
        {
            var agent = new MockAgent($"fail-{i}", "mock", $"Fail {i}");
            agent.ResponseFuncAsync = _ => throw new Exception($"error-{i}");
            return (IAgent)agent;
        }).ToArray();

        var orch = new ParallelOrchestrator();
        orch.AddAgents(agents);

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        var ok = !result.IsSuccess && result.Error!.Contains("All agents failed");

        results.Add(("parallel-all-fail", ok
            ? TestResult.Pass($"Error: {result.Error?[..Math.Min(60, result.Error.Length)]}")
            : TestResult.Error($"IsSuccess={result.IsSuccess}, Error={result.Error}")));
    }
    catch (Exception ex) { results.Add(("parallel-all-fail", TestResult.Error(ex.Message))); }

    // 8-5: Graph — no hub set on HubSpoke
    try
    {
        var orch = new HubSpokeOrchestrator();
        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        var ok = !result.IsSuccess && result.Error!.Contains("Hub agent is not set");

        results.Add(("hubspoke-no-hub", ok
            ? TestResult.Pass($"Error: {result.Error}")
            : TestResult.Error($"IsSuccess={result.IsSuccess}, Error={result.Error}")));
    }
    catch (Exception ex) { results.Add(("hubspoke-no-hub", TestResult.Error(ex.Message))); }

    return results;
}

// =============================================================================
// Mock Agent
// =============================================================================

IEnumerable<Message> MakeUserMessages(string text)
{
    return new List<Message>
    {
        new UserMessage { Content = [new TextMessageContent { Value = text }] }
    };
}

string GetLastUserText(IEnumerable<Message> messages)
{
    var last = messages.OfType<UserMessage>().LastOrDefault();
    if (last == null) return "";
    return last.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "";
}

string GetLastAssistantText(IEnumerable<Message> messages)
{
    var last = messages.OfType<AssistantMessage>().LastOrDefault();
    if (last == null) return "";
    return last.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "";
}

string GetTextFromMessage(Message? message)
{
    return message switch
    {
        AssistantMessage a => a.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "",
        UserMessage u => u.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "",
        _ => ""
    };
}

string GetTextFromResponse(MessageResponse response)
{
    return response.Message.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "";
}

string GetTextFromStepResult(AgentStepResult step)
{
    if (step.Response?.Message == null) return "";
    return step.Response.Message.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "";
}

// =============================================================================
// Mock Agent Implementation
// =============================================================================

class MockAgent : IAgent
{
    public string Provider { get; set; }
    public string Model { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string? Instructions { get; set; }

    public IEnumerable<ToolItem>? Tools { get; set; }
    public MessageGenerationParameters? Parameters { get; set; }

    /// <summary>
    /// Synchronous response function — return text directly.
    /// </summary>
    public Func<IEnumerable<Message>, string>? ResponseFunc { get; set; }

    /// <summary>
    /// Async response function — for simulating delays or failures.
    /// </summary>
    public Func<IEnumerable<Message>, Task<string>>? ResponseFuncAsync { get; set; }

    public MockAgent(string name, string provider, string description)
    {
        Name = name;
        Provider = provider;
        Model = "mock-model";
        Description = description;
    }

    public async Task<MessageResponse> InvokeAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default)
    {
        string responseText;

        if (ResponseFuncAsync != null)
        {
            responseText = await ResponseFuncAsync(messages);
        }
        else if (ResponseFunc != null)
        {
            responseText = ResponseFunc(messages);
        }
        else
        {
            responseText = $"MockAgent '{Name}' default response";
        }

        return new MessageResponse
        {
            Id = Guid.NewGuid().ToString("N"),
            DoneReason = MessageDoneReason.EndTurn,
            Message = new AssistantMessage
            {
                Name = Name,
                Content = [new TextMessageContent { Value = responseText }]
            },
            TokenUsage = new MessageTokenUsage
            {
                InputTokens = 10,
                OutputTokens = responseText.Length
            }
        };
    }

    public async IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IEnumerable<Message> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Simulate streaming: Begin → Delta chunks → Done
        var fullText = ResponseFunc != null ? ResponseFunc(messages) : $"MockAgent '{Name}' stream";

        yield return new StreamingMessageBeginResponse
        {
            Id = Guid.NewGuid().ToString("N")
        };

        // Split into chunks
        var chunkSize = Math.Max(1, fullText.Length / 3);
        var index = 0;
        for (var i = 0; i < fullText.Length; i += chunkSize)
        {
            var chunk = fullText.Substring(i, Math.Min(chunkSize, fullText.Length - i));
            yield return new StreamingContentDeltaResponse
            {
                Index = index,
                Delta = new TextDeltaContent { Value = chunk }
            };
            index++;
            await Task.Yield(); // Simulate async
        }

        yield return new StreamingMessageDoneResponse
        {
            Id = Guid.NewGuid().ToString("N"),
            DoneReason = MessageDoneReason.EndTurn,
            TokenUsage = new MessageTokenUsage { InputTokens = 10, OutputTokens = fullText.Length },
            Model = "mock-model",
            Timestamp = DateTime.UtcNow
        };
    }
}

// =============================================================================
// Simple Executor (for TypedPipeline tests)
// =============================================================================

class SimpleExecutor<TInput, TOutput> : ITypedExecutor<TInput, TOutput>
{
    private readonly Func<TInput, TOutput> _func;

    public string Name { get; }

    public SimpleExecutor(string name, Func<TInput, TOutput> func)
    {
        Name = name;
        _func = func;
    }

    public Task<TOutput> ExecuteAsync(TInput input, CancellationToken ct = default)
    {
        return Task.FromResult(_func(input));
    }
}

// =============================================================================
// TestResult Record
// =============================================================================

record TestResult(bool Success, string Message)
{
    public static TestResult Pass(string message) => new(true, message);
    public static TestResult Error(string error) => new(false, error);
}
