using FluentAssertions;
using IronHive.Abstractions.Workflow;

namespace IronHive.Tests.Workflow;

public class WorkflowDefinitionTests
{
    [Fact]
    public void Validate_EmptySteps_ShouldNotThrow()
    {
        var def = new WorkflowDefinition { Steps = [] };

        var act = () => def.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_UniqueIds_ShouldNotThrow()
    {
        var def = new WorkflowDefinition
        {
            Steps =
            [
                new TaskNode { Id = "a", Step = "step1" },
                new TaskNode { Id = "b", Step = "step2" }
            ]
        };

        var act = () => def.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_DuplicateIds_ShouldThrow()
    {
        var def = new WorkflowDefinition
        {
            Steps =
            [
                new TaskNode { Id = "dup", Step = "step1" },
                new TaskNode { Id = "dup", Step = "step2" }
            ]
        };

        var act = () => def.Validate();

        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain("dup");
    }

    [Fact]
    public void Validate_NullOrWhitespaceId_ShouldBeSkipped()
    {
        var def = new WorkflowDefinition
        {
            Steps =
            [
                new TaskNode { Id = null, Step = "step1" },
                new TaskNode { Id = "", Step = "step2" },
                new TaskNode { Id = "  ", Step = "step3" }
            ]
        };

        var act = () => def.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_DuplicateInNestedCondition_ShouldThrow()
    {
        var def = new WorkflowDefinition
        {
            Steps =
            [
                new TaskNode { Id = "task1", Step = "step1" },
                new ConditionNode
                {
                    Id = "cond1",
                    Step = "check",
                    Branches = new Dictionary<string, IEnumerable<WorkflowNode>>
                    {
                        ["yes"] = [new TaskNode { Id = "task1", Step = "step2" }]
                    }
                }
            ]
        };

        var act = () => def.Validate();

        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain("task1");
    }

    [Fact]
    public void Validate_DuplicateInParallelBranches_ShouldThrow()
    {
        var def = new WorkflowDefinition
        {
            Steps =
            [
                new ParallelNode
                {
                    Id = "par1",
                    Join = JoinMode.WaitAll,
                    Context = ContextMode.Copied,
                    Branches =
                    [
                        [new TaskNode { Id = "inner", Step = "s1" }],
                        [new TaskNode { Id = "inner", Step = "s2" }]
                    ]
                }
            ]
        };

        var act = () => def.Validate();

        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain("inner");
    }

    [Fact]
    public void EnumerateRecursively_FlatTaskNodes_ShouldReturnAll()
    {
        var nodes = new WorkflowNode[]
        {
            new TaskNode { Id = "a", Step = "s1" },
            new TaskNode { Id = "b", Step = "s2" }
        };

        var result = WorkflowDefinition.EnumerateRecursively(nodes).ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public void EnumerateRecursively_ConditionWithBranches_ShouldFlatten()
    {
        var nodes = new WorkflowNode[]
        {
            new ConditionNode
            {
                Id = "cond",
                Step = "check",
                Branches = new Dictionary<string, IEnumerable<WorkflowNode>>
                {
                    ["yes"] = [new TaskNode { Id = "t1", Step = "s1" }],
                    ["no"] = [new TaskNode { Id = "t2", Step = "s2" }]
                }
            }
        };

        var result = WorkflowDefinition.EnumerateRecursively(nodes).ToList();

        // cond + t1 + t2
        result.Should().HaveCount(3);
        result[0].Id.Should().Be("cond");
    }

    [Fact]
    public void EnumerateRecursively_ParallelWithBranches_ShouldFlatten()
    {
        var nodes = new WorkflowNode[]
        {
            new ParallelNode
            {
                Id = "par",
                Join = JoinMode.WaitAny,
                Context = ContextMode.Shared,
                Branches =
                [
                    [new TaskNode { Id = "p1", Step = "s1" }],
                    [new TaskNode { Id = "p2", Step = "s2" }, new TaskNode { Id = "p3", Step = "s3" }]
                ]
            }
        };

        var result = WorkflowDefinition.EnumerateRecursively(nodes).ToList();

        // par + p1 + p2 + p3
        result.Should().HaveCount(4);
        result[0].Id.Should().Be("par");
    }

    [Fact]
    public void EnumerateRecursively_Empty_ShouldReturnEmpty()
    {
        var result = WorkflowDefinition.EnumerateRecursively([]).ToList();

        result.Should().BeEmpty();
    }
}

public class WorkflowNodeTests
{
    [Fact]
    public void TaskNode_ShouldSetProperties()
    {
        var node = new TaskNode { Id = "t1", Step = "doSomething", With = "options" };

        node.Id.Should().Be("t1");
        node.Step.Should().Be("doSomething");
        node.With.Should().Be("options");
    }

    [Fact]
    public void ConditionNode_ShouldSetProperties()
    {
        var node = new ConditionNode
        {
            Id = "c1",
            Step = "evaluate",
            Branches = new Dictionary<string, IEnumerable<WorkflowNode>>
            {
                ["branch1"] = [new TaskNode { Step = "s1" }]
            },
            DefaultBranch = [new TaskNode { Step = "default" }]
        };

        node.Id.Should().Be("c1");
        node.Branches.Should().HaveCount(1);
        node.DefaultBranch.Should().NotBeNull();
    }

    [Fact]
    public void ParallelNode_ShouldSetProperties()
    {
        var node = new ParallelNode
        {
            Join = JoinMode.WaitAll,
            Context = ContextMode.Shared,
            Branches = [[new TaskNode { Step = "s1" }]]
        };

        node.Join.Should().Be(JoinMode.WaitAll);
        node.Context.Should().Be(ContextMode.Shared);
    }

    [Theory]
    [InlineData(JoinMode.WaitAll, 0)]
    [InlineData(JoinMode.WaitAny, 1)]
    public void JoinMode_ShouldHaveExpectedValues(JoinMode mode, int expected)
    {
        ((int)mode).Should().Be(expected);
    }

    [Theory]
    [InlineData(ContextMode.Copied, 0)]
    [InlineData(ContextMode.Shared, 1)]
    public void ContextMode_ShouldHaveExpectedValues(ContextMode mode, int expected)
    {
        ((int)mode).Should().Be(expected);
    }
}
