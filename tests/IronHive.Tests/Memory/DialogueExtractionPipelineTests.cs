using FluentAssertions;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Core.Memory.Pipelines;
using NSubstitute;

namespace IronHive.Tests.Memory;

public class DialogueExtractionPipelineTests
{
    private readonly IMessageService _mockMessages;
    private readonly DialogueExtractionPipeline _pipeline;
    private readonly DialogueExtractionPipeline.Options _options;

    public DialogueExtractionPipelineTests()
    {
        _mockMessages = Substitute.For<IMessageService>();
        _pipeline = new DialogueExtractionPipeline(_mockMessages);
        _options = new DialogueExtractionPipeline.Options("test-provider", "test-model");
    }

    #region ExecuteAsync

    [Fact]
    public async Task ExecuteAsync_ValidChunks_ExtractsDialogues()
    {
        // Arrange
        var context = CreateContext(["Some text about Apollo 11."]);
        SetupResponse("<qa><q>When did Apollo 11 land?</q><a>July 20, 1969.</a></qa>");

        // Act
        var result = await _pipeline.ExecuteAsync(context, _options);

        // Assert
        result.IsError.Should().BeFalse();
        context.Payload.TryGetValue("chunks", out var chunksObj).Should().BeTrue();
        var dialogues = chunksObj as List<DialogueExtractionPipeline.Dialogue>;
        dialogues.Should().NotBeNull();
        dialogues.Should().HaveCount(1);
        dialogues![0].Question.Should().Be("When did Apollo 11 land?");
        dialogues[0].Answer.Should().Be("July 20, 1969.");
    }

    [Fact]
    public async Task ExecuteAsync_MultipleQaPairs_ExtractsAll()
    {
        // Arrange
        var context = CreateContext(["Text chunk."]);
        SetupResponse("""
            <qa><q>Question 1?</q><a>Answer 1.</a></qa>
            <qa><q>Question 2?</q><a>Answer 2.</a></qa>
            <qa><q>Question 3?</q><a>Answer 3.</a></qa>
            """);

        // Act
        var result = await _pipeline.ExecuteAsync(context, _options);

        // Assert
        result.IsError.Should().BeFalse();
        var dialogues = context.Payload["chunks"] as List<DialogueExtractionPipeline.Dialogue>;
        dialogues.Should().HaveCount(3);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleChunks_AggregatesDialogues()
    {
        // Arrange
        var context = CreateContext(["Chunk 1.", "Chunk 2."]);
        _mockMessages
            .GenerateMessageAsync(Arg.Any<MessageRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                CreateResponse("<qa><q>Q from chunk 1?</q><a>A1.</a></qa>"),
                CreateResponse("<qa><q>Q from chunk 2?</q><a>A2.</a></qa>"));

        // Act
        var result = await _pipeline.ExecuteAsync(context, _options);

        // Assert
        result.IsError.Should().BeFalse();
        var dialogues = context.Payload["chunks"] as List<DialogueExtractionPipeline.Dialogue>;
        dialogues.Should().HaveCount(2);
        dialogues![0].Question.Should().Contain("chunk 1");
        dialogues[1].Question.Should().Contain("chunk 2");
    }

    [Fact]
    public async Task ExecuteAsync_PassesProviderAndModel()
    {
        // Arrange
        var context = CreateContext(["Text."]);
        SetupResponse("<qa><q>Q?</q><a>A.</a></qa>");
        var options = new DialogueExtractionPipeline.Options("my-provider", "my-model");

        // Act
        await _pipeline.ExecuteAsync(context, options);

        // Assert
        await _mockMessages.Received(1).GenerateMessageAsync(
            Arg.Is<MessageRequest>(r => r.Provider == "my-provider" && r.Model == "my-model"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SetsTemperatureAndTopP()
    {
        // Arrange
        var context = CreateContext(["Text."]);
        SetupResponse("<qa><q>Q?</q><a>A.</a></qa>");

        // Act
        await _pipeline.ExecuteAsync(context, _options);

        // Assert
        await _mockMessages.Received(1).GenerateMessageAsync(
            Arg.Is<MessageRequest>(r => r.Temperature == 0.0f && r.TopP == 0.5f),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SetsSystemPromptWithInstructions()
    {
        // Arrange
        var context = CreateContext(["Text."]);
        SetupResponse("<qa><q>Q?</q><a>A.</a></qa>");

        // Act
        await _pipeline.ExecuteAsync(context, _options);

        // Assert
        await _mockMessages.Received(1).GenerateMessageAsync(
            Arg.Is<MessageRequest>(r =>
                r.System != null &&
                r.System.Contains("<qa>") &&
                r.System.Contains("QA")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_IncludesChunkInUserMessage()
    {
        // Arrange
        var context = CreateContext(["Special content about space."]);
        SetupResponse("<qa><q>Q?</q><a>A.</a></qa>");

        // Act
        await _pipeline.ExecuteAsync(context, _options);

        // Assert
        await _mockMessages.Received(1).GenerateMessageAsync(
            Arg.Is<MessageRequest>(r =>
                r.Messages.OfType<UserMessage>().Any(m =>
                    m.Content.OfType<TextMessageContent>().Any(c =>
                        c.Value.Contains("Special content about space.")))),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReplacesChunksInPayload()
    {
        // Arrange: Payload starts with string chunks
        var context = CreateContext(["Text."]);
        SetupResponse("<qa><q>Q?</q><a>A.</a></qa>");

        // Act
        await _pipeline.ExecuteAsync(context, _options);

        // Assert: Payload "chunks" is now List<Dialogue>
        context.Payload["chunks"].Should().BeOfType<List<DialogueExtractionPipeline.Dialogue>>();
    }

    #endregion

    #region Error Handling

    [Fact]
    public async Task ExecuteAsync_MissingChunksKey_ThrowsInvalidOperation()
    {
        // Arrange: No "chunks" in payload
        var context = new MemoryContext
        {
            Source = Substitute.For<IMemorySource>(),
            Target = Substitute.For<IMemoryTarget>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _pipeline.ExecuteAsync(context, _options));
    }

    [Fact]
    public async Task ExecuteAsync_WrongChunksType_ThrowsInvalidOperation()
    {
        // Arrange: "chunks" is not IEnumerable<string>
        var context = new MemoryContext
        {
            Source = Substitute.For<IMemorySource>(),
            Target = Substitute.For<IMemoryTarget>()
        };
        context.Payload["chunks"] = 42; // wrong type

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _pipeline.ExecuteAsync(context, _options));
    }

    [Fact]
    public async Task ExecuteAsync_NullResponseMessage_ThrowsInvalidOperation()
    {
        // Arrange
        var context = CreateContext(["Text."]);
        _mockMessages
            .GenerateMessageAsync(Arg.Any<MessageRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Id = "r1", Message = null! });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _pipeline.ExecuteAsync(context, _options));
    }

    [Fact]
    public async Task ExecuteAsync_EmptyResponseContent_ThrowsInvalidOperation()
    {
        // Arrange
        var context = CreateContext(["Text."]);
        _mockMessages
            .GenerateMessageAsync(Arg.Any<MessageRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                Id = "r1",
                Message = new AssistantMessage { Content = [] }
            });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _pipeline.ExecuteAsync(context, _options));
    }

    [Fact]
    public async Task ExecuteAsync_NoQaPairsInResponse_ThrowsFormatException()
    {
        // Arrange
        var context = CreateContext(["Text."]);
        SetupResponse("This response has no QA tags at all.");

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(
            () => _pipeline.ExecuteAsync(context, _options));
    }

    #endregion

    #region ParseFrom (via ExecuteAsync)

    [Fact]
    public async Task ParseFrom_WhitespaceInTags_TrimsCorrectly()
    {
        // Arrange
        var context = CreateContext(["Text."]);
        SetupResponse("<qa>  <q>  Trimmed question?  </q>  <a>  Trimmed answer.  </a>  </qa>");

        // Act
        await _pipeline.ExecuteAsync(context, _options);

        // Assert
        var dialogues = context.Payload["chunks"] as List<DialogueExtractionPipeline.Dialogue>;
        dialogues.Should().HaveCount(1);
        dialogues![0].Question.Should().Be("Trimmed question?");
        dialogues[0].Answer.Should().Be("Trimmed answer.");
    }

    [Fact]
    public async Task ParseFrom_MultilineContent_ExtractsCorrectly()
    {
        // Arrange
        var context = CreateContext(["Text."]);
        SetupResponse("""
            <qa>
              <q>What happened
              on the moon?</q>
              <a>Astronauts landed
              and walked on it.</a>
            </qa>
            """);

        // Act
        await _pipeline.ExecuteAsync(context, _options);

        // Assert
        var dialogues = context.Payload["chunks"] as List<DialogueExtractionPipeline.Dialogue>;
        dialogues.Should().HaveCount(1);
        dialogues![0].Question.Should().Contain("moon");
        dialogues[0].Answer.Should().Contain("walked");
    }

    [Fact]
    public async Task ParseFrom_EmptyQuestionOrAnswer_SkipsPair()
    {
        // Arrange: One empty Q, one valid pair
        var context = CreateContext(["Text."]);
        SetupResponse("""
            <qa><q></q><a>Answer without question.</a></qa>
            <qa><q>Valid question?</q><a>Valid answer.</a></qa>
            """);

        // Act
        await _pipeline.ExecuteAsync(context, _options);

        // Assert
        var dialogues = context.Payload["chunks"] as List<DialogueExtractionPipeline.Dialogue>;
        dialogues.Should().HaveCount(1);
        dialogues![0].Question.Should().Be("Valid question?");
    }

    [Fact]
    public async Task ParseFrom_WhitespaceOnlyQA_SkipsPair()
    {
        // Arrange
        var context = CreateContext(["Text."]);
        SetupResponse("""
            <qa><q>   </q><a>Answer.</a></qa>
            <qa><q>Real Q?</q><a>Real A.</a></qa>
            """);

        // Act
        await _pipeline.ExecuteAsync(context, _options);

        // Assert
        var dialogues = context.Payload["chunks"] as List<DialogueExtractionPipeline.Dialogue>;
        dialogues.Should().HaveCount(1);
        dialogues![0].Question.Should().Be("Real Q?");
    }

    #endregion

    #region Dialogue Record

    [Fact]
    public void Dialogue_Record_EqualityByValue()
    {
        var d1 = new DialogueExtractionPipeline.Dialogue("Q?", "A.");
        var d2 = new DialogueExtractionPipeline.Dialogue("Q?", "A.");

        d1.Should().Be(d2);
    }

    [Fact]
    public void Options_Record_EqualityByValue()
    {
        var o1 = new DialogueExtractionPipeline.Options("p", "m");
        var o2 = new DialogueExtractionPipeline.Options("p", "m");

        o1.Should().Be(o2);
    }

    #endregion

    #region CancellationToken

    [Fact]
    public async Task ExecuteAsync_PassesCancellationToken()
    {
        // Arrange
        var context = CreateContext(["Text."]);
        SetupResponse("<qa><q>Q?</q><a>A.</a></qa>");
        using var cts = new CancellationTokenSource();

        // Act
        await _pipeline.ExecuteAsync(context, _options, cts.Token);

        // Assert
        await _mockMessages.Received(1).GenerateMessageAsync(
            Arg.Any<MessageRequest>(),
            cts.Token);
    }

    #endregion

    #region Helpers

    private static MemoryContext CreateContext(IEnumerable<string> chunks)
    {
        var context = new MemoryContext
        {
            Source = Substitute.For<IMemorySource>(),
            Target = Substitute.For<IMemoryTarget>()
        };
        context.Payload["chunks"] = chunks;
        return context;
    }

    private void SetupResponse(string text)
    {
        _mockMessages
            .GenerateMessageAsync(Arg.Any<MessageRequest>(), Arg.Any<CancellationToken>())
            .Returns(CreateResponse(text));
    }

    private static MessageResponse CreateResponse(string text)
    {
        return new MessageResponse
        {
            Id = Guid.NewGuid().ToString(),
            Message = new AssistantMessage
            {
                Content = [new TextMessageContent { Value = text }]
            }
        };
    }

    #endregion
}
