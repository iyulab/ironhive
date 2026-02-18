using System.Runtime.CompilerServices;
using FluentAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Resilience;
using IronHive.Core.Resilience;
using NSubstitute;

namespace IronHive.Tests.Resilience;

public class ResilientMessageGeneratorTests : IDisposable
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

    private readonly IMessageGenerator _inner;

    public ResilientMessageGeneratorTests()
    {
        _inner = Substitute.For<IMessageGenerator>();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    #region Constructor

    [Fact]
    public void Constructor_NullInner_ThrowsArgumentNullException()
    {
        var act = () => new ResilientMessageGenerator(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithOptions_CreatesInstance()
    {
        var options = new ResilienceOptions
        {
            Enabled = false
        };

        var sut = new ResilientMessageGenerator(_inner, options);

        sut.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NullOptions_UsesDefaults()
    {
        var sut = new ResilientMessageGenerator(_inner);

        sut.Should().NotBeNull();
    }

    #endregion

    #region GenerateMessageAsync

    [Fact]
    public async Task GenerateMessageAsync_DelegatesToInner()
    {
        var request = new MessageGenerationRequest { Model = "test-model" };
        _inner.GenerateMessageAsync(request, Arg.Any<CancellationToken>())
            .Returns(s_okResponse);

        var sut = new ResilientMessageGenerator(_inner, new ResilienceOptions { Enabled = false });

        var result = await sut.GenerateMessageAsync(request);

        result.Should().BeSameAs(s_okResponse);
        await _inner.Received(1).GenerateMessageAsync(request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateMessageAsync_WithRetry_RetriesOnFailure()
    {
        var request = new MessageGenerationRequest { Model = "test-model" };
        var callCount = 0;
        _inner.GenerateMessageAsync(request, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                if (callCount < 3)
                    throw new HttpRequestException("transient error");
                return s_okResponse;
            });

        var sut = new ResilientMessageGenerator(_inner, new ResilienceOptions
        {
            Retry = new RetryOptions
            {
                MaxRetries = 3,
                InitialDelay = TimeSpan.FromMilliseconds(10),
                MaxDelay = TimeSpan.FromMilliseconds(50)
            },
            CircuitBreaker = new CircuitBreakerOptions { Enabled = false },
            Timeout = new TimeoutOptions { Enabled = false }
        });

        var result = await sut.GenerateMessageAsync(request);

        result.Should().BeSameAs(s_okResponse);
        callCount.Should().Be(3);
    }

    [Fact]
    public async Task GenerateMessageAsync_AllRetriesFail_ThrowsException()
    {
        var request = new MessageGenerationRequest { Model = "test-model" };
        _inner.GenerateMessageAsync(request, Arg.Any<CancellationToken>())
            .Returns<MessageResponse>(_ => throw new HttpRequestException("persistent error"));

        var sut = new ResilientMessageGenerator(_inner, new ResilienceOptions
        {
            Retry = new RetryOptions
            {
                MaxRetries = 2,
                InitialDelay = TimeSpan.FromMilliseconds(10),
                MaxDelay = TimeSpan.FromMilliseconds(50)
            },
            CircuitBreaker = new CircuitBreakerOptions { Enabled = false },
            Timeout = new TimeoutOptions { Enabled = false }
        });

        var act = async () => await sut.GenerateMessageAsync(request);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    #endregion

    #region GenerateStreamingMessageAsync

    [Fact]
    public async Task GenerateStreamingMessageAsync_DelegatesToInner()
    {
        var request = new MessageGenerationRequest { Model = "test-model" };
        var items = new StreamingMessageResponse[]
        {
            new StreamingMessageBeginResponse { Id = "stream-1" },
            new StreamingMessageDoneResponse
            {
                Id = "stream-1",
                Model = "test-model",
                Timestamp = DateTime.UtcNow,
                DoneReason = MessageDoneReason.EndTurn,
                TokenUsage = new MessageTokenUsage { InputTokens = 1, OutputTokens = 1 }
            }
        };

        _inner.GenerateStreamingMessageAsync(request, Arg.Any<CancellationToken>())
            .Returns(items.ToAsyncEnumerable());

        var sut = new ResilientMessageGenerator(_inner, new ResilienceOptions
        {
            Retry = new RetryOptions { Enabled = false },
            CircuitBreaker = new CircuitBreakerOptions { Enabled = false },
            Timeout = new TimeoutOptions { Enabled = false }
        });

        var results = new List<StreamingMessageResponse>();
        await foreach (var item in sut.GenerateStreamingMessageAsync(request))
        {
            results.Add(item);
        }

        results.Should().HaveCount(2);
        results[0].Should().BeOfType<StreamingMessageBeginResponse>()
            .Which.Id.Should().Be("stream-1");
        results[1].Should().BeOfType<StreamingMessageDoneResponse>();
    }

    [Fact]
    public async Task GenerateStreamingMessageAsync_EmptyStream_YieldsNothing()
    {
        var request = new MessageGenerationRequest { Model = "test-model" };
        _inner.GenerateStreamingMessageAsync(request, Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<StreamingMessageResponse>());

        var sut = new ResilientMessageGenerator(_inner, new ResilienceOptions
        {
            Retry = new RetryOptions { Enabled = false },
            CircuitBreaker = new CircuitBreakerOptions { Enabled = false },
            Timeout = new TimeoutOptions { Enabled = false }
        });

        var results = new List<StreamingMessageResponse>();
        await foreach (var item in sut.GenerateStreamingMessageAsync(request))
        {
            results.Add(item);
        }

        results.Should().BeEmpty();
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_DelegatesToInner()
    {
        var sut = new ResilientMessageGenerator(_inner);

        sut.Dispose();

        _inner.Received(1).Dispose();
    }

    #endregion
}
