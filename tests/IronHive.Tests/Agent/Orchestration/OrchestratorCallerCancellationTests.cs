using AwesomeAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Core.Agent.Orchestration;
using NSubstitute;

namespace IronHive.Tests.Agent.Orchestration;

/// <summary>
/// A caller that cancels an orchestration gets <see cref="OperationCanceledException"/>, not a failure result. Each
/// orchestrator also has its own timeout, reported as a failure; the caller's cancellation is a different thing and
/// must not be turned into "orchestration failed: A task was canceled".
/// </summary>
public class OrchestratorCallerCancellationTests
{
    public static TheoryData<string> Orchestrators =>
        ["Sequential", "Parallel", "GroupChat", "Handoff", "HubSpoke", "Graph"];

    [Theory]
    [MemberData(nameof(Orchestrators))]
    public async Task ExecuteAsync_WhenTheCallerCancels_ThrowsOperationCanceled(string kind)
    {
        using var caller = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var agent = CancellingAgent("a", caller);
        var orchestrator = Build(kind, agent);

        var act = () => orchestrator.ExecuteAsync(
            [new Message { Role = MessageRole.User, Content = [new TextMessageContent { Value = "go" }] }],
            caller.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    /// <summary>An agent that, when invoked, cancels the caller's token and then observes the token it was given.</summary>
    private static IAgent CancellingAgent(string name, CancellationTokenSource caller)
    {
        var agent = Substitute.For<IAgent>();
        agent.Name.Returns(name);
        agent.Provider.Returns("mock");
        agent.Model.Returns("mock-model");
        agent.Description.Returns("Mock");
        agent.InvokeAsync(Arg.Any<IEnumerable<Message>>(), Arg.Any<AgentInvokeOptions?>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                caller.Cancel();
                call.ArgAt<CancellationToken>(2).ThrowIfCancellationRequested();
                return Task.FromResult(new MessageResponse
                {
                    DoneReason = MessageDoneReason.EndTurn,
                    Message = new Message { Role = MessageRole.Assistant, Content = [new TextMessageContent { Value = "unreachable" }] },
                });
            });
        return agent;
    }

    private static IAgentOrchestrator Build(string kind, IAgent agent)
    {
        switch (kind)
        {
            case "Sequential":
                var sequential = new SequentialOrchestrator();
                sequential.AddAgent(agent);
                return sequential;
            case "Parallel":
                var parallel = new ParallelOrchestrator();
                parallel.AddAgent(agent);
                return parallel;
            case "GroupChat":
                return new GroupChatOrchestratorBuilder()
                    .AddAgent(agent)
                    .WithRoundRobin()
                    .TerminateAfterRounds(3)
                    .Build();
            case "Handoff":
                return new HandoffOrchestratorBuilder()
                    .AddAgent(agent)
                    .SetInitialAgent(agent.Name)
                    .Build();
            case "HubSpoke":
                var hubSpoke = new HubSpokeOrchestrator();
                hubSpoke.SetHubAgent(agent);
                var spoke = Substitute.For<IAgent>();
                spoke.Name.Returns("spoke");
                hubSpoke.AddSpokeAgent(spoke);
                return hubSpoke;
            case "Graph":
                return new GraphOrchestratorBuilder()
                    .AddNode("a", agent)
                    .SetStartNode("a")
                    .SetOutputNode("a")
                    .Build();
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }
    }
}
