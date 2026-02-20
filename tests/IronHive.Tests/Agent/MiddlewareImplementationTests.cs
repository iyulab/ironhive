using System.Runtime.CompilerServices;
using FluentAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Tools;
using IronHive.Core.Agent;
using NSubstitute;

namespace IronHive.Tests.Agent;

public class MiddlewareImplementationTests
{
    #region RetryMiddleware

    [Fact]
    public async Task Retry_Success_NoRetry()
    {
        var callCount = 0;
        var middleware = new RetryMiddleware(3);

        var result = await middleware.InvokeAsync(
            MakeAgent("a"),
            MakeMessages("input"),
            _ => { callCount++; return Task.FromResult(MakeResponse("ok")); });

        callCount.Should().Be(1);
    }

    [Fact]
    public async Task Retry_FailsThenSucceeds_RetriesCorrectly()
    {
        var callCount = 0;
        var middleware = new RetryMiddleware(new RetryMiddlewareOptions
        {
            MaxRetries = 3,
            InitialDelay = TimeSpan.FromMilliseconds(1),
            JitterFactor = 0
        });

        var result = await middleware.InvokeAsync(
            MakeAgent("a"),
            MakeMessages("input"),
            _ =>
            {
                callCount++;
                if (callCount < 3)
                    throw new InvalidOperationException("transient");
                return Task.FromResult(MakeResponse("ok"));
            });

        callCount.Should().Be(3);
    }

    [Fact]
    public async Task Retry_AllFail_ThrowsAfterMaxRetries()
    {
        var callCount = 0;
        var middleware = new RetryMiddleware(new RetryMiddlewareOptions
        {
            MaxRetries = 2,
            InitialDelay = TimeSpan.FromMilliseconds(1),
            JitterFactor = 0
        });

        var act = () => middleware.InvokeAsync(
            MakeAgent("a"),
            MakeMessages("input"),
            _ => { callCount++; throw new InvalidOperationException("fail"); });

        await act.Should().ThrowAsync<InvalidOperationException>();
        callCount.Should().Be(3); // 1 initial + 2 retries
    }

    [Fact]
    public async Task Retry_ShouldRetryFalse_NoRetry()
    {
        var callCount = 0;
        var middleware = new RetryMiddleware(new RetryMiddlewareOptions
        {
            MaxRetries = 3,
            ShouldRetry = _ => false
        });

        var act = () => middleware.InvokeAsync(
            MakeAgent("a"),
            MakeMessages("input"),
            _ => { callCount++; throw new InvalidOperationException("fail"); });

        await act.Should().ThrowAsync<InvalidOperationException>();
        callCount.Should().Be(1); // no retry
    }

    [Fact]
    public async Task Retry_OnRetryCallback_Called()
    {
        var retryAttempts = new List<int>();
        var middleware = new RetryMiddleware(new RetryMiddlewareOptions
        {
            MaxRetries = 2,
            InitialDelay = TimeSpan.FromMilliseconds(1),
            JitterFactor = 0,
            OnRetry = (_, attempt, _, _) => retryAttempts.Add(attempt)
        });

        var callCount = 0;
        await middleware.InvokeAsync(
            MakeAgent("a"),
            MakeMessages("input"),
            _ =>
            {
                callCount++;
                if (callCount <= 2)
                    throw new InvalidOperationException("fail");
                return Task.FromResult(MakeResponse("ok"));
            });

        retryAttempts.Should().Equal(1, 2);
    }

    [Fact]
    public async Task Retry_Cancellation_DoesNotRetry()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var middleware = new RetryMiddleware(3);

        var act = () => middleware.InvokeAsync(
            MakeAgent("a"),
            MakeMessages("input"),
            _ => Task.FromResult(MakeResponse("ok")),
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region CircuitBreakerMiddleware

    [Fact]
    public async Task CircuitBreaker_Success_StaysClosed()
    {
        var cb = new CircuitBreakerMiddleware(3, TimeSpan.FromSeconds(30));

        await cb.InvokeAsync(
            MakeAgent("a"),
            MakeMessages("input"),
            _ => Task.FromResult(MakeResponse("ok")));

        cb.State.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public async Task CircuitBreaker_FailuresBelowThreshold_StaysClosed()
    {
        var cb = new CircuitBreakerMiddleware(3, TimeSpan.FromSeconds(30));

        // 2 failures (below threshold of 3)
        for (int i = 0; i < 2; i++)
        {
            try
            {
                await cb.InvokeAsync(
                    MakeAgent("a"),
                    MakeMessages("input"),
                    _ => throw new InvalidOperationException("fail"));
            }
            catch (InvalidOperationException) { }
        }

        cb.State.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public async Task CircuitBreaker_FailuresReachThreshold_Opens()
    {
        var cb = new CircuitBreakerMiddleware(3, TimeSpan.FromSeconds(30));

        for (int i = 0; i < 3; i++)
        {
            try
            {
                await cb.InvokeAsync(
                    MakeAgent("a"),
                    MakeMessages("input"),
                    _ => throw new InvalidOperationException("fail"));
            }
            catch (InvalidOperationException) { }
        }

        cb.State.Should().Be(CircuitState.Open);
    }

    [Fact]
    public async Task CircuitBreaker_Open_RejectsRequests()
    {
        var cb = new CircuitBreakerMiddleware(2, TimeSpan.FromSeconds(30));

        // Open the circuit
        for (int i = 0; i < 2; i++)
        {
            try
            {
                await cb.InvokeAsync(
                    MakeAgent("a"),
                    MakeMessages("input"),
                    _ => throw new InvalidOperationException("fail"));
            }
            catch (InvalidOperationException) { }
        }

        // Next request should be rejected
        var act = () => cb.InvokeAsync(
            MakeAgent("a"),
            MakeMessages("input"),
            _ => Task.FromResult(MakeResponse("ok")));

        await act.Should().ThrowAsync<CircuitBreakerOpenException>();
    }

    [Fact]
    public async Task CircuitBreaker_Reset_ReturnsToClosed()
    {
        var cb = new CircuitBreakerMiddleware(1, TimeSpan.FromSeconds(30));

        // Open the circuit
        try
        {
            await cb.InvokeAsync(
                MakeAgent("a"),
                MakeMessages("input"),
                _ => throw new InvalidOperationException("fail"));
        }
        catch (InvalidOperationException) { }

        cb.State.Should().Be(CircuitState.Open);

        cb.Reset();
        cb.State.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public async Task CircuitBreaker_StateChangeCallback_Called()
    {
        var transitions = new List<(CircuitState From, CircuitState To)>();
        var cb = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 1,
            BreakDuration = TimeSpan.FromSeconds(30),
            OnStateChanged = (from, to) => transitions.Add((from, to))
        });

        try
        {
            await cb.InvokeAsync(
                MakeAgent("a"),
                MakeMessages("input"),
                _ => throw new InvalidOperationException("fail"));
        }
        catch (InvalidOperationException) { }

        transitions.Should().Contain((CircuitState.Closed, CircuitState.Open));
    }

    #endregion

    #region TimeoutMiddleware

    [Fact]
    public async Task Timeout_CompletesInTime_ReturnsResult()
    {
        var middleware = new TimeoutMiddleware(TimeSpan.FromSeconds(5));

        var result = await middleware.InvokeAsync(
            MakeAgent("a"),
            MakeMessages("input"),
            _ => Task.FromResult(MakeResponse("ok")));

        GetText(result).Should().Be("ok");
    }

    [Fact]
    public async Task Timeout_ExceedsTimeout_ThrowsTimeoutException()
    {
        var middleware = new TimeoutMiddleware(TimeSpan.FromMilliseconds(50));

        var act = () => middleware.InvokeAsync(
            MakeAgent("a"),
            MakeMessages("input"),
            async _ =>
            {
                await Task.Delay(5000);
                return MakeResponse("late");
            });

        await act.Should().ThrowAsync<TimeoutException>();
    }

    [Fact]
    public async Task Timeout_OnTimeoutCallback_Called()
    {
        var timedOutAgent = "";
        var middleware = new TimeoutMiddleware(new TimeoutMiddlewareOptions
        {
            Timeout = TimeSpan.FromMilliseconds(50),
            OnTimeout = (name, _) => timedOutAgent = name
        });

        try
        {
            await middleware.InvokeAsync(
                MakeAgent("slow-agent"),
                MakeMessages("input"),
                async _ =>
                {
                    await Task.Delay(5000);
                    return MakeResponse("late");
                });
        }
        catch (TimeoutException) { }

        timedOutAgent.Should().Be("slow-agent");
    }

    #endregion

    #region FallbackMiddleware

    [Fact]
    public async Task Fallback_PrimarySucceeds_ReturnsPrimaryResult()
    {
        var fallbackAgent = MakeAgent("fallback", "fallback-output");
        var middleware = new FallbackMiddleware(fallbackAgent);

        var result = await middleware.InvokeAsync(
            MakeAgent("primary"),
            MakeMessages("input"),
            _ => Task.FromResult(MakeResponse("primary-output")));

        GetText(result).Should().Be("primary-output");
    }

    [Fact]
    public async Task Fallback_PrimaryFails_ReturnsFallbackResult()
    {
        var fallbackAgent = MakeAgent("fallback", "fallback-output");
        var middleware = new FallbackMiddleware(fallbackAgent);

        var result = await middleware.InvokeAsync(
            MakeAgent("primary"),
            MakeMessages("input"),
            _ => throw new InvalidOperationException("primary failed"));

        GetText(result).Should().Be("fallback-output");
    }

    [Fact]
    public async Task Fallback_ResponseValidatorFails_UsesFallback()
    {
        var fallbackAgent = MakeAgent("fallback", "fallback-output");
        var middleware = new FallbackMiddleware(new FallbackMiddlewareOptions
        {
            FallbackAgent = fallbackAgent,
            ResponseValidator = _ => false // always fail validation
        });

        var result = await middleware.InvokeAsync(
            MakeAgent("primary"),
            MakeMessages("input"),
            _ => Task.FromResult(MakeResponse("bad-response")));

        GetText(result).Should().Be("fallback-output");
    }

    [Fact]
    public async Task Fallback_ShouldFallbackFalse_ThrowsOriginal()
    {
        var fallbackAgent = MakeAgent("fallback", "fallback-output");
        var middleware = new FallbackMiddleware(new FallbackMiddlewareOptions
        {
            FallbackAgent = fallbackAgent,
            ShouldFallback = _ => false // don't fallback
        });

        var act = () => middleware.InvokeAsync(
            MakeAgent("primary"),
            MakeMessages("input"),
            _ => throw new InvalidOperationException("primary failed"));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Fallback_BothFail_ThrowsFallbackFailedException()
    {
        var mock = Substitute.For<IAgent>();
        mock.Name.Returns("fallback");
        mock.InvokeAsync(Arg.Any<IEnumerable<Message>>(), Arg.Any<CancellationToken>())
            .Returns<MessageResponse>(_ => throw new InvalidOperationException("fallback also failed"));

        var middleware = new FallbackMiddleware(mock);

        var act = () => middleware.InvokeAsync(
            MakeAgent("primary"),
            MakeMessages("input"),
            _ => throw new InvalidOperationException("primary failed"));

        await act.Should().ThrowAsync<FallbackFailedException>();
    }

    [Fact]
    public void Fallback_NoFallbackAgentOrFactory_ThrowsOnConstruction()
    {
        var act = () => new FallbackMiddleware(new FallbackMiddlewareOptions());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task Fallback_Factory_CreatesAgentDynamically()
    {
        var middleware = new FallbackMiddleware(new FallbackMiddlewareOptions
        {
            FallbackFactory = primary => MakeAgent($"fallback-for-{primary.Name}", "factory-output")
        });

        var result = await middleware.InvokeAsync(
            MakeAgent("primary"),
            MakeMessages("input"),
            _ => throw new InvalidOperationException("fail"));

        GetText(result).Should().Be("factory-output");
    }

    #endregion

    #region BulkheadMiddleware

    [Fact]
    public async Task Bulkhead_WithinLimit_Succeeds()
    {
        using var bulkhead = new BulkheadMiddleware(2);

        var result = await bulkhead.InvokeAsync(
            MakeAgent("a"),
            MakeMessages("input"),
            _ => Task.FromResult(MakeResponse("ok")));

        GetText(result).Should().Be("ok");
    }

    [Fact]
    public async Task Bulkhead_QueueFull_Rejects()
    {
        var task1Entered = new TaskCompletionSource();
        var queuedCount = 0;
        var task2Queued = new TaskCompletionSource();
        using var bulkhead = new BulkheadMiddleware(new BulkheadMiddlewareOptions
        {
            MaxConcurrency = 1,
            MaxQueueSize = 1,
            OnQueued = (_, _, _) =>
            {
                if (Interlocked.Increment(ref queuedCount) == 2)
                    task2Queued.TrySetResult();
            }
        });

        var blocker = new TaskCompletionSource<MessageResponse>();

        // Request 1: takes the execution slot
        var task1 = Task.Run(() => bulkhead.InvokeAsync(
            MakeAgent("a"), MakeMessages("1"),
            _ => { task1Entered.SetResult(); return blocker.Task; }));
        await task1Entered.Task;

        // Request 2: enters the queue
        var task2 = Task.Run(() => bulkhead.InvokeAsync(
            MakeAgent("a"), MakeMessages("2"),
            _ => Task.FromResult(MakeResponse("ok"))));
        await task2Queued.Task;

        // Request 3: queue full → rejected
        var act = () => bulkhead.InvokeAsync(
            MakeAgent("a"), MakeMessages("3"),
            _ => Task.FromResult(MakeResponse("ok")));

        await act.Should().ThrowAsync<BulkheadRejectedException>();

        blocker.SetResult(MakeResponse("ok"));
        await Task.WhenAll(task1, task2);
    }

    [Fact]
    public async Task Bulkhead_QueueTimeout_Rejects()
    {
        var task1Entered = new TaskCompletionSource();
        using var bulkhead = new BulkheadMiddleware(new BulkheadMiddlewareOptions
        {
            MaxConcurrency = 1,
            QueueTimeout = TimeSpan.FromMilliseconds(100)
        });

        var blocker = new TaskCompletionSource<MessageResponse>();

        // Take the execution slot
        var task1 = Task.Run(() => bulkhead.InvokeAsync(
            MakeAgent("a"), MakeMessages("1"),
            _ => { task1Entered.SetResult(); return blocker.Task; }));
        await task1Entered.Task;

        // Next request times out waiting for slot
        var act = () => bulkhead.InvokeAsync(
            MakeAgent("a"), MakeMessages("2"),
            _ => Task.FromResult(MakeResponse("ok")));

        await act.Should().ThrowAsync<BulkheadRejectedException>();

        blocker.SetResult(MakeResponse("ok"));
        await task1;
    }

    [Fact]
    public async Task Bulkhead_OnRejectedCallback_Called()
    {
        var task1Entered = new TaskCompletionSource();
        var rejectedAgent = "";
        using var bulkhead = new BulkheadMiddleware(new BulkheadMiddlewareOptions
        {
            MaxConcurrency = 1,
            QueueTimeout = TimeSpan.FromMilliseconds(100),
            OnRejected = (name, _, _) => rejectedAgent = name
        });

        var blocker = new TaskCompletionSource<MessageResponse>();
        var task1 = Task.Run(() => bulkhead.InvokeAsync(
            MakeAgent("a"), MakeMessages("1"),
            _ => { task1Entered.SetResult(); return blocker.Task; }));
        await task1Entered.Task;

        try
        {
            await bulkhead.InvokeAsync(
                MakeAgent("test-agent"), MakeMessages("2"),
                _ => Task.FromResult(MakeResponse("ok")));
        }
        catch (BulkheadRejectedException) { }

        rejectedAgent.Should().Be("test-agent");

        blocker.SetResult(MakeResponse("ok"));
        await task1;
    }

    #endregion

    #region RateLimitMiddleware

    [Fact]
    public async Task RateLimit_WithinLimit_Succeeds()
    {
        using var rateLimit = new RateLimitMiddleware(5, TimeSpan.FromSeconds(10));

        var result = await rateLimit.InvokeAsync(
            MakeAgent("a"),
            MakeMessages("input"),
            _ => Task.FromResult(MakeResponse("ok")));

        GetText(result).Should().Be("ok");
    }

    [Fact]
    public async Task RateLimit_ExceedsLimit_Blocks()
    {
        using var rateLimit = new RateLimitMiddleware(2, TimeSpan.FromSeconds(10));

        // Fill the window
        for (int i = 0; i < 2; i++)
        {
            await rateLimit.InvokeAsync(
                MakeAgent("a"), MakeMessages("input"),
                _ => Task.FromResult(MakeResponse("ok")));
        }

        // 3rd request should be rate limited and block until cancelled
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        var act = () => rateLimit.InvokeAsync(
            MakeAgent("a"), MakeMessages("input"),
            _ => Task.FromResult(MakeResponse("ok")),
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task RateLimit_OnRateLimitedCallback_Called()
    {
        var rateLimited = false;
        using var rateLimit = new RateLimitMiddleware(new RateLimitMiddlewareOptions
        {
            MaxRequests = 1,
            Window = TimeSpan.FromSeconds(10),
            OnRateLimited = (_, _) => rateLimited = true
        });

        // Fill the window
        await rateLimit.InvokeAsync(
            MakeAgent("a"), MakeMessages("input"),
            _ => Task.FromResult(MakeResponse("ok")));

        // 2nd request triggers rate limiting
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        try
        {
            await rateLimit.InvokeAsync(
                MakeAgent("a"), MakeMessages("input"),
                _ => Task.FromResult(MakeResponse("ok")),
                cts.Token);
        }
        catch (OperationCanceledException) { }

        rateLimited.Should().BeTrue();
    }

    #endregion

    #region CompositeMiddleware

    [Fact]
    public void Composite_NoMiddlewares_Throws()
    {
        var act = () => new CompositeMiddleware("test");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task Composite_SingleMiddleware_Invoked()
    {
        var invoked = false;
        var middleware = new TrackingMiddleware(() => invoked = true);
        var composite = new CompositeMiddleware("test", middleware);

        await composite.InvokeAsync(
            MakeAgent("a"), MakeMessages("input"),
            _ => Task.FromResult(MakeResponse("ok")));

        invoked.Should().BeTrue();
        composite.Name.Should().Be("test");
        composite.Count.Should().Be(1);
    }

    [Fact]
    public async Task Composite_ChainOrder_FirstToLast()
    {
        var order = new List<string>();
        var m1 = new TrackingMiddleware(() => order.Add("first"));
        var m2 = new TrackingMiddleware(() => order.Add("second"));
        var composite = new CompositeMiddleware("test", m1, m2);

        await composite.InvokeAsync(
            MakeAgent("a"), MakeMessages("input"),
            _ => Task.FromResult(MakeResponse("ok")));

        order.Should().Equal("first", "second");
    }

    [Fact]
    public async Task Composite_With_AddsMiddleware()
    {
        var order = new List<string>();
        var m1 = new TrackingMiddleware(() => order.Add("first"));
        var m2 = new TrackingMiddleware(() => order.Add("added"));
        var composite = new CompositeMiddleware("test", m1).With(m2);

        composite.Count.Should().Be(2);
        await composite.InvokeAsync(
            MakeAgent("a"), MakeMessages("input"),
            _ => Task.FromResult(MakeResponse("ok")));

        order.Should().Equal("first", "added");
    }

    [Fact]
    public async Task Composite_Prepend_AddsAtFront()
    {
        var order = new List<string>();
        var m1 = new TrackingMiddleware(() => order.Add("original"));
        var m2 = new TrackingMiddleware(() => order.Add("prepended"));
        var composite = new CompositeMiddleware("test", m1).Prepend(m2);

        composite.Count.Should().Be(2);
        await composite.InvokeAsync(
            MakeAgent("a"), MakeMessages("input"),
            _ => Task.FromResult(MakeResponse("ok")));

        order.Should().Equal("prepended", "original");
    }

    [Fact]
    public void Composite_MiddlewarePacks_Creates()
    {
        var resilience = MiddlewarePacks.Resilience();
        resilience.Name.Should().Be("resilience");
        resilience.Count.Should().Be(2);

        var advanced = MiddlewarePacks.AdvancedResilience();
        advanced.Name.Should().Be("advanced-resilience");
        advanced.Count.Should().Be(3);

        var production = MiddlewarePacks.Production();
        production.Name.Should().Be("production");
        production.Count.Should().Be(5);
    }

    #endregion

    #region CachingMiddleware

    [Fact]
    public async Task Caching_FirstMiss_SecondHit()
    {
        var callCount = 0;
        var hits = new List<string>();
        var misses = new List<string>();
        var middleware = new CachingMiddleware(new CachingMiddlewareOptions
        {
            Expiration = TimeSpan.FromMinutes(5),
            OnCacheHit = (name, _) => hits.Add(name),
            OnCacheMiss = (name, _) => misses.Add(name)
        });

        // First call: cache miss
        await middleware.InvokeAsync(
            MakeAgent("a"), MakeMessages("hello"),
            _ => { callCount++; return Task.FromResult(MakeResponse("response")); });

        // Second call with same input: cache hit
        await middleware.InvokeAsync(
            MakeAgent("a"), MakeMessages("hello"),
            _ => { callCount++; return Task.FromResult(MakeResponse("response")); });

        callCount.Should().Be(1); // next only called once
        misses.Should().HaveCount(1);
        hits.Should().HaveCount(1);
        middleware.CacheCount.Should().Be(1);
    }

    [Fact]
    public async Task Caching_DifferentInput_CacheMiss()
    {
        var callCount = 0;
        var middleware = new CachingMiddleware(TimeSpan.FromMinutes(5));

        await middleware.InvokeAsync(
            MakeAgent("a"), MakeMessages("hello"),
            _ => { callCount++; return Task.FromResult(MakeResponse("r1")); });

        await middleware.InvokeAsync(
            MakeAgent("a"), MakeMessages("world"),
            _ => { callCount++; return Task.FromResult(MakeResponse("r2")); });

        callCount.Should().Be(2);
        middleware.CacheCount.Should().Be(2);
    }

    [Fact]
    public async Task Caching_ClearCache_RemovesAll()
    {
        var middleware = new CachingMiddleware(TimeSpan.FromMinutes(5));

        await middleware.InvokeAsync(
            MakeAgent("a"), MakeMessages("hello"),
            _ => Task.FromResult(MakeResponse("ok")));

        middleware.CacheCount.Should().Be(1);
        middleware.ClearCache();
        middleware.CacheCount.Should().Be(0);
    }

    [Fact]
    public async Task Caching_MaxCacheSize_Limits()
    {
        var middleware = new CachingMiddleware(new CachingMiddlewareOptions
        {
            MaxCacheSize = 2,
            Expiration = TimeSpan.Zero
        });

        for (int i = 0; i < 5; i++)
        {
            await middleware.InvokeAsync(
                MakeAgent("a"), MakeMessages($"msg-{i}"),
                _ => Task.FromResult(MakeResponse($"response-{i}")));
        }

        middleware.CacheCount.Should().BeLessThanOrEqualTo(2);
    }

    #endregion

    #region LoggingMiddleware

    [Fact]
    public async Task Logging_Success_LogsStartAndComplete()
    {
        var logs = new List<string>();
        var middleware = new LoggingMiddleware(msg => logs.Add(msg));

        await middleware.InvokeAsync(
            MakeAgent("test-agent"), MakeMessages("hello"),
            _ => Task.FromResult(MakeResponse("ok")));

        logs.Should().HaveCount(2);
        logs[0].Should().Contain("test-agent").And.Contain("starting");
        logs[1].Should().Contain("test-agent").And.Contain("completed");
    }

    [Fact]
    public async Task Logging_Failure_LogsStartAndError()
    {
        var logs = new List<string>();
        var middleware = new LoggingMiddleware(msg => logs.Add(msg));

        try
        {
            await middleware.InvokeAsync(
                MakeAgent("test-agent"), MakeMessages("hello"),
                _ => throw new InvalidOperationException("boom"));
        }
        catch (InvalidOperationException) { }

        logs.Should().HaveCount(2);
        logs[0].Should().Contain("starting");
        logs[1].Should().Contain("failed").And.Contain("boom");
    }

    [Fact]
    public async Task Logging_IncludesTokenInfo()
    {
        var logs = new List<string>();
        var middleware = new LoggingMiddleware(msg => logs.Add(msg));

        await middleware.InvokeAsync(
            MakeAgent("a"), MakeMessages("hello"),
            _ => Task.FromResult(MakeResponse("ok")));

        logs[1].Should().Contain("tokens:");
    }

    #endregion

    #region MiddlewareAgent

    [Fact]
    public async Task MiddlewareAgent_NoMiddlewares_DelegatesToInner()
    {
        var inner = MakeAgent("inner", "inner-output");
        var agent = new MiddlewareAgent(inner, Array.Empty<IAgentMiddleware>());

        var result = await agent.InvokeAsync(MakeMessages("hello"));
        GetText(result).Should().Be("inner-output");
    }

    [Fact]
    public async Task MiddlewareAgent_WithMiddleware_InvokesChain()
    {
        var middlewareInvoked = false;
        var inner = MakeAgent("inner", "inner-output");
        var middleware = new TrackingMiddleware(() => middlewareInvoked = true);
        var agent = new MiddlewareAgent(inner, new[] { middleware });

        var result = await agent.InvokeAsync(MakeMessages("hello"));

        middlewareInvoked.Should().BeTrue();
        GetText(result).Should().Be("inner-output");
    }

    [Fact]
    public void MiddlewareAgent_Properties_DelegateToInner()
    {
        var inner = MakeAgent("inner");
        inner.Description = "Test Description";
        inner.Instructions = "Test Instructions";
        var agent = new MiddlewareAgent(inner, Array.Empty<IAgentMiddleware>());

        agent.Name.Should().Be("inner");
        agent.Description.Should().Be("Test Description");
        agent.Instructions.Should().Be("Test Instructions");
        agent.Provider.Should().Be("mock");
        agent.Model.Should().Be("mock-model");
    }

    #endregion

    #region Helpers

    private static IEnumerable<Message> MakeMessages(string text)
    {
        return [new UserMessage { Content = [new TextMessageContent { Value = text }] }];
    }

    private static MockAgent MakeAgent(string name, string? defaultResponse = null)
    {
        return new MockAgent(name)
        {
            ResponseFunc = defaultResponse != null
                ? _ => defaultResponse
                : null
        };
    }

    private static MessageResponse MakeResponse(string text)
    {
        return new MessageResponse
        {
            Id = Guid.NewGuid().ToString("N"),
            DoneReason = MessageDoneReason.EndTurn,
            Message = new AssistantMessage
            {
                Content = [new TextMessageContent { Value = text }]
            },
            TokenUsage = new MessageTokenUsage { InputTokens = 10, OutputTokens = text.Length }
        };
    }

    private static string GetText(MessageResponse response)
    {
        return response.Message is AssistantMessage a
            ? a.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? ""
            : "";
    }

    private sealed class MockAgent : IAgent
    {
        public string Provider { get; set; } = "mock";
        public string Model { get; set; } = "mock-model";
        public string Name { get; set; }
        public string Description { get; set; } = "Mock";
        public string? Instructions { get; set; }
        public IEnumerable<ToolItem>? Tools { get; set; }
        public MessageGenerationParameters? Parameters { get; set; }
        public Func<IEnumerable<Message>, string>? ResponseFunc { get; set; }

        public MockAgent(string name) { Name = name; }

        public Task<MessageResponse> InvokeAsync(IEnumerable<Message> messages, CancellationToken ct = default)
        {
            var text = ResponseFunc != null ? ResponseFunc(messages) : $"MockAgent '{Name}'";
            return Task.FromResult(MakeResponse(text));
        }

        public async IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
            IEnumerable<Message> messages,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var text = ResponseFunc != null ? ResponseFunc(messages) : $"MockAgent '{Name}'";
            yield return new StreamingContentDeltaResponse
            {
                Index = 0,
                Delta = new TextDeltaContent { Value = text }
            };
            await Task.Yield();
            yield return new StreamingMessageDoneResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                DoneReason = MessageDoneReason.EndTurn,
                TokenUsage = new MessageTokenUsage { InputTokens = 10, OutputTokens = text.Length },
                Model = "mock-model",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    private sealed class TrackingMiddleware : IAgentMiddleware
    {
        private readonly Action _onInvoke;

        public TrackingMiddleware(Action onInvoke) { _onInvoke = onInvoke; }

        public async Task<MessageResponse> InvokeAsync(
            IAgent agent,
            IEnumerable<Message> messages,
            Func<IEnumerable<Message>, Task<MessageResponse>> next,
            CancellationToken cancellationToken = default)
        {
            _onInvoke();
            return await next(messages);
        }
    }

    #endregion
}
