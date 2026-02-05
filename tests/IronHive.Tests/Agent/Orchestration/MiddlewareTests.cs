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

    #region Test Middlewares

    private class TestMiddleware : IAgentMiddleware
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

    private class InputModifyingMiddleware : IAgentMiddleware
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

    private class ShortCircuitMiddleware : IAgentMiddleware
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

    private class FailingMiddleware : IAgentMiddleware
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

    private class MockAgent : IAgent
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
}
