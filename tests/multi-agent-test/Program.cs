// =============================================================================
// IronHive Multi-Agent Orchestration Tests
// =============================================================================
// Tests orchestrator functionality: Sequential, Parallel, HubSpoke, Graph,
// Checkpointing, Human-in-the-Loop, TypedPipeline.
// Uses mock agents — no API keys or LLM calls required.
// =============================================================================

using System.Runtime.CompilerServices;
using System.Text;
using IronHive.Abstractions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Tools;
using Microsoft.Extensions.DependencyInjection;
using IronHive.Core;
using IronHive.Core.Agent;
using IronHive.Core.Agent.Orchestration;
using IronHive.Providers.OpenAI;
using IronHive.Providers.Anthropic;

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

// ---- Composability (Phase 1) ----
results.AddRange(await RunCategory("composability", RunComposabilityTests));

// ---- Handoff (Phase 2) ----
results.AddRange(await RunCategory("handoff", RunHandoffTests));

// ---- GroupChat (Phase 3) ----
results.AddRange(await RunCategory("groupchat", RunGroupChatTests));

// ---- Middleware (Phase 4) ----
results.AddRange(await RunCategory("middleware", RunMiddlewareTests));

// ---- Real LLM Integration (Cycle 3) ----
// These tests require API keys in .env file
results.AddRange(await RunCategory("llm-integration", RunLlmIntegrationTests));

// Summary
Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("SUMMARY");
Console.WriteLine(new string('=', 60));

var passed = results.Count(r => r.Result.Success && !r.Result.Skipped);
var skipped = results.Count(r => r.Result.Skipped);
var failed = results.Count(r => !r.Result.Success);

Console.WriteLine($"  Passed:  {passed}");
Console.WriteLine($"  Skipped: {skipped}");
Console.WriteLine($"  Failed:  {failed}");
Console.WriteLine($"  Total:   {results.Count}");

if (skipped > 0)
{
    Console.WriteLine("\nSkipped tests:");
    foreach (var (category, scenario, result) in results.Where(r => r.Result.Skipped))
        Console.WriteLine($"  - {category}/{scenario}: {result.Message}");
}

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

    // 2-4: FirstSuccess aggregation
    try
    {
        var callOrder = new List<string>();
        var agent1 = new MockAgent("slow", "mock", "Slow")
        {
            ResponseFuncAsync = async _ => { await Task.Delay(100); callOrder.Add("slow"); return "slow-result"; }
        };
        var agent2 = new MockAgent("fast", "mock", "Fast")
        {
            ResponseFunc = _ => { callOrder.Add("fast"); return "fast-result"; }
        };

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            ResultAggregation = ParallelResultAggregation.FirstSuccess
        });
        orch.AddAgents([agent1, agent2]);

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));
        var output = GetTextFromMessage(result.FinalOutput);

        // FirstSuccess should return the first agent's result (by order, not by completion time)
        var ok = result.IsSuccess && output.Contains("result");

        results.Add(("first-success", ok
            ? TestResult.Pass($"output={output}")
            : TestResult.Error($"output={output}")));
    }
    catch (Exception ex) { results.Add(("first-success", TestResult.Error(ex.Message))); }

    // 2-5: Fastest aggregation
    try
    {
        var agent1 = new MockAgent("slow", "mock", "Slow")
        {
            ResponseFuncAsync = async _ => { await Task.Delay(200); return "slow-result"; }
        };
        var agent2 = new MockAgent("fast", "mock", "Fast")
        {
            ResponseFuncAsync = async _ => { await Task.Delay(10); return "fast-result"; }
        };

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            ResultAggregation = ParallelResultAggregation.Fastest
        });
        orch.AddAgents([agent1, agent2]);

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));
        var output = GetTextFromMessage(result.FinalOutput);

        var ok = result.IsSuccess && output.Contains("fast-result");

        results.Add(("fastest", ok
            ? TestResult.Pass($"output={output}")
            : TestResult.Error($"output={output}, expected fast-result")));
    }
    catch (Exception ex) { results.Add(("fastest", TestResult.Error(ex.Message))); }

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
            ResponseFuncAsync = _ => { executed.Add("fail"); throw new InvalidOperationException("boom"); }
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
            ResponseFuncAsync = _ => { executed.Add("fail"); throw new InvalidOperationException("boom"); }
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
            agent.ResponseFuncAsync = _ => throw new InvalidOperationException($"error-{i}");
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
// 9. Composability Tests (Phase 1)
// =============================================================================

async Task<List<(string, TestResult)>> RunComposabilityTests()
{
    var results = new List<(string, TestResult)>();

    // 9-1: Orchestrator as Agent — nested in outer Sequential
    try
    {
        var innerAgent = new MockAgent("inner", "mock", "Inner agent")
        {
            ResponseFunc = _ => "inner-output"
        };
        var innerOrch = new SequentialOrchestrator();
        innerOrch.AddAgent(innerAgent);

        var outerAgent = new MockAgent("outer", "mock", "Outer agent")
        {
            ResponseFunc = msgs => $"got: {GetLastAssistantText(msgs)}"
        };

        var outerOrch = new SequentialOrchestrator(new SequentialOrchestratorOptions { PassOutputAsInput = true });
        outerOrch.AddAgents([innerOrch.AsAgent("nested"), outerAgent]);

        var result = await outerOrch.ExecuteAsync(MakeUserMessages("start"));

        var ok = result.IsSuccess
            && result.Steps.Count == 2
            && GetTextFromMessage(result.FinalOutput).Contains("got: inner-output");

        results.Add(("nested-orchestrator", ok
            ? TestResult.Pass($"output={GetTextFromMessage(result.FinalOutput)}")
            : TestResult.Error($"output={GetTextFromMessage(result.FinalOutput)}")));
    }
    catch (Exception ex) { results.Add(("nested-orchestrator", TestResult.Error(ex.Message))); }

    // 9-2: AsAgent streaming — forward events
    try
    {
        var agent = new MockAgent("stream-agent", "mock", "Streamer")
        {
            ResponseFunc = _ => "streamed"
        };
        var orch = new SequentialOrchestrator();
        orch.AddAgent(agent);

        var adapter = orch.AsAgent();
        var eventCount = 0;
        await foreach (var evt in adapter.InvokeStreamingAsync(MakeUserMessages("go")))
        {
            eventCount++;
        }

        var ok = eventCount > 0;
        results.Add(("streaming-forward", ok
            ? TestResult.Pass($"events={eventCount}")
            : TestResult.Error("No streaming events received")));
    }
    catch (Exception ex) { results.Add(("streaming-forward", TestResult.Error(ex.Message))); }

    // 9-3: AsAgent failure propagation
    try
    {
        var failAgent = new MockAgent("fail", "mock", "Fail")
        {
            ResponseFuncAsync = _ => throw new InvalidOperationException("boom")
        };
        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions { StopOnAgentFailure = true });
        orch.AddAgent(failAgent);

        var adapter = orch.AsAgent();
        var threw = false;
        try
        {
            await adapter.InvokeAsync(MakeUserMessages("go"));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("failed"))
        {
            threw = true;
        }

        results.Add(("failure-propagation", threw
            ? TestResult.Pass("Exception propagated correctly")
            : TestResult.Error("Expected exception was not thrown")));
    }
    catch (Exception ex) { results.Add(("failure-propagation", TestResult.Error(ex.Message))); }

    return results;
}

// =============================================================================
// 10. Handoff Tests (Phase 2)
// =============================================================================

async Task<List<(string, TestResult)>> RunHandoffTests()
{
    var results = new List<(string, TestResult)>();

    // 10-1: Triage → Billing → Triage → End
    try
    {
        var callSeq = new List<string>();
        var triage = new MockAgent("triage", "mock", "Triage")
        {
            ResponseFunc = _ =>
            {
                callSeq.Add("triage");
                return callSeq.Count == 1
                    ? """{"handoff_to": "billing", "context": "billing issue"}"""
                    : "Resolved!";
            }
        };
        var billing = new MockAgent("billing", "mock", "Billing")
        {
            ResponseFunc = _ =>
            {
                callSeq.Add("billing");
                return """{"handoff_to": "triage", "context": "done"}""";
            }
        };

        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(triage, new HandoffTarget { AgentName = "billing", Description = "Billing" })
            .AddAgent(billing, new HandoffTarget { AgentName = "triage", Description = "Triage" })
            .SetInitialAgent("triage")
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("help"));

        var ok = result.IsSuccess && callSeq.Count == 3
            && string.Join("→", callSeq) == "triage→billing→triage";

        results.Add(("basic-handoff", ok
            ? TestResult.Pass($"flow={string.Join("→", callSeq)}")
            : TestResult.Error($"flow={string.Join("→", callSeq)}, success={result.IsSuccess}")));
    }
    catch (Exception ex) { results.Add(("basic-handoff", TestResult.Error(ex.Message))); }

    // 10-2: MaxTransitions limit
    try
    {
        var a = new MockAgent("a", "mock", "A") { ResponseFunc = _ => """{"handoff_to": "b"}""" };
        var b = new MockAgent("b", "mock", "B") { ResponseFunc = _ => """{"handoff_to": "a"}""" };

        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(a, new HandoffTarget { AgentName = "b" })
            .AddAgent(b, new HandoffTarget { AgentName = "a" })
            .SetInitialAgent("a")
            .SetMaxTransitions(3)
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));
        var ok = result.IsSuccess && result.Steps.Count <= 5;

        results.Add(("max-transitions", ok
            ? TestResult.Pass($"steps={result.Steps.Count}")
            : TestResult.Error($"steps={result.Steps.Count}, success={result.IsSuccess}")));
    }
    catch (Exception ex) { results.Add(("max-transitions", TestResult.Error(ex.Message))); }

    // 10-3: Builder validation
    try
    {
        var threw = false;
        try
        {
            new HandoffOrchestratorBuilder()
                .AddAgent(new MockAgent("a", "mock", "A"), new HandoffTarget { AgentName = "nonexistent" })
                .SetInitialAgent("a")
                .Build();
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }

        results.Add(("builder-validation", threw
            ? TestResult.Pass("Invalid target detected")
            : TestResult.Error("Expected validation error")));
    }
    catch (Exception ex) { results.Add(("builder-validation", TestResult.Error(ex.Message))); }

    // 10-4: NoHandoffHandler
    try
    {
        var handlerCalled = false;
        var agent = new MockAgent("a", "mock", "A") { ResponseFunc = _ => "no handoff" };

        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(agent)
            .SetInitialAgent("a")
            .SetNoHandoffHandler((name, step) =>
            {
                handlerCalled = true;
                return Task.FromResult<Message?>(null);
            })
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        results.Add(("no-handoff-handler", handlerCalled
            ? TestResult.Pass("Handler called")
            : TestResult.Error("Handler not called")));
    }
    catch (Exception ex) { results.Add(("no-handoff-handler", TestResult.Error(ex.Message))); }

    // 10-5: Streaming with handoff events
    try
    {
        var triage = new MockAgent("triage", "mock", "Triage")
        {
            ResponseFunc = _ => """{"handoff_to": "specialist"}"""
        };
        var specialist = new MockAgent("specialist", "mock", "Specialist")
        {
            ResponseFunc = _ => "Done"
        };

        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(triage, new HandoffTarget { AgentName = "specialist" })
            .AddAgent(specialist, new HandoffTarget { AgentName = "triage" })
            .SetInitialAgent("triage")
            .Build();

        var eventTypes = new List<OrchestrationEventType>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("go")))
        {
            eventTypes.Add(evt.EventType);
        }

        var hasHandoff = eventTypes.Contains(OrchestrationEventType.Handoff);
        var hasCompleted = eventTypes.Contains(OrchestrationEventType.Completed);

        results.Add(("streaming-handoff", hasHandoff && hasCompleted
            ? TestResult.Pass($"events={string.Join(",", eventTypes)}")
            : TestResult.Error($"events={string.Join(",", eventTypes)}")));
    }
    catch (Exception ex) { results.Add(("streaming-handoff", TestResult.Error(ex.Message))); }

    return results;
}

// =============================================================================
// 11. GroupChat Tests (Phase 3)
// =============================================================================

async Task<List<(string, TestResult)>> RunGroupChatTests()
{
    var results = new List<(string, TestResult)>();

    // 11-1: RoundRobin 3 agents, 6 rounds
    try
    {
        var order = new List<string>();
        var a = new MockAgent("a", "mock", "A") { ResponseFunc = _ => { order.Add("a"); return "a-out"; } };
        var b = new MockAgent("b", "mock", "B") { ResponseFunc = _ => { order.Add("b"); return "b-out"; } };
        var c = new MockAgent("c", "mock", "C") { ResponseFunc = _ => { order.Add("c"); return "c-out"; } };

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(a).AddAgent(b).AddAgent(c)
            .WithRoundRobin()
            .TerminateAfterRounds(6)
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("start"));

        var ok = result.IsSuccess && order.Count == 6
            && string.Join(",", order) == "a,b,c,a,b,c";

        results.Add(("roundrobin", ok
            ? TestResult.Pass($"order={string.Join(",", order)}")
            : TestResult.Error($"order={string.Join(",", order)}, count={order.Count}")));
    }
    catch (Exception ex) { results.Add(("roundrobin", TestResult.Error(ex.Message))); }

    // 11-2: Keyword termination
    try
    {
        var callCount = 0;
        var agent = new MockAgent("critic", "mock", "Critic")
        {
            ResponseFunc = _ => { callCount++; return callCount >= 3 ? "APPROVED" : "needs work"; }
        };

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(agent)
            .WithRoundRobin()
            .TerminateOnKeyword("APPROVED")
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("review"));

        var ok = result.IsSuccess && callCount == 3
            && GetTextFromMessage(result.FinalOutput).Contains("APPROVED");

        results.Add(("keyword-termination", ok
            ? TestResult.Pass($"rounds={callCount}")
            : TestResult.Error($"rounds={callCount}, output={GetTextFromMessage(result.FinalOutput)}")));
    }
    catch (Exception ex) { results.Add(("keyword-termination", TestResult.Error(ex.Message))); }

    // 11-3: MaxRounds safety limit
    try
    {
        var agent = new MockAgent("talker", "mock", "Talker") { ResponseFunc = _ => "talking" };

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(agent)
            .WithRoundRobin()
            .TerminateAfterRounds(100)
            .SetMaxRounds(5)
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        var ok = result.IsSuccess && result.Steps.Count == 5;

        results.Add(("max-rounds", ok
            ? TestResult.Pass($"steps={result.Steps.Count}")
            : TestResult.Error($"steps={result.Steps.Count}")));
    }
    catch (Exception ex) { results.Add(("max-rounds", TestResult.Error(ex.Message))); }

    // 11-4: CompositeTermination (any)
    try
    {
        var callCount = 0;
        var agent = new MockAgent("worker", "mock", "Worker")
        {
            ResponseFunc = _ => { callCount++; return callCount >= 2 ? "DONE" : "working"; }
        };

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(agent)
            .WithRoundRobin()
            .WithTerminationCondition(new CompositeTermination(
                requireAll: false,
                new KeywordTermination("DONE"),
                new MaxRoundsTermination(10)))
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        var ok = result.IsSuccess && callCount == 2;

        results.Add(("composite-any", ok
            ? TestResult.Pass($"rounds={callCount}")
            : TestResult.Error($"rounds={callCount}")));
    }
    catch (Exception ex) { results.Add(("composite-any", TestResult.Error(ex.Message))); }

    // 11-5: Streaming with SpeakerSelected events
    try
    {
        var a = new MockAgent("a", "mock", "A") { ResponseFunc = _ => "hello" };
        var b = new MockAgent("b", "mock", "B") { ResponseFunc = _ => "world" };

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(a).AddAgent(b)
            .WithRoundRobin()
            .TerminateAfterRounds(2)
            .Build();

        var eventTypes = new List<OrchestrationEventType>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("go")))
        {
            eventTypes.Add(evt.EventType);
        }

        var speakerSelected = eventTypes.Count(e => e == OrchestrationEventType.SpeakerSelected);
        var ok = speakerSelected == 2 && eventTypes.Contains(OrchestrationEventType.Completed);

        results.Add(("streaming-speaker", ok
            ? TestResult.Pass($"SpeakerSelected={speakerSelected}")
            : TestResult.Error($"events={string.Join(",", eventTypes)}")));
    }
    catch (Exception ex) { results.Add(("streaming-speaker", TestResult.Error(ex.Message))); }

    return results;
}

// =============================================================================
// 12. Middleware Tests (Phase 4)
// =============================================================================

async Task<List<(string, TestResult)>> RunMiddlewareTests()
{
    var results = new List<(string, TestResult)>();

    // 12-1: Middleware chain execution order
    try
    {
        var order = new List<string>();
        var middleware1 = new TestOrderMiddleware("m1", order);
        var middleware2 = new TestOrderMiddleware("m2", order);

        var agent = new MockAgent("a", "mock", "A")
        {
            ResponseFunc = _ => { order.Add("agent"); return "result"; }
        };

        var wrapped = IronHive.Core.Agent.AgentExtensions.WithMiddleware(agent, middleware1, middleware2);
        await wrapped.InvokeAsync(MakeUserMessages("go"));

        var expected = "m1-before,m2-before,agent,m2-after,m1-after";
        var actual = string.Join(",", order);

        results.Add(("chain-order", actual == expected
            ? TestResult.Pass(actual)
            : TestResult.Error($"expected={expected}, actual={actual}")));
    }
    catch (Exception ex) { results.Add(("chain-order", TestResult.Error(ex.Message))); }

    // 12-2: Short-circuit middleware
    try
    {
        var agentCalled = false;
        var agent = new MockAgent("a", "mock", "A")
        {
            ResponseFunc = _ => { agentCalled = true; return "should not reach"; }
        };

        var middleware = new ShortCircuitMiddleware("intercepted");
        var wrapped = IronHive.Core.Agent.AgentExtensions.WithMiddleware(agent, middleware);
        var response = await wrapped.InvokeAsync(MakeUserMessages("go"));

        var text = response.Message.Content.OfType<TextMessageContent>().First().Value;
        var ok = !agentCalled && text == "intercepted";

        results.Add(("short-circuit", ok
            ? TestResult.Pass("Agent was not called, middleware returned response")
            : TestResult.Error($"agentCalled={agentCalled}, text={text}")));
    }
    catch (Exception ex) { results.Add(("short-circuit", TestResult.Error(ex.Message))); }

    // 12-3: Orchestrator-level middleware
    try
    {
        var middlewareCalls = 0;
        var middleware = new CountingMiddleware(() => middlewareCalls++);

        var a1 = new MockAgent("a1", "mock", "A1") { ResponseFunc = _ => "out1" };
        var a2 = new MockAgent("a2", "mock", "A2") { ResponseFunc = _ => "out2" };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            AgentMiddlewares = [middleware]
        });
        orch.AddAgents([a1, a2]);

        await orch.ExecuteAsync(MakeUserMessages("go"));

        var ok = middlewareCalls == 2; // once per agent

        results.Add(("orchestrator-middleware", ok
            ? TestResult.Pass($"middleware called {middlewareCalls} times")
            : TestResult.Error($"middleware called {middlewareCalls} times, expected 2")));
    }
    catch (Exception ex) { results.Add(("orchestrator-middleware", TestResult.Error(ex.Message))); }

    // 12-4: Properties delegation
    try
    {
        var inner = new MockAgent("test", "openai", "Test")
        {
            Model = "gpt-4",
            Instructions = "Be helpful"
        };
        var wrapped = IronHive.Core.Agent.AgentExtensions.WithMiddleware(
            inner, new TestOrderMiddleware("m", new List<string>()));

        var ok = wrapped.Name == "test" && wrapped.Provider == "openai"
            && wrapped.Model == "gpt-4" && wrapped.Instructions == "Be helpful";

        results.Add(("property-delegation", ok
            ? TestResult.Pass("All properties delegated correctly")
            : TestResult.Error($"Name={wrapped.Name}, Provider={wrapped.Provider}")));
    }
    catch (Exception ex) { results.Add(("property-delegation", TestResult.Error(ex.Message))); }

    // 12-5: RetryMiddleware - successful on first attempt
    try
    {
        var callCount = 0;
        var agent = new MockAgent("a", "mock", "A")
        {
            ResponseFunc = _ => { callCount++; return "success"; }
        };

        var middleware = new RetryMiddleware(3);
        var wrapped = IronHive.Core.Agent.AgentExtensions.WithMiddleware(agent, middleware);
        await wrapped.InvokeAsync(MakeUserMessages("go"));

        results.Add(("retry-first-success", callCount == 1
            ? TestResult.Pass("Single call on first success")
            : TestResult.Error($"callCount={callCount}, expected 1")));
    }
    catch (Exception ex) { results.Add(("retry-first-success", TestResult.Error(ex.Message))); }

    // 12-6: RetryMiddleware - retry on failure
    try
    {
        var retries = new List<int>();
        var failCount = 0;

        var middleware = new RetryMiddleware(new RetryMiddlewareOptions
        {
            MaxRetries = 3,
            InitialDelay = TimeSpan.FromMilliseconds(10),
            OnRetry = (name, attempt, ex, delay) => retries.Add(attempt)
        });

        // 2번 실패 후 성공
        var failingMiddleware = new FailAfterMiddleware(
            failUntil: 3,
            onFail: () => failCount++);

        var agent = new MockAgent("a", "mock", "A") { ResponseFunc = _ => "success" };
        var wrapped = IronHive.Core.Agent.AgentExtensions.WithMiddleware(agent, middleware, failingMiddleware);
        var response = await wrapped.InvokeAsync(MakeUserMessages("go"));

        var text = response.Message.Content.OfType<TextMessageContent>().First().Value;
        var ok = text == "success" && failCount == 2 && retries.Count == 2;

        results.Add(("retry-on-failure", ok
            ? TestResult.Pass($"Retried {retries.Count} times, failed {failCount} times")
            : TestResult.Error($"text={text}, failCount={failCount}, retries={string.Join(",", retries)}")));
    }
    catch (Exception ex) { results.Add(("retry-on-failure", TestResult.Error(ex.Message))); }

    // 12-7: RetryMiddleware - max retries exceeded
    try
    {
        var middleware = new RetryMiddleware(new RetryMiddlewareOptions
        {
            MaxRetries = 2,
            InitialDelay = TimeSpan.FromMilliseconds(10)
        });

        var alwaysFailMiddleware = new FailAfterMiddleware(failUntil: 100); // always fail
        var agent = new MockAgent("a", "mock", "A") { ResponseFunc = _ => "should not reach" };
        var wrapped = IronHive.Core.Agent.AgentExtensions.WithMiddleware(agent, middleware, alwaysFailMiddleware);

        var threw = false;
        try
        {
            await wrapped.InvokeAsync(MakeUserMessages("go"));
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }

        results.Add(("retry-max-exceeded", threw
            ? TestResult.Pass("Threw after max retries")
            : TestResult.Error("Did not throw after max retries")));
    }
    catch (Exception ex) { results.Add(("retry-max-exceeded", TestResult.Error(ex.Message))); }

    // 12-8: LoggingMiddleware - logs start and complete
    try
    {
        var logs = new List<string>();
        var middleware = new LoggingMiddleware(msg => logs.Add(msg));
        var agent = new MockAgent("TestAgent", "mock", "Test") { ResponseFunc = _ => "response" };
        var wrapped = IronHive.Core.Agent.AgentExtensions.WithMiddleware(agent, middleware);

        await wrapped.InvokeAsync(MakeUserMessages("hello"));

        var hasStart = logs.Any(l => l.Contains("starting"));
        var hasComplete = logs.Any(l => l.Contains("completed"));
        var hasAgentName = logs.Any(l => l.Contains("TestAgent"));
        var ok = logs.Count == 2 && hasStart && hasComplete && hasAgentName;

        results.Add(("logging-basic", ok
            ? TestResult.Pass($"Logged {logs.Count} messages")
            : TestResult.Error($"logs={string.Join("; ", logs)}")));
    }
    catch (Exception ex) { results.Add(("logging-basic", TestResult.Error(ex.Message))); }

    // 12-9: LoggingMiddleware - logs errors
    try
    {
        var logs = new List<string>();
        var middleware = new LoggingMiddleware(msg => logs.Add(msg));
        var failingMiddleware = new FailAfterMiddleware(failUntil: 100);
        var agent = new MockAgent("a", "mock", "A") { ResponseFunc = _ => "x" };
        var wrapped = IronHive.Core.Agent.AgentExtensions.WithMiddleware(agent, middleware, failingMiddleware);

        try { await wrapped.InvokeAsync(MakeUserMessages("go")); } catch { }

        var hasError = logs.Any(l => l.Contains("failed"));
        results.Add(("logging-error", hasError
            ? TestResult.Pass("Error logged correctly")
            : TestResult.Error($"logs={string.Join("; ", logs)}")));
    }
    catch (Exception ex) { results.Add(("logging-error", TestResult.Error(ex.Message))); }

    // 12-10: RetryMiddleware + LoggingMiddleware combined
    try
    {
        var logs = new List<string>();
        var retryMiddleware = new RetryMiddleware(new RetryMiddlewareOptions
        {
            MaxRetries = 2,
            InitialDelay = TimeSpan.FromMilliseconds(10)
        });
        var loggingMiddleware = new LoggingMiddleware(msg => logs.Add(msg));

        var failingMiddleware = new FailAfterMiddleware(failUntil: 2);
        var agent = new MockAgent("a", "mock", "A") { ResponseFunc = _ => "success" };

        // Logging wraps Retry wraps Failing wraps Agent
        var wrapped = IronHive.Core.Agent.AgentExtensions.WithMiddleware(
            agent, loggingMiddleware, retryMiddleware, failingMiddleware);

        var response = await wrapped.InvokeAsync(MakeUserMessages("go"));
        var text = response.Message.Content.OfType<TextMessageContent>().First().Value;

        // Logging should see 1 start and 1 complete (from its perspective, retry is internal)
        var ok = text == "success" && logs.Count == 2;

        results.Add(("retry-logging-combined", ok
            ? TestResult.Pass($"Combined middlewares work correctly")
            : TestResult.Error($"text={text}, logs={logs.Count}")));
    }
    catch (Exception ex) { results.Add(("retry-logging-combined", TestResult.Error(ex.Message))); }

    // 12-11: CachingMiddleware - cache hit
    try
    {
        var callCount = 0;
        var agent = new MockAgent("a", "mock", "A")
        {
            ResponseFunc = _ => { callCount++; return "cached"; }
        };

        var middleware = new CachingMiddleware(TimeSpan.FromMinutes(5));
        var wrapped = IronHive.Core.Agent.AgentExtensions.WithMiddleware(agent, middleware);

        await wrapped.InvokeAsync(MakeUserMessages("hello"));
        await wrapped.InvokeAsync(MakeUserMessages("hello")); // cache hit

        var ok = callCount == 1;
        results.Add(("caching-hit", ok
            ? TestResult.Pass("Agent called once, cache hit on second call")
            : TestResult.Error($"callCount={callCount}, expected 1")));
    }
    catch (Exception ex) { results.Add(("caching-hit", TestResult.Error(ex.Message))); }

    // 12-12: CachingMiddleware - cache miss on different input
    try
    {
        var callCount = 0;
        var agent = new MockAgent("a", "mock", "A")
        {
            ResponseFunc = _ => { callCount++; return $"resp-{callCount}"; }
        };

        var middleware = new CachingMiddleware(TimeSpan.FromMinutes(5));
        var wrapped = IronHive.Core.Agent.AgentExtensions.WithMiddleware(agent, middleware);

        await wrapped.InvokeAsync(MakeUserMessages("hello"));
        await wrapped.InvokeAsync(MakeUserMessages("world")); // different input

        var ok = callCount == 2;
        results.Add(("caching-miss", ok
            ? TestResult.Pass("Cache miss on different input")
            : TestResult.Error($"callCount={callCount}, expected 2")));
    }
    catch (Exception ex) { results.Add(("caching-miss", TestResult.Error(ex.Message))); }

    // 12-13: LoggingMiddleware streaming
    try
    {
        var logs = new List<string>();
        var middleware = new LoggingMiddleware(msg => logs.Add(msg));
        var agent = new MockAgent("TestAgent", "mock", "Test") { ResponseFunc = _ => "streamed" };
        var wrapped = IronHive.Core.Agent.AgentExtensions.WithMiddleware(agent, middleware);

        var events = new List<StreamingMessageResponse>();
        await foreach (var evt in wrapped.InvokeStreamingAsync(MakeUserMessages("hello")))
        {
            events.Add(evt);
        }

        var hasStreaming = logs.Any(l => l.Contains("streaming"));
        var hasCompleted = logs.Any(l => l.Contains("completed"));

        results.Add(("logging-streaming", hasStreaming && hasCompleted
            ? TestResult.Pass($"Streaming logged: {logs.Count} messages")
            : TestResult.Error($"logs={string.Join("; ", logs)}")));
    }
    catch (Exception ex) { results.Add(("logging-streaming", TestResult.Error(ex.Message))); }

    // 12-14: TimeoutMiddleware - success before timeout
    try
    {
        var middleware = new TimeoutMiddleware(TimeSpan.FromSeconds(5));
        var agent = new MockAgent("a", "mock", "A") { ResponseFunc = _ => "fast" };
        var wrapped = IronHive.Core.Agent.AgentExtensions.WithMiddleware(agent, middleware);

        var response = await wrapped.InvokeAsync(MakeUserMessages("go"));
        var text = response.Message.Content.OfType<TextMessageContent>().First().Value;

        results.Add(("timeout-success", text == "fast"
            ? TestResult.Pass("Completed before timeout")
            : TestResult.Error($"text={text}")));
    }
    catch (Exception ex) { results.Add(("timeout-success", TestResult.Error(ex.Message))); }

    // 12-15: TimeoutMiddleware - timeout throws
    try
    {
        var timeoutCalled = false;
        var middleware = new TimeoutMiddleware(new TimeoutMiddlewareOptions
        {
            Timeout = TimeSpan.FromMilliseconds(50),
            OnTimeout = (_, _) => timeoutCalled = true
        });

        var slowMiddleware = new SlowMiddleware(TimeSpan.FromSeconds(5));
        var agent = new MockAgent("a", "mock", "A") { ResponseFunc = _ => "slow" };
        var wrapped = IronHive.Core.Agent.AgentExtensions.WithMiddleware(agent, middleware, slowMiddleware);

        var threw = false;
        try
        {
            await wrapped.InvokeAsync(MakeUserMessages("go"));
        }
        catch (TimeoutException)
        {
            threw = true;
        }

        results.Add(("timeout-throws", threw && timeoutCalled
            ? TestResult.Pass("TimeoutException thrown")
            : TestResult.Error($"threw={threw}, timeoutCalled={timeoutCalled}")));
    }
    catch (Exception ex) { results.Add(("timeout-throws", TestResult.Error(ex.Message))); }

    // 12-16: RateLimitMiddleware - within limit
    try
    {
        var middleware = new RateLimitMiddleware(5, TimeSpan.FromSeconds(10));
        var agent = new MockAgent("a", "mock", "A") { ResponseFunc = _ => "ok" };
        var wrapped = IronHive.Core.Agent.AgentExtensions.WithMiddleware(agent, middleware);

        for (var i = 0; i < 5; i++)
        {
            await wrapped.InvokeAsync(MakeUserMessages($"req-{i}"));
        }

        results.Add(("ratelimit-within", middleware.CurrentRequestCount == 5
            ? TestResult.Pass("5 requests within limit")
            : TestResult.Error($"count={middleware.CurrentRequestCount}")));
    }
    catch (Exception ex) { results.Add(("ratelimit-within", TestResult.Error(ex.Message))); }

    // CircuitBreaker - 연속 실패 시 Open
    try
    {
        var cbMiddleware = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 2,
            BreakDuration = TimeSpan.FromMinutes(1)
        });
        var agent = new MockAgent("test-agent", "mock", "test") { ResponseFunc = _ => "ok" };
        var failCount = 0;

        // 2번 연속 실패
        for (int i = 0; i < 2; i++)
        {
            try
            {
                await cbMiddleware.InvokeAsync(agent, MakeUserMessages("test"),
                    _ => throw new InvalidOperationException($"fail-{++failCount}"));
            }
            catch (InvalidOperationException) { }
        }

        var isOpen = cbMiddleware.State == CircuitState.Open;

        // Open 상태에서 요청 시 예외 발생 확인
        var rejected = false;
        try
        {
            await cbMiddleware.InvokeAsync(agent, MakeUserMessages("test"),
                _ => Task.FromResult(MakeMessageResponse("should-not-reach")));
        }
        catch (CircuitBreakerOpenException) { rejected = true; }

        results.Add(("circuitbreaker-open", isOpen && rejected
            ? TestResult.Pass("Circuit opened after 2 failures, requests rejected")
            : TestResult.Error($"isOpen={isOpen}, rejected={rejected}")));
    }
    catch (Exception ex) { results.Add(("circuitbreaker-open", TestResult.Error(ex.Message))); }

    // CircuitBreaker - Half-Open 전환 후 성공 시 Closed
    try
    {
        var stateChanges = new List<string>();
        var cbMiddleware = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 1,
            BreakDuration = TimeSpan.FromMilliseconds(50),
            OnStateChanged = (from, to) => stateChanges.Add($"{from}->{to}")
        });
        var agent = new MockAgent("test-agent", "mock", "test") { ResponseFunc = _ => "ok" };

        // 1번 실패로 Open
        try
        {
            await cbMiddleware.InvokeAsync(agent, MakeUserMessages("test"),
                _ => throw new InvalidOperationException("fail"));
        }
        catch (InvalidOperationException) { }

        // Break duration 대기
        await Task.Delay(100);
        var isHalfOpen = cbMiddleware.State == CircuitState.HalfOpen;

        // 성공하면 Closed
        await cbMiddleware.InvokeAsync(agent, MakeUserMessages("test"),
            _ => Task.FromResult(MakeMessageResponse("ok")));

        var isClosed = cbMiddleware.State == CircuitState.Closed;

        results.Add(("circuitbreaker-recovery", isHalfOpen && isClosed
            ? TestResult.Pass($"Recovered: {string.Join(",", stateChanges)}")
            : TestResult.Error($"halfOpen={isHalfOpen}, closed={isClosed}")));
    }
    catch (Exception ex) { results.Add(("circuitbreaker-recovery", TestResult.Error(ex.Message))); }

    // Bulkhead - 동시성 제한
    try
    {
        var bulkhead = new BulkheadMiddleware(maxConcurrency: 2);
        var agent = new MockAgent("test-agent", "mock", "test") { ResponseFunc = _ => "ok" };
        var executing = 0;
        var maxExecuting = 0;

        var tasks = Enumerable.Range(0, 5).Select(async i =>
        {
            await bulkhead.InvokeAsync(agent, MakeUserMessages($"test-{i}"),
                async _ =>
                {
                    var current = Interlocked.Increment(ref executing);
                    lock (bulkhead) { if (current > maxExecuting) maxExecuting = current; }
                    await Task.Delay(30);
                    Interlocked.Decrement(ref executing);
                    return MakeMessageResponse("ok");
                });
        }).ToArray();

        await Task.WhenAll(tasks);

        results.Add(("bulkhead-concurrency", maxExecuting <= 2
            ? TestResult.Pass($"Max concurrent={maxExecuting} (limit=2)")
            : TestResult.Error($"maxConcurrent={maxExecuting}, exceeded limit")));
    }
    catch (Exception ex) { results.Add(("bulkhead-concurrency", TestResult.Error(ex.Message))); }

    // Bulkhead - 대기열 초과 시 거부
    try
    {
        var rejected = false;
        var bulkhead = new BulkheadMiddleware(new BulkheadMiddlewareOptions
        {
            MaxConcurrency = 1,
            MaxQueueSize = 1,
            OnRejected = (_, _, _) => rejected = true
        });
        var agent = new MockAgent("test-agent", "mock", "test") { ResponseFunc = _ => "ok" };
        var gate = new TaskCompletionSource<bool>();

        // 첫 번째: 실행 슬롯 점유
        var task1 = bulkhead.InvokeAsync(agent, MakeUserMessages("test-1"),
            async _ => { await gate.Task; return MakeMessageResponse("ok"); });
        await Task.Delay(20);

        // 두 번째: 대기열 점유
        var task2 = bulkhead.InvokeAsync(agent, MakeUserMessages("test-2"),
            async _ => { await gate.Task; return MakeMessageResponse("ok"); });
        await Task.Delay(20);

        // 세 번째: 거부되어야 함
        var wasRejected = false;
        try
        {
            await bulkhead.InvokeAsync(agent, MakeUserMessages("test-3"),
                _ => Task.FromResult(MakeMessageResponse("should-not-reach")));
        }
        catch (BulkheadRejectedException) { wasRejected = true; }

        gate.SetResult(true);
        await Task.WhenAll(task1, task2);

        results.Add(("bulkhead-reject", wasRejected && rejected
            ? TestResult.Pass("Request rejected when queue full")
            : TestResult.Error($"rejected={wasRejected}, callback={rejected}")));
    }
    catch (Exception ex) { results.Add(("bulkhead-reject", TestResult.Error(ex.Message))); }

    // FallbackMiddleware - 실패 시 폴백
    try
    {
        var fallbackCalled = false;
        var fallbackAgent = new MockAgent("fallback", "mock", "Fallback")
        {
            ResponseFunc = _ => { fallbackCalled = true; return "fallback-result"; }
        };
        var middleware = new FallbackMiddleware(fallbackAgent);
        var primaryAgent = new MockAgent("primary", "mock", "Primary");

        var response = await middleware.InvokeAsync(
            primaryAgent,
            MakeUserMessages("test"),
            _ => throw new InvalidOperationException("primary failed"));

        var text = response.Message.Content.OfType<TextMessageContent>().First().Value;

        results.Add(("fallback-on-failure", fallbackCalled && text == "fallback-result"
            ? TestResult.Pass("Fallback executed successfully")
            : TestResult.Error($"fallbackCalled={fallbackCalled}, text={text}")));
    }
    catch (Exception ex) { results.Add(("fallback-on-failure", TestResult.Error(ex.Message))); }

    // FallbackMiddleware - 성공 시 폴백 안함
    try
    {
        var fallbackCalled = false;
        var fallbackAgent = new MockAgent("fallback", "mock", "Fallback")
        {
            ResponseFunc = _ => { fallbackCalled = true; return "fallback"; }
        };
        var middleware = new FallbackMiddleware(fallbackAgent);
        var primaryAgent = new MockAgent("primary", "mock", "Primary")
        {
            ResponseFunc = _ => "primary-result"
        };

        var response = await middleware.InvokeAsync(
            primaryAgent,
            MakeUserMessages("test"),
            msgs => primaryAgent.InvokeAsync(msgs));

        var text = response.Message.Content.OfType<TextMessageContent>().First().Value;

        results.Add(("fallback-no-call", !fallbackCalled && text == "primary-result"
            ? TestResult.Pass("Fallback not called on success")
            : TestResult.Error($"fallbackCalled={fallbackCalled}, text={text}")));
    }
    catch (Exception ex) { results.Add(("fallback-no-call", TestResult.Error(ex.Message))); }

    // CompositeMiddleware - 실행 순서
    try
    {
        var order = new List<string>();
        var m1 = new TestOrderMiddleware("m1", order);
        var m2 = new TestOrderMiddleware("m2", order);
        var composite = new CompositeMiddleware("test-pack", m1, m2);

        var agent = new MockAgent("a", "mock", "A")
        {
            ResponseFunc = _ => { order.Add("agent"); return "result"; }
        };

        await composite.InvokeAsync(agent, MakeUserMessages("test"), msgs => agent.InvokeAsync(msgs));

        var expected = new[] { "m1-before", "m2-before", "agent", "m2-after", "m1-after" };
        var ok = order.SequenceEqual(expected);

        results.Add(("composite-order", ok
            ? TestResult.Pass($"Order: {string.Join(",", order)}")
            : TestResult.Error($"Order: {string.Join(",", order)}")));
    }
    catch (Exception ex) { results.Add(("composite-order", TestResult.Error(ex.Message))); }

    // MiddlewarePacks.Resilience
    try
    {
        var pack = MiddlewarePacks.Resilience(maxRetries: 2, timeout: TimeSpan.FromSeconds(5));

        results.Add(("pack-resilience", pack.Name == "resilience" && pack.Count == 2
            ? TestResult.Pass($"name={pack.Name}, count={pack.Count}")
            : TestResult.Error($"name={pack.Name}, count={pack.Count}")));
    }
    catch (Exception ex) { results.Add(("pack-resilience", TestResult.Error(ex.Message))); }

    // MiddlewarePacks.Production
    try
    {
        var pack = MiddlewarePacks.Production();

        results.Add(("pack-production", pack.Name == "production" && pack.Count == 5
            ? TestResult.Pass($"name={pack.Name}, count={pack.Count}")
            : TestResult.Error($"name={pack.Name}, count={pack.Count}")));
    }
    catch (Exception ex) { results.Add(("pack-production", TestResult.Error(ex.Message))); }

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

MessageResponse MakeMessageResponse(string text)
{
    return new MessageResponse
    {
        Id = Guid.NewGuid().ToString("N"),
        DoneReason = MessageDoneReason.EndTurn,
        Message = new AssistantMessage
        {
            Name = "mock",
            Content = [new TextMessageContent { Value = text }]
        }
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
// 13. LLM Integration Tests (Cycle 3)
// =============================================================================

async Task<List<(string, TestResult)>> RunLlmIntegrationTests()
{
    var results = new List<(string, TestResult)>();

    // Load .env file
    LoadEnvFile();

    var openAiKey = GetEnv("OPENAI_API_KEY");
    var anthropicKey = GetEnv("ANTHROPIC_API_KEY");

    // Check if any API key is available
    if ((string.IsNullOrWhiteSpace(openAiKey) || openAiKey.StartsWith("sk-xxxx", StringComparison.Ordinal)) &&
        (string.IsNullOrWhiteSpace(anthropicKey) || anthropicKey.StartsWith("sk-ant-xxxx", StringComparison.Ordinal)))
    {
        results.Add(("skip", TestResult.Skip("No API keys configured (OPENAI_API_KEY or ANTHROPIC_API_KEY)")));
        return results;
    }

    // Create HiveService with available providers
    var builder = new HiveServiceBuilder();

    string? primaryProvider = null;
    string? primaryModel = null;

    if (!string.IsNullOrWhiteSpace(openAiKey) && !openAiKey.StartsWith("sk-xxxx", StringComparison.Ordinal))
    {
        builder.AddOpenAIProviders("openai", new OpenAIConfig { ApiKey = openAiKey }, OpenAIServiceType.Responses);
        primaryProvider = "openai";
        primaryModel = GetEnv("OPENAI_MODEL") ?? "gpt-4o-mini";
    }

    if (!string.IsNullOrWhiteSpace(anthropicKey) && !anthropicKey.StartsWith("sk-ant-xxxx", StringComparison.Ordinal))
    {
        builder.AddAnthropicProviders("anthropic", new AnthropicConfig { ApiKey = anthropicKey });
        if (primaryProvider == null)
        {
            primaryProvider = "anthropic";
            primaryModel = GetEnv("ANTHROPIC_MODEL") ?? "claude-3-5-haiku-latest";
        }
    }

    var hive = builder.Build();
    var messageService = hive.Services.GetRequiredService<IMessageService>();

    // Helper to create real LLM agent
    IAgent CreateLlmAgent(string name, string description, string? instructions = null)
    {
        var agent = new BasicAgent(messageService)
        {
            Provider = primaryProvider!,
            Model = primaryModel!,
            Name = name,
            Description = description,
            Instructions = instructions,
            Parameters = new MessageGenerationParameters { MaxTokens = 150 }
        };

        // LLM API 일시적 오류에 대한 재시도 로직 적용
        return agent.WithMiddleware(new RetryMiddleware(new RetryMiddlewareOptions
        {
            MaxRetries = 2,
            InitialDelay = TimeSpan.FromSeconds(1),
            MaxDelay = TimeSpan.FromSeconds(5),
            BackoffMultiplier = 2.0,
            ShouldRetry = ex => ex.Message.Contains("error occurred") ||
                                ex.Message.Contains("rate limit") ||
                                ex.Message.Contains("500") ||
                                ex.Message.Contains("503") ||
                                ex.Message.Contains("timeout"),
            OnRetry = (agentName, attempt, ex, delay) =>
                Console.WriteLine($"      [Retry] {agentName}: attempt {attempt}, waiting {delay.TotalSeconds:F1}s...")
        }));
    }

    // 13-1: LLM Handoff — Triage → Specialist → Response
    try
    {
        var triageAgent = CreateLlmAgent(
            "triage",
            "Initial triage agent",
            """
            You are a triage agent. Analyze the user's request.
            If it's a technical question, respond with EXACTLY this JSON (no other text):
            {"handoff_to": "tech-support", "context": "User has a technical question"}
            If it's a general question, answer directly without JSON.
            """);

        var techAgent = CreateLlmAgent(
            "tech-support",
            "Technical support specialist",
            "You are a technical support specialist. Provide a brief, helpful answer to technical questions. Keep it under 50 words.");

        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(triageAgent, new HandoffTarget { AgentName = "tech-support", Description = "Technical support" })
            .AddAgent(techAgent, new HandoffTarget { AgentName = "triage", Description = "Return to triage" })
            .SetInitialAgent("triage")
            .SetMaxTransitions(3)
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("How do I fix a null reference exception in C#?"));

        var ok = result.IsSuccess && result.Steps.Count >= 2;
        var finalText = GetTextFromMessage(result.FinalOutput);

        results.Add(("llm-handoff", ok
            ? TestResult.Pass($"steps={result.Steps.Count}, output={finalText[..Math.Min(80, finalText.Length)]}...")
            : TestResult.Error($"steps={result.Steps.Count}, success={result.IsSuccess}, error={result.Error}")));
    }
    catch (Exception ex) { results.Add(("llm-handoff", TestResult.Error(ex.Message))); }

    // 13-2: LLM GroupChat — Two critics reviewing content
    try
    {
        var writer = CreateLlmAgent(
            "writer",
            "Content writer",
            "You are a content writer. Write a single sentence about artificial intelligence. Keep it brief.");

        var critic1 = CreateLlmAgent(
            "grammar-critic",
            "Grammar reviewer",
            """
            You are a grammar critic. Review the previous message for grammar issues.
            If the grammar is perfect, respond with exactly: APPROVED
            Otherwise, suggest one specific improvement.
            """);

        var critic2 = CreateLlmAgent(
            "style-critic",
            "Style reviewer",
            """
            You are a style critic. Review the previous message for clarity and style.
            If the style is excellent, respond with exactly: APPROVED
            Otherwise, suggest one specific improvement.
            """);

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(writer)
            .AddAgent(critic1)
            .AddAgent(critic2)
            .WithRoundRobin()
            .WithTerminationCondition(new CompositeTermination(
                requireAll: false,
                new KeywordTermination("APPROVED"),
                new MaxRoundsTermination(4)))
            .SetMaxRounds(6)
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("Write and review a sentence about AI."));

        var ok = result.IsSuccess && result.Steps.Count >= 2;
        var finalText = GetTextFromMessage(result.FinalOutput);

        results.Add(("llm-groupchat", ok
            ? TestResult.Pass($"rounds={result.Steps.Count}, output={finalText[..Math.Min(80, finalText.Length)]}...")
            : TestResult.Error($"rounds={result.Steps.Count}, success={result.IsSuccess}, error={result.Error}")));
    }
    catch (Exception ex) { results.Add(("llm-groupchat", TestResult.Error(ex.Message))); }

    // 13-3: LLM SpeakerSelector — LLM chooses next speaker
    try
    {
        var mathAgent = CreateLlmAgent(
            "math-expert",
            "Mathematics expert for calculations",
            "You are a math expert. Only answer math questions. Keep answers brief.");

        var historyAgent = CreateLlmAgent(
            "history-expert",
            "History expert for historical questions",
            "You are a history expert. Only answer history questions. Keep answers brief.");

        var selectorAgent = CreateLlmAgent(
            "selector",
            "Agent that selects the next speaker",
            null); // LlmSpeakerSelector will provide its own instructions

        var selector = new LlmSpeakerSelector(selectorAgent);

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(mathAgent)
            .AddAgent(historyAgent)
            .WithSpeakerSelector(selector)
            .TerminateAfterRounds(1)
            .Build();

        // Ask a math question — should select math-expert
        var result = await orch.ExecuteAsync(MakeUserMessages("What is 15 multiplied by 7?"));

        var selectedAgent = (result.Steps.Count > 0 ? result.Steps[0] : null)?.AgentName;
        var ok = result.IsSuccess && selectedAgent == "math-expert";

        results.Add(("llm-speaker-math", ok
            ? TestResult.Pass($"Selected: {selectedAgent} (correct)")
            : TestResult.Error($"Selected: {selectedAgent}, expected: math-expert")));

        // Ask a history question — should select history-expert
        var result2 = await orch.ExecuteAsync(MakeUserMessages("When did World War II end?"));

        var selectedAgent2 = (result2.Steps.Count > 0 ? result2.Steps[0] : null)?.AgentName;
        var ok2 = result2.IsSuccess && selectedAgent2 == "history-expert";

        results.Add(("llm-speaker-history", ok2
            ? TestResult.Pass($"Selected: {selectedAgent2} (correct)")
            : TestResult.Error($"Selected: {selectedAgent2}, expected: history-expert, success={result2.IsSuccess}")));
    }
    catch (Exception ex)
    {
        results.Add(("llm-speaker-math", TestResult.Error(ex.Message)));
        results.Add(("llm-speaker-history", TestResult.Error(ex.Message)));
    }

    // 13-4: Handoff streaming with real LLM
    try
    {
        var agent1 = CreateLlmAgent(
            "greeter",
            "Greeting agent",
            """
            You are a greeter agent. Your ONLY job is to greet briefly and then IMMEDIATELY hand off to the helper.

            IMPORTANT: After your greeting, you MUST respond with ONLY this exact JSON on its own line:
            {"handoff_to": "helper", "context": "User needs assistance"}

            Example response:
            Hello! Welcome!
            {"handoff_to": "helper", "context": "User needs assistance"}

            Do NOT add any text after the JSON. The JSON must be the last thing in your response.
            """);

        var agent2 = CreateLlmAgent(
            "helper",
            "Helper agent",
            "You are a helper. Provide a brief, helpful response. End with 'Hope this helps!'");

        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(agent1, new HandoffTarget { AgentName = "helper", Description = "Helper" })
            .AddAgent(agent2, new HandoffTarget { AgentName = "greeter", Description = "Greeter" })
            .SetInitialAgent("greeter")
            .Build();

        var eventTypes = new List<OrchestrationEventType>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("Hello!")))
        {
            eventTypes.Add(evt.EventType);
        }

        var hasHandoff = eventTypes.Contains(OrchestrationEventType.Handoff);
        var hasCompleted = eventTypes.Contains(OrchestrationEventType.Completed);
        var agentStartedCount = eventTypes.Count(e => e == OrchestrationEventType.AgentStarted);

        var ok = hasHandoff && hasCompleted && agentStartedCount >= 2;

        results.Add(("llm-streaming-handoff", ok
            ? TestResult.Pass($"events: Handoff={hasHandoff}, AgentStarted={agentStartedCount}")
            : TestResult.Error($"Handoff={hasHandoff}, Completed={hasCompleted}, AgentStarted={agentStartedCount}")));
    }
    catch (Exception ex) { results.Add(("llm-streaming-handoff", TestResult.Error(ex.Message))); }

    return results;
}

// =============================================================================
// Environment Helpers
// =============================================================================

void LoadEnvFile()
{
    // Try multiple locations for .env file
    var searchPaths = new[]
    {
        Path.Combine(Directory.GetCurrentDirectory(), ".env"),
        Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", ".env"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env"),
        @"D:\data\ironhive\.env"
    };

    foreach (var envPath in searchPaths)
    {
        if (File.Exists(envPath))
        {
            foreach (var line in File.ReadAllLines(envPath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                    continue;

                var idx = trimmed.IndexOf('=');
                if (idx > 0)
                {
                    var key = trimmed[..idx].Trim();
                    var value = trimmed[(idx + 1)..].Trim().Trim('"', '\'');
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
            break;
        }
    }
}

string? GetEnv(string key) => Environment.GetEnvironmentVariable(key);

// =============================================================================
// Mock Agent Implementation
// =============================================================================

sealed class MockAgent : IAgent
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

sealed class SimpleExecutor<TInput, TOutput> : ITypedExecutor<TInput, TOutput>
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

sealed record TestResult(bool Success, string Message, bool Skipped = false)
{
    public static TestResult Pass(string message) => new(true, message);
    public static TestResult Error(string error) => new(false, error);
    public static TestResult Skip(string reason) => new(true, reason, true);
}

// =============================================================================
// Middleware Helper Classes
// =============================================================================

sealed class TestOrderMiddleware : IAgentMiddleware
{
    private readonly string _name;
    private readonly List<string> _order;

    public TestOrderMiddleware(string name, List<string> order)
    {
        _name = name;
        _order = order;
    }

    public async Task<MessageResponse> InvokeAsync(
        IAgent agent, IEnumerable<Message> messages,
        Func<IEnumerable<Message>, Task<MessageResponse>> next,
        CancellationToken ct = default)
    {
        _order.Add($"{_name}-before");
        var result = await next(messages);
        _order.Add($"{_name}-after");
        return result;
    }
}

sealed class ShortCircuitMiddleware : IAgentMiddleware
{
    private readonly string _response;
    public ShortCircuitMiddleware(string response) { _response = response; }

    public Task<MessageResponse> InvokeAsync(
        IAgent agent, IEnumerable<Message> messages,
        Func<IEnumerable<Message>, Task<MessageResponse>> next,
        CancellationToken ct = default)
    {
        return Task.FromResult(new MessageResponse
        {
            Id = Guid.NewGuid().ToString("N"),
            DoneReason = MessageDoneReason.EndTurn,
            Message = new AssistantMessage
            {
                Content = [new TextMessageContent { Value = _response }]
            }
        });
    }
}

sealed class CountingMiddleware : IAgentMiddleware
{
    private readonly Action _onCall;
    public CountingMiddleware(Action onCall) { _onCall = onCall; }

    public async Task<MessageResponse> InvokeAsync(
        IAgent agent, IEnumerable<Message> messages,
        Func<IEnumerable<Message>, Task<MessageResponse>> next,
        CancellationToken ct = default)
    {
        _onCall();
        return await next(messages);
    }
}

/// <summary>
/// 지정된 횟수만큼 실패한 후 성공하는 미들웨어
/// </summary>
sealed class FailAfterMiddleware : IAgentMiddleware
{
    private readonly int _failUntil;
    private readonly Action? _onFail;
    private int _callCount;

    public FailAfterMiddleware(int failUntil, Action? onFail = null)
    {
        _failUntil = failUntil;
        _onFail = onFail;
    }

    public Task<MessageResponse> InvokeAsync(
        IAgent agent, IEnumerable<Message> messages,
        Func<IEnumerable<Message>, Task<MessageResponse>> next,
        CancellationToken ct = default)
    {
        _callCount++;
        if (_callCount < _failUntil)
        {
            _onFail?.Invoke();
            throw new InvalidOperationException($"Simulated failure {_callCount}");
        }
        return next(messages);
    }
}

/// <summary>
/// 지정된 시간만큼 지연시키는 미들웨어 (테스트용)
/// </summary>
sealed class SlowMiddleware : IAgentMiddleware
{
    private readonly TimeSpan _delay;

    public SlowMiddleware(TimeSpan delay)
    {
        _delay = delay;
    }

    public async Task<MessageResponse> InvokeAsync(
        IAgent agent, IEnumerable<Message> messages,
        Func<IEnumerable<Message>, Task<MessageResponse>> next,
        CancellationToken ct = default)
    {
        await Task.Delay(_delay, ct);
        return await next(messages);
    }
}
