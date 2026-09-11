using System.Runtime.CompilerServices;
using AwesomeAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Core.Agent;
using IronHive.Core.Agent.Orchestration;
using NSubstitute;

namespace IronHive.Tests.Agent;

/// <summary>
/// The streaming half of every built-in agent middleware. Before these existed, each of them was
/// skipped on streaming calls without a word, so a timeout, retry or rate limit configured for an
/// agent applied to <c>InvokeAsync</c> only. Semantics pinned here: Timeout bounds the whole stream;
/// Retry and Fallback act only while no frame has reached the caller; RateLimit, Bulkhead and
/// CircuitBreaker take and settle their slot or state around the stream; Caching stores a stream that
/// was read to a normal end and replays it.
/// </summary>
public class AgentMiddlewareStreamingTests
{
    private static readonly Message[] Input = [Message.User("hi")];

    #region Timeout

    [Fact]
    public async Task Timeout_StreamWithinDeadline_PassesEveryFrame()
    {
        var middleware = new TimeoutMiddleware(TimeSpan.FromSeconds(10));

        var frames = await DrainAsync(middleware.InvokeStreamingAsync(Agent(), Input, null, (_, _) => Stream(Frames("ok")), Ct));

        frames.Should().HaveCount(3);
    }

    [Fact]
    public async Task Timeout_SourceThatNeverYieldsAgain_TimesOutAtTheDeadline()
    {
        // The source ignores its token, as a stalled connection would: only the race can end it.
        var release = new TaskCompletionSource();
        var timedOut = 0;
        var middleware = new TimeoutMiddleware(new TimeoutMiddlewareOptions
        {
            Timeout = TimeSpan.FromMilliseconds(200),
            OnTimeout = (_, _) => Interlocked.Increment(ref timedOut)
        });

        try
        {
            // Asserted as "finished in time" first: a regression would hang here, and wrapping the run in
            // WaitAsync would turn that hang into the very TimeoutException this test expects.
            var run = DrainAsync(middleware.InvokeStreamingAsync(Agent(), Input, null, (_, _) => Stalled(release.Task), Ct));
            (await Task.WhenAny(run, Task.Delay(TimeSpan.FromSeconds(10), Ct))).Should().BeSameAs(run);

            await ((Func<Task>)(() => run)).Should().ThrowAsync<TimeoutException>();
            timedOut.Should().Be(1);
        }
        finally
        {
            release.TrySetResult(); // lets the abandoned step settle so the enumerator is released
        }
    }

    [Fact]
    public async Task Timeout_CooperativeSource_IsCancelledByTheDeadline_AndReportedAsTimeout()
    {
        var middleware = new TimeoutMiddleware(TimeSpan.FromMilliseconds(200));

        var act = () => DrainAsync(middleware.InvokeStreamingAsync(Agent(), Input, null, (_, _) => WaitsForCancellation(), Ct));

        await act.Should().ThrowAsync<TimeoutException>();
    }

    [Fact]
    public async Task Timeout_CallerCancels_IsCancellation_NotTimeout()
    {
        var middleware = new TimeoutMiddleware(TimeSpan.FromSeconds(30));
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        var act = () => DrainAsync(middleware.InvokeStreamingAsync(Agent(), Input, null, (_, _) => WaitsForCancellation(), cts.Token));

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Retry

    [Fact]
    public async Task Retry_FailureBeforeTheFirstFrame_IsRetried()
    {
        var attempts = 0;
        var middleware = new RetryMiddleware(new RetryMiddlewareOptions { MaxRetries = 3, InitialDelay = TimeSpan.Zero, JitterFactor = 0 });

        var frames = await DrainAsync(middleware.InvokeStreamingAsync(Agent(), Input, null, (_, _) =>
            ++attempts < 3 ? FailingAfter(0) : Stream(Frames("third")), Ct));

        attempts.Should().Be(3);
        frames.OfType<StreamingContentAddedResponse>().Single().Content
            .Should().BeOfType<TextMessageContent>().Which.Value.Should().Be("third");
    }

    [Fact]
    public async Task Retry_FailureAfterAFrameWasDelivered_IsNotRetried()
    {
        var attempts = 0;
        var middleware = new RetryMiddleware(new RetryMiddlewareOptions { MaxRetries = 3, InitialDelay = TimeSpan.Zero, JitterFactor = 0 });

        var act = () => DrainAsync(middleware.InvokeStreamingAsync(Agent(), Input, null, (_, _) =>
        {
            attempts++;
            return FailingAfter(1);
        }, Ct));

        await act.Should().ThrowAsync<InvalidOperationException>();
        attempts.Should().Be(1, "a frame already handed to the caller cannot be taken back by retrying");
    }

    #endregion

    #region Fallback

    [Fact]
    public async Task Fallback_PrimaryFailsBeforeTheFirstFrame_StreamsTheFallback()
    {
        var fallback = Agent("fallback");
        fallback.InvokeStreamingAsync(Arg.Any<IEnumerable<Message>>(), Arg.Any<AgentInvokeOptions?>(), Arg.Any<CancellationToken>())
            .Returns(_ => Stream(Frames("from-fallback")));
        var middleware = new FallbackMiddleware(fallback);

        var frames = await DrainAsync(middleware.InvokeStreamingAsync(Agent(), Input, null, (_, _) => FailingAfter(0), Ct));

        frames.OfType<StreamingContentAddedResponse>().Single().Content
            .Should().BeOfType<TextMessageContent>().Which.Value.Should().Be("from-fallback");
    }

    [Fact]
    public async Task Fallback_PrimaryFailsAfterAFrame_Propagates()
    {
        var fallback = Agent("fallback");
        var middleware = new FallbackMiddleware(fallback);

        var act = () => DrainAsync(middleware.InvokeStreamingAsync(Agent(), Input, null, (_, _) => FailingAfter(1), Ct));

        await act.Should().ThrowAsync<InvalidOperationException>();
        fallback.DidNotReceiveWithAnyArgs().InvokeStreamingAsync(default!, default, Ct);
    }

    [Fact]
    public async Task Fallback_FallbackStreamFails_ThrowsFallbackFailed()
    {
        var fallback = Agent("fallback");
        fallback.InvokeStreamingAsync(Arg.Any<IEnumerable<Message>>(), Arg.Any<AgentInvokeOptions?>(), Arg.Any<CancellationToken>())
            .Returns(_ => FailingAfter(0));
        var middleware = new FallbackMiddleware(fallback);

        var act = () => DrainAsync(middleware.InvokeStreamingAsync(Agent(), Input, null, (_, _) => FailingAfter(0), Ct));

        await act.Should().ThrowAsync<FallbackFailedException>();
    }

    #endregion

    #region RateLimit · Bulkhead · CircuitBreaker

    [Fact]
    public async Task RateLimit_StreamingCall_UsesASlot()
    {
        using var middleware = new RateLimitMiddleware(maxRequests: 5, window: TimeSpan.FromMinutes(1));

        await DrainAsync(middleware.InvokeStreamingAsync(Agent(), Input, null, (_, _) => Stream(Frames("ok")), Ct));

        middleware.CurrentRequestCount.Should().Be(1);
    }

    [Fact]
    public async Task Bulkhead_HoldsTheSlotWhileStreaming_AndReleasesItAfter()
    {
        using var middleware = new BulkheadMiddleware(maxConcurrency: 1);
        var executingDuringStream = -1;

        await foreach (var _ in middleware.InvokeStreamingAsync(Agent(), Input, null, (_, _) => Stream(Frames("ok")), Ct))
        {
            executingDuringStream = middleware.CurrentExecuting;
        }

        executingDuringStream.Should().Be(1);
        middleware.CurrentExecuting.Should().Be(0);
        middleware.AvailableSlots.Should().Be(1);
    }

    [Fact]
    public async Task Bulkhead_CallerStopsReadingEarly_SlotIsReleased()
    {
        using var middleware = new BulkheadMiddleware(maxConcurrency: 1);

        await foreach (var _ in middleware.InvokeStreamingAsync(Agent(), Input, null, (_, _) => Stream(Frames("ok")), Ct))
        {
            break;
        }

        middleware.AvailableSlots.Should().Be(1);
    }

    [Fact]
    public async Task Bulkhead_FailureAfterEntering_LeavesTheQueueAccountingOfOthersAlone()
    {
        // Before the streaming half, the buffered path's catch also ran for a failure of the call itself
        // — after the caller had already given its queue slot back — and, seeing another request
        // queued, decremented that request's count and released the queue semaphore a second time.
        using var middleware = new BulkheadMiddleware(maxConcurrency: 1, maxQueueSize: 1);
        var holdFirst = new TaskCompletionSource<MessageResponse>();

        var first = middleware.InvokeAsync(Agent(), Input, null, (_, _) => holdFirst.Task, Ct);
        var second = middleware.InvokeAsync(Agent(), Input, null, (_, _) => Task.FromResult(Response("second")), Ct);
        await WaitUntilAsync(() => middleware.CurrentQueued == 1);

        holdFirst.SetException(new InvalidOperationException("first failed"));

        await ((Func<Task>)(() => first)).Should().ThrowAsync<InvalidOperationException>();
        (await second).ResponseId.Should().Be("second");
        middleware.CurrentQueued.Should().Be(0);
        middleware.CurrentExecuting.Should().Be(0);
    }

    [Fact]
    public async Task CircuitBreaker_StreamingFailure_OpensTheCircuit()
    {
        var middleware = new CircuitBreakerMiddleware(failureThreshold: 1, breakDuration: TimeSpan.FromMinutes(1));

        var fail = () => DrainAsync(middleware.InvokeStreamingAsync(Agent(), Input, null, (_, _) => FailingAfter(1), Ct));
        await fail.Should().ThrowAsync<InvalidOperationException>();

        middleware.State.Should().Be(CircuitState.Open);
        var rejected = () => DrainAsync(middleware.InvokeStreamingAsync(Agent(), Input, null, (_, _) => Stream(Frames("ok")), Ct));
        await rejected.Should().ThrowAsync<CircuitBreakerOpenException>();
    }

    [Fact]
    public async Task CircuitBreaker_CompletedStream_CountsAsSuccess()
    {
        var middleware = new CircuitBreakerMiddleware(failureThreshold: 2, breakDuration: TimeSpan.FromMinutes(1));

        var fail = () => DrainAsync(middleware.InvokeStreamingAsync(Agent(), Input, null, (_, _) => FailingAfter(0), Ct));
        await fail.Should().ThrowAsync<InvalidOperationException>();
        await DrainAsync(middleware.InvokeStreamingAsync(Agent(), Input, null, (_, _) => Stream(Frames("ok")), Ct));
        await fail.Should().ThrowAsync<InvalidOperationException>();

        middleware.State.Should().Be(CircuitState.Closed, "the completed stream in between reset the failure count");
    }

    #endregion

    #region Caching

    [Fact]
    public async Task Caching_StreamReadToTheEnd_IsReplayedWithoutCallingTheAgent()
    {
        var middleware = new CachingMiddleware();
        var calls = 0;
        IAsyncEnumerable<StreamingMessageResponse> Next(IEnumerable<Message> m, AgentInvokeOptions? o)
        {
            calls++;
            return Stream(Frames("cached"));
        }

        var first = await DrainAsync(middleware.InvokeStreamingAsync(Agent(), Input, null, Next, Ct));
        var second = await DrainAsync(middleware.InvokeStreamingAsync(Agent(), Input, null, Next, Ct));

        calls.Should().Be(1);
        second.Should().Equal(first);
    }

    [Fact]
    public async Task Caching_StreamAbandonedHalfWay_IsNotStored()
    {
        var middleware = new CachingMiddleware();
        var calls = 0;
        IAsyncEnumerable<StreamingMessageResponse> Next(IEnumerable<Message> m, AgentInvokeOptions? o)
        {
            calls++;
            return Stream(Frames("partial"));
        }

        await foreach (var _ in middleware.InvokeStreamingAsync(Agent(), Input, null, Next, Ct))
        {
            break;
        }

        await DrainAsync(middleware.InvokeStreamingAsync(Agent(), Input, null, Next, Ct));

        calls.Should().Be(2, "replaying a truncated stream would hand the next caller half an answer");
    }

    #endregion

    #region Orchestrator — the path a configured timeout actually takes

    [Fact]
    public async Task Orchestrator_TimeoutMiddleware_StopsAStalledAgent_OnBothHalves()
    {
        // The consumer-visible acceptance of the defect: an orchestration configured with a timeout
        // used to wait forever on a stalled agent when run with ExecuteStreamingAsync.
        var release = new TaskCompletionSource();
        var orchestrator = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            AgentMiddlewares = [new TimeoutMiddleware(TimeSpan.FromMilliseconds(200))]
        });
        orchestrator.AddAgents([new StalledAgent(release.Task)]);

        try
        {
            var buffered = await orchestrator.ExecuteAsync(Input, Ct);
            buffered.IsSuccess.Should().BeFalse();

            var streaming = StreamToResultAsync(orchestrator);
            (await Task.WhenAny(streaming, Task.Delay(TimeSpan.FromSeconds(10), Ct))).Should().BeSameAs(streaming,
                "before the streaming half existed this run waited on the stalled agent forever");

            var streamed = await streaming;
            streamed.Should().NotBeNull();
            streamed!.IsSuccess.Should().BeFalse("the streaming run must fail at the deadline like the buffered one");
        }
        finally
        {
            release.TrySetResult();
        }
    }

    private static async Task<OrchestrationResult?> StreamToResultAsync(SequentialOrchestrator orchestrator)
    {
        OrchestrationResult? result = null;
        try
        {
            await foreach (var e in orchestrator.ExecuteStreamingAsync(Input, Ct))
            {
                result = e.Result ?? result;
            }
        }
        catch (TimeoutException)
        {
            result = new OrchestrationResult { IsSuccess = false, Error = "timeout" };
        }

        return result;
    }

    #endregion

    #region Helpers

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static IAgent Agent(string name = "a")
    {
        var agent = Substitute.For<IAgent>();
        agent.Name.Returns(name);
        return agent;
    }

    private static MessageResponse Response(string id) => new()
    {
        ResponseId = id,
        DoneReason = MessageDoneReason.EndTurn,
        Message = Message.Assistant(id),
    };

    private static StreamingMessageResponse[] Frames(string text) =>
    [
        new StreamingMessageBeginResponse(),
        new StreamingContentAddedResponse { Index = 0, Content = new TextMessageContent { Value = text } },
        new StreamingMessageDoneResponse { DoneReason = MessageDoneReason.EndTurn },
    ];

    private static async IAsyncEnumerable<StreamingMessageResponse> Stream(
        IEnumerable<StreamingMessageResponse> frames, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var frame in frames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
            yield return frame;
        }
    }

    private static async IAsyncEnumerable<StreamingMessageResponse> FailingAfter(int framesBeforeFailure)
    {
        for (var i = 0; i < framesBeforeFailure; i++)
        {
            yield return new StreamingMessageBeginResponse();
        }

        await Task.Yield();
        throw new InvalidOperationException("stream failed");
    }

    private static async IAsyncEnumerable<StreamingMessageResponse> Stalled(Task release)
    {
        yield return new StreamingMessageBeginResponse();
        await release;
    }

    private static async IAsyncEnumerable<StreamingMessageResponse> WaitsForCancellation(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return new StreamingMessageBeginResponse();
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private static async Task<List<StreamingMessageResponse>> DrainAsync(IAsyncEnumerable<StreamingMessageResponse> frames)
    {
        var list = new List<StreamingMessageResponse>();
        await foreach (var frame in frames)
        {
            list.Add(frame);
        }

        return list;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException("condition was not reached");
            }

            await Task.Delay(10, Ct);
        }
    }

    private sealed class StalledAgent(Task release) : IAgent
    {
        public string Provider { get; set; } = "mock";

        public string Model { get; set; } = "mock-model";

        public string Name { get; set; } = "stalled";

        public string Description { get; set; } = "never answers";

        public string? Instructions { get; set; }

        public IToolCollection? Tools { get; set; }

        public int? MaxTokens { get; set; }

        public async Task<MessageResponse> InvokeAsync(
            IEnumerable<Message> messages, AgentInvokeOptions? options = null, CancellationToken cancellationToken = default)
        {
            await release;
            return Response("late");
        }

        public IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
            IEnumerable<Message> messages, AgentInvokeOptions? options = null, CancellationToken cancellationToken = default)
            => Stalled(release);
    }

    #endregion
}
