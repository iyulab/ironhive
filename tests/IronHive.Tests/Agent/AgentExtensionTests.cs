using FluentAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using IronHive.Core.Agent;
using NSubstitute;

namespace IronHive.Tests.Agent;

public class AgentExtensionTests
{
    [Fact]
    public void WithMiddleware_NoMiddlewares_ReturnsSameAgent()
    {
        var agent = Substitute.For<IAgent>();
        agent.Name.Returns("test");

        var result = agent.WithMiddleware();

        result.Should().BeSameAs(agent);
    }

    [Fact]
    public void WithMiddleware_WithMiddlewares_ReturnsMiddlewareAgent()
    {
        var agent = Substitute.For<IAgent>();
        agent.Name.Returns("test");
        var middleware = Substitute.For<IAgentMiddleware>();

        var result = agent.WithMiddleware(middleware);

        result.Should().BeOfType<MiddlewareAgent>();
        result.Name.Should().Be("test");
    }

    [Fact]
    public async Task WithMiddleware_ChainApplied_MiddlewareInvoked()
    {
        var middlewareInvoked = false;
        var middleware = Substitute.For<IAgentMiddleware>();
        middleware.InvokeAsync(
                Arg.Any<IAgent>(),
                Arg.Any<IEnumerable<Message>>(),
                Arg.Any<Func<IEnumerable<Message>, Task<MessageResponse>>>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                middlewareInvoked = true;
                var next = callInfo.ArgAt<Func<IEnumerable<Message>, Task<MessageResponse>>>(2);
                return next(callInfo.ArgAt<IEnumerable<Message>>(1));
            });

        var innerAgent = Substitute.For<IAgent>();
        innerAgent.Name.Returns("inner");
        innerAgent.InvokeAsync(Arg.Any<IEnumerable<Message>>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                Id = "test",
                DoneReason = MessageDoneReason.EndTurn,
                Message = new IronHive.Abstractions.Messages.Roles.AssistantMessage
                {
                    Content = [new IronHive.Abstractions.Messages.Content.TextMessageContent { Value = "ok" }]
                },
                TokenUsage = new MessageTokenUsage { InputTokens = 1, OutputTokens = 1 }
            });

        var wrappedAgent = innerAgent.WithMiddleware(middleware);
        await wrappedAgent.InvokeAsync(Array.Empty<Message>());

        middlewareInvoked.Should().BeTrue();
    }
}
