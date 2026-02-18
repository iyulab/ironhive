using FluentAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Core.Agent;
using NSubstitute;

namespace IronHive.Tests.Agent;

public class CircuitBreakerMiddlewareTests
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

    private static IAgent CreateMockAgent(string name = "test-agent")
    {
        var agent = Substitute.For<IAgent>();
        agent.Name.Returns(name);
        return agent;
    }

    #region Initial State

    [Fact]
    public void InitialState_IsClosed()
    {
        var sut = new CircuitBreakerMiddleware();

        sut.State.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void Constructor_WithThresholdAndDuration_SetsOptions()
    {
        var sut = new CircuitBreakerMiddleware(3, TimeSpan.FromSeconds(10));

        sut.State.Should().Be(CircuitState.Closed);
    }

    #endregion

    #region Closed State — Success

    [Fact]
    public async Task Closed_Success_RemainsClosedAndReturnsResponse()
    {
        var sut = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 3
        });
        var agent = CreateMockAgent();

        var result = await sut.InvokeAsync(
            agent,
            [],
            _ => Task.FromResult(s_okResponse));

        result.Should().BeSameAs(s_okResponse);
        sut.State.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public async Task Closed_SuccessAfterFailures_ResetsFailureCount()
    {
        var sut = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 3,
            FailureWindow = TimeSpan.FromMinutes(5)
        });
        var agent = CreateMockAgent();

        // 2 failures (below threshold)
        for (var i = 0; i < 2; i++)
        {
            var act = async () => await sut.InvokeAsync(
                agent, [],
                _ => throw new InvalidOperationException("fail"));
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // Success resets count
        await sut.InvokeAsync(agent, [], _ => Task.FromResult(s_okResponse));

        // 2 more failures should not open (count was reset)
        for (var i = 0; i < 2; i++)
        {
            var act = async () => await sut.InvokeAsync(
                agent, [],
                _ => throw new InvalidOperationException("fail"));
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        sut.State.Should().Be(CircuitState.Closed);
    }

    #endregion

    #region Closed → Open Transition

    [Fact]
    public async Task Closed_FailuresReachThreshold_TransitionsToOpen()
    {
        var sut = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 3,
            BreakDuration = TimeSpan.FromMinutes(1),
            FailureWindow = TimeSpan.FromMinutes(5)
        });
        var agent = CreateMockAgent();

        for (var i = 0; i < 3; i++)
        {
            var act = async () => await sut.InvokeAsync(
                agent, [],
                _ => throw new InvalidOperationException("fail"));
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        sut.State.Should().Be(CircuitState.Open);
    }

    [Fact]
    public async Task Closed_FailuresBelowThreshold_StaysClosed()
    {
        var sut = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 5,
            FailureWindow = TimeSpan.FromMinutes(5)
        });
        var agent = CreateMockAgent();

        for (var i = 0; i < 4; i++)
        {
            var act = async () => await sut.InvokeAsync(
                agent, [],
                _ => throw new InvalidOperationException("fail"));
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        sut.State.Should().Be(CircuitState.Closed);
    }

    #endregion

    #region Open State — Rejection

    [Fact]
    public async Task Open_RejectsRequestsWithCircuitBreakerOpenException()
    {
        var sut = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 1,
            BreakDuration = TimeSpan.FromMinutes(5)
        });
        var agent = CreateMockAgent("my-agent");

        // Open the circuit
        var act1 = async () => await sut.InvokeAsync(
            agent, [],
            _ => throw new InvalidOperationException("fail"));
        await act1.Should().ThrowAsync<InvalidOperationException>();

        // Subsequent request should be rejected
        var act2 = async () => await sut.InvokeAsync(
            agent, [],
            _ => Task.FromResult(s_okResponse));
        await act2.Should().ThrowAsync<CircuitBreakerOpenException>()
            .WithMessage("*my-agent*");
    }

    #endregion

    #region Open → HalfOpen → Closed Transition

    [Fact]
    public async Task Open_AfterBreakDuration_TransitionsToHalfOpen()
    {
        var sut = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 1,
            BreakDuration = TimeSpan.FromMilliseconds(50)
        });
        var agent = CreateMockAgent();

        // Open the circuit
        var act = async () => await sut.InvokeAsync(
            agent, [],
            _ => throw new InvalidOperationException("fail"));
        await act.Should().ThrowAsync<InvalidOperationException>();
        sut.State.Should().Be(CircuitState.Open);

        // Wait for break duration
        await Task.Delay(100);

        sut.State.Should().Be(CircuitState.HalfOpen);
    }

    [Fact]
    public async Task HalfOpen_SuccessfulRequest_TransitionsToClosed()
    {
        var sut = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 1,
            BreakDuration = TimeSpan.FromMilliseconds(50)
        });
        var agent = CreateMockAgent();

        // Open → wait → HalfOpen
        var act = async () => await sut.InvokeAsync(
            agent, [],
            _ => throw new InvalidOperationException("fail"));
        await act.Should().ThrowAsync<InvalidOperationException>();
        await Task.Delay(100);

        // Success in HalfOpen → Closed
        await sut.InvokeAsync(agent, [], _ => Task.FromResult(s_okResponse));

        sut.State.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public async Task HalfOpen_FailedRequest_TransitionsBackToOpen()
    {
        var sut = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 1,
            BreakDuration = TimeSpan.FromMilliseconds(50)
        });
        var agent = CreateMockAgent();

        // Open → wait → HalfOpen
        var act1 = async () => await sut.InvokeAsync(
            agent, [],
            _ => throw new InvalidOperationException("fail"));
        await act1.Should().ThrowAsync<InvalidOperationException>();
        await Task.Delay(100);
        sut.State.Should().Be(CircuitState.HalfOpen);

        // Failure in HalfOpen → Open again
        var act2 = async () => await sut.InvokeAsync(
            agent, [],
            _ => throw new InvalidOperationException("fail again"));
        await act2.Should().ThrowAsync<InvalidOperationException>();

        sut.State.Should().Be(CircuitState.Open);
    }

    #endregion

    #region Reset

    [Fact]
    public async Task Reset_FromOpen_TransitionsToClosed()
    {
        var sut = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 1,
            BreakDuration = TimeSpan.FromMinutes(5)
        });
        var agent = CreateMockAgent();

        // Open the circuit
        var act = async () => await sut.InvokeAsync(
            agent, [],
            _ => throw new InvalidOperationException("fail"));
        await act.Should().ThrowAsync<InvalidOperationException>();
        sut.State.Should().Be(CircuitState.Open);

        // Reset
        sut.Reset();

        sut.State.Should().Be(CircuitState.Closed);

        // Should work again
        var result = await sut.InvokeAsync(agent, [], _ => Task.FromResult(s_okResponse));
        result.Should().BeSameAs(s_okResponse);
    }

    [Fact]
    public void Reset_FromClosed_NoStateChangeCallback()
    {
        var stateChanges = new List<(CircuitState Old, CircuitState New)>();
        var sut = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            OnStateChanged = (old, @new) => stateChanges.Add((old, @new))
        });

        sut.Reset();

        stateChanges.Should().BeEmpty();
    }

    #endregion

    #region Callbacks

    [Fact]
    public async Task OnStateChanged_InvokedOnTransitions()
    {
        var stateChanges = new List<(CircuitState Old, CircuitState New)>();
        var sut = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 1,
            BreakDuration = TimeSpan.FromMilliseconds(50),
            OnStateChanged = (old, @new) => stateChanges.Add((old, @new))
        });
        var agent = CreateMockAgent();

        // Closed → Open
        var act = async () => await sut.InvokeAsync(
            agent, [],
            _ => throw new InvalidOperationException("fail"));
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Open → HalfOpen (via timer)
        await Task.Delay(100);
        _ = sut.State; // Trigger state update

        // HalfOpen → Closed (via success)
        await sut.InvokeAsync(agent, [], _ => Task.FromResult(s_okResponse));

        stateChanges.Should().HaveCount(3);
        stateChanges[0].Should().Be((CircuitState.Closed, CircuitState.Open));
        stateChanges[1].Should().Be((CircuitState.Open, CircuitState.HalfOpen));
        stateChanges[2].Should().Be((CircuitState.HalfOpen, CircuitState.Closed));
    }

    [Fact]
    public async Task OnBreak_InvokedWhenCircuitOpens()
    {
        string? capturedAgent = null;
        Exception? capturedException = null;
        var sut = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 1,
            OnBreak = (name, ex, _) =>
            {
                capturedAgent = name;
                capturedException = ex;
            }
        });
        var agent = CreateMockAgent("breaker-agent");

        var act = async () => await sut.InvokeAsync(
            agent, [],
            _ => throw new InvalidOperationException("boom"));
        await act.Should().ThrowAsync<InvalidOperationException>();

        capturedAgent.Should().Be("breaker-agent");
        capturedException.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("boom");
    }

    [Fact]
    public async Task OnRejected_InvokedWhenRequestRejected()
    {
        string? rejectedAgent = null;
        CircuitState? rejectedState = null;
        var sut = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 1,
            BreakDuration = TimeSpan.FromMinutes(5),
            OnRejected = (name, state) =>
            {
                rejectedAgent = name;
                rejectedState = state;
            }
        });
        var agent = CreateMockAgent("rejected-agent");

        // Open the circuit
        var act1 = async () => await sut.InvokeAsync(
            agent, [],
            _ => throw new InvalidOperationException("fail"));
        await act1.Should().ThrowAsync<InvalidOperationException>();

        // Trigger rejection
        var act2 = async () => await sut.InvokeAsync(
            agent, [],
            _ => Task.FromResult(s_okResponse));
        await act2.Should().ThrowAsync<CircuitBreakerOpenException>();

        rejectedAgent.Should().Be("rejected-agent");
        rejectedState.Should().Be(CircuitState.Open);
    }

    #endregion

    #region Failure Window

    [Fact]
    public async Task FailureWindow_ExpiredFailures_DoNotCountTowardThreshold()
    {
        var sut = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 3,
            FailureWindow = TimeSpan.FromMilliseconds(50),
            BreakDuration = TimeSpan.FromMinutes(5)
        });
        var agent = CreateMockAgent();

        // 2 failures
        for (var i = 0; i < 2; i++)
        {
            var act = async () => await sut.InvokeAsync(
                agent, [],
                _ => throw new InvalidOperationException("fail"));
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // Wait for failure window to expire
        await Task.Delay(100);

        // 2 more failures (total in window = 2, threshold = 3)
        for (var i = 0; i < 2; i++)
        {
            var act = async () => await sut.InvokeAsync(
                agent, [],
                _ => throw new InvalidOperationException("fail"));
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        sut.State.Should().Be(CircuitState.Closed);
    }

    #endregion

    #region Cancellation

    [Fact]
    public async Task CancellationRequested_DoesNotCountAsFailure()
    {
        var sut = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 1,
            BreakDuration = TimeSpan.FromMinutes(5)
        });
        var agent = CreateMockAgent();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await sut.InvokeAsync(
            agent, [],
            _ => throw new OperationCanceledException(),
            cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();

        sut.State.Should().Be(CircuitState.Closed);
    }

    #endregion
}
