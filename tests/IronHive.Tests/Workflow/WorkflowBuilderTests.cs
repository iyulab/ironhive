using FluentAssertions;
using IronHive.Abstractions.Workflow;
using IronHive.Core.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Tests.Workflow;

public class WorkflowBuilderTests
{
    #region WorkflowBuilder

    [Fact]
    public void WithName_SetsName()
    {
        var workflow = new WorkflowBuilder()
            .WithName("my-workflow")
            .StartWith<TestContext>()
            .Then<NoOpStep>("step1")
            .Build();

        workflow.Name.Should().Be("my-workflow");
    }

    [Fact]
    public void WithVersion_SetsVersion()
    {
        var version = new Version(2, 3, 1);
        var workflow = new WorkflowBuilder()
            .WithVersion(version)
            .StartWith<TestContext>()
            .Then<NoOpStep>("step1")
            .Build();

        workflow.Version.Should().Be(version);
    }

    [Fact]
    public void FluentChaining_SetsNameAndVersion()
    {
        var workflow = new WorkflowBuilder()
            .WithName("chained")
            .WithVersion(new Version(1, 0))
            .StartWith<TestContext>()
            .Then<NoOpStep>("step1")
            .Build();

        workflow.Name.Should().Be("chained");
        workflow.Version.Should().Be(new Version(1, 0));
    }

    [Fact]
    public void StartWith_ReturnsStepBuilder()
    {
        var builder = new WorkflowBuilder()
            .StartWith<TestContext>();

        builder.Should().NotBeNull();
        builder.Should().BeOfType<WorkflowStepBuilder<TestContext>>();
    }

    #endregion

    #region WorkflowStepBuilder — Then

    [Fact]
    public void Then_AddsTaskNode()
    {
        var workflow = new WorkflowBuilder()
            .StartWith<TestContext>()
            .Then<NoOpStep>("step1")
            .Build();

        var engine = workflow.Should().BeOfType<WorkflowEngine<TestContext>>().Subject;
        var nodes = engine.Nodes.ToList();
        nodes.Should().ContainSingle();
        nodes[0].Should().BeOfType<TaskNode>()
            .Which.Step.Should().Be("step1");
    }

    [Fact]
    public void Then_MultipleSteps_AddsInOrder()
    {
        var workflow = new WorkflowBuilder()
            .StartWith<TestContext>()
            .Then<NoOpStep>("first")
            .Then<NoOpStep>("second")
            .Then<NoOpStep>("third")
            .Build();

        var engine = workflow.Should().BeOfType<WorkflowEngine<TestContext>>().Subject;
        var nodes = engine.Nodes.ToList();
        nodes.Should().HaveCount(3);
        nodes[0].Should().BeOfType<TaskNode>().Which.Step.Should().Be("first");
        nodes[1].Should().BeOfType<TaskNode>().Which.Step.Should().Be("second");
        nodes[2].Should().BeOfType<TaskNode>().Which.Step.Should().Be("third");
    }

    [Fact]
    public void Then_WithOptions_AddsTaskNodeWithOptions()
    {
        var options = new TestOptions { Multiplier = 42 };
        var workflow = new WorkflowBuilder()
            .StartWith<TestContext>()
            .Then<OptionsStep, TestOptions>("opts-step", options)
            .Build();

        var engine = workflow.Should().BeOfType<WorkflowEngine<TestContext>>().Subject;
        var node = engine.Nodes.Single().Should().BeOfType<TaskNode>().Subject;
        node.Step.Should().Be("opts-step");
        node.With.Should().Be(options);
    }

    #endregion

    #region WorkflowStepBuilder — Switch

    [Fact]
    public void Switch_AddsConditionNode()
    {
        var workflow = new WorkflowBuilder()
            .StartWith<TestContext>()
            .Switch<NoOpCondition>("cond", new Dictionary<string, Action<WorkflowStepBuilder<TestContext>>>
            {
                ["yes"] = b => b.Then<NoOpStep>("yes-step"),
                ["no"] = b => b.Then<NoOpStep>("no-step")
            })
            .Build();

        var engine = workflow.Should().BeOfType<WorkflowEngine<TestContext>>().Subject;
        var node = engine.Nodes.Single().Should().BeOfType<ConditionNode>().Subject;
        node.Step.Should().Be("cond");
        node.Branches.Should().ContainKey("yes");
        node.Branches.Should().ContainKey("no");
    }

    [Fact]
    public void Switch_WithDefaultBranch_SetsDefaultBranch()
    {
        var workflow = new WorkflowBuilder()
            .StartWith<TestContext>()
            .Switch<NoOpCondition>(
                "cond",
                new Dictionary<string, Action<WorkflowStepBuilder<TestContext>>>
                {
                    ["a"] = b => b.Then<NoOpStep>("a-step")
                },
                defaultBuildAction: b => b.Then<NoOpStep>("default-step"))
            .Build();

        var engine = workflow.Should().BeOfType<WorkflowEngine<TestContext>>().Subject;
        var node = engine.Nodes.Single().Should().BeOfType<ConditionNode>().Subject;
        node.DefaultBranch.Should().NotBeEmpty();
    }

    #endregion

    #region WorkflowStepBuilder — Split

    [Fact]
    public void Split_AddsParallelNode()
    {
        var workflow = new WorkflowBuilder()
            .StartWith<TestContext>()
            .Split(
            [
                b => b.Then<NoOpStep>("branch1"),
                b => b.Then<NoOpStep>("branch2")
            ])
            .Build();

        var engine = workflow.Should().BeOfType<WorkflowEngine<TestContext>>().Subject;
        var node = engine.Nodes.Single().Should().BeOfType<ParallelNode>().Subject;
        node.Branches.Should().HaveCount(2);
        node.Join.Should().Be(JoinMode.WaitAll);
        node.Context.Should().Be(ContextMode.Copied);
    }

    [Fact]
    public void Split_WithJoinAndContextMode_SetsCorrectValues()
    {
        var workflow = new WorkflowBuilder()
            .StartWith<TestContext>()
            .Split(
            [
                b => b.Then<NoOpStep>("b1")
            ],
            joinMode: JoinMode.WaitAny,
            contextMode: ContextMode.Shared)
            .Build();

        var engine = workflow.Should().BeOfType<WorkflowEngine<TestContext>>().Subject;
        var node = engine.Nodes.Single().Should().BeOfType<ParallelNode>().Subject;
        node.Join.Should().Be(JoinMode.WaitAny);
        node.Context.Should().Be(ContextMode.Shared);
    }

    #endregion

    #region WorkflowStepBuilder — Build

    [Fact]
    public void Build_ReturnsWorkflowEngine()
    {
        var workflow = new WorkflowBuilder()
            .StartWith<TestContext>()
            .Then<NoOpStep>("step")
            .Build();

        workflow.Should().BeOfType<WorkflowEngine<TestContext>>();
    }

    [Fact]
    public async Task Build_ProducesRunnableWorkflow()
    {
        var workflow = new WorkflowBuilder()
            .StartWith<TestContext>()
            .Then<CountStep>("step1")
            .Then<CountStep>("step2")
            .Build();

        var context = new TestContext();
        await workflow.RunAsync(context);

        context.Log.Should().HaveCount(2);
    }

    #endregion

    #region WorkflowStepBuilder — AddStep Resolution

    [Fact]
    public void Then_WithoutServices_UsesActivatorCreateInstance()
    {
        // NoOpStep has parameterless constructor, should work without DI
        var act = () => new WorkflowBuilder()
            .StartWith<TestContext>()
            .Then<NoOpStep>("step")
            .Build();

        act.Should().NotThrow();
    }

    [Fact]
    public void Then_WithServices_UsesKeyedService()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IWorkflowStep>("keyed-step", new NoOpStep());
        using var sp = services.BuildServiceProvider();

        var workflow = new WorkflowBuilder(sp)
            .StartWith<TestContext>()
            .Then<NoOpStep>("keyed-step")
            .Build();

        var engine = workflow.Should().BeOfType<WorkflowEngine<TestContext>>().Subject;
        engine.Nodes.Should().ContainSingle();
    }

    [Fact]
    public void Then_WithServices_FallsBackToActivatorUtilities()
    {
        var services = new ServiceCollection();
        // No keyed service registered, but ActivatorUtilities can still create NoOpStep
        using var sp = services.BuildServiceProvider();

        var act = () => new WorkflowBuilder(sp)
            .StartWith<TestContext>()
            .Then<NoOpStep>("step")
            .Build();

        act.Should().NotThrow();
    }

    [Fact]
    public void Then_DuplicateStepName_SharesSameInstance()
    {
        // TryAdd behavior: second add with same name is ignored
        var act = () => new WorkflowBuilder()
            .StartWith<TestContext>()
            .Then<NoOpStep>("same")
            .Then<NoOpStep>("same")
            .Build();

        act.Should().NotThrow();
    }

    #endregion

    #region Mixed Node Types

    [Fact]
    public void MixedNodes_Then_Switch_Split()
    {
        var workflow = new WorkflowBuilder()
            .WithName("mixed")
            .StartWith<TestContext>()
            .Then<NoOpStep>("task1")
            .Switch<NoOpCondition>("cond1", new Dictionary<string, Action<WorkflowStepBuilder<TestContext>>>
            {
                ["x"] = b => b.Then<NoOpStep>("x-step")
            })
            .Split(
            [
                b => b.Then<NoOpStep>("p1"),
                b => b.Then<NoOpStep>("p2")
            ])
            .Build();

        var engine = workflow.Should().BeOfType<WorkflowEngine<TestContext>>().Subject;
        var nodes = engine.Nodes.ToList();
        nodes.Should().HaveCount(3);
        nodes[0].Should().BeOfType<TaskNode>();
        nodes[1].Should().BeOfType<ConditionNode>();
        nodes[2].Should().BeOfType<ParallelNode>();
    }

    #endregion

    #region Test Types

    private sealed class TestContext
    {
        public List<string> Log { get; } = [];
    }

    private sealed class TestOptions
    {
        public int Multiplier { get; set; }
    }

    private sealed class NoOpStep : IWorkflowTask<TestContext>
    {
        public Task<TaskStepResult> ExecuteAsync(
            TestContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TaskStepResult.Success());
        }
    }

    private sealed class CountStep : IWorkflowTask<TestContext>
    {
        public Task<TaskStepResult> ExecuteAsync(
            TestContext context, CancellationToken cancellationToken = default)
        {
            context.Log.Add("executed");
            return Task.FromResult(TaskStepResult.Success());
        }
    }

    private sealed class OptionsStep : IWorkflowTask<TestContext, TestOptions>
    {
        public Task<TaskStepResult> ExecuteAsync(
            TestContext context, TestOptions options, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TaskStepResult.Success());
        }
    }

    private sealed class NoOpCondition : IWorkflowCondition<TestContext>
    {
        public Task<ConditionStepResult> EvaluateAsync(
            TestContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ConditionStepResult.Select("default"));
        }
    }

    #endregion
}
