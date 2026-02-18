using System.Globalization;
using System.Runtime.CompilerServices;
using FluentAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Tools;
using IronHive.Core.Agent;
using IronHive.Core.Agent.Orchestration;

namespace IronHive.Tests.Agent.Orchestration;

public class MiddlewareTests
{
    [Fact]
    public async Task Middleware_ShouldExecuteInOrder()
    {
        var order = new List<string>();

        var middleware1 = new TestMiddleware("m1", order);
        var middleware2 = new TestMiddleware("m2", order);

        var agent = new MockAgent("a") { ResponseFunc = _ => { order.Add("agent"); return "result"; } };
        var wrapped = agent.WithMiddleware(middleware1, middleware2);

        await wrapped.InvokeAsync(MakeUserMessages("go"));

        order.Should().Equal("m1-before", "m2-before", "agent", "m2-after", "m1-after");
    }

    [Fact]
    public async Task Middleware_ShouldModifyInput()
    {
        var middleware = new InputModifyingMiddleware("MODIFIED: ");
        var receivedText = "";

        var agent = new MockAgent("a")
        {
            ResponseFunc = msgs =>
            {
                receivedText = msgs.OfType<UserMessage>().Last()
                    .Content.OfType<TextMessageContent>().First().Value;
                return "ok";
            }
        };

        var wrapped = agent.WithMiddleware(middleware);
        await wrapped.InvokeAsync(MakeUserMessages("hello"));

        receivedText.Should().Be("MODIFIED: hello");
    }

    [Fact]
    public async Task Middleware_ShortCircuit_ShouldNotCallNext()
    {
        var agentCalled = false;
        var middleware = new ShortCircuitMiddleware("short-circuited");
        var agent = new MockAgent("a")
        {
            ResponseFunc = _ => { agentCalled = true; return "should not reach"; }
        };

        var wrapped = agent.WithMiddleware(middleware);
        var response = await wrapped.InvokeAsync(MakeUserMessages("go"));

        agentCalled.Should().BeFalse();
        GetText(response.Message).Should().Be("short-circuited");
    }

    [Fact]
    public async Task Middleware_InOrchestrator_ShouldApplyToAllAgents()
    {
        var order = new List<string>();
        var middleware = new TestMiddleware("m1", order);

        var agent1 = new MockAgent("a1") { ResponseFunc = _ => { order.Add("a1"); return "a1-out"; } };
        var agent2 = new MockAgent("a2") { ResponseFunc = _ => { order.Add("a2"); return "a2-out"; } };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            PassOutputAsInput = true,
            AgentMiddlewares = [middleware]
        });
        orch.AddAgents([agent1, agent2]);

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeTrue();
        // middleware wraps each agent call
        order.Should().Contain("m1-before");
        order.Should().Contain("a1");
        order.Should().Contain("a2");
        order.Count(s => s == "m1-before").Should().Be(2); // called for each agent
    }

    [Fact]
    public async Task MiddlewareAgent_Properties_ShouldDelegateToInner()
    {
        var inner = new MockAgent("test")
        {
            Provider = "openai",
            Model = "gpt-4",
            Description = "Test agent",
            Instructions = "Be helpful"
        };

        var wrapped = inner.WithMiddleware(new TestMiddleware("m", new List<string>()));

        wrapped.Name.Should().Be("test");
        wrapped.Provider.Should().Be("openai");
        wrapped.Model.Should().Be("gpt-4");
        wrapped.Description.Should().Be("Test agent");
        wrapped.Instructions.Should().Be("Be helpful");
    }

    #region RetryMiddleware Tests

    [Fact]
    public async Task RetryMiddleware_ShouldSucceedOnFirstAttempt()
    {
        var callCount = 0;
        var agent = new MockAgent("test") { ResponseFunc = _ => { callCount++; return "success"; } };
        var middleware = new RetryMiddleware(3);
        var wrapped = agent.WithMiddleware(middleware);

        var response = await wrapped.InvokeAsync(MakeUserMessages("go"));

        callCount.Should().Be(1);
        GetText(response.Message).Should().Be("success");
    }

    [Fact]
    public async Task RetryMiddleware_ShouldRetryOnFailure()
    {
        var callCount = 0;
        var retryAttempts = new List<int>();

        var middleware = new RetryMiddleware(new RetryMiddlewareOptions
        {
            MaxRetries = 3,
            InitialDelay = TimeSpan.FromMilliseconds(10),
            OnRetry = (name, attempt, ex, delay) => retryAttempts.Add(attempt)
        });

        // failUntilAttempt: 3 means fail on call 1 and 2, succeed on call 3
        var failingMiddleware = new FailingMiddleware(failUntilAttempt: 3);
        var agent = new MockAgent("test") { ResponseFunc = _ => { callCount++; return "success"; } };
        var wrapped = agent.WithMiddleware(middleware, failingMiddleware);

        var response = await wrapped.InvokeAsync(MakeUserMessages("go"));

        callCount.Should().Be(1); // Only 1 successful call to agent
        failingMiddleware.CallCount.Should().Be(3); // Failed 2 times + 1 success
        retryAttempts.Should().Equal(1, 2);
        GetText(response.Message).Should().Be("success");
    }

    [Fact]
    public async Task RetryMiddleware_ShouldThrowAfterMaxRetries()
    {
        var middleware = new RetryMiddleware(new RetryMiddlewareOptions
        {
            MaxRetries = 2,
            InitialDelay = TimeSpan.FromMilliseconds(10)
        });

        var failingMiddleware = new FailingMiddleware(failUntilAttempt: 10); // Always fail
        var agent = new MockAgent("test");
        var wrapped = agent.WithMiddleware(middleware, failingMiddleware);

        var act = () => wrapped.InvokeAsync(MakeUserMessages("go"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Simulated failure*");
    }

    [Fact]
    public async Task RetryMiddleware_ShouldRespectShouldRetryPredicate()
    {
        var callCount = 0;
        var middleware = new RetryMiddleware(new RetryMiddlewareOptions
        {
            MaxRetries = 3,
            InitialDelay = TimeSpan.FromMilliseconds(10),
            ShouldRetry = ex => ex is not ArgumentException
        });

        var failingMiddleware = new FailingMiddleware(failUntilAttempt: 10,
            exceptionFactory: () => new ArgumentException("No retry"));
        var agent = new MockAgent("test") { ResponseFunc = _ => { callCount++; return "ok"; } };
        var wrapped = agent.WithMiddleware(middleware, failingMiddleware);

        var act = () => wrapped.InvokeAsync(MakeUserMessages("go"));

        await act.Should().ThrowAsync<ArgumentException>();
        failingMiddleware.CallCount.Should().Be(1); // No retry for ArgumentException
    }

    [Fact]
    public async Task RetryMiddleware_ShouldNotRetryOnCancellation()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var middleware = new RetryMiddleware(3);
        var agent = new MockAgent("test");
        var wrapped = agent.WithMiddleware(middleware);

        var act = () => wrapped.InvokeAsync(MakeUserMessages("go"), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region LoggingMiddleware Tests

    [Fact]
    public async Task LoggingMiddleware_ShouldLogStartAndComplete()
    {
        var logs = new List<string>();
        var middleware = new LoggingMiddleware(msg => logs.Add(msg));
        var agent = new MockAgent("TestAgent");
        var wrapped = agent.WithMiddleware(middleware);

        await wrapped.InvokeAsync(MakeUserMessages("hello"));

        logs.Should().HaveCount(2);
        logs[0].Should().Contain("TestAgent").And.Contain("starting");
        logs[1].Should().Contain("TestAgent").And.Contain("completed");
    }

    [Fact]
    public async Task LoggingMiddleware_ShouldLogError()
    {
        var logs = new List<string>();
        var middleware = new LoggingMiddleware(msg => logs.Add(msg));
        var failingMiddleware = new FailingMiddleware(failUntilAttempt: 10);
        var agent = new MockAgent("TestAgent");
        var wrapped = agent.WithMiddleware(middleware, failingMiddleware);

        var act = () => wrapped.InvokeAsync(MakeUserMessages("go"));

        await act.Should().ThrowAsync<InvalidOperationException>();
        logs.Should().HaveCount(2);
        logs[0].Should().Contain("starting");
        logs[1].Should().Contain("failed").And.Contain("InvalidOperationException");
    }

    [Fact]
    public async Task LoggingMiddleware_ShouldIncludeTokenUsage()
    {
        var logs = new List<string>();
        var middleware = new LoggingMiddleware(msg => logs.Add(msg));
        var agent = new MockAgent("TestAgent") { ResponseFunc = _ => "short response" };
        var wrapped = agent.WithMiddleware(middleware);

        await wrapped.InvokeAsync(MakeUserMessages("hi"));

        logs[1].Should().Contain("tokens:");
    }

    [Fact]
    public async Task LoggingMiddleware_ShouldRespectOptions()
    {
        var logs = new List<string>();
        var middleware = new LoggingMiddleware(new LoggingMiddlewareOptions
        {
            LogAction = msg => logs.Add(msg),
            LogPrefix = "MyApp",
            IncludeMessagePreview = false,
            IncludeResponsePreview = false
        });

        var agent = new MockAgent("TestAgent");
        var wrapped = agent.WithMiddleware(middleware);

        await wrapped.InvokeAsync(MakeUserMessages("hello world"));

        logs[0].Should().Contain("[MyApp]");
        logs[0].Should().NotContain("hello world");
    }

    [Fact]
    public async Task LoggingMiddleware_WithNullLogAction_ShouldNotThrow()
    {
        var middleware = new LoggingMiddleware(new LoggingMiddlewareOptions { LogAction = null });
        var agent = new MockAgent("TestAgent");
        var wrapped = agent.WithMiddleware(middleware);

        var response = await wrapped.InvokeAsync(MakeUserMessages("go"));

        GetText(response.Message).Should().NotBeEmpty();
    }

    #endregion

    #region StreamingMiddleware Tests

    [Fact]
    public async Task StreamingMiddleware_LoggingMiddleware_ShouldLogStreamingCalls()
    {
        var logs = new List<string>();
        var middleware = new LoggingMiddleware(msg => logs.Add(msg));
        var agent = new MockAgent("TestAgent");
        var wrapped = agent.WithMiddleware(middleware);

        var events = new List<StreamingMessageResponse>();
        await foreach (var evt in wrapped.InvokeStreamingAsync(MakeUserMessages("hello")))
        {
            events.Add(evt);
        }

        logs.Should().HaveCount(2);
        logs[0].Should().Contain("streaming").And.Contain("starting");
        logs[1].Should().Contain("streaming completed");
        events.Should().NotBeEmpty();
    }

    [Fact]
    public async Task StreamingMiddleware_NonStreamingMiddleware_ShouldBypass()
    {
        var callCount = 0;
        // TestMiddleware는 IStreamingAgentMiddleware를 구현하지 않음
        var middleware = new TestMiddleware("m", new List<string>());
        var agent = new MockAgent("TestAgent")
        {
            ResponseFunc = _ => { callCount++; return "ok"; }
        };
        var wrapped = agent.WithMiddleware(middleware);

        // 스트리밍 호출 - 미들웨어 바이패스되어야 함
        var events = new List<StreamingMessageResponse>();
        await foreach (var evt in wrapped.InvokeStreamingAsync(MakeUserMessages("hello")))
        {
            events.Add(evt);
        }

        // 에이전트가 호출되지 않은 이유: InvokeStreamingAsync는 InvokeAsync와 별개
        events.Should().NotBeEmpty();
    }

    #endregion

    #region CachingMiddleware Tests

    [Fact]
    public async Task CachingMiddleware_ShouldCacheResponse()
    {
        var callCount = 0;
        var agent = new MockAgent("test") { ResponseFunc = _ => { callCount++; return "cached"; } };
        var middleware = new CachingMiddleware(TimeSpan.FromMinutes(5));
        var wrapped = agent.WithMiddleware(middleware);

        // 첫 번째 호출
        var response1 = await wrapped.InvokeAsync(MakeUserMessages("hello"));
        // 두 번째 호출 - 캐시 히트
        var response2 = await wrapped.InvokeAsync(MakeUserMessages("hello"));

        callCount.Should().Be(1); // 에이전트는 1번만 호출
        GetText(response1.Message).Should().Be("cached");
        GetText(response2.Message).Should().Be("cached");
    }

    [Fact]
    public async Task CachingMiddleware_DifferentInput_ShouldNotUseCache()
    {
        var callCount = 0;
        var agent = new MockAgent("test") { ResponseFunc = _ => { callCount++; return $"response-{callCount}"; } };
        var middleware = new CachingMiddleware(TimeSpan.FromMinutes(5));
        var wrapped = agent.WithMiddleware(middleware);

        var response1 = await wrapped.InvokeAsync(MakeUserMessages("hello"));
        var response2 = await wrapped.InvokeAsync(MakeUserMessages("world")); // 다른 입력

        callCount.Should().Be(2);
        GetText(response1.Message).Should().Be("response-1");
        GetText(response2.Message).Should().Be("response-2");
    }

    [Fact]
    public async Task CachingMiddleware_ShouldReportHitAndMiss()
    {
        var hits = 0;
        var misses = 0;

        var middleware = new CachingMiddleware(new CachingMiddlewareOptions
        {
            Expiration = TimeSpan.FromMinutes(5),
            OnCacheHit = (_, _) => hits++,
            OnCacheMiss = (_, _) => misses++
        });

        var agent = new MockAgent("test");
        var wrapped = agent.WithMiddleware(middleware);

        await wrapped.InvokeAsync(MakeUserMessages("hello")); // miss
        await wrapped.InvokeAsync(MakeUserMessages("hello")); // hit
        await wrapped.InvokeAsync(MakeUserMessages("world")); // miss

        misses.Should().Be(2);
        hits.Should().Be(1);
    }

    [Fact]
    public void CachingMiddleware_ClearCache_ShouldWork()
    {
        var middleware = new CachingMiddleware(TimeSpan.FromMinutes(5));

        // 캐시에 항목 추가 (직접 테스트는 불가하므로 public 메서드로 확인)
        middleware.CacheCount.Should().Be(0);
        middleware.ClearCache(); // 예외 없이 실행되어야 함
    }

    #endregion

    #region TimeoutMiddleware Tests

    [Fact]
    public async Task TimeoutMiddleware_ShouldSucceedBeforeTimeout()
    {
        var middleware = new TimeoutMiddleware(TimeSpan.FromSeconds(5));
        var agent = new MockAgent("test") { ResponseFunc = _ => "fast response" };
        var wrapped = agent.WithMiddleware(middleware);

        var response = await wrapped.InvokeAsync(MakeUserMessages("go"));

        GetText(response.Message).Should().Be("fast response");
    }

    [Fact]
    public async Task TimeoutMiddleware_ShouldThrowOnTimeout()
    {
        var timeoutCalled = false;
        var middleware = new TimeoutMiddleware(new TimeoutMiddlewareOptions
        {
            Timeout = TimeSpan.FromMilliseconds(50),
            OnTimeout = (_, _) => timeoutCalled = true
        });

        var slowMiddleware = new SlowMiddleware(TimeSpan.FromSeconds(5));
        var agent = new MockAgent("test");
        var wrapped = agent.WithMiddleware(middleware, slowMiddleware);

        var act = () => wrapped.InvokeAsync(MakeUserMessages("go"));

        await act.Should().ThrowAsync<TimeoutException>()
            .WithMessage("*timed out*");
        timeoutCalled.Should().BeTrue();
    }

    #endregion

    #region RateLimitMiddleware Tests

    [Fact]
    public async Task RateLimitMiddleware_ShouldAllowRequestsWithinLimit()
    {
        var middleware = new RateLimitMiddleware(5, TimeSpan.FromSeconds(10));
        var agent = new MockAgent("test");
        var wrapped = agent.WithMiddleware(middleware);

        // 5 requests should all succeed immediately
        for (var i = 0; i < 5; i++)
        {
            await wrapped.InvokeAsync(MakeUserMessages($"request-{i}"));
        }

        middleware.CurrentRequestCount.Should().Be(5);
    }

    [Fact]
    public async Task RateLimitMiddleware_ShouldReportRateLimited()
    {
        var rateLimitedCount = 0;
        var middleware = new RateLimitMiddleware(new RateLimitMiddlewareOptions
        {
            MaxRequests = 2,
            Window = TimeSpan.FromMilliseconds(500),
            OnRateLimited = (_, _) => rateLimitedCount++
        });

        var agent = new MockAgent("test");
        var wrapped = agent.WithMiddleware(middleware);

        // First 2 requests - immediate
        await wrapped.InvokeAsync(MakeUserMessages("req-1"));
        await wrapped.InvokeAsync(MakeUserMessages("req-2"));

        // 3rd request should trigger rate limit (but eventually succeed)
        var task = wrapped.InvokeAsync(MakeUserMessages("req-3"));

        // Wait briefly then check callback was triggered
        await Task.Delay(100);
        rateLimitedCount.Should().BeGreaterThan(0);
    }

    #endregion

    #region Test Middlewares

    private sealed class TestMiddleware : IAgentMiddleware
    {
        private readonly string _name;
        private readonly List<string> _order;

        public TestMiddleware(string name, List<string> order)
        {
            _name = name;
            _order = order;
        }

        public async Task<MessageResponse> InvokeAsync(
            IAgent agent, IEnumerable<Message> messages,
            Func<IEnumerable<Message>, Task<MessageResponse>> next,
            CancellationToken ct = default)
        {
            _order.Add($"{_name}-before");
            var result = await next(messages);
            _order.Add($"{_name}-after");
            return result;
        }
    }

    private sealed class InputModifyingMiddleware : IAgentMiddleware
    {
        private readonly string _prefix;

        public InputModifyingMiddleware(string prefix) { _prefix = prefix; }

        public Task<MessageResponse> InvokeAsync(
            IAgent agent, IEnumerable<Message> messages,
            Func<IEnumerable<Message>, Task<MessageResponse>> next,
            CancellationToken ct = default)
        {
            var modified = messages.Select(m =>
            {
                if (m is UserMessage um)
                {
                    return new UserMessage
                    {
                        Content = um.Content.Select<MessageContent, MessageContent>(c =>
                            c is TextMessageContent tc
                                ? new TextMessageContent { Value = _prefix + tc.Value }
                                : c).ToList()
                    };
                }
                return m;
            });

            return next(modified);
        }
    }

    private sealed class ShortCircuitMiddleware : IAgentMiddleware
    {
        private readonly string _response;

        public ShortCircuitMiddleware(string response) { _response = response; }

        public Task<MessageResponse> InvokeAsync(
            IAgent agent, IEnumerable<Message> messages,
            Func<IEnumerable<Message>, Task<MessageResponse>> next,
            CancellationToken ct = default)
        {
            return Task.FromResult(new MessageResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage
                {
                    Content = [new TextMessageContent { Value = _response }]
                }
            });
        }
    }

    private sealed class FailingMiddleware : IAgentMiddleware
    {
        private readonly int _failUntilAttempt;
        private readonly Func<Exception>? _exceptionFactory;
        public int CallCount { get; private set; }

        public FailingMiddleware(int failUntilAttempt, Func<Exception>? exceptionFactory = null)
        {
            _failUntilAttempt = failUntilAttempt;
            _exceptionFactory = exceptionFactory;
        }

        public Task<MessageResponse> InvokeAsync(
            IAgent agent, IEnumerable<Message> messages,
            Func<IEnumerable<Message>, Task<MessageResponse>> next,
            CancellationToken ct = default)
        {
            CallCount++;
            if (CallCount < _failUntilAttempt)
            {
                throw _exceptionFactory?.Invoke()
                    ?? new InvalidOperationException($"Simulated failure {CallCount}");
            }
            return next(messages);
        }
    }

    private sealed class SlowMiddleware : IAgentMiddleware
    {
        private readonly TimeSpan _delay;

        public SlowMiddleware(TimeSpan delay) { _delay = delay; }

        public async Task<MessageResponse> InvokeAsync(
            IAgent agent, IEnumerable<Message> messages,
            Func<IEnumerable<Message>, Task<MessageResponse>> next,
            CancellationToken ct = default)
        {
            await Task.Delay(_delay, ct);
            return await next(messages);
        }
    }

    #endregion

    #region Helpers

    private static IEnumerable<Message> MakeUserMessages(string text)
    {
        return [new UserMessage { Content = [new TextMessageContent { Value = text }] }];
    }

    private static string GetText(AssistantMessage msg)
    {
        return msg.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "";
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
            return Task.FromResult(new MessageResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage
                {
                    Name = Name,
                    Content = [new TextMessageContent { Value = text }]
                },
                TokenUsage = new MessageTokenUsage { InputTokens = 10, OutputTokens = text.Length }
            });
        }

        public async IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
            IEnumerable<Message> messages,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            await Task.Yield();
            yield return new StreamingMessageBeginResponse { Id = Guid.NewGuid().ToString("N") };
        }
    }

    #endregion

    #region CircuitBreakerMiddleware Tests

    [Fact]
    public async Task CircuitBreakerMiddleware_ShouldAllowRequestsWhenClosed()
    {
        // Arrange
        var middleware = new CircuitBreakerMiddleware(failureThreshold: 3, breakDuration: TimeSpan.FromSeconds(10));
        var agent = new MockAgent("test-agent");

        // Act & Assert - 초기 상태는 Closed
        Assert.Equal(CircuitState.Closed, middleware.State);

        var response = await middleware.InvokeAsync(
            agent,
            MakeUserMessages("test"),
            _ => Task.FromResult(new MessageResponse
            {
                Id = "1",
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage { Content = [new TextMessageContent { Value = "ok" }] }
            }));

        Assert.Equal("ok", GetText(response.Message!));
        Assert.Equal(CircuitState.Closed, middleware.State);
    }

    [Fact]
    public async Task CircuitBreakerMiddleware_ShouldOpenAfterFailureThreshold()
    {
        // Arrange
        var stateChanges = new List<(CircuitState from, CircuitState to)>();
        var middleware = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 2,
            BreakDuration = TimeSpan.FromMinutes(1),
            OnStateChanged = (from, to) => stateChanges.Add((from, to))
        });
        var agent = new MockAgent("test-agent");
        var failCount = 0;

        // Act - 2번 연속 실패
        for (int i = 0; i < 2; i++)
        {
            try
            {
                await middleware.InvokeAsync(
                    agent,
                    MakeUserMessages("test"),
                    _ => throw new InvalidOperationException($"fail-{++failCount}"));
            }
            catch (InvalidOperationException) { }
        }

        // Assert
        Assert.Equal(CircuitState.Open, middleware.State);
        Assert.Single(stateChanges);
        Assert.Equal((CircuitState.Closed, CircuitState.Open), stateChanges[0]);

        // Open 상태에서 요청 시 CircuitBreakerOpenException 발생
        await Assert.ThrowsAsync<CircuitBreakerOpenException>(async () =>
            await middleware.InvokeAsync(
                agent,
                MakeUserMessages("test"),
                _ => Task.FromResult<MessageResponse>(null!)));
    }

    [Fact]
    public async Task CircuitBreakerMiddleware_ShouldTransitionToHalfOpenAfterBreakDuration()
    {
        // Arrange
        var stateChanges = new List<(CircuitState from, CircuitState to)>();
        var middleware = new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = 1,
            BreakDuration = TimeSpan.FromMilliseconds(50),
            OnStateChanged = (from, to) => stateChanges.Add((from, to))
        });
        var agent = new MockAgent("test-agent");

        // Act - 1번 실패로 Open
        try
        {
            await middleware.InvokeAsync(
                agent,
                MakeUserMessages("test"),
                _ => throw new InvalidOperationException("fail"));
        }
        catch (InvalidOperationException) { }

        Assert.Equal(CircuitState.Open, middleware.State);

        // BreakDuration 대기 후 Half-Open
        await Task.Delay(100);
        Assert.Equal(CircuitState.HalfOpen, middleware.State);

        // Half-Open에서 성공하면 Closed
        await middleware.InvokeAsync(
            agent,
            MakeUserMessages("test"),
            _ => Task.FromResult(new MessageResponse
            {
                Id = "1",
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage { Content = [new TextMessageContent { Value = "ok" }] }
            }));

        Assert.Equal(CircuitState.Closed, middleware.State);
    }

    [Fact]
    public async Task CircuitBreakerMiddleware_ShouldResetManually()
    {
        // Arrange
        var middleware = new CircuitBreakerMiddleware(failureThreshold: 1, breakDuration: TimeSpan.FromMinutes(1));
        var agent = new MockAgent("test-agent");

        // Act - 실패로 Open
        try
        {
            await middleware.InvokeAsync(
                agent,
                MakeUserMessages("test"),
                _ => throw new InvalidOperationException("fail"));
        }
        catch { }

        Assert.Equal(CircuitState.Open, middleware.State);

        // 수동 리셋
        middleware.Reset();

        // Assert
        Assert.Equal(CircuitState.Closed, middleware.State);
    }

    #endregion

    #region BulkheadMiddleware Tests

    [Fact]
    public async Task BulkheadMiddleware_ShouldLimitConcurrency()
    {
        // Arrange
        var middleware = new BulkheadMiddleware(maxConcurrency: 2);
        var agent = new MockAgent("test-agent");
        var executing = 0;
        var maxExecuting = 0;

        // Act - 5개 동시 요청
        var tasks = Enumerable.Range(0, 5).Select(async i =>
        {
            await middleware.InvokeAsync(
                agent,
                MakeUserMessages($"test-{i}"),
                async _ =>
                {
                    var current = Interlocked.Increment(ref executing);
                    lock (middleware)
                    {
                        if (current > maxExecuting) maxExecuting = current;
                    }
                    await Task.Delay(50);
                    Interlocked.Decrement(ref executing);
                    return new MessageResponse
                    {
                        Id = i.ToString(CultureInfo.InvariantCulture),
                        DoneReason = MessageDoneReason.EndTurn,
                        Message = new AssistantMessage { Content = [new TextMessageContent { Value = "ok" }] }
                    };
                });
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert - 최대 동시 실행 수가 2를 초과하지 않아야 함
        Assert.True(maxExecuting <= 2, $"Max concurrent execution was {maxExecuting}, expected <= 2");
    }

    [Fact]
    public async Task BulkheadMiddleware_ShouldRejectWhenQueueFull()
    {
        // Arrange
        var rejected = false;
        var middleware = new BulkheadMiddleware(new BulkheadMiddlewareOptions
        {
            MaxConcurrency = 1,
            MaxQueueSize = 1,
            OnRejected = (_, _, _) => rejected = true
        });
        var agent = new MockAgent("test-agent");
        var gate = new TaskCompletionSource<bool>();

        // Act - 첫 번째 요청이 실행 중 (블로킹)
        var task1 = middleware.InvokeAsync(
            agent,
            MakeUserMessages("test-1"),
            async _ =>
            {
                await gate.Task;
                return new MessageResponse
                {
                    Id = "1",
                    DoneReason = MessageDoneReason.EndTurn,
                    Message = new AssistantMessage { Content = [new TextMessageContent { Value = "ok" }] }
                };
            });

        await Task.Delay(20); // task1이 실행 슬롯 점유

        // 두 번째 요청 (대기열로)
        var task2 = middleware.InvokeAsync(
            agent,
            MakeUserMessages("test-2"),
            async _ =>
            {
                await gate.Task;
                return new MessageResponse
                {
                    Id = "2",
                    DoneReason = MessageDoneReason.EndTurn,
                    Message = new AssistantMessage { Content = [new TextMessageContent { Value = "ok" }] }
                };
            });

        await Task.Delay(20); // task2가 대기열 점유

        // 세 번째 요청 - 거부되어야 함
        await Assert.ThrowsAsync<BulkheadRejectedException>(async () =>
            await middleware.InvokeAsync(
                agent,
                MakeUserMessages("test-3"),
                _ => Task.FromResult<MessageResponse>(null!)));

        Assert.True(rejected);

        // Cleanup
        gate.SetResult(true);
        await Task.WhenAll(task1, task2);
    }

    [Fact]
    public async Task BulkheadMiddleware_ShouldTrackStatistics()
    {
        // Arrange
        var middleware = new BulkheadMiddleware(maxConcurrency: 2);
        var agent = new MockAgent("test-agent");
        var gate = new TaskCompletionSource<bool>();

        // Act - 슬롯 점유
        var task1 = middleware.InvokeAsync(
            agent,
            MakeUserMessages("test-1"),
            async _ =>
            {
                await gate.Task;
                return new MessageResponse
                {
                    Id = "1",
                    DoneReason = MessageDoneReason.EndTurn,
                    Message = new AssistantMessage { Content = [new TextMessageContent { Value = "ok" }] }
                };
            });

        await Task.Delay(20);

        // Assert
        Assert.Equal(1, middleware.CurrentExecuting);
        Assert.Equal(1, middleware.AvailableSlots);

        // Cleanup
        gate.SetResult(true);
        await task1;

        Assert.Equal(0, middleware.CurrentExecuting);
        Assert.Equal(2, middleware.AvailableSlots);
    }

    #endregion

    #region FallbackMiddleware Tests

    [Fact]
    public async Task FallbackMiddleware_ShouldUseFallbackOnFailure()
    {
        // Arrange
        var fallbackCalled = false;
        var fallbackAgent = new MockAgent("fallback") { ResponseFunc = _ => { fallbackCalled = true; return "fallback-result"; } };
        var middleware = new FallbackMiddleware(fallbackAgent);
        var agent = new MockAgent("primary");

        // Act
        var response = await middleware.InvokeAsync(
            agent,
            MakeUserMessages("test"),
            _ => throw new InvalidOperationException("primary failed"));

        // Assert
        Assert.True(fallbackCalled);
        Assert.Equal("fallback-result", GetText(response.Message!));
    }

    [Fact]
    public async Task FallbackMiddleware_ShouldNotFallbackOnSuccess()
    {
        // Arrange
        var fallbackCalled = false;
        var fallbackAgent = new MockAgent("fallback") { ResponseFunc = _ => { fallbackCalled = true; return "fallback"; } };
        var middleware = new FallbackMiddleware(fallbackAgent);
        var agent = new MockAgent("primary") { ResponseFunc = _ => "primary-result" };

        // Act
        var response = await middleware.InvokeAsync(
            agent,
            MakeUserMessages("test"),
            _ => agent.InvokeAsync(MakeUserMessages("test")));

        // Assert
        Assert.False(fallbackCalled);
        Assert.Equal("primary-result", GetText(response.Message!));
    }

    [Fact]
    public async Task FallbackMiddleware_ShouldRespectShouldFallbackPredicate()
    {
        // Arrange
        var fallbackAgent = new MockAgent("fallback") { ResponseFunc = _ => "fallback" };
        var middleware = new FallbackMiddleware(new FallbackMiddlewareOptions
        {
            FallbackAgent = fallbackAgent,
            ShouldFallback = ex => ex is TimeoutException // Only fallback on timeout
        });
        var agent = new MockAgent("primary");

        // Act & Assert - InvalidOperationException should NOT trigger fallback
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await middleware.InvokeAsync(
                agent,
                MakeUserMessages("test"),
                _ => throw new InvalidOperationException("not a timeout")));

        // TimeoutException SHOULD trigger fallback
        var response = await middleware.InvokeAsync(
            agent,
            MakeUserMessages("test"),
            _ => throw new TimeoutException("timed out"));

        Assert.Equal("fallback", GetText(response.Message!));
    }

    [Fact]
    public async Task FallbackMiddleware_ShouldThrowFallbackFailedExceptionWhenBothFail()
    {
        // Arrange
        var fallbackAgent = new MockAgent("fallback")
        {
            ResponseFunc = _ => throw new InvalidOperationException("fallback also failed")
        };
        var middleware = new FallbackMiddleware(fallbackAgent);
        var agent = new MockAgent("primary");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<FallbackFailedException>(async () =>
            await middleware.InvokeAsync(
                agent,
                MakeUserMessages("test"),
                _ => throw new InvalidOperationException("primary failed")));

        Assert.Contains("primary", ex.Message);
        Assert.Contains("fallback", ex.Message);
    }

    [Fact]
    public async Task FallbackMiddleware_ShouldUseFallbackFactoryForDynamicFallback()
    {
        // Arrange
        var factoryCalled = false;
        var middleware = new FallbackMiddleware(new FallbackMiddlewareOptions
        {
            FallbackFactory = primary =>
            {
                factoryCalled = true;
                return new MockAgent($"fallback-for-{primary.Name}")
                {
                    ResponseFunc = _ => $"fallback-for-{primary.Name}"
                };
            }
        });
        var agent = new MockAgent("primary-agent");

        // Act
        var response = await middleware.InvokeAsync(
            agent,
            MakeUserMessages("test"),
            _ => throw new InvalidOperationException("fail"));

        // Assert
        Assert.True(factoryCalled);
        Assert.Equal("fallback-for-primary-agent", GetText(response.Message!));
    }

    #endregion

    #region CompositeMiddleware Tests

    [Fact]
    public async Task CompositeMiddleware_ShouldExecuteMiddlewaresInOrder()
    {
        // Arrange
        var order = new List<string>();
        var m1 = new TestMiddleware("m1", order);
        var m2 = new TestMiddleware("m2", order);
        var composite = new CompositeMiddleware("test-composite", m1, m2);

        var agent = new MockAgent("agent") { ResponseFunc = _ => { order.Add("agent"); return "result"; } };

        // Act
        await composite.InvokeAsync(
            agent,
            MakeUserMessages("test"),
            msgs => agent.InvokeAsync(msgs));

        // Assert
        order.Should().Equal("m1-before", "m2-before", "agent", "m2-after", "m1-after");
    }

    [Fact]
    public void CompositeMiddleware_ShouldHaveCorrectProperties()
    {
        // Arrange
        var m1 = new TestMiddleware("m1", new List<string>());
        var m2 = new TestMiddleware("m2", new List<string>());
        var composite = new CompositeMiddleware("my-pack", m1, m2);

        // Assert
        Assert.Equal("my-pack", composite.Name);
        Assert.Equal(2, composite.Count);
        Assert.Equal(2, composite.Middlewares.Count);
    }

    [Fact]
    public void CompositeMiddleware_With_ShouldReturnNewInstance()
    {
        // Arrange
        var m1 = new TestMiddleware("m1", new List<string>());
        var original = new CompositeMiddleware("pack", m1);

        // Act
        var m2 = new TestMiddleware("m2", new List<string>());
        var extended = original.With(m2);

        // Assert
        Assert.Equal(1, original.Count);
        Assert.Equal(2, extended.Count);
        Assert.NotSame(original, extended);
    }

    [Fact]
    public void CompositeMiddleware_Prepend_ShouldAddAtBeginning()
    {
        // Arrange
        var m1 = new TestMiddleware("m1", new List<string>());
        var original = new CompositeMiddleware("pack", m1);

        // Act
        var m2 = new TestMiddleware("m2", new List<string>());
        var prepended = original.Prepend(m2);

        // Assert
        Assert.Equal(2, prepended.Count);
        Assert.Same(m2, prepended.Middlewares[0]);
        Assert.Same(m1, prepended.Middlewares[1]);
    }

    [Fact]
    public void MiddlewarePacks_Resilience_ShouldContainCorrectMiddlewares()
    {
        // Act
        var pack = MiddlewarePacks.Resilience(maxRetries: 2, timeout: TimeSpan.FromSeconds(10));

        // Assert
        Assert.Equal("resilience", pack.Name);
        Assert.Equal(2, pack.Count);
        Assert.IsType<RetryMiddleware>(pack.Middlewares[0]);
        Assert.IsType<TimeoutMiddleware>(pack.Middlewares[1]);
    }

    [Fact]
    public void MiddlewarePacks_Production_ShouldContainAllMiddlewares()
    {
        // Act
        var pack = MiddlewarePacks.Production();

        // Assert
        Assert.Equal("production", pack.Name);
        Assert.Equal(5, pack.Count);
        Assert.IsType<LoggingMiddleware>(pack.Middlewares[0]);
        Assert.IsType<RateLimitMiddleware>(pack.Middlewares[1]);
        Assert.IsType<CircuitBreakerMiddleware>(pack.Middlewares[2]);
        Assert.IsType<RetryMiddleware>(pack.Middlewares[3]);
        Assert.IsType<TimeoutMiddleware>(pack.Middlewares[4]);
    }

    #endregion
}
