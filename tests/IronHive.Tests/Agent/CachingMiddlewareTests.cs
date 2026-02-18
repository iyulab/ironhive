using FluentAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Core.Agent;
using NSubstitute;

namespace IronHive.Tests.Agent;

public class CachingMiddlewareTests
{
    private static IAgent CreateMockAgent(string name = "test-agent", string model = "gpt-4", string? instructions = null)
    {
        var agent = Substitute.For<IAgent>();
        agent.Name.Returns(name);
        agent.Model.Returns(model);
        agent.Instructions.Returns(instructions);
        return agent;
    }

    private static MessageResponse CreateResponse(MessageDoneReason reason = MessageDoneReason.EndTurn, string text = "response")
    {
        return new MessageResponse
        {
            Id = Guid.NewGuid().ToString(),
            DoneReason = reason,
            Message = new AssistantMessage
            {
                Content = [new TextMessageContent { Value = text }]
            }
        };
    }

    private static List<Message> CreateUserMessages(params string[] texts)
    {
        return texts.Select(t => (Message)new UserMessage
        {
            Content = [new TextMessageContent { Value = t }]
        }).ToList();
    }

    // --- Constructor ---

    [Fact]
    public void Constructor_DefaultOptions_CacheCountIsZero()
    {
        var middleware = new CachingMiddleware();

        middleware.CacheCount.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithExpiration_CreatesInstance()
    {
        var middleware = new CachingMiddleware(TimeSpan.FromMinutes(10));

        middleware.CacheCount.Should().Be(0);
    }

    // --- ClearCache ---

    [Fact]
    public async Task ClearCache_RemovesAllEntries()
    {
        var middleware = new CachingMiddleware();
        var agent = CreateMockAgent();
        var messages = CreateUserMessages("hello");
        var response = CreateResponse();

        await middleware.InvokeAsync(agent, messages, _ => Task.FromResult(response));
        middleware.CacheCount.Should().Be(1);

        middleware.ClearCache();
        middleware.CacheCount.Should().Be(0);
    }

    // --- Cache hit/miss ---

    [Fact]
    public async Task InvokeAsync_FirstCall_CallsNextAndCachesResult()
    {
        var middleware = new CachingMiddleware();
        var agent = CreateMockAgent();
        var messages = CreateUserMessages("hello");
        var response = CreateResponse();
        var callCount = 0;

        var result = await middleware.InvokeAsync(agent, messages, _ =>
        {
            callCount++;
            return Task.FromResult(response);
        });

        result.Should().BeSameAs(response);
        callCount.Should().Be(1);
        middleware.CacheCount.Should().Be(1);
    }

    [Fact]
    public async Task InvokeAsync_SameInput_ReturnsCachedResult()
    {
        var middleware = new CachingMiddleware();
        var agent = CreateMockAgent();
        var messages = CreateUserMessages("hello");
        var response = CreateResponse();
        var callCount = 0;

        await middleware.InvokeAsync(agent, messages, _ =>
        {
            callCount++;
            return Task.FromResult(response);
        });

        // Second call with same input
        var result = await middleware.InvokeAsync(agent, messages, _ =>
        {
            callCount++;
            return Task.FromResult(CreateResponse(text: "different"));
        });

        result.Should().BeSameAs(response);
        callCount.Should().Be(1); // next() not called on cache hit
    }

    [Fact]
    public async Task InvokeAsync_DifferentInput_CallsNextAgain()
    {
        var middleware = new CachingMiddleware();
        var agent = CreateMockAgent();
        var callCount = 0;

        await middleware.InvokeAsync(agent, CreateUserMessages("hello"), _ =>
        {
            callCount++;
            return Task.FromResult(CreateResponse(text: "first"));
        });

        await middleware.InvokeAsync(agent, CreateUserMessages("world"), _ =>
        {
            callCount++;
            return Task.FromResult(CreateResponse(text: "second"));
        });

        callCount.Should().Be(2);
        middleware.CacheCount.Should().Be(2);
    }

    [Fact]
    public async Task InvokeAsync_DifferentAgent_CacheMiss()
    {
        var middleware = new CachingMiddleware();
        var messages = CreateUserMessages("hello");
        var callCount = 0;

        await middleware.InvokeAsync(CreateMockAgent("agent-a"), messages, _ =>
        {
            callCount++;
            return Task.FromResult(CreateResponse());
        });

        await middleware.InvokeAsync(CreateMockAgent("agent-b"), messages, _ =>
        {
            callCount++;
            return Task.FromResult(CreateResponse());
        });

        callCount.Should().Be(2);
    }

    // --- Expiration ---

    [Fact]
    public async Task InvokeAsync_ExpiredEntry_CallsNextAgain()
    {
        var options = new CachingMiddlewareOptions
        {
            Expiration = TimeSpan.FromMilliseconds(50)
        };
        var middleware = new CachingMiddleware(options);
        var agent = CreateMockAgent();
        var messages = CreateUserMessages("hello");
        var callCount = 0;

        await middleware.InvokeAsync(agent, messages, _ =>
        {
            callCount++;
            return Task.FromResult(CreateResponse(text: "first"));
        });

        // Wait for expiration
        await Task.Delay(100);

        var result = await middleware.InvokeAsync(agent, messages, _ =>
        {
            callCount++;
            return Task.FromResult(CreateResponse(text: "second"));
        });

        callCount.Should().Be(2);
        result.Message.Content.OfType<TextMessageContent>().First().Value.Should().Be("second");
    }

    [Fact]
    public async Task InvokeAsync_ZeroExpiration_NeverExpires()
    {
        var options = new CachingMiddlewareOptions
        {
            Expiration = TimeSpan.Zero
        };
        var middleware = new CachingMiddleware(options);
        var agent = CreateMockAgent();
        var messages = CreateUserMessages("hello");
        var callCount = 0;

        await middleware.InvokeAsync(agent, messages, _ =>
        {
            callCount++;
            return Task.FromResult(CreateResponse());
        });

        // Second call should still be cached
        await middleware.InvokeAsync(agent, messages, _ =>
        {
            callCount++;
            return Task.FromResult(CreateResponse());
        });

        callCount.Should().Be(1);
    }

    // --- ShouldCache ---

    [Fact]
    public async Task InvokeAsync_ToolCallResponse_NotCached()
    {
        var middleware = new CachingMiddleware();
        var agent = CreateMockAgent();
        var messages = CreateUserMessages("hello");

        await middleware.InvokeAsync(agent, messages, _ =>
            Task.FromResult(CreateResponse(reason: MessageDoneReason.ToolCall)));

        middleware.CacheCount.Should().Be(0);
    }

    [Fact]
    public async Task InvokeAsync_EndTurnResponse_IsCached()
    {
        var middleware = new CachingMiddleware();
        var agent = CreateMockAgent();
        var messages = CreateUserMessages("hello");

        await middleware.InvokeAsync(agent, messages, _ =>
            Task.FromResult(CreateResponse(reason: MessageDoneReason.EndTurn)));

        middleware.CacheCount.Should().Be(1);
    }

    [Fact]
    public async Task InvokeAsync_MaxTokensResponse_IsCached()
    {
        var middleware = new CachingMiddleware();
        var agent = CreateMockAgent();
        var messages = CreateUserMessages("hello");

        await middleware.InvokeAsync(agent, messages, _ =>
            Task.FromResult(CreateResponse(reason: MessageDoneReason.MaxTokens)));

        middleware.CacheCount.Should().Be(1);
    }

    // --- MaxCacheSize ---

    [Fact]
    public async Task InvokeAsync_ExceedsMaxCacheSize_DoesNotAddMore()
    {
        var options = new CachingMiddlewareOptions { MaxCacheSize = 2 };
        var middleware = new CachingMiddleware(options);
        var agent = CreateMockAgent();

        for (var i = 0; i < 5; i++)
        {
            await middleware.InvokeAsync(agent, CreateUserMessages($"msg-{i}"), _ =>
                Task.FromResult(CreateResponse()));
        }

        middleware.CacheCount.Should().BeLessThanOrEqualTo(2);
    }

    // --- Callbacks ---

    [Fact]
    public async Task InvokeAsync_CacheHitCallback_Invoked()
    {
        string? hitAgent = null;
        var options = new CachingMiddlewareOptions
        {
            OnCacheHit = (agent, _) => hitAgent = agent
        };
        var middleware = new CachingMiddleware(options);
        var agent = CreateMockAgent("my-agent");
        var messages = CreateUserMessages("hello");

        await middleware.InvokeAsync(agent, messages, _ =>
            Task.FromResult(CreateResponse()));

        // Second call triggers cache hit
        await middleware.InvokeAsync(agent, messages, _ =>
            Task.FromResult(CreateResponse()));

        hitAgent.Should().Be("my-agent");
    }

    [Fact]
    public async Task InvokeAsync_CacheMissCallback_Invoked()
    {
        string? missAgent = null;
        var options = new CachingMiddlewareOptions
        {
            OnCacheMiss = (agent, _) => missAgent = agent
        };
        var middleware = new CachingMiddleware(options);
        var agent = CreateMockAgent("my-agent");
        var messages = CreateUserMessages("hello");

        await middleware.InvokeAsync(agent, messages, _ =>
            Task.FromResult(CreateResponse()));

        missAgent.Should().Be("my-agent");
    }

    // --- IncludeInstructionsInKey ---

    [Fact]
    public async Task InvokeAsync_DifferentInstructions_SameKey_WhenExcluded()
    {
        var options = new CachingMiddlewareOptions
        {
            IncludeInstructionsInKey = false
        };
        var middleware = new CachingMiddleware(options);
        var messages = CreateUserMessages("hello");
        var callCount = 0;

        await middleware.InvokeAsync(CreateMockAgent(instructions: "Be helpful"), messages, _ =>
        {
            callCount++;
            return Task.FromResult(CreateResponse());
        });

        await middleware.InvokeAsync(CreateMockAgent(instructions: "Be concise"), messages, _ =>
        {
            callCount++;
            return Task.FromResult(CreateResponse());
        });

        // When instructions excluded from key, same agent+messages = cache hit
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task InvokeAsync_DifferentInstructions_DifferentKey_WhenIncluded()
    {
        var options = new CachingMiddlewareOptions
        {
            IncludeInstructionsInKey = true
        };
        var middleware = new CachingMiddleware(options);
        var messages = CreateUserMessages("hello");
        var callCount = 0;

        await middleware.InvokeAsync(CreateMockAgent(instructions: "Be helpful"), messages, _ =>
        {
            callCount++;
            return Task.FromResult(CreateResponse());
        });

        await middleware.InvokeAsync(CreateMockAgent(instructions: "Be concise"), messages, _ =>
        {
            callCount++;
            return Task.FromResult(CreateResponse());
        });

        callCount.Should().Be(2);
    }

    // --- CachingMiddlewareOptions defaults ---

    [Fact]
    public void Options_Defaults_AreCorrect()
    {
        var options = new CachingMiddlewareOptions();

        options.Expiration.Should().Be(TimeSpan.FromMinutes(5));
        options.MaxCacheSize.Should().Be(1000);
        options.IncludeInstructionsInKey.Should().BeTrue();
        options.OnCacheHit.Should().BeNull();
        options.OnCacheMiss.Should().BeNull();
    }
}
