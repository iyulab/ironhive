using FluentAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Tools;
using IronHive.Core.Agent;
using NSubstitute;

namespace IronHive.Tests.Agent;

/// <summary>
/// Tests for BasicAgent.
/// P0-1.3: BasicAgent functionality tests with mocked MessageService.
/// </summary>
public class BasicAgentTests
{
    private readonly IMessageService _mockMessageService;

    public BasicAgentTests()
    {
        _mockMessageService = Substitute.For<IMessageService>();
    }

    private BasicAgent CreateAgent(
        string provider = "openai",
        string model = "gpt-4o",
        string name = "TestAgent",
        string description = "Test agent description")
    {
        return new BasicAgent(_mockMessageService)
        {
            Provider = provider,
            Model = model,
            Name = name,
            Description = description
        };
    }

    #region Property Tests

    [Fact]
    public void Agent_ShouldHave_RequiredProperties()
    {
        // Arrange & Act
        var agent = CreateAgent();

        // Assert
        agent.Provider.Should().Be("openai");
        agent.Model.Should().Be("gpt-4o");
        agent.Name.Should().Be("TestAgent");
        agent.Description.Should().Be("Test agent description");
    }

    [Fact]
    public void Agent_ShouldHave_OptionalSystemPrompt()
    {
        // Arrange
        var agent = CreateAgent();
        agent.Instructions = "You are a helpful assistant.";

        // Assert
        agent.Instructions.Should().Be("You are a helpful assistant.");
    }

    [Fact]
    public void Agent_ShouldHave_OptionalTools()
    {
        // Arrange
        var agent = CreateAgent();
        var tools = new List<ToolItem>
        {
            new() { Name = "calculator" },
            new() { Name = "web-search" }
        };
        agent.Tools = tools;

        // Assert
        agent.Tools.Should().HaveCount(2);
    }

    [Fact]
    public void Agent_ShouldHave_OptionalParameters()
    {
        // Arrange
        var agent = CreateAgent();
        agent.Parameters = new MessageGenerationParameters
        {
            MaxTokens = 1000,
            Temperature = 0.7f,
            TopP = 0.9f
        };

        // Assert
        agent.Parameters.Should().NotBeNull();
        agent.Parameters!.MaxTokens.Should().Be(1000);
        agent.Parameters.Temperature.Should().Be(0.7f);
        agent.Parameters.TopP.Should().Be(0.9f);
    }

    #endregion

    #region InvokeAsync Tests

    [Fact]
    public async Task InvokeAsync_ShouldDelegateToMessageService()
    {
        // Arrange
        var agent = CreateAgent();
        var messages = new List<Message>
        {
            new UserMessage { Content = [new TextMessageContent { Value = "Hello" }] }
        };

        var expectedResponse = new MessageResponse
        {
            Id = "msg-1",
            DoneReason = MessageDoneReason.EndTurn,
            Message = new AssistantMessage
            {
                Content = [new TextMessageContent { Value = "Hello back!" }]
            }
        };

        _mockMessageService
            .GenerateMessageAsync(Arg.Any<MessageRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await agent.InvokeAsync(messages);

        // Assert
        result.Should().NotBeNull();
        result.DoneReason.Should().Be(MessageDoneReason.EndTurn);
        await _mockMessageService.Received(1)
            .GenerateMessageAsync(Arg.Any<MessageRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_ShouldPassProviderAndModel()
    {
        // Arrange
        var agent = CreateAgent(provider: "anthropic", model: "claude-3-opus");
        var messages = new List<Message>
        {
            new UserMessage { Content = [new TextMessageContent { Value = "Hello" }] }
        };

        MessageRequest? capturedRequest = null;
        _mockMessageService
            .GenerateMessageAsync(Arg.Do<MessageRequest>(req => capturedRequest = req), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                Id = "msg-gen",
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage()
            });

        // Act
        await agent.InvokeAsync(messages);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Provider.Should().Be("anthropic");
        capturedRequest.Model.Should().Be("claude-3-opus");
    }

    [Fact]
    public async Task InvokeAsync_ShouldPassSystemPrompt()
    {
        // Arrange
        var agent = CreateAgent();
        agent.Instructions = "You are a coding assistant.";

        var messages = new List<Message>
        {
            new UserMessage { Content = [new TextMessageContent { Value = "Help me code" }] }
        };

        MessageRequest? capturedRequest = null;
        _mockMessageService
            .GenerateMessageAsync(Arg.Do<MessageRequest>(req => capturedRequest = req), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                Id = "msg-gen",
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage()
            });

        // Act
        await agent.InvokeAsync(messages);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.System.Should().Be("You are a coding assistant.");
    }

    [Fact]
    public async Task InvokeAsync_ShouldPassParameters()
    {
        // Arrange
        var agent = CreateAgent();
        agent.Parameters = new MessageGenerationParameters
        {
            MaxTokens = 500,
            Temperature = 0.5f
        };

        var messages = new List<Message>
        {
            new UserMessage { Content = [new TextMessageContent { Value = "Hello" }] }
        };

        MessageRequest? capturedRequest = null;
        _mockMessageService
            .GenerateMessageAsync(Arg.Do<MessageRequest>(req => capturedRequest = req), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                Id = "msg-gen",
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage()
            });

        // Act
        await agent.InvokeAsync(messages);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.MaxTokens.Should().Be(500);
        capturedRequest.Temperature.Should().Be(0.5f);
    }

    [Fact]
    public async Task InvokeAsync_ShouldPassTools()
    {
        // Arrange
        var agent = CreateAgent();
        agent.Tools = [new ToolItem { Name = "calculator" }];

        var messages = new List<Message>
        {
            new UserMessage { Content = [new TextMessageContent { Value = "Calculate 2+2" }] }
        };

        MessageRequest? capturedRequest = null;
        _mockMessageService
            .GenerateMessageAsync(Arg.Do<MessageRequest>(req => capturedRequest = req), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                Id = "msg-gen",
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage()
            });

        // Act
        await agent.InvokeAsync(messages);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Tools.Should().HaveCount(1);
        capturedRequest.Tools.First().Name.Should().Be("calculator");
    }

    #endregion

    #region InvokeStreamingAsync Tests

    [Fact]
    public async Task InvokeStreamingAsync_ShouldDelegateToMessageService()
    {
        // Arrange
        var agent = CreateAgent();
        var messages = new List<Message>
        {
            new UserMessage { Content = [new TextMessageContent { Value = "Hello" }] }
        };

        var streamingResponses = new List<StreamingMessageResponse>
        {
            new StreamingMessageBeginResponse { Id = "msg-1" },
            new StreamingContentAddedResponse { Index = 0, Content = new TextMessageContent { Value = "" } },
            new StreamingMessageDoneResponse { Id = "msg-1", DoneReason = MessageDoneReason.EndTurn, Model = "gpt-4o", Timestamp = DateTime.UtcNow }
        };

        _mockMessageService
            .GenerateStreamingMessageAsync(Arg.Any<MessageRequest>(), Arg.Any<CancellationToken>())
            .Returns(streamingResponses.ToAsyncEnumerable());

        // Act
        var responses = new List<StreamingMessageResponse>();
        await foreach (var response in agent.InvokeStreamingAsync(messages))
        {
            responses.Add(response);
        }

        // Assert
        responses.Should().HaveCount(3);
        _mockMessageService.Received(1)
            .GenerateStreamingMessageAsync(Arg.Any<MessageRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeStreamingAsync_ShouldPassProviderAndModel()
    {
        // Arrange
        var agent = CreateAgent(provider: "ollama", model: "llama3");
        var messages = new List<Message>
        {
            new UserMessage { Content = [new TextMessageContent { Value = "Hello" }] }
        };

        MessageRequest? capturedRequest = null;
        _mockMessageService
            .GenerateStreamingMessageAsync(Arg.Do<MessageRequest>(req => capturedRequest = req), Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<StreamingMessageResponse>());

        // Act
        await foreach (var _ in agent.InvokeStreamingAsync(messages))
        {
        }

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Provider.Should().Be("ollama");
        capturedRequest.Model.Should().Be("llama3");
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldNotThrow_WithValidDependency()
    {
        // Act
        var act = () => CreateAgent();

        // Assert
        act.Should().NotThrow();
    }

    #endregion
}

/// <summary>
/// Extension to convert IEnumerable to IAsyncEnumerable for testing.
/// </summary>
internal static class BasicAgentAsyncEnumerableExtensions
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
