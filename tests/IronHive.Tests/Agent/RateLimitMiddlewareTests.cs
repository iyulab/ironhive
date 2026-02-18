using FluentAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Core.Agent;
using NSubstitute;

namespace IronHive.Tests.Agent;

public class RateLimitMiddlewareTests : IDisposable
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

    private readonly List<RateLimitMiddleware> _disposables = [];

    private static IAgent CreateMockAgent(string name = "test-agent")
    {
        var agent = Substitute.For<IAgent>();
        agent.Name.Returns(name);
        return agent;
    }

    private RateLimitMiddleware CreateSut(RateLimitMiddlewareOptions? options = null)
    {
        var sut = new RateLimitMiddleware(options);
        _disposables.Add(sut);
        return sut;
    }

    private RateLimitMiddleware CreateSut(int maxRequests, TimeSpan window)
    {
        var sut = new RateLimitMiddleware(maxRequests, window);
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
    public async Task InvokeAsync_WithinLimit_PassesThrough()
    {
        var sut = CreateSut(new RateLimitMiddlewareOptions
        {
            MaxRequests = 5,
            Window = TimeSpan.FromSeconds(10)
        });
        var agent = CreateMockAgent();

        var result = await sut.InvokeAsync(
            agent, [],
            _ => Task.FromResult(s_okResponse));

        result.Should().BeSameAs(s_okResponse);
    }

    [Fact]
    public async Task InvokeAsync_MultipleWithinLimit_AllSucceed()
    {
        var sut = CreateSut(new RateLimitMiddlewareOptions
        {
            MaxRequests = 3,
            Window = TimeSpan.FromSeconds(10)
        });
        var agent = CreateMockAgent();

        for (var i = 0; i < 3; i++)
        {
            var result = await sut.InvokeAsync(
                agent, [],
                _ => Task.FromResult(s_okResponse));
            result.Should().BeSameAs(s_okResponse);
        }
    }

    [Fact]
    public async Task Constructor_WithMaxRequestsAndWindow_Works()
    {
        var sut = CreateSut(10, TimeSpan.FromMinutes(1));
        var agent = CreateMockAgent();

        var result = await sut.InvokeAsync(
            agent, [],
            _ => Task.FromResult(s_okResponse));

        result.Should().BeSameAs(s_okResponse);
    }

    #endregion

    #region CurrentRequestCount

    [Fact]
    public async Task CurrentRequestCount_TracksCompletedRequests()
    {
        var sut = CreateSut(new RateLimitMiddlewareOptions
        {
            MaxRequests = 10,
            Window = TimeSpan.FromSeconds(10)
        });
        var agent = CreateMockAgent();

        sut.CurrentRequestCount.Should().Be(0);

        await sut.InvokeAsync(agent, [], _ => Task.FromResult(s_okResponse));
        sut.CurrentRequestCount.Should().Be(1);

        await sut.InvokeAsync(agent, [], _ => Task.FromResult(s_okResponse));
        sut.CurrentRequestCount.Should().Be(2);
    }

    [Fact]
    public async Task CurrentRequestCount_ExcludesExpiredTimestamps()
    {
        var sut = CreateSut(new RateLimitMiddlewareOptions
        {
            MaxRequests = 10,
            Window = TimeSpan.FromMilliseconds(50)
        });
        var agent = CreateMockAgent();

        await sut.InvokeAsync(agent, [], _ => Task.FromResult(s_okResponse));
        sut.CurrentRequestCount.Should().Be(1);

        await Task.Delay(100);

        sut.CurrentRequestCount.Should().Be(0);
    }

    #endregion

    #region Rate Limiting

    [Fact]
    public async Task InvokeAsync_ExceedsLimit_WaitsForWindowExpiry()
    {
        var sut = CreateSut(new RateLimitMiddlewareOptions
        {
            MaxRequests = 2,
            Window = TimeSpan.FromMilliseconds(100)
        });
        var agent = CreateMockAgent();

        // Fill the limit
        await sut.InvokeAsync(agent, [], _ => Task.FromResult(s_okResponse));
        await sut.InvokeAsync(agent, [], _ => Task.FromResult(s_okResponse));

        // Third request should wait and then succeed
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await sut.InvokeAsync(
            agent, [],
            _ => Task.FromResult(s_okResponse));
        sw.Stop();

        result.Should().BeSameAs(s_okResponse);
        sw.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(50); // waited at least some time
    }

    [Fact]
    public async Task OnRateLimited_InvokedWhenWaiting()
    {
        string? rateLimitedAgent = null;
        TimeSpan? waitDuration = null;
        var sut = CreateSut(new RateLimitMiddlewareOptions
        {
            MaxRequests = 1,
            Window = TimeSpan.FromMilliseconds(100),
            OnRateLimited = (name, wait) =>
            {
                rateLimitedAgent = name;
                waitDuration = wait;
            }
        });
        var agent = CreateMockAgent("limited-agent");

        // Fill the limit
        await sut.InvokeAsync(agent, [], _ => Task.FromResult(s_okResponse));

        // Second request triggers rate limiting
        await sut.InvokeAsync(agent, [], _ => Task.FromResult(s_okResponse));

        rateLimitedAgent.Should().Be("limited-agent");
        waitDuration.Should().NotBeNull();
        waitDuration!.Value.Should().BeGreaterThan(TimeSpan.Zero);
    }

    #endregion

    #region Cancellation

    [Fact]
    public async Task InvokeAsync_CancelledWhileWaiting_ThrowsOperationCanceledException()
    {
        var sut = CreateSut(new RateLimitMiddlewareOptions
        {
            MaxRequests = 1,
            Window = TimeSpan.FromSeconds(30)
        });
        var agent = CreateMockAgent();

        // Fill the limit
        await sut.InvokeAsync(agent, [], _ => Task.FromResult(s_okResponse));

        // Cancel while waiting for slot
        using var cts = new CancellationTokenSource(50);
        var act = async () => await sut.InvokeAsync(
            agent, [],
            _ => Task.FromResult(s_okResponse),
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_CanBeCalledSafely()
    {
        var sut = new RateLimitMiddleware();
        var act = () => sut.Dispose();

        act.Should().NotThrow();
    }

    #endregion
}
