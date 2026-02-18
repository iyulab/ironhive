using FluentAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Core.Agent.Orchestration;
using NSubstitute;

namespace IronHive.Tests.Agent.Orchestration;

public class SpeakerSelectorTests
{
    #region RoundRobinSpeakerSelector

    [Fact]
    public async Task RoundRobin_Empty_ReturnsNull()
    {
        var selector = new RoundRobinSpeakerSelector();

        var result = await selector.SelectNextSpeakerAsync(
            Array.Empty<AgentStepResult>(),
            Array.Empty<Message>(),
            Array.Empty<IAgent>());

        result.Should().BeNull();
    }

    [Fact]
    public async Task RoundRobin_SingleAgent_AlwaysReturnsSame()
    {
        var selector = new RoundRobinSpeakerSelector();
        var agents = MakeAgents("only");

        for (int i = 0; i < 5; i++)
        {
            var result = await selector.SelectNextSpeakerAsync(
                Array.Empty<AgentStepResult>(),
                Array.Empty<Message>(),
                agents);

            result.Should().Be("only");
        }
    }

    [Fact]
    public async Task RoundRobin_MultipleAgents_CyclesInOrder()
    {
        var selector = new RoundRobinSpeakerSelector();
        var agents = MakeAgents("a", "b", "c");
        var selected = new List<string?>();

        for (int i = 0; i < 6; i++)
        {
            selected.Add(await selector.SelectNextSpeakerAsync(
                Array.Empty<AgentStepResult>(),
                Array.Empty<Message>(),
                agents));
        }

        selected.Should().Equal("a", "b", "c", "a", "b", "c");
    }

    #endregion

    #region RandomSpeakerSelector

    [Fact]
    public async Task Random_Empty_ReturnsNull()
    {
        var selector = new RandomSpeakerSelector();

        var result = await selector.SelectNextSpeakerAsync(
            Array.Empty<AgentStepResult>(),
            Array.Empty<Message>(),
            Array.Empty<IAgent>());

        result.Should().BeNull();
    }

    [Fact]
    public async Task Random_SingleAgent_AlwaysReturnsSame()
    {
        var selector = new RandomSpeakerSelector();
        var agents = MakeAgents("only");

        for (int i = 0; i < 10; i++)
        {
            var result = await selector.SelectNextSpeakerAsync(
                Array.Empty<AgentStepResult>(),
                Array.Empty<Message>(),
                agents);

            result.Should().Be("only");
        }
    }

    [Fact]
    public async Task Random_MultipleAgents_ReturnsValidAgent()
    {
        var selector = new RandomSpeakerSelector();
        var agents = MakeAgents("a", "b", "c");
        var validNames = new[] { "a", "b", "c" };

        for (int i = 0; i < 20; i++)
        {
            var result = await selector.SelectNextSpeakerAsync(
                Array.Empty<AgentStepResult>(),
                Array.Empty<Message>(),
                agents);

            result.Should().BeOneOf(validNames);
        }
    }

    #endregion

    #region LlmSpeakerSelector

    [Fact]
    public async Task Llm_Empty_ReturnsNull()
    {
        var manager = Substitute.For<IAgent>();
        var selector = new LlmSpeakerSelector(manager);

        var result = await selector.SelectNextSpeakerAsync(
            Array.Empty<AgentStepResult>(),
            Array.Empty<Message>(),
            Array.Empty<IAgent>());

        result.Should().BeNull();
    }

    [Fact]
    public async Task Llm_ManagerReturnsAgentName_SelectsAgent()
    {
        var manager = CreateManagerAgent("writer");
        var selector = new LlmSpeakerSelector(manager);
        var agents = MakeAgents("writer", "editor");

        var result = await selector.SelectNextSpeakerAsync(
            Array.Empty<AgentStepResult>(),
            Array.Empty<Message>(),
            agents);

        result.Should().Be("writer");
    }

    [Fact]
    public async Task Llm_ManagerReturnsNone_ReturnsNull()
    {
        var manager = CreateManagerAgent("NONE");
        var selector = new LlmSpeakerSelector(manager);
        var agents = MakeAgents("writer", "editor");

        var result = await selector.SelectNextSpeakerAsync(
            Array.Empty<AgentStepResult>(),
            Array.Empty<Message>(),
            agents);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Llm_ManagerReturnsUnknown_ReturnsNull()
    {
        var manager = CreateManagerAgent("nonexistent");
        var selector = new LlmSpeakerSelector(manager);
        var agents = MakeAgents("writer", "editor");

        var result = await selector.SelectNextSpeakerAsync(
            Array.Empty<AgentStepResult>(),
            Array.Empty<Message>(),
            agents);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Llm_ManagerReturnsCaseInsensitive_SelectsAgent()
    {
        var manager = CreateManagerAgent("WRITER");
        var selector = new LlmSpeakerSelector(manager);
        var agents = MakeAgents("writer", "editor");

        var result = await selector.SelectNextSpeakerAsync(
            Array.Empty<AgentStepResult>(),
            Array.Empty<Message>(),
            agents);

        result.Should().Be("writer");
    }

    [Fact]
    public void Llm_NullManager_Throws()
    {
        var act = () => new LlmSpeakerSelector(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Helpers

    private static IAgent[] MakeAgents(params string[] names)
    {
        return names.Select(n =>
        {
            var agent = Substitute.For<IAgent>();
            agent.Name.Returns(n);
            agent.Description.Returns($"Agent {n}");
            return agent;
        }).ToArray();
    }

    private static IAgent CreateManagerAgent(string responseText)
    {
        var manager = Substitute.For<IAgent>();
        manager.InvokeAsync(Arg.Any<IEnumerable<Message>>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage
                {
                    Content = [new TextMessageContent { Value = responseText }]
                },
                TokenUsage = new MessageTokenUsage { InputTokens = 10, OutputTokens = responseText.Length }
            });
        return manager;
    }

    #endregion
}
