using FluentAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Core.Compatibility;
using Microsoft.Extensions.AI;
using NSubstitute;

namespace IronHive.Tests.Compatibility;

public class ChatClientAdapterTests : IDisposable
{
    private readonly IMessageGenerator _mockGenerator;
    private readonly ChatClientAdapter _adapter;

    public ChatClientAdapterTests()
    {
        _mockGenerator = Substitute.For<IMessageGenerator>();
        _adapter = new ChatClientAdapter(_mockGenerator, "test-model", "TestProvider");
    }

    public void Dispose()
    {
        _adapter.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Constructor

    [Fact]
    public void Constructor_NullGenerator_ThrowsArgumentNullException()
    {
        var act = () => new ChatClientAdapter(null!, "model");

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("generator");
    }

    [Fact]
    public void Constructor_NullModelId_ThrowsArgumentNullException()
    {
        var act = () => new ChatClientAdapter(_mockGenerator, null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("modelId");
    }

    [Fact]
    public void Constructor_NullProviderName_DefaultsToIronHive()
    {
        var adapter = new ChatClientAdapter(_mockGenerator, "model");

        adapter.Metadata.ProviderName.Should().Be("IronHive");
    }

    #endregion

    #region Metadata

    [Fact]
    public void Metadata_ReturnsCorrectProviderAndModel()
    {
        _adapter.Metadata.ProviderName.Should().Be("TestProvider");
        _adapter.Metadata.DefaultModelId.Should().Be("test-model");
        _adapter.Metadata.ProviderUri.Should().BeNull();
    }

    #endregion

    #region GetService

    [Fact]
    public void GetService_IMessageGenerator_ReturnsGenerator()
    {
        var result = _adapter.GetService(typeof(IMessageGenerator));

        result.Should().BeSameAs(_mockGenerator);
    }

    [Fact]
    public void GetService_UnknownType_ReturnsNull()
    {
        var result = _adapter.GetService(typeof(string));

        result.Should().BeNull();
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var act = () => _adapter.Dispose();

        act.Should().NotThrow();
    }

    #endregion

    #region GetResponseAsync — Request conversion

    [Fact]
    public async Task GetResponseAsync_SystemMessage_SetsSystemProperty()
    {
        var capturedRequest = SetupGeneratorReturns();
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful assistant."),
            new(ChatRole.User, "Hello")
        };

        await _adapter.GetResponseAsync(messages);

        capturedRequest().System.Should().Be("You are a helpful assistant.");
    }

    [Fact]
    public async Task GetResponseAsync_UsesDefaultModel_WhenNoOptionsModel()
    {
        var capturedRequest = SetupGeneratorReturns();
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "Hello")
        };

        await _adapter.GetResponseAsync(messages);

        capturedRequest().Model.Should().Be("test-model");
    }

    [Fact]
    public async Task GetResponseAsync_UsesOptionsModel_WhenProvided()
    {
        var capturedRequest = SetupGeneratorReturns();
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "Hello")
        };
        var options = new ChatOptions { ModelId = "override-model" };

        await _adapter.GetResponseAsync(messages, options);

        capturedRequest().Model.Should().Be("override-model");
    }

    [Fact]
    public async Task GetResponseAsync_MapsTemperatureAndTopP()
    {
        var capturedRequest = SetupGeneratorReturns();
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "Hello")
        };
        var options = new ChatOptions
        {
            Temperature = 0.7f,
            TopP = 0.9f,
            MaxOutputTokens = 1024
        };

        await _adapter.GetResponseAsync(messages, options);

        var req = capturedRequest();
        req.Temperature.Should().Be(0.7f);
        req.TopP.Should().Be(0.9f);
        req.MaxTokens.Should().Be(1024);
    }

    [Fact]
    public async Task GetResponseAsync_MapsStopSequences()
    {
        var capturedRequest = SetupGeneratorReturns();
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "Hello")
        };
        var options = new ChatOptions
        {
            StopSequences = ["stop1", "stop2"]
        };

        await _adapter.GetResponseAsync(messages, options);

        capturedRequest().StopSequences.Should().BeEquivalentTo(["stop1", "stop2"]);
    }

    #endregion

    #region GetResponseAsync — Message conversion

    [Fact]
    public async Task GetResponseAsync_UserTextMessage_ConvertsToUserMessage()
    {
        var capturedRequest = SetupGeneratorReturns();
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "Hello world")
        };

        await _adapter.GetResponseAsync(messages);

        var req = capturedRequest();
        req.Messages.Should().HaveCount(1);
        var userMsg = req.Messages.First().Should().BeOfType<UserMessage>().Subject;
        userMsg.Content.Should().ContainSingle()
            .Which.Should().BeOfType<TextMessageContent>()
            .Which.Value.Should().Be("Hello world");
    }

    [Fact]
    public async Task GetResponseAsync_UserImageMessage_ConvertsToImageContent()
    {
        var capturedRequest = SetupGeneratorReturns();
        var imageBytes = new byte[] { 1, 2, 3, 4 };
        var imageContent = new DataContent(imageBytes, "image/png");
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, [imageContent])
        };

        await _adapter.GetResponseAsync(messages);

        var req = capturedRequest();
        var userMsg = req.Messages.First().Should().BeOfType<UserMessage>().Subject;
        var imgContent = userMsg.Content.Should().ContainSingle()
            .Which.Should().BeOfType<ImageMessageContent>().Subject;
        imgContent.Format.Should().Be(ImageFormat.Png);
        imgContent.Base64.Should().Be(Convert.ToBase64String(imageBytes));
    }

    [Fact]
    public async Task GetResponseAsync_AssistantFunctionCall_ConvertsToToolContent()
    {
        var capturedRequest = SetupGeneratorReturns();
        var functionCall = new FunctionCallContent("call-1", "get_weather",
            new Dictionary<string, object?> { ["city"] = "Seoul" });
        var messages = new List<ChatMessage>
        {
            new(ChatRole.Assistant, [functionCall])
        };

        await _adapter.GetResponseAsync(messages);

        var req = capturedRequest();
        var assistantMsg = req.Messages.First().Should().BeOfType<AssistantMessage>().Subject;
        var toolContent = assistantMsg.Content.Should().ContainSingle()
            .Which.Should().BeOfType<ToolMessageContent>().Subject;
        toolContent.Id.Should().Be("call-1");
        toolContent.Name.Should().Be("get_weather");
        toolContent.IsApproved.Should().BeTrue();
    }

    [Fact]
    public async Task GetResponseAsync_NonImageDataContent_Ignored()
    {
        var capturedRequest = SetupGeneratorReturns();
        var dataContent = new DataContent(new byte[] { 1, 2 }, "application/pdf");
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, [new TextContent("text"), dataContent])
        };

        await _adapter.GetResponseAsync(messages);

        var req = capturedRequest();
        var userMsg = req.Messages.First().Should().BeOfType<UserMessage>().Subject;
        userMsg.Content.Should().ContainSingle()
            .Which.Should().BeOfType<TextMessageContent>();
    }

    #endregion

    #region GetResponseAsync — Response conversion

    [Fact]
    public async Task GetResponseAsync_TextResponse_ConvertsCorrectly()
    {
        SetupGeneratorReturns(new MessageResponse
        {
            Id = "resp-1",
            DoneReason = MessageDoneReason.EndTurn,
            Message = new AssistantMessage
            {
                Model = "test-model",
                Content = { new TextMessageContent { Value = "Hello back!" } }
            },
            TokenUsage = new MessageTokenUsage
            {
                InputTokens = 10,
                OutputTokens = 5
            }
        });

        var messages = new List<ChatMessage> { new(ChatRole.User, "Hi") };
        var response = await _adapter.GetResponseAsync(messages);

        response.ResponseId.Should().Be("resp-1");
        response.FinishReason.Should().Be(ChatFinishReason.Stop);
        response.ModelId.Should().Be("test-model");
        response.Usage.Should().NotBeNull();
        response.Usage!.InputTokenCount.Should().Be(10);
        response.Usage.OutputTokenCount.Should().Be(5);
        response.Usage.TotalTokenCount.Should().Be(15);
    }

    [Fact]
    public async Task GetResponseAsync_ToolCallResponse_ConvertsFunctionCallContent()
    {
        SetupGeneratorReturns(new MessageResponse
        {
            Id = "resp-2",
            DoneReason = MessageDoneReason.ToolCall,
            Message = new AssistantMessage
            {
                Content =
                {
                    new ToolMessageContent
                    {
                        IsApproved = true,
                        Id = "tool-1",
                        Name = "search",
                        Input = "{\"query\":\"test\"}"
                    }
                }
            }
        });

        var messages = new List<ChatMessage> { new(ChatRole.User, "Search for test") };
        var response = await _adapter.GetResponseAsync(messages);

        response.FinishReason.Should().Be(ChatFinishReason.ToolCalls);
        var msg = response.Messages.First();
        msg.Role.Should().Be(ChatRole.Assistant);
        var fc = msg.Contents.OfType<FunctionCallContent>().Single();
        fc.CallId.Should().Be("tool-1");
        fc.Name.Should().Be("search");
    }

    [Fact]
    public async Task GetResponseAsync_NullTokenUsage_UsageIsNull()
    {
        SetupGeneratorReturns(new MessageResponse
        {
            Id = "resp-3",
            Message = new AssistantMessage
            {
                Content = { new TextMessageContent { Value = "ok" } }
            },
            TokenUsage = null
        });

        var messages = new List<ChatMessage> { new(ChatRole.User, "Hello") };
        var response = await _adapter.GetResponseAsync(messages);

        response.Usage.Should().BeNull();
    }

    #endregion

    #region GetResponseAsync — DoneReason mapping

    [Theory]
    [InlineData(MessageDoneReason.EndTurn, "stop")]
    [InlineData(MessageDoneReason.StopSequence, "stop")]
    [InlineData(MessageDoneReason.MaxTokens, "length")]
    [InlineData(MessageDoneReason.ToolCall, "tool_calls")]
    [InlineData(MessageDoneReason.ContentFilter, "content_filter")]
    public async Task GetResponseAsync_DoneReason_MapsCorrectly(MessageDoneReason reason, string expectedValue)
    {
        SetupGeneratorReturns(new MessageResponse
        {
            Id = "resp",
            DoneReason = reason,
            Message = new AssistantMessage
            {
                Content = { new TextMessageContent { Value = "ok" } }
            }
        });

        var messages = new List<ChatMessage> { new(ChatRole.User, "Hi") };
        var response = await _adapter.GetResponseAsync(messages);

        response.FinishReason.Should().NotBeNull();
        response.FinishReason!.Value.Value.Should().Be(expectedValue);
    }

    [Fact]
    public async Task GetResponseAsync_NullDoneReason_NullFinishReason()
    {
        SetupGeneratorReturns(new MessageResponse
        {
            Id = "resp",
            DoneReason = null,
            Message = new AssistantMessage
            {
                Content = { new TextMessageContent { Value = "ok" } }
            }
        });

        var messages = new List<ChatMessage> { new(ChatRole.User, "Hi") };
        var response = await _adapter.GetResponseAsync(messages);

        response.FinishReason.Should().BeNull();
    }

    #endregion

    #region GetResponseAsync — ImageFormat mapping

    [Theory]
    [InlineData("image/png", ImageFormat.Png)]
    [InlineData("image/gif", ImageFormat.Gif)]
    [InlineData("image/webp", ImageFormat.Webp)]
    [InlineData("image/jpeg", ImageFormat.Jpeg)]
    [InlineData("image/unknown", ImageFormat.Jpeg)]
    public async Task GetResponseAsync_ImageMediaType_MapsToCorrectFormat(
        string mediaType, ImageFormat expectedFormat)
    {
        var capturedRequest = SetupGeneratorReturns();
        var imageContent = new DataContent(new byte[] { 1 }, mediaType);
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, [imageContent])
        };

        await _adapter.GetResponseAsync(messages);

        var req = capturedRequest();
        var userMsg = req.Messages.First().Should().BeOfType<UserMessage>().Subject;
        var imgContent = userMsg.Content.First().Should().BeOfType<ImageMessageContent>().Subject;
        imgContent.Format.Should().Be(expectedFormat);
    }

    #endregion

    #region GetStreamingResponseAsync

    [Fact]
    public async Task GetStreamingResponseAsync_BeginChunk_ReturnsNothing()
    {
        var chunks = new List<StreamingMessageResponse>
        {
            new StreamingMessageBeginResponse { Id = "stream-1" }
        };
        SetupStreamingGenerator(chunks);

        var messages = new List<ChatMessage> { new(ChatRole.User, "Hi") };
        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in _adapter.GetStreamingResponseAsync(messages))
        {
            updates.Add(update);
        }

        updates.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStreamingResponseAsync_TextDelta_ReturnsTextContent()
    {
        var chunks = new List<StreamingMessageResponse>
        {
            new StreamingMessageBeginResponse { Id = "stream-1" },
            new StreamingContentDeltaResponse
            {
                Index = 0,
                Delta = new TextDeltaContent { Value = "Hello " }
            },
            new StreamingContentDeltaResponse
            {
                Index = 0,
                Delta = new TextDeltaContent { Value = "World" }
            }
        };
        SetupStreamingGenerator(chunks);

        var messages = new List<ChatMessage> { new(ChatRole.User, "Hi") };
        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in _adapter.GetStreamingResponseAsync(messages))
        {
            updates.Add(update);
        }

        updates.Should().HaveCount(2);
        updates[0].ResponseId.Should().Be("stream-1");
        updates[0].Contents.OfType<TextContent>().Single().Text.Should().Be("Hello ");
        updates[1].Contents.OfType<TextContent>().Single().Text.Should().Be("World");
    }

    [Fact]
    public async Task GetStreamingResponseAsync_DoneChunk_ReturnsFinalUpdate()
    {
        var timestamp = new DateTime(2026, 2, 18, 12, 0, 0, DateTimeKind.Utc);
        var chunks = new List<StreamingMessageResponse>
        {
            new StreamingMessageBeginResponse { Id = "stream-1" },
            new StreamingMessageDoneResponse
            {
                Id = "stream-1",
                DoneReason = MessageDoneReason.EndTurn,
                Model = "test-model",
                Timestamp = timestamp
            }
        };
        SetupStreamingGenerator(chunks);

        var messages = new List<ChatMessage> { new(ChatRole.User, "Hi") };
        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in _adapter.GetStreamingResponseAsync(messages))
        {
            updates.Add(update);
        }

        updates.Should().ContainSingle();
        var done = updates[0];
        done.ResponseId.Should().Be("stream-1");
        done.FinishReason.Should().Be(ChatFinishReason.Stop);
        done.ModelId.Should().Be("test-model");
        done.CreatedAt.Should().Be(timestamp);
    }

    [Fact]
    public async Task GetStreamingResponseAsync_NonTextDelta_ReturnsEmptyContents()
    {
        var chunks = new List<StreamingMessageResponse>
        {
            new StreamingMessageBeginResponse { Id = "stream-1" },
            new StreamingContentDeltaResponse
            {
                Index = 0,
                Delta = new ToolDeltaContent { Input = "{\"key\":" }
            }
        };
        SetupStreamingGenerator(chunks);

        var messages = new List<ChatMessage> { new(ChatRole.User, "Hi") };
        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in _adapter.GetStreamingResponseAsync(messages))
        {
            updates.Add(update);
        }

        updates.Should().ContainSingle();
        updates[0].Contents.OfType<TextContent>().Should().BeEmpty();
    }

    [Fact]
    public async Task GetStreamingResponseAsync_UnknownChunkType_Skipped()
    {
        var chunks = new List<StreamingMessageResponse>
        {
            new StreamingMessageBeginResponse { Id = "stream-1" },
            new StreamingContentAddedResponse
            {
                Index = 0,
                Content = new TextMessageContent { Value = "added" }
            }
        };
        SetupStreamingGenerator(chunks);

        var messages = new List<ChatMessage> { new(ChatRole.User, "Hi") };
        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in _adapter.GetStreamingResponseAsync(messages))
        {
            updates.Add(update);
        }

        updates.Should().BeEmpty();
    }

    #endregion

    #region Helpers

    private Func<MessageGenerationRequest> SetupGeneratorReturns(MessageResponse? response = null)
    {
        MessageGenerationRequest? captured = null;
        var defaultResponse = response ?? new MessageResponse
        {
            Id = "default-resp",
            Message = new AssistantMessage
            {
                Content = { new TextMessageContent { Value = "default" } }
            }
        };

        _mockGenerator
            .GenerateMessageAsync(Arg.Do<MessageGenerationRequest>(r => captured = r), Arg.Any<CancellationToken>())
            .Returns(defaultResponse);

        return () => captured!;
    }

    private void SetupStreamingGenerator(IEnumerable<StreamingMessageResponse> chunks)
    {
        _mockGenerator
            .GenerateStreamingMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(ToAsyncEnumerable(chunks));
    }

    private static async IAsyncEnumerable<StreamingMessageResponse> ToAsyncEnumerable(
        IEnumerable<StreamingMessageResponse> items)
    {
        foreach (var item in items)
        {
            yield return item;
            await Task.CompletedTask;
        }
    }

    #endregion
}
