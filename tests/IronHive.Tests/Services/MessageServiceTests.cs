using FluentAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Tools;
using IronHive.Core.Services;
using Moq;

namespace IronHive.Tests.Services;

/// <summary>
/// Tests for MessageService.
/// P0-1.3: Message generation service tests with mocked dependencies.
/// </summary>
public class MessageServiceTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IProviderRegistry> _mockProviderRegistry;
    private readonly Mock<IToolCollection> _mockToolCollection;
    private readonly MessageService _service;

    public MessageServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockProviderRegistry = new Mock<IProviderRegistry>();
        _mockToolCollection = new Mock<IToolCollection>();

        _mockToolCollection
            .Setup(t => t.FilterBy(It.IsAny<IEnumerable<string>>()))
            .Returns(_mockToolCollection.Object);

        _service = new MessageService(
            _mockServiceProvider.Object,
            _mockProviderRegistry.Object,
            _mockToolCollection.Object);
    }

    [Fact]
    public async Task GenerateMessageAsync_ShouldThrow_WhenProviderNotFound()
    {
        // Arrange
        var request = new MessageRequest
        {
            Provider = "nonexistent-provider",
            Model = "test-model",
            Messages = [new UserMessage { Content = [new TextMessageContent { Value = "Hello" }] }]
        };

        IMessageGenerator? generator = null;
        _mockProviderRegistry
            .Setup(r => r.TryGet("nonexistent-provider", out generator))
            .Returns(false);

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
        var mockGenerator = new Mock<IMessageGenerator>();
        var expectedResponse = new MessageResponse
        {
            Id = "msg-1",
            DoneReason = MessageDoneReason.EndTurn,
            Message = new AssistantMessage
            {
                Content = [new TextMessageContent { Value = "Hello back!" }]
            }
        };

        mockGenerator
            .Setup(g => g.GenerateMessageAsync(It.IsAny<MessageGenerationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        IMessageGenerator? generator = mockGenerator.Object;
        _mockProviderRegistry
            .Setup(r => r.TryGet("openai", out generator))
            .Returns(true);

        var request = new MessageRequest
        {
            Provider = "openai",
            Model = "gpt-4o",
            Messages = [new UserMessage { Content = [new TextMessageContent { Value = "Hello" }] }]
        };

        // Act
        var result = await _service.GenerateMessageAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.DoneReason.Should().Be(MessageDoneReason.EndTurn);
        result.Message.Should().NotBeNull();
        mockGenerator.Verify(
            g => g.GenerateMessageAsync(It.IsAny<MessageGenerationRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateMessageAsync_ShouldPassModelToGenerator()
    {
        // Arrange
        var mockGenerator = new Mock<IMessageGenerator>();
        MessageGenerationRequest? capturedRequest = null;

        mockGenerator
            .Setup(g => g.GenerateMessageAsync(It.IsAny<MessageGenerationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<MessageGenerationRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new MessageResponse
            {
                Id = "msg-gen",
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage()
            });

        IMessageGenerator? generator = mockGenerator.Object;
        _mockProviderRegistry
            .Setup(r => r.TryGet("openai", out generator))
            .Returns(true);

        var request = new MessageRequest
        {
            Provider = "openai",
            Model = "gpt-4o-mini",
            Messages = [new UserMessage { Content = [new TextMessageContent { Value = "Test" }] }]
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
        var mockGenerator = new Mock<IMessageGenerator>();
        MessageGenerationRequest? capturedRequest = null;

        mockGenerator
            .Setup(g => g.GenerateMessageAsync(It.IsAny<MessageGenerationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<MessageGenerationRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new MessageResponse
            {
                Id = "msg-gen",
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage()
            });

        IMessageGenerator? generator = mockGenerator.Object;
        _mockProviderRegistry
            .Setup(r => r.TryGet("openai", out generator))
            .Returns(true);

        var request = new MessageRequest
        {
            Provider = "openai",
            Model = "gpt-4o",
            System = "You are a helpful assistant.",
            Messages = [new UserMessage { Content = [new TextMessageContent { Value = "Test" }] }]
        };

        // Act
        await _service.GenerateMessageAsync(request);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.System.Should().Be("You are a helpful assistant.");
    }

    [Fact]
    public async Task GenerateMessageAsync_ShouldPassTemperatureToGenerator()
    {
        // Arrange
        var mockGenerator = new Mock<IMessageGenerator>();
        MessageGenerationRequest? capturedRequest = null;

        mockGenerator
            .Setup(g => g.GenerateMessageAsync(It.IsAny<MessageGenerationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<MessageGenerationRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new MessageResponse
            {
                Id = "msg-gen",
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage()
            });

        IMessageGenerator? generator = mockGenerator.Object;
        _mockProviderRegistry
            .Setup(r => r.TryGet("openai", out generator))
            .Returns(true);

        var request = new MessageRequest
        {
            Provider = "openai",
            Model = "gpt-4o",
            Temperature = 0.7f,
            Messages = [new UserMessage { Content = [new TextMessageContent { Value = "Test" }] }]
        };

        // Act
        await _service.GenerateMessageAsync(request);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Temperature.Should().Be(0.7f);
    }

    [Fact]
    public async Task GenerateStreamingMessageAsync_ShouldThrow_WhenProviderNotFound()
    {
        // Arrange
        var request = new MessageRequest
        {
            Provider = "nonexistent-provider",
            Model = "test-model",
            Messages = [new UserMessage { Content = [new TextMessageContent { Value = "Hello" }] }]
        };

        IMessageGenerator? generator = null;
        _mockProviderRegistry
            .Setup(r => r.TryGet("nonexistent-provider", out generator))
            .Returns(false);

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
        var mockGenerator = new Mock<IMessageGenerator>();

        var streamingResponses = new List<StreamingMessageResponse>
        {
            new StreamingMessageBeginResponse { Id = "msg-1" },
            new StreamingMessageDoneResponse
            {
                Id = "msg-1",
                DoneReason = MessageDoneReason.EndTurn,
                Model = "gpt-4o",
                Timestamp = DateTime.UtcNow
            }
        };

        mockGenerator
            .Setup(g => g.GenerateStreamingMessageAsync(It.IsAny<MessageGenerationRequest>(), It.IsAny<CancellationToken>()))
            .Returns(streamingResponses.ToAsyncEnumerable());

        IMessageGenerator? generator = mockGenerator.Object;
        _mockProviderRegistry
            .Setup(r => r.TryGet("openai", out generator))
            .Returns(true);

        var request = new MessageRequest
        {
            Provider = "openai",
            Model = "gpt-4o",
            Messages = [new UserMessage { Content = [new TextMessageContent { Value = "Test" }] }]
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
        var mockGenerator = new Mock<IMessageGenerator>();

        var streamingResponses = new List<StreamingMessageResponse>
        {
            new StreamingMessageBeginResponse { Id = "msg-1" },
            new StreamingMessageDoneResponse
            {
                Id = "msg-1",
                DoneReason = MessageDoneReason.EndTurn,
                Model = "gpt-4o",
                Timestamp = DateTime.UtcNow
            }
        };

        mockGenerator
            .Setup(g => g.GenerateStreamingMessageAsync(It.IsAny<MessageGenerationRequest>(), It.IsAny<CancellationToken>()))
            .Returns(streamingResponses.ToAsyncEnumerable());

        IMessageGenerator? generator = mockGenerator.Object;
        _mockProviderRegistry
            .Setup(r => r.TryGet("openai", out generator))
            .Returns(true);

        var request = new MessageRequest
        {
            Provider = "openai",
            Model = "gpt-4o",
            Messages = [new UserMessage { Content = [new TextMessageContent { Value = "Test" }] }]
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
            _mockServiceProvider.Object,
            _mockProviderRegistry.Object,
            _mockToolCollection.Object);

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
