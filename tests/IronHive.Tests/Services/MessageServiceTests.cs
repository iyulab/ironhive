using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Core.Services;
using IronHive.Core.Tools;
using NSubstitute;

namespace IronHive.Tests.Services;

/// <summary>
/// Tests for MessageService.
/// </summary>
public class MessageServiceTests
{
    private readonly Dictionary<string, IMessageGenerator> _generators;
    private readonly MessageService _service;

    public MessageServiceTests()
    {
        _generators = new Dictionary<string, IMessageGenerator>();
        _service = new MessageService(_generators);
    }

    [Fact]
    public async Task GenerateMessageAsync_ShouldThrow_WhenProviderNotFound()
    {
        // Arrange — generators dict is empty
        var request = new MessageRequest
        {
            Provider = "nonexistent-provider",
            Model = "test-model",
            Messages = [Message.User("Hello")]
        };

        // Act
        var act = async () => await _service.GenerateMessageAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*nonexistent-provider*");
    }

    [Fact]
    public async Task GenerateMessageAsync_ShouldDelegateToGenerator()
    {
        // Arrange
        var mockGenerator = Substitute.For<IMessageGenerator>();
        var expectedResponse = new MessageResponse
        {
            ResponseId = "msg-1",
            DoneReason = MessageDoneReason.EndTurn,
            Message = Message.Assistant("Hello back!")
        };
        mockGenerator
            .GenerateMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);
        _generators["openai"] = mockGenerator;

        var request = new MessageRequest
        {
            Provider = "openai",
            Model = "gpt-4o",
            Messages = [Message.User("Hello")]
        };

        // Act
        var result = await _service.GenerateMessageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.DoneReason.Should().Be(MessageDoneReason.EndTurn);
        result.Message.Should().NotBeNull();
        await mockGenerator.Received(1)
            .GenerateMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateMessageAsync_ShouldPassModelToGenerator()
    {
        // Arrange
        var mockGenerator = Substitute.For<IMessageGenerator>();
        MessageGenerationRequest? capturedRequest = null;
        mockGenerator
            .GenerateMessageAsync(Arg.Do<MessageGenerationRequest>(req => capturedRequest = req), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                ResponseId = "msg-gen",
                DoneReason = MessageDoneReason.EndTurn,
                Message = new Message { Role = MessageRole.Assistant },
                Model = string.Empty,
                Timestamp = DateTime.UtcNow
            });
        _generators["openai"] = mockGenerator;

        var request = new MessageRequest
        {
            Provider = "openai",
            Model = "gpt-4o-mini",
            Messages = [Message.User("Test")]
        };

        // Act
        await _service.GenerateMessageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Model.Should().Be("gpt-4o-mini");
    }

    [Fact]
    public async Task GenerateMessageAsync_ShouldPassSystemPromptToGenerator()
    {
        // Arrange
        var mockGenerator = Substitute.For<IMessageGenerator>();
        MessageGenerationRequest? capturedRequest = null;
        mockGenerator
            .GenerateMessageAsync(Arg.Do<MessageGenerationRequest>(req => capturedRequest = req), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                ResponseId = "msg-gen",
                DoneReason = MessageDoneReason.EndTurn,
                Message = new Message { Role = MessageRole.Assistant },
                Model = string.Empty,
                Timestamp = DateTime.UtcNow
            });
        _generators["openai"] = mockGenerator;

        var request = new MessageRequest
        {
            Provider = "openai",
            Model = "gpt-4o",
            System = "You are a helpful assistant.",
            Messages = [Message.User("Test")]
        };

        // Act
        await _service.GenerateMessageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.System.Should().Be("You are a helpful assistant.");
    }

    [Fact]
    public async Task GenerateMessageAsync_ShouldPassMaxTokensToGenerator()
    {
        // Arrange
        var mockGenerator = Substitute.For<IMessageGenerator>();
        MessageGenerationRequest? capturedRequest = null;
        mockGenerator
            .GenerateMessageAsync(Arg.Do<MessageGenerationRequest>(req => capturedRequest = req), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                ResponseId = "msg-gen",
                DoneReason = MessageDoneReason.EndTurn,
                Message = new Message { Role = MessageRole.Assistant },
                Model = string.Empty,
                Timestamp = DateTime.UtcNow
            });
        _generators["openai"] = mockGenerator;

        var request = new MessageRequest
        {
            Provider = "openai",
            Model = "gpt-4o",
            MaxTokens = 500,
            Messages = [Message.User("Test")]
        };

        // Act
        await _service.GenerateMessageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.MaxTokens.Should().Be(500);
    }

    [Fact]
    public async Task GenerateStreamingMessageAsync_ShouldThrow_WhenProviderNotFound()
    {
        // Arrange — generators dict is empty
        var request = new MessageRequest
        {
            Provider = "nonexistent-provider",
            Model = "test-model",
            Messages = [Message.User("Hello")]
        };

        // Act
        var act = async () =>
        {
            await foreach (var _ in _service.GenerateStreamingMessageAsync(request))
            {
                // Just enumerate to trigger the exception
            }
        };

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*nonexistent-provider*");
    }

    [Fact]
    public async Task GenerateStreamingMessageAsync_ShouldYieldBeginResponse()
    {
        // Arrange
        var mockGenerator = Substitute.For<IMessageGenerator>();
        var streamingResponses = new List<StreamingMessageResponse>
        {
            new StreamingMessageBeginResponse(),
            new StreamingMessageDoneResponse
            {
                ResponseId = "msg-1",
                DoneReason = MessageDoneReason.EndTurn,
                Model = "gpt-4o",
                Timestamp = DateTime.UtcNow
            }
        };
        mockGenerator
            .GenerateStreamingMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(streamingResponses.ToAsyncEnumerable());
        _generators["openai"] = mockGenerator;

        var request = new MessageRequest
        {
            Provider = "openai",
            Model = "gpt-4o",
            Messages = [Message.User("Test")]
        };

        // Act
        var responses = new List<StreamingMessageResponse>();
        await foreach (var response in _service.GenerateStreamingMessageAsync(request, TestContext.Current.CancellationToken))
        {
            responses.Add(response);
        }

        // Assert
        responses.Should().NotBeEmpty();
        responses.First().Should().BeOfType<StreamingMessageBeginResponse>();
    }

    [Fact]
    public async Task GenerateStreamingMessageAsync_ShouldYieldDoneResponse()
    {
        // Arrange
        var mockGenerator = Substitute.For<IMessageGenerator>();
        var streamingResponses = new List<StreamingMessageResponse>
        {
            new StreamingMessageBeginResponse(),
            new StreamingMessageDoneResponse
            {
                ResponseId = "msg-1",
                DoneReason = MessageDoneReason.EndTurn,
                Model = "gpt-4o",
                Timestamp = DateTime.UtcNow
            }
        };
        mockGenerator
            .GenerateStreamingMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(streamingResponses.ToAsyncEnumerable());
        _generators["openai"] = mockGenerator;

        var request = new MessageRequest
        {
            Provider = "openai",
            Model = "gpt-4o",
            Messages = [Message.User("Test")]
        };

        // Act
        var responses = new List<StreamingMessageResponse>();
        await foreach (var response in _service.GenerateStreamingMessageAsync(request, TestContext.Current.CancellationToken))
        {
            responses.Add(response);
        }

        // Assert
        responses.Should().NotBeEmpty();
        responses.Last().Should().BeOfType<StreamingMessageDoneResponse>();
        ((StreamingMessageDoneResponse)responses.Last()).DoneReason.Should().Be(MessageDoneReason.EndTurn);
    }

    [Fact]
    public void Constructor_ShouldNotThrow_WithValidDependencies()
    {
        // Act
        var act = () => new MessageService(new Dictionary<string, IMessageGenerator>());

        // Assert
        act.Should().NotThrow();
    }

    #region ToolOptions.Timeout behavior (regression)

    // ToolOptions.Timeout exceeded is swallowed into a ToolOutput.Failure fed back to the model —
    // not thrown as an exception — while the caller's own CancellationToken firing during a tool
    // call still surfaces as a thrown exception. Nothing previously pinned this distinction.

    private sealed class DelayingTool(string name, TimeSpan delay) : ITool
    {
        public string UniqueName => name;
        public string? Description => null;
        public object? Parameters => null;
        public bool RequiresApproval => false;

        public async Task<ToolOutput> InvokeAsync(ToolInput input, CancellationToken cancellationToken = default)
        {
            await Task.Delay(delay, cancellationToken);
            return ToolOutput.Success("done");
        }
    }

    private static Message ToolCallMessage(string toolName) => new()
    {
        Role = MessageRole.Assistant,
        Content = [new ToolMessageContent { Id = "1", Name = toolName, Input = "{}", IsApproved = true }]
    };

    [Fact]
    public async Task ExecuteTools_TimeoutExceeded_ReturnsFailureOutput_DoesNotThrow()
    {
        var mockGenerator = Substitute.For<IMessageGenerator>();
        mockGenerator
            .GenerateMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                new MessageResponse { ResponseId = "r1", DoneReason = MessageDoneReason.ToolCall, Message = ToolCallMessage("slow") },
                new MessageResponse { ResponseId = "r2", DoneReason = MessageDoneReason.EndTurn, Message = Message.Assistant("done") });
        _generators["openai"] = mockGenerator;

        var request = new MessageRequest
        {
            Provider = "openai",
            Model = "gpt-4o",
            Messages = [Message.User("go")],
            Tools = new ToolCollection([new DelayingTool("slow", TimeSpan.FromSeconds(5))]),
            ToolOptions = new ToolOptions { Timeout = TimeSpan.FromMilliseconds(50) }
        };

        var result = await _service.GenerateMessageAsync(request, TestContext.Current.CancellationToken);

        var toolContent = result.Message!.Content.OfType<ToolMessageContent>().Single();
        toolContent.Output.Should().NotBeNull();
        toolContent.Output!.IsSuccess.Should().BeFalse();
        toolContent.Output.Content.OfType<TextMessageContent>().Single().Value.Should().Contain("timed out");
    }

    [Fact]
    public async Task ExecuteTools_CallerCancels_ThrowsInsteadOfSwallowedAsTimeoutFailure()
    {
        // Unlike a ToolOptions.Timeout expiring (swallowed into ToolOutput.Failure, above), the
        // caller's own token firing during a tool call is not swallowed — it still surfaces as a
        // thrown exception (wrapped in InvalidOperationException by the per-tool Task.Run catch in
        // ExecuteToolsAsync, with the cancellation as its InnerException).
        var mockGenerator = Substitute.For<IMessageGenerator>();
        mockGenerator
            .GenerateMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { ResponseId = "r1", DoneReason = MessageDoneReason.ToolCall, Message = ToolCallMessage("slow") });
        _generators["openai"] = mockGenerator;

        using var cts = new CancellationTokenSource();
        var request = new MessageRequest
        {
            Provider = "openai",
            Model = "gpt-4o",
            Messages = [Message.User("go")],
            Tools = new ToolCollection([new DelayingTool("slow", TimeSpan.FromSeconds(5))]),
            // No ToolOptions.Timeout — the caller's own token is what fires during the tool call.
        };
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        var act = async () => await _service.GenerateMessageAsync(request, cts.Token);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithInnerException<OperationCanceledException>();
    }

    #endregion
}

/// <summary>
/// Extension to convert IEnumerable to IAsyncEnumerable for testing.
/// </summary>
internal static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
            await Task.Yield();
        }
    }
}
