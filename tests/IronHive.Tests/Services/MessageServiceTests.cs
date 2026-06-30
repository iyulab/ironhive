using FluentAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Core.Services;
using NSubstitute;

namespace IronHive.Tests.Services;

/// <summary>
/// Tests for MessageService.
/// </summary>
public class MessageServiceTests
{
    private readonly IServiceProvider _mockServiceProvider;
    private readonly Dictionary<string, IMessageGenerator> _generators;
    private readonly IToolCollection _mockToolCollection;
    private readonly MessageService _service;

    public MessageServiceTests()
    {
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _generators = new Dictionary<string, IMessageGenerator>();
        _mockToolCollection = Substitute.For<IToolCollection>();

        _mockToolCollection
            .FilterBy(Arg.Any<IEnumerable<string>>())
            .Returns(_mockToolCollection);

        _service = new MessageService(_generators, _mockToolCollection, _mockServiceProvider);
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
        var result = await _service.GenerateMessageAsync(request);

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
        await _service.GenerateMessageAsync(request);

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
        await _service.GenerateMessageAsync(request);

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
        await _service.GenerateMessageAsync(request);

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
        await foreach (var response in _service.GenerateStreamingMessageAsync(request))
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
        await foreach (var response in _service.GenerateStreamingMessageAsync(request))
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
        var act = () => new MessageService(
            new Dictionary<string, IMessageGenerator>(),
            _mockToolCollection,
            _mockServiceProvider);

        // Assert
        act.Should().NotThrow();
    }
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
