using FluentAssertions;
using IronHive.Abstractions.Workflow;
using IronHive.Core.Workflow;

namespace IronHive.Tests.Workflow;

public class WorkflowEngineTests
{
    #region RunAsync — Sequential Execution

    [Fact]
    public async Task RunAsync_SequentialTasks_ExecutesInOrder()
    {
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["step1"] = new AppendStep("A"),
            ["step2"] = new AppendStep("B"),
            ["step3"] = new AppendStep("C")
        };
        var engine = CreateEngine(steps,
            new TaskNode { Id = "n1", Step = "step1" },
            new TaskNode { Id = "n2", Step = "step2" },
            new TaskNode { Id = "n3", Step = "step3" });

        var ctx = new TestContext();
        await engine.RunAsync(ctx);

        ctx.Log.Should().Equal("A", "B", "C");
    }

    [Fact]
    public async Task RunAsync_EmptyNodes_CompletesWithoutError()
    {
        var engine = CreateEngine(new Dictionary<string, IWorkflowStep>());

        var ctx = new TestContext();
        await engine.RunAsync(ctx);

        ctx.Log.Should().BeEmpty();
    }

    [Fact]
    public async Task RunAsync_UnregisteredStep_RaisesFailedEvent()
    {
        var engine = CreateEngine(
            new Dictionary<string, IWorkflowStep>(),
            new TaskNode { Id = "n1", Step = "missing" });

        var events = new List<WorkflowProgressType>();
        engine.Progressed += (_, e) => events.Add(e.Type);

        var ctx = new TestContext();
        await engine.RunAsync(ctx);

        events.Should().Contain(WorkflowProgressType.Failed);
    }

    #endregion

    #region RunAsync — Events

    [Fact]
    public async Task RunAsync_Success_FiresStartedProgressedCompleted()
    {
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["step1"] = new AppendStep("A")
        };
        var engine = CreateEngine(steps,
            new TaskNode { Id = "n1", Step = "step1" });

        var events = new List<WorkflowProgressType>();
        engine.Progressed += (_, e) => events.Add(e.Type);

        await engine.RunAsync(new TestContext());

        events.First().Should().Be(WorkflowProgressType.Started);
        events.Last().Should().Be(WorkflowProgressType.Completed);
        events.Should().Contain(WorkflowProgressType.Progressed);
    }

    [Fact]
    public async Task RunAsync_ProgressedEvents_IncludeNodeIdAndStepName()
    {
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["myStep"] = new AppendStep("X")
        };
        var engine = CreateEngine(steps,
            new TaskNode { Id = "myNode", Step = "myStep" });

        var progressEvents = new List<WorkflowEventArgs<TestContext>>();
        engine.Progressed += (_, e) =>
        {
            if (e.Type == WorkflowProgressType.Progressed)
                progressEvents.Add(e);
        };

        await engine.RunAsync(new TestContext());

        progressEvents.Should().HaveCount(2); // before and after execution
        progressEvents[0].NodeId.Should().Be("myNode");
        progressEvents[0].StepName.Should().Be("myStep");
    }

    #endregion

    #region RunAsync — Error Handling

    [Fact]
    public async Task RunAsync_TaskStepResultFail_RaisesFailedEvent()
    {
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["fail"] = new FailStep()
        };
        var engine = CreateEngine(steps,
            new TaskNode { Id = "n1", Step = "fail" });

        var failedEvents = new List<WorkflowEventArgs<TestContext>>();
        engine.Progressed += (_, e) =>
        {
            if (e.Type == WorkflowProgressType.Failed)
                failedEvents.Add(e);
        };

        await engine.RunAsync(new TestContext());

        failedEvents.Should().HaveCount(1);
        failedEvents[0].Exception.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task RunAsync_StepThrowsException_RaisesFailedEvent()
    {
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["throw"] = new ThrowStep()
        };
        var engine = CreateEngine(steps,
            new TaskNode { Id = "n1", Step = "throw" });

        var events = new List<WorkflowProgressType>();
        engine.Progressed += (_, e) => events.Add(e.Type);

        await engine.RunAsync(new TestContext());

        events.Should().Contain(WorkflowProgressType.Failed);
    }

    [Fact]
    public async Task RunAsync_FailedTaskStopsFurtherExecution()
    {
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["step1"] = new AppendStep("A"),
            ["fail"] = new FailStep(),
            ["step3"] = new AppendStep("C")
        };
        var engine = CreateEngine(steps,
            new TaskNode { Step = "step1" },
            new TaskNode { Step = "fail" },
            new TaskNode { Step = "step3" });

        var ctx = new TestContext();
        await engine.RunAsync(ctx);

        ctx.Log.Should().Equal("A"); // C should NOT be executed
    }

    #endregion

    #region RunAsync — Cancellation

    [Fact]
    public async Task RunAsync_CancellationRequested_RaisesCancelledEvent()
    {
        using var cts = new CancellationTokenSource();
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["slow"] = new SlowStep(TimeSpan.FromSeconds(5))
        };
        var engine = CreateEngine(steps,
            new TaskNode { Step = "slow" });

        var events = new List<WorkflowProgressType>();
        engine.Progressed += (_, e) => events.Add(e.Type);

        cts.CancelAfter(50);
        await engine.RunAsync(new TestContext(), cts.Token);

        events.Should().Contain(WorkflowProgressType.Cancelled);
        events.Should().NotContain(WorkflowProgressType.Completed);
    }

    #endregion

    #region RunAsync — ConditionNode

    [Fact]
    public async Task RunAsync_ConditionNode_SelectsMatchingBranch()
    {
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["check"] = new ValueCondition(),
            ["high"] = new AppendStep("HIGH"),
            ["low"] = new AppendStep("LOW")
        };
        var engine = CreateEngine(steps,
            new ConditionNode
            {
                Id = "cond",
                Step = "check",
                Branches = new Dictionary<string, IEnumerable<WorkflowNode>>
                {
                    ["high"] = [new TaskNode { Step = "high" }],
                    ["low"] = [new TaskNode { Step = "low" }]
                }
            });

        var ctx = new TestContext { Value = 10 }; // > 5 → "high"
        await engine.RunAsync(ctx);

        ctx.Log.Should().Equal("HIGH");
    }

    [Fact]
    public async Task RunAsync_ConditionNode_UsesDefaultBranch()
    {
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["check"] = new FixedCondition("unknown"),
            ["fallback"] = new AppendStep("DEFAULT")
        };
        var engine = CreateEngine(steps,
            new ConditionNode
            {
                Step = "check",
                Branches = new Dictionary<string, IEnumerable<WorkflowNode>>
                {
                    ["known"] = [new TaskNode { Step = "fallback" }]
                },
                DefaultBranch = [new TaskNode { Step = "fallback" }]
            });

        var ctx = new TestContext();
        await engine.RunAsync(ctx);

        ctx.Log.Should().Equal("DEFAULT");
    }

    [Fact]
    public async Task RunAsync_ConditionNode_NoMatchingBranch_RaisesFailedEvent()
    {
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["check"] = new FixedCondition("unmatched")
        };
        var engine = CreateEngine(steps,
            new ConditionNode
            {
                Step = "check",
                Branches = new Dictionary<string, IEnumerable<WorkflowNode>>
                {
                    ["other"] = [new TaskNode { Step = "noop" }]
                }
            });

        var events = new List<WorkflowProgressType>();
        engine.Progressed += (_, e) => events.Add(e.Type);

        await engine.RunAsync(new TestContext());

        events.Should().Contain(WorkflowProgressType.Failed);
    }

    #endregion

    #region RunAsync — ParallelNode

    [Fact]
    public async Task RunAsync_ParallelNode_WaitAll_ExecutesAllBranches()
    {
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["a"] = new AppendStep("A"),
            ["b"] = new AppendStep("B")
        };
        var engine = CreateEngine(steps,
            new ParallelNode
            {
                Join = JoinMode.WaitAll,
                Context = ContextMode.Shared,
                Branches =
                [
                    [new TaskNode { Step = "a" }],
                    [new TaskNode { Step = "b" }]
                ]
            });

        var ctx = new TestContext();
        await engine.RunAsync(ctx);

        ctx.Log.Should().HaveCount(2);
        ctx.Log.Should().Contain("A");
        ctx.Log.Should().Contain("B");
    }

    [Fact]
    public async Task RunAsync_ParallelNode_WaitAny_CompletesWhenOneFinishes()
    {
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["fast"] = new AppendStep("FAST"),
            ["slow"] = new SlowStep(TimeSpan.FromSeconds(5))
        };
        var engine = CreateEngine(steps,
            new ParallelNode
            {
                Join = JoinMode.WaitAny,
                Context = ContextMode.Shared,
                Branches =
                [
                    [new TaskNode { Step = "fast" }],
                    [new TaskNode { Step = "slow" }]
                ]
            });

        var ctx = new TestContext();
        await engine.RunAsync(ctx);

        ctx.Log.Should().Contain("FAST");
    }

    [Fact]
    public async Task RunAsync_ParallelNode_CopiedContext_BranchesGetIndependentCopies()
    {
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["inc"] = new IncrementStep()
        };
        var engine = CreateEngine(steps,
            new ParallelNode
            {
                Join = JoinMode.WaitAll,
                Context = ContextMode.Copied,
                Branches =
                [
                    [new TaskNode { Step = "inc" }],
                    [new TaskNode { Step = "inc" }]
                ]
            });

        var ctx = new TestContext { Value = 0 };
        await engine.RunAsync(ctx);

        // With Copied context, each branch gets a clone, so the original is unchanged
        // (Clone happens via JSON serialization, the original ctx is not modified)
        ctx.Value.Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_ParallelNode_SharedContext_BranchesShareState()
    {
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["inc"] = new IncrementStep()
        };
        var engine = CreateEngine(steps,
            new ParallelNode
            {
                Join = JoinMode.WaitAll,
                Context = ContextMode.Shared,
                Branches =
                [
                    [new TaskNode { Step = "inc" }],
                    [new TaskNode { Step = "inc" }]
                ]
            });

        var ctx = new TestContext { Value = 0 };
        await engine.RunAsync(ctx);

        // With Shared context, both branches modify the same context
        ctx.Value.Should().Be(2);
    }

    #endregion

    #region RunFromAsync

    [Fact]
    public async Task RunFromAsync_ValidNodeId_StartsFromThatNode()
    {
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["step1"] = new AppendStep("A"),
            ["step2"] = new AppendStep("B"),
            ["step3"] = new AppendStep("C")
        };
        var engine = CreateEngine(steps,
            new TaskNode { Id = "n1", Step = "step1" },
            new TaskNode { Id = "n2", Step = "step2" },
            new TaskNode { Id = "n3", Step = "step3" });

        var ctx = new TestContext();
        await engine.RunFromAsync("n2", ctx);

        ctx.Log.Should().Equal("B", "C"); // Skips n1
    }

    [Fact]
    public async Task RunFromAsync_InvalidNodeId_RaisesFailedEvent()
    {
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["step1"] = new AppendStep("A")
        };
        var engine = CreateEngine(steps,
            new TaskNode { Id = "n1", Step = "step1" });

        var events = new List<WorkflowProgressType>();
        engine.Progressed += (_, e) => events.Add(e.Type);

        var act = () => engine.RunFromAsync("nonexistent", new TestContext());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*nonexistent*");
    }

    [Fact]
    public async Task RunFromAsync_NodeIdInConditionBranch_FindsNested()
    {
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["check"] = new FixedCondition("yes"),
            ["inner"] = new AppendStep("INNER"),
            ["after"] = new AppendStep("AFTER")
        };
        var engine = CreateEngine(steps,
            new ConditionNode
            {
                Id = "cond",
                Step = "check",
                Branches = new Dictionary<string, IEnumerable<WorkflowNode>>
                {
                    ["yes"] =
                    [
                        new TaskNode { Id = "target", Step = "inner" },
                        new TaskNode { Id = "post", Step = "after" }
                    ]
                }
            });

        var ctx = new TestContext();
        await engine.RunFromAsync("target", ctx);

        ctx.Log.Should().Equal("INNER", "AFTER");
    }

    [Fact]
    public async Task RunFromAsync_NodeIdInParallelBranch_FindsNested()
    {
        var steps = new Dictionary<string, IWorkflowStep>
        {
            ["a"] = new AppendStep("A"),
            ["b"] = new AppendStep("B")
        };
        var engine = CreateEngine(steps,
            new ParallelNode
            {
                Join = JoinMode.WaitAll,
                Context = ContextMode.Shared,
                Branches =
                [
                    [
                        new TaskNode { Id = "branch-a", Step = "a" },
                    ],
                    [
                        new TaskNode { Id = "branch-b", Step = "b" }
                    ]
                ]
            });

        var ctx = new TestContext();
        await engine.RunFromAsync("branch-b", ctx);

        // Should find branch-b and execute from there (only B in that sub-sequence)
        ctx.Log.Should().Contain("B");
    }

    #endregion

    #region Properties

    [Fact]
    public void NameAndVersion_SetCorrectly()
    {
        var engine = new WorkflowEngine<TestContext>(new Dictionary<string, IWorkflowStep>())
        {
            Nodes = [],
            Name = "TestWorkflow",
            Version = new Version(1, 2, 3)
        };

        engine.Name.Should().Be("TestWorkflow");
        engine.Version.Should().Be(new Version(1, 2, 3));
    }

    #endregion

    #region Helpers

    private static WorkflowEngine<TestContext> CreateEngine(
        IReadOnlyDictionary<string, IWorkflowStep> steps,
        params WorkflowNode[] nodes)
    {
        return new WorkflowEngine<TestContext>(steps)
        {
            Nodes = nodes
        };
    }

    private sealed class TestContext
    {
        public List<string> Log { get; set; } = [];
        public int Value { get; set; }
    }

    private sealed class AppendStep : IWorkflowTask<TestContext>
    {
        private readonly string _text;
        public AppendStep(string text) => _text = text;

        public Task<TaskStepResult> ExecuteAsync(
            TestContext context, CancellationToken cancellationToken = default)
        {
            context.Log.Add(_text);
            return Task.FromResult(TaskStepResult.Success());
        }
    }

    private sealed class FailStep : IWorkflowTask<TestContext>
    {
        public Task<TaskStepResult> ExecuteAsync(
            TestContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TaskStepResult.Fail(new InvalidOperationException("forced error")));
        }
    }

    private sealed class ThrowStep : IWorkflowTask<TestContext>
    {
        public Task<TaskStepResult> ExecuteAsync(
            TestContext context, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("unhandled error");
        }
    }

    private sealed class SlowStep(TimeSpan delay) : IWorkflowTask<TestContext>
    {
        public async Task<TaskStepResult> ExecuteAsync(
            TestContext context, CancellationToken cancellationToken = default)
        {
            await Task.Delay(delay, cancellationToken);
            context.Log.Add("slow");
            return TaskStepResult.Success();
        }
    }

    private sealed class IncrementStep : IWorkflowTask<TestContext>
    {
        public Task<TaskStepResult> ExecuteAsync(
            TestContext context, CancellationToken cancellationToken = default)
        {
            context.Value++;
            return Task.FromResult(TaskStepResult.Success());
        }
    }

    private sealed class ValueCondition : IWorkflowCondition<TestContext>
    {
        public Task<ConditionStepResult> EvaluateAsync(
            TestContext context, CancellationToken cancellationToken = default)
        {
            var key = context.Value > 5 ? "high" : "low";
            return Task.FromResult(ConditionStepResult.Select(key));
        }
    }

    private sealed class FixedCondition(string key) : IWorkflowCondition<TestContext>
    {
        public Task<ConditionStepResult> EvaluateAsync(
            TestContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ConditionStepResult.Select(key));
        }
    }

    #endregion
}
