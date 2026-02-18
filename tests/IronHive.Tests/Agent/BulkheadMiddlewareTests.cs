using FluentAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Core.Agent;
using NSubstitute;

namespace IronHive.Tests.Agent;

public class BulkheadMiddlewareTests : IDisposable
{
    private static readonly MessageResponse s_okResponse = new()
    {
        Id = "test",
        DoneReason = MessageDoneReason.EndTurn,
        Message = new AssistantMessage
        {
            Content = [new TextMessageContent { Value = "ok" }]
        },
        TokenUsage = new MessageTokenUsage { InputTokens = 1, OutputTokens = 1 }
    };

    private readonly List<BulkheadMiddleware> _disposables = [];

    private static IAgent CreateMockAgent(string name = "test-agent")
    {
        var agent = Substitute.For<IAgent>();
        agent.Name.Returns(name);
        return agent;
    }

    private BulkheadMiddleware CreateSut(BulkheadMiddlewareOptions? options = null)
    {
        var sut = new BulkheadMiddleware(options);
        _disposables.Add(sut);
        return sut;
    }

    public void Dispose()
    {
        foreach (var d in _disposables)
            d.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Basic Functionality

    [Fact]
    public async Task InvokeAsync_WithinConcurrencyLimit_PassesThrough()
    {
        var sut = CreateSut(new BulkheadMiddlewareOptions { MaxConcurrency = 5 });
        var agent = CreateMockAgent();

        var result = await sut.InvokeAsync(
            agent, [],
            _ => Task.FromResult(s_okResponse));

        result.Should().BeSameAs(s_okResponse);
    }

    [Fact]
    public void Constructor_MaxConcurrencyOnly_Works()
    {
        using var sut = new BulkheadMiddleware(3);
        sut.AvailableSlots.Should().Be(3);
    }

    [Fact]
    public void Constructor_MaxConcurrencyAndQueueSize_Works()
    {
        using var sut = new BulkheadMiddleware(3, 5);
        sut.AvailableSlots.Should().Be(3);
    }

    #endregion

    #region Counters

    [Fact]
    public async Task CurrentExecuting_TracksActiveRequests()
    {
        var sut = CreateSut(new BulkheadMiddlewareOptions { MaxConcurrency = 5 });
        var agent = CreateMockAgent();
        var tcs = new TaskCompletionSource<MessageResponse>();

        sut.CurrentExecuting.Should().Be(0);

        // Start a request that blocks
        var task = sut.InvokeAsync(agent, [], _ => tcs.Task);
        await Task.Delay(50); // let it start

        sut.CurrentExecuting.Should().Be(1);

        // Complete the request
        tcs.SetResult(s_okResponse);
        await task;

        sut.CurrentExecuting.Should().Be(0);
    }

    [Fact]
    public void AvailableSlots_ReflectsSemaphoreState()
    {
        var sut = CreateSut(new BulkheadMiddlewareOptions { MaxConcurrency = 3 });

        sut.AvailableSlots.Should().Be(3);
    }

    #endregion

    #region Concurrency Limiting

    [Fact]
    public async Task InvokeAsync_ConcurrentRequests_LimitedByMaxConcurrency()
    {
        var sut = CreateSut(new BulkheadMiddlewareOptions
        {
            MaxConcurrency = 2,
            QueueTimeout = Timeout.InfiniteTimeSpan
        });
        var agent = CreateMockAgent();

        var tcs1 = new TaskCompletionSource<MessageResponse>();
        var tcs2 = new TaskCompletionSource<MessageResponse>();

        // Fill concurrency slots
        var task1 = sut.InvokeAsync(agent, [], _ => tcs1.Task);
        var task2 = sut.InvokeAsync(agent, [], _ => tcs2.Task);
        await Task.Delay(50);

        sut.CurrentExecuting.Should().Be(2);
        sut.AvailableSlots.Should().Be(0);

        // Complete first request — frees a slot
        tcs1.SetResult(s_okResponse);
        await task1;

        sut.CurrentExecuting.Should().Be(1);
        sut.AvailableSlots.Should().Be(1);

        tcs2.SetResult(s_okResponse);
        await task2;
    }

    #endregion

    #region Queue Rejection

    [Fact]
    public async Task InvokeAsync_QueueFull_ThrowsBulkheadRejectedException()
    {
        var sut = CreateSut(new BulkheadMiddlewareOptions
        {
            MaxConcurrency = 1,
            MaxQueueSize = 1,
            QueueTimeout = Timeout.InfiniteTimeSpan
        });
        var agent = CreateMockAgent("rejected-agent");
        var tcs = new TaskCompletionSource<MessageResponse>();

        // Fill concurrency slot
        var task1 = sut.InvokeAsync(agent, [], _ => tcs.Task);
        await Task.Delay(50);

        // Fill queue slot (waiting for concurrency)
        var task2 = Task.Run(() => sut.InvokeAsync(agent, [], _ => Task.FromResult(s_okResponse)));
        await Task.Delay(50);

        // Third request exceeds queue — should be rejected
        var act = async () => await sut.InvokeAsync(
            agent, [],
            _ => Task.FromResult(s_okResponse));

        await act.Should().ThrowAsync<BulkheadRejectedException>()
            .WithMessage("*rejected-agent*");

        tcs.SetResult(s_okResponse);
        await task1;
        await task2;
    }

    #endregion

    #region Queue Timeout

    [Fact]
    public async Task InvokeAsync_QueueTimeout_ThrowsBulkheadRejectedException()
    {
        var sut = CreateSut(new BulkheadMiddlewareOptions
        {
            MaxConcurrency = 1,
            QueueTimeout = TimeSpan.FromMilliseconds(50)
        });
        var agent = CreateMockAgent("timeout-agent");
        var tcs = new TaskCompletionSource<MessageResponse>();

        // Fill concurrency slot
        var task1 = sut.InvokeAsync(agent, [], _ => tcs.Task);
        await Task.Delay(20);

        // Second request should timeout waiting for slot
        var act = async () => await sut.InvokeAsync(
            agent, [],
            _ => Task.FromResult(s_okResponse));

        await act.Should().ThrowAsync<BulkheadRejectedException>()
            .WithMessage("*timeout-agent*");

        tcs.SetResult(s_okResponse);
        await task1;
    }

    #endregion

    #region Callbacks

    [Fact]
    public async Task OnQueued_InvokedWhenRequestEntersQueue()
    {
        string? queuedAgent = null;
        var sut = CreateSut(new BulkheadMiddlewareOptions
        {
            MaxConcurrency = 5,
            OnQueued = (name, _, _) => queuedAgent = name
        });
        var agent = CreateMockAgent("queued-agent");

        await sut.InvokeAsync(agent, [], _ => Task.FromResult(s_okResponse));

        queuedAgent.Should().Be("queued-agent");
    }

    [Fact]
    public async Task OnRejected_InvokedWhenQueueFull()
    {
        string? rejectedAgent = null;
        var sut = CreateSut(new BulkheadMiddlewareOptions
        {
            MaxConcurrency = 1,
            MaxQueueSize = 1,
            QueueTimeout = Timeout.InfiniteTimeSpan,
            OnRejected = (name, _, _) => rejectedAgent = name
        });
        var agent = CreateMockAgent("rej-agent");
        var tcs = new TaskCompletionSource<MessageResponse>();

        // Fill concurrency + queue
        var task1 = sut.InvokeAsync(agent, [], _ => tcs.Task);
        await Task.Delay(50);
        var task2 = Task.Run(() => sut.InvokeAsync(agent, [], _ => Task.FromResult(s_okResponse)));
        await Task.Delay(50);

        // Rejection
        try
        {
            await sut.InvokeAsync(agent, [], _ => Task.FromResult(s_okResponse));
        }
        catch (BulkheadRejectedException)
        {
            // Expected
        }

        rejectedAgent.Should().Be("rej-agent");

        tcs.SetResult(s_okResponse);
        await task1;
        await task2;
    }

    #endregion

    #region Cancellation

    [Fact]
    public async Task InvokeAsync_CancelledWhileWaiting_ThrowsOperationCanceledException()
    {
        var sut = CreateSut(new BulkheadMiddlewareOptions
        {
            MaxConcurrency = 1,
            QueueTimeout = Timeout.InfiniteTimeSpan
        });
        var agent = CreateMockAgent();
        var tcs = new TaskCompletionSource<MessageResponse>();

        // Fill concurrency slot
        var task1 = sut.InvokeAsync(agent, [], _ => tcs.Task);
        await Task.Delay(20);

        // Cancel while waiting for slot
        using var cts = new CancellationTokenSource(50);
        var act = async () => await sut.InvokeAsync(
            agent, [],
            _ => Task.FromResult(s_okResponse),
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();

        tcs.SetResult(s_okResponse);
        await task1;
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_CanBeCalledSafely()
    {
        var sut = new BulkheadMiddleware(new BulkheadMiddlewareOptions
        {
            MaxConcurrency = 5,
            MaxQueueSize = 3
        });

        var act = () => sut.Dispose();
        act.Should().NotThrow();
    }

    #endregion
}
