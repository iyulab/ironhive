using FluentAssertions;
using IronHive.Abstractions.Workflow;
using IronHive.Core.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Tests.Workflow;

public class WorkflowFactoryTests
{
    #region CreateBuilder

    [Fact]
    public void CreateBuilder_ReturnsNonNullBuilder()
    {
        var factory = new WorkflowFactory();

        var builder = factory.CreateBuilder();

        builder.Should().NotBeNull();
    }

    [Fact]
    public void CreateBuilder_WithServices_ReturnsBuilder()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var factory = new WorkflowFactory(services);

        var builder = factory.CreateBuilder();

        builder.Should().NotBeNull();
    }

    #endregion

    #region CreateFrom

    [Fact]
    public void CreateFrom_ValidDefinition_ReturnsWorkflow()
    {
        var services = CreateServicesWithStep("step1", new TestStep("A"));
        var factory = new WorkflowFactory(services);
        var def = new WorkflowDefinition
        {
            Name = "test-workflow",
            Version = new Version(1, 0, 0),
            Steps = [new TaskNode { Id = "n1", Step = "step1" }]
        };

        var workflow = factory.CreateFrom<TestContext>(def);

        workflow.Should().NotBeNull();
        workflow.Name.Should().Be("test-workflow");
        workflow.Version.Should().Be(new Version(1, 0, 0));
    }

    [Fact]
    public void CreateFrom_EmptyDefinition_ReturnsWorkflow()
    {
        var factory = new WorkflowFactory();
        var def = new WorkflowDefinition { Steps = [] };

        var workflow = factory.CreateFrom<TestContext>(def);

        workflow.Should().NotBeNull();
    }

    [Fact]
    public void CreateFrom_MissingStep_ThrowsInvalidOperation()
    {
        var factory = new WorkflowFactory(new ServiceCollection().BuildServiceProvider());
        var def = new WorkflowDefinition
        {
            Steps = [new TaskNode { Id = "n1", Step = "unregistered" }]
        };

        var act = () => factory.CreateFrom<TestContext>(def);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unregistered*");
    }

    [Fact]
    public void CreateFrom_NullServices_ThrowsInvalidOperation()
    {
        var factory = new WorkflowFactory(null);
        var def = new WorkflowDefinition
        {
            Steps = [new TaskNode { Id = "n1", Step = "step1" }]
        };

        var act = () => factory.CreateFrom<TestContext>(def);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CreateFrom_ConditionNode_ResolvesConditionStep()
    {
        var sc = new ServiceCollection();
        sc.AddKeyedSingleton<IWorkflowStep>("check", new TestCondition("yes"));
        var services = sc.BuildServiceProvider();
        var factory = new WorkflowFactory(services);
        var def = new WorkflowDefinition
        {
            Steps =
            [
                new ConditionNode
                {
                    Id = "c1",
                    Step = "check",
                    Branches = new Dictionary<string, IEnumerable<WorkflowNode>>
                    {
                        ["yes"] = []
                    }
                }
            ]
        };

        var workflow = factory.CreateFrom<TestContext>(def);

        workflow.Should().NotBeNull();
    }

    [Fact]
    public void CreateFrom_NestedNodes_ResolvesAllSteps()
    {
        var sc = new ServiceCollection();
        sc.AddKeyedSingleton<IWorkflowStep>("task1", new TestStep("A"));
        sc.AddKeyedSingleton<IWorkflowStep>("task2", new TestStep("B"));
        sc.AddKeyedSingleton<IWorkflowStep>("cond", new TestCondition("branch1"));
        var services = sc.BuildServiceProvider();
        var factory = new WorkflowFactory(services);
        var def = new WorkflowDefinition
        {
            Steps =
            [
                new TaskNode { Step = "task1" },
                new ConditionNode
                {
                    Step = "cond",
                    Branches = new Dictionary<string, IEnumerable<WorkflowNode>>
                    {
                        ["branch1"] = [new TaskNode { Step = "task2" }]
                    }
                }
            ]
        };

        var workflow = factory.CreateFrom<TestContext>(def);

        workflow.Should().NotBeNull();
    }

    [Fact]
    public void CreateFrom_PreservesNameAndVersion()
    {
        var factory = new WorkflowFactory();
        var def = new WorkflowDefinition
        {
            Name = "my-flow",
            Version = new Version(2, 3, 4),
            Steps = []
        };

        var workflow = factory.CreateFrom<TestContext>(def);

        workflow.Name.Should().Be("my-flow");
        workflow.Version.Should().Be(new Version(2, 3, 4));
    }

    [Fact]
    public void CreateFrom_NullNameAndVersion_SetsNull()
    {
        var factory = new WorkflowFactory();
        var def = new WorkflowDefinition { Steps = [] };

        var workflow = factory.CreateFrom<TestContext>(def);

        workflow.Name.Should().BeNull();
        workflow.Version.Should().BeNull();
    }

    [Fact]
    public async Task CreateFrom_ProducesRunnableWorkflow()
    {
        var services = CreateServicesWithStep("step1", new TestStep("executed"));
        var factory = new WorkflowFactory(services);
        var def = new WorkflowDefinition
        {
            Steps = [new TaskNode { Id = "n1", Step = "step1" }]
        };

        var workflow = factory.CreateFrom<TestContext>(def);
        var ctx = new TestContext();
        await workflow.RunAsync(ctx);

        ctx.Log.Should().Contain("executed");
    }

    #endregion

    #region CreateFromJson

    [Fact]
    public void CreateFromJson_ValidJson_ReturnsWorkflow()
    {
        var services = CreateServicesWithStep("greet", new TestStep("hello"));
        var factory = new WorkflowFactory(services);
        var json = """
        {
            "name": "json-workflow",
            "steps": [
                { "type": "task", "id": "n1", "step": "greet" }
            ]
        }
        """;

        var workflow = factory.CreateFromJson<TestContext>(json);

        workflow.Should().NotBeNull();
        workflow.Name.Should().Be("json-workflow");
    }

    [Fact]
    public void CreateFromJson_CaseInsensitiveProperties()
    {
        var services = CreateServicesWithStep("step1", new TestStep("ok"));
        var factory = new WorkflowFactory(services);
        var json = """
        {
            "Name": "test",
            "Steps": [
                { "type": "task", "Id": "n1", "Step": "step1" }
            ]
        }
        """;

        var workflow = factory.CreateFromJson<TestContext>(json);

        workflow.Should().NotBeNull();
        workflow.Name.Should().Be("test");
    }

    [Fact]
    public void CreateFromJson_EmptySteps_ReturnsWorkflow()
    {
        var factory = new WorkflowFactory();
        var json = """{ "steps": [] }""";

        var workflow = factory.CreateFromJson<TestContext>(json);

        workflow.Should().NotBeNull();
    }

    [Fact]
    public void CreateFromJson_NullResult_ThrowsInvalidOperation()
    {
        var factory = new WorkflowFactory();

        var act = () => factory.CreateFromJson<TestContext>("null");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*null*");
    }

    [Fact]
    public void CreateFromJson_InvalidJson_ThrowsJsonException()
    {
        var factory = new WorkflowFactory();

        var act = () => factory.CreateFromJson<TestContext>("not valid json");

        act.Should().Throw<System.Text.Json.JsonException>();
    }

    [Fact]
    public void CreateFromJson_MissingStepInDI_ThrowsInvalidOperation()
    {
        var factory = new WorkflowFactory(new ServiceCollection().BuildServiceProvider());
        var json = """
        {
            "steps": [
                { "type": "task", "step": "missing-step" }
            ]
        }
        """;

        var act = () => factory.CreateFromJson<TestContext>(json);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing-step*");
    }

    [Fact]
    public void CreateFromJson_NoTypeDiscriminator_ThrowsNotSupported()
    {
        var factory = new WorkflowFactory();
        var json = """
        {
            "steps": [
                { "step": "some-step" }
            ]
        }
        """;

        var act = () => factory.CreateFromJson<TestContext>(json);

        act.Should().Throw<NotSupportedException>();
    }

    #endregion

    #region CreateFromYaml

    [Fact]
    public void CreateFromYaml_ValidYaml_ReturnsWorkflow()
    {
        var services = CreateServicesWithStep("greet", new TestStep("hello"));
        var factory = new WorkflowFactory(services);
        var yaml = """
            Name: yaml-workflow
            Steps:
              - type: task
                Id: n1
                Step: greet
            """;

        var workflow = factory.CreateFromYaml<TestContext>(yaml);

        workflow.Should().NotBeNull();
        workflow.Name.Should().Be("yaml-workflow");
    }

    [Fact]
    public void CreateFromYaml_EmptySteps_ReturnsWorkflow()
    {
        var factory = new WorkflowFactory();
        var yaml = """
            Name: empty
            Steps: []
            """;

        var workflow = factory.CreateFromYaml<TestContext>(yaml);

        workflow.Should().NotBeNull();
    }

    [Fact]
    public void CreateFromYaml_MissingStep_ThrowsInvalidOperation()
    {
        var factory = new WorkflowFactory(new ServiceCollection().BuildServiceProvider());
        var yaml = """
            Steps:
              - type: task
                Step: not-registered
            """;

        var act = () => factory.CreateFromYaml<TestContext>(yaml);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not-registered*");
    }

    [Fact]
    public void CreateFromYaml_ConditionTypeDiscriminator_DeserializesCorrectly()
    {
        var sc = new ServiceCollection();
        sc.AddKeyedSingleton<IWorkflowStep>("check", new TestCondition("yes"));
        sc.AddKeyedSingleton<IWorkflowStep>("inner", new TestStep("ok"));
        var services = sc.BuildServiceProvider();
        var factory = new WorkflowFactory(services);
        var yaml = """
            Steps:
              - type: condition
                Step: check
                Branches:
                  yes:
                    - type: task
                      Step: inner
            """;

        var workflow = factory.CreateFromYaml<TestContext>(yaml);

        workflow.Should().NotBeNull();
    }

    #endregion

    #region Helpers

    private static ServiceProvider CreateServicesWithStep(string name, IWorkflowStep step)
    {
        var sc = new ServiceCollection();
        sc.AddKeyedSingleton<IWorkflowStep>(name, step);
        return sc.BuildServiceProvider();
    }

    private sealed class TestContext
    {
        public List<string> Log { get; set; } = [];
    }

    private sealed class TestStep(string text) : IWorkflowTask<TestContext>
    {
        public Task<TaskStepResult> ExecuteAsync(
            TestContext context, CancellationToken cancellationToken = default)
        {
            context.Log.Add(text);
            return Task.FromResult(TaskStepResult.Success());
        }
    }

    private sealed class TestCondition(string key) : IWorkflowCondition<TestContext>
    {
        public Task<ConditionStepResult> EvaluateAsync(
            TestContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ConditionStepResult.Select(key));
        }
    }

    #endregion
}
