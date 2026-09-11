using System.Runtime.CompilerServices;
using AwesomeAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Core.Agent;
using IronHive.Core.Agent.Orchestration;

namespace IronHive.Tests.Conventions;

// Known limitation, pinned rather than hidden: a middleware that implements only IAgentMiddleware is
// not run on streaming calls -- neither by MiddlewareAgent nor by an orchestrator's ExecuteStreamingAsync.
// Every built-in middleware implements both halves since 0.26.0 (AgentMiddlewareStreamingRosterTests keeps
// it that way), so this now concerns a caller's own middleware. These tests record the behaviour so that
// changing it is a decision, and each has a control with a middleware implementing both halves, so that
// "skipped" cannot be confused with "the test could not observe the middleware at all";
// docs/MIDDLEWARE.md describes the consequence.
//
// Not part of OrchestratorStreamingEquivalenceTests on purpose: a reader of that class expects the two
// halves to agree, and here they do not.
public class AgentMiddlewareStreamingLimitationTests
{
    [Fact]
    public async Task MiddlewareAgent_BufferedOnlyMiddleware_IsNotRunWhenStreaming()
    {
        var marker = new BufferedOnlyMarker();
        var agent = new EchoAgent("a").WithMiddleware(marker);

        await agent.InvokeAsync([Message.User("hi")], cancellationToken: TestContext.Current.CancellationToken);
        marker.BufferedCalls.Should().Be(1, "the fixture's middleware must run on the buffered call");

        await DrainAsync(agent.InvokeStreamingAsync([Message.User("hi")], cancellationToken: TestContext.Current.CancellationToken));
        marker.BufferedCalls.Should().Be(1, "known limitation: the streaming call does not run it");
    }

    [Fact]
    public async Task MiddlewareAgent_MiddlewareWithBothHalves_RunsOnBoth()
    {
        var marker = new BothHalvesMarker();
        var agent = new EchoAgent("a").WithMiddleware(marker);

        await agent.InvokeAsync([Message.User("hi")], cancellationToken: TestContext.Current.CancellationToken);
        await DrainAsync(agent.InvokeStreamingAsync([Message.User("hi")], cancellationToken: TestContext.Current.CancellationToken));

        marker.BufferedCalls.Should().Be(1);
        marker.StreamingCalls.Should().Be(1);
    }

    [Fact]
    public async Task Orchestrator_BufferedOnlyAgentMiddleware_IsNotRunWhenStreaming()
    {
        var marker = new BufferedOnlyMarker();

        var buffered = await Chain(marker).ExecuteAsync([Message.User("start")], TestContext.Current.CancellationToken);
        buffered.IsSuccess.Should().BeTrue("the fixture must run the chain");
        marker.BufferedCalls.Should().Be(2, "the fixture's middleware must run once per agent on the buffered call");

        var streamed = await StreamAsync(Chain(marker));
        streamed!.IsSuccess.Should().BeTrue("the fixture must run the chain");
        marker.BufferedCalls.Should().Be(2, "known limitation: ExecuteStreamingAsync does not run it");
    }

    [Fact]
    public async Task Orchestrator_AgentMiddlewareWithBothHalves_RunsOnBoth()
    {
        var marker = new BothHalvesMarker();

        await Chain(marker).ExecuteAsync([Message.User("start")], TestContext.Current.CancellationToken);
        var streamed = await StreamAsync(Chain(marker));

        streamed!.IsSuccess.Should().BeTrue("the fixture must run the chain");
        marker.BufferedCalls.Should().Be(2);
        marker.StreamingCalls.Should().Be(2);
    }

    private static SequentialOrchestrator Chain(IAgentMiddleware middleware)
    {
        var orchestrator = new SequentialOrchestrator(new SequentialOrchestratorOptions { AgentMiddlewares = [middleware] });
        orchestrator.AddAgents([new EchoAgent("first"), new EchoAgent("second")]);
        return orchestrator;
    }

    private static async Task<OrchestrationResult?> StreamAsync(SequentialOrchestrator orchestrator)
    {
        OrchestrationResult? result = null;
        await foreach (var e in orchestrator.ExecuteStreamingAsync([Message.User("start")], TestContext.Current.CancellationToken))
        {
            result = e.Result ?? result;
        }

        return result;
    }

    private static async Task DrainAsync(IAsyncEnumerable<StreamingMessageResponse> frames)
    {
        await foreach (var _ in frames)
        {
        }
    }

    private sealed class BufferedOnlyMarker : IAgentMiddleware
    {
        public int BufferedCalls { get; private set; }

        public Task<MessageResponse> InvokeAsync(
            IAgent agent,
            IEnumerable<Message> messages,
            AgentInvokeOptions? options,
            Func<IEnumerable<Message>, AgentInvokeOptions?, Task<MessageResponse>> next,
            CancellationToken cancellationToken = default)
        {
            BufferedCalls++;
            return next(messages, options);
        }
    }

    private sealed class BothHalvesMarker : IAgentMiddleware, IStreamingAgentMiddleware
    {
        public int BufferedCalls { get; private set; }

        public int StreamingCalls { get; private set; }

        public Task<MessageResponse> InvokeAsync(
            IAgent agent,
            IEnumerable<Message> messages,
            AgentInvokeOptions? options,
            Func<IEnumerable<Message>, AgentInvokeOptions?, Task<MessageResponse>> next,
            CancellationToken cancellationToken = default)
        {
            BufferedCalls++;
            return next(messages, options);
        }

        public IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
            IAgent agent,
            IEnumerable<Message> messages,
            AgentInvokeOptions? options,
            Func<IEnumerable<Message>, AgentInvokeOptions?, IAsyncEnumerable<StreamingMessageResponse>> next,
            CancellationToken cancellationToken = default)
        {
            StreamingCalls++;
            return next(messages, options);
        }
    }

    private sealed class EchoAgent(string name) : IAgent
    {
        public string Provider { get; set; } = "mock";

        public string Model { get; set; } = "mock-model";

        public string Name { get; set; } = name;

        public string Description { get; set; } = "Echo";

        public string? Instructions { get; set; }

        public IToolCollection? Tools { get; set; }

        public int? MaxTokens { get; set; }

        private static MessageTokenUsage Usage => new() { InputTokens = 3, OutputTokens = 2 };

        public Task<MessageResponse> InvokeAsync(
            IEnumerable<Message> messages,
            AgentInvokeOptions? options = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new MessageResponse
            {
                ResponseId = Name,
                DoneReason = MessageDoneReason.EndTurn,
                Message = Message.Assistant($"from-{Name}"),
                TokenUsage = Usage,
            });

        public async IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
            IEnumerable<Message> messages,
            AgentInvokeOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return new StreamingMessageBeginResponse();
            yield return new StreamingContentAddedResponse { Index = 0, Content = new TextMessageContent { Value = $"from-{Name}" } };
            yield return new StreamingContentCompletedResponse { Index = 0 };
            await Task.Yield();
            yield return new StreamingMessageDoneResponse
            {
                ResponseId = Name,
                DoneReason = MessageDoneReason.EndTurn,
                Message = Message.Assistant($"from-{Name}"),
                TokenUsage = Usage,
            };
        }
    }
}
