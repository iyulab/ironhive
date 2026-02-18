using System.Runtime.CompilerServices;
using FluentAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Streaming;
using IronHive.Core.Streaming;
using NSubstitute;

namespace IronHive.Tests.Streaming;

public class ResumableStreamingGeneratorTests
{
    private readonly IMessageGenerator _generator;
    private readonly IStreamStateManager _stateManager;

    public ResumableStreamingGeneratorTests()
    {
        _generator = Substitute.For<IMessageGenerator>();
        _stateManager = Substitute.For<IStreamStateManager>();
    }

    #region Constructor

    [Fact]
    public void Constructor_NullGenerator_ThrowsArgumentNullException()
    {
        var act = () => new ResumableStreamingGenerator(null!, _stateManager);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("generator");
    }

    [Fact]
    public void Constructor_NullStateManager_ThrowsArgumentNullException()
    {
        var act = () => new ResumableStreamingGenerator(_generator, null!);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("stateManager");
    }

    [Fact]
    public void Constructor_NullOptions_UsesDefaults()
    {
        var sut = new ResumableStreamingGenerator(_generator, _stateManager, options: null);

        sut.Should().NotBeNull();
    }

    #endregion

    #region ResumeStreamAsync

    [Fact]
    public async Task ResumeStreamAsync_StateNotFound_ReturnsNotFound()
    {
        _stateManager.GetStateAsync("unknown-id", Arg.Any<CancellationToken>())
            .Returns((IStreamState?)null);

        var sut = new ResumableStreamingGenerator(_generator, _stateManager);

        var result = await sut.ResumeStreamAsync("unknown-id");

        result.IsSuccess.Should().BeFalse();
        result.FailureReason.Should().Be(StreamResumeFailureReason.NotFound);
        result.StreamId.Should().Be("unknown-id");
    }

    [Fact]
    public async Task ResumeStreamAsync_CannotResume_ReturnsInvalidState()
    {
        var state = CreateMockState("stream-1", StreamStatus.Completed, canResume: false);
        _stateManager.GetStateAsync("stream-1", Arg.Any<CancellationToken>())
            .Returns(state);

        var sut = new ResumableStreamingGenerator(_generator, _stateManager);

        var result = await sut.ResumeStreamAsync("stream-1");

        result.IsSuccess.Should().BeFalse();
        result.FailureReason.Should().Be(StreamResumeFailureReason.InvalidState);
        result.ErrorMessage.Should().Contain("Completed");
    }

    [Fact]
    public async Task ResumeStreamAsync_ResumeWindowExpired_ReturnsExpired()
    {
        var state = CreateMockState("stream-2", StreamStatus.Disconnected, canResume: true,
            lastUpdatedAt: DateTime.UtcNow.AddMinutes(-10));
        _stateManager.GetStateAsync("stream-2", Arg.Any<CancellationToken>())
            .Returns(state);

        var options = new StreamStateOptions { ResumeWindow = TimeSpan.FromMinutes(5) };
        var sut = new ResumableStreamingGenerator(_generator, _stateManager, options);

        var result = await sut.ResumeStreamAsync("stream-2");

        result.IsSuccess.Should().BeFalse();
        result.FailureReason.Should().Be(StreamResumeFailureReason.Expired);
        await _stateManager.Received(1).UpdateStatusAsync(
            "stream-2", StreamStatus.Expired,
            Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResumeStreamAsync_WithinWindow_ReturnsSuccess()
    {
        var state = CreateMockState("stream-3", StreamStatus.Paused, canResume: true,
            lastUpdatedAt: DateTime.UtcNow.AddSeconds(-30),
            accumulatedContent: "Hello world",
            lastChunkIndex: 5,
            totalChunksReceived: 6);
        _stateManager.GetStateAsync("stream-3", Arg.Any<CancellationToken>())
            .Returns(state);

        var options = new StreamStateOptions { ResumeWindow = TimeSpan.FromMinutes(5) };
        var sut = new ResumableStreamingGenerator(_generator, _stateManager, options);

        var result = await sut.ResumeStreamAsync("stream-3");

        result.IsSuccess.Should().BeTrue();
        result.StreamId.Should().Be("stream-3");
        result.AccumulatedContent.Should().Be("Hello world");
        result.LastChunkIndex.Should().Be(5);
        result.TotalChunksReceived.Should().Be(6);
    }

    [Fact]
    public async Task ResumeStreamAsync_DisconnectedWithinWindow_ReturnsSuccess()
    {
        var state = CreateMockState("stream-4", StreamStatus.Disconnected, canResume: true,
            lastUpdatedAt: DateTime.UtcNow.AddSeconds(-10),
            accumulatedContent: "partial content");
        _stateManager.GetStateAsync("stream-4", Arg.Any<CancellationToken>())
            .Returns(state);

        var sut = new ResumableStreamingGenerator(_generator, _stateManager,
            new StreamStateOptions { ResumeWindow = TimeSpan.FromMinutes(5) });

        var result = await sut.ResumeStreamAsync("stream-4");

        result.IsSuccess.Should().BeTrue();
        result.AccumulatedContent.Should().Be("partial content");
    }

    #endregion

    #region GetResumableStreamsAsync

    [Fact]
    public async Task GetResumableStreamsAsync_NoStreams_ReturnsEmpty()
    {
        _stateManager.GetResumableStreamsAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<string>());

        var sut = new ResumableStreamingGenerator(_generator, _stateManager);

        var streams = await sut.GetResumableStreamsAsync();

        streams.Should().BeEmpty();
    }

    [Fact]
    public async Task GetResumableStreamsAsync_MultipleStreams_ReturnsAll()
    {
        var ids = new List<string> { "s1", "s2", "s3" };
        _stateManager.GetResumableStreamsAsync(Arg.Any<CancellationToken>())
            .Returns(ids);

        var state1 = CreateMockState("s1", StreamStatus.Paused, canResume: true);
        var state2 = CreateMockState("s2", StreamStatus.Disconnected, canResume: true);
        var state3 = CreateMockState("s3", StreamStatus.Paused, canResume: true);
        _stateManager.GetStateAsync("s1", Arg.Any<CancellationToken>()).Returns(state1);
        _stateManager.GetStateAsync("s2", Arg.Any<CancellationToken>()).Returns(state2);
        _stateManager.GetStateAsync("s3", Arg.Any<CancellationToken>()).Returns(state3);

        var sut = new ResumableStreamingGenerator(_generator, _stateManager);

        var streams = await sut.GetResumableStreamsAsync();

        streams.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetResumableStreamsAsync_SomeNullStates_FiltersOut()
    {
        _stateManager.GetResumableStreamsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<string> { "exists", "gone" });

        var existsState = CreateMockState("exists", StreamStatus.Paused, canResume: true);
        _stateManager.GetStateAsync("exists", Arg.Any<CancellationToken>()).Returns(existsState);
        _stateManager.GetStateAsync("gone", Arg.Any<CancellationToken>()).Returns((IStreamState?)null);

        var sut = new ResumableStreamingGenerator(_generator, _stateManager);

        var streams = await sut.GetResumableStreamsAsync();

        streams.Should().HaveCount(1);
        streams[0].StreamId.Should().Be("exists");
    }

    #endregion

    #region StartStreamingAsync

    [Fact]
    public async Task StartStreamingAsync_SuccessfulStreaming_YieldsChunksAndCompletes()
    {
        var state = CreateMockState("new-stream", StreamStatus.Pending, canResume: false);
        _stateManager.CreateStateAsync(Arg.Any<string?>(), Arg.Any<IDictionary<string, object>?>(), Arg.Any<CancellationToken>())
            .Returns(state);

        _generator.GenerateStreamingMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerableOf(
                new StreamingContentDeltaResponse
                {
                    Index = 0,
                    Delta = new TextDeltaContent { Value = "Hello " }
                },
                new StreamingContentDeltaResponse
                {
                    Index = 1,
                    Delta = new TextDeltaContent { Value = "World" }
                }));

        var sut = new ResumableStreamingGenerator(_generator, _stateManager);
        var request = new MessageGenerationRequest { Model = "test-model" };

        var chunks = new List<ResumableStreamChunk>();
        await foreach (var chunk in sut.StartStreamingAsync(request))
        {
            chunks.Add(chunk);
        }

        chunks.Should().HaveCount(2);
        chunks[0].StreamId.Should().Be("new-stream");
        chunks[0].ChunkIndex.Should().Be(0);
        chunks[1].ChunkIndex.Should().Be(1);

        await _stateManager.Received(2).AppendChunkAsync(
            "new-stream", Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _stateManager.Received(1).UpdateStatusAsync(
            "new-stream", StreamStatus.Completed, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartStreamingAsync_Cancellation_MarksDisconnected()
    {
        var state = CreateMockState("cancel-stream", StreamStatus.Pending, canResume: false);
        _stateManager.CreateStateAsync(Arg.Any<string?>(), Arg.Any<IDictionary<string, object>?>(), Arg.Any<CancellationToken>())
            .Returns(state);

        using var cts = new CancellationTokenSource();

        _generator.GenerateStreamingMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var ct = callInfo.ArgAt<CancellationToken>(1);
                return CancellingAsyncEnumerable(cts, ct);
            });

        var sut = new ResumableStreamingGenerator(_generator, _stateManager);
        var request = new MessageGenerationRequest { Model = "test-model" };

        var chunks = new List<ResumableStreamChunk>();
        try
        {
            await foreach (var chunk in sut.StartStreamingAsync(request, cts.Token))
            {
                chunks.Add(chunk);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Wait for background processing to complete
        await Task.Delay(500);

        await _stateManager.Received(1).MarkAsDisconnectedAsync(
            "cancel-stream", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartStreamingAsync_GeneratorThrows_MarksDisconnectedWithError()
    {
        var state = CreateMockState("error-stream", StreamStatus.Pending, canResume: false);
        _stateManager.CreateStateAsync(Arg.Any<string?>(), Arg.Any<IDictionary<string, object>?>(), Arg.Any<CancellationToken>())
            .Returns(state);

        _generator.GenerateStreamingMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(ThrowingAsyncEnumerable("Generator failed"));

        var sut = new ResumableStreamingGenerator(_generator, _stateManager);
        var request = new MessageGenerationRequest { Model = "test-model" };

        var chunks = new List<ResumableStreamChunk>();
        await foreach (var chunk in sut.StartStreamingAsync(request))
        {
            chunks.Add(chunk);
        }

        // Wait briefly for background processing to complete
        await Task.Delay(100);

        await _stateManager.Received(1).UpdateStatusAsync(
            "error-stream", StreamStatus.Disconnected, "Generator failed", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartStreamingAsync_NonTextDelta_DoesNotAppendChunk()
    {
        var state = CreateMockState("mixed-stream", StreamStatus.Pending, canResume: false);
        _stateManager.CreateStateAsync(Arg.Any<string?>(), Arg.Any<IDictionary<string, object>?>(), Arg.Any<CancellationToken>())
            .Returns(state);

        _generator.GenerateStreamingMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerableOf(
                new StreamingContentDeltaResponse
                {
                    Index = 0,
                    Delta = new ToolDeltaContent { Input = "tool call data" }
                }));

        var sut = new ResumableStreamingGenerator(_generator, _stateManager);
        var request = new MessageGenerationRequest { Model = "test-model" };

        var chunks = new List<ResumableStreamChunk>();
        await foreach (var chunk in sut.StartStreamingAsync(request))
        {
            chunks.Add(chunk);
        }

        chunks.Should().HaveCount(1);
        // AppendChunkAsync should NOT have been called since delta is not TextDeltaContent
        await _stateManager.DidNotReceive().AppendChunkAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region StreamResumeResult

    [Fact]
    public void StreamResumeResult_Success_HasCorrectProperties()
    {
        var result = StreamResumeResult.Success("s1", "accumulated", 10, 11);

        result.IsSuccess.Should().BeTrue();
        result.StreamId.Should().Be("s1");
        result.AccumulatedContent.Should().Be("accumulated");
        result.LastChunkIndex.Should().Be(10);
        result.TotalChunksReceived.Should().Be(11);
        result.FailureReason.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void StreamResumeResult_NotFound_HasCorrectProperties()
    {
        var result = StreamResumeResult.NotFound("missing-id");

        result.IsSuccess.Should().BeFalse();
        result.StreamId.Should().Be("missing-id");
        result.FailureReason.Should().Be(StreamResumeFailureReason.NotFound);
        result.ErrorMessage.Should().Contain("missing-id");
    }

    [Fact]
    public void StreamResumeResult_CannotResume_HasCorrectProperties()
    {
        var result = StreamResumeResult.CannotResume("s2", StreamStatus.Completed, "Stream completed");

        result.IsSuccess.Should().BeFalse();
        result.FailureReason.Should().Be(StreamResumeFailureReason.InvalidState);
        result.ErrorMessage.Should().Be("Stream completed");
    }

    [Fact]
    public void StreamResumeResult_Expired_HasCorrectProperties()
    {
        var elapsed = TimeSpan.FromMinutes(15);
        var result = StreamResumeResult.Expired("s3", elapsed);

        result.IsSuccess.Should().BeFalse();
        result.FailureReason.Should().Be(StreamResumeFailureReason.Expired);
        result.ErrorMessage.Should().Contain("15.0");
    }

    #endregion

    #region Helpers

    private static IStreamState CreateMockState(
        string streamId,
        StreamStatus status,
        bool canResume,
        DateTime? lastUpdatedAt = null,
        string accumulatedContent = "",
        int lastChunkIndex = 0,
        int totalChunksReceived = 0)
    {
        var mock = Substitute.For<IStreamState>();
        mock.StreamId.Returns(streamId);
        mock.Status.Returns(status);
        mock.CanResume.Returns(canResume);
        mock.LastUpdatedAt.Returns(lastUpdatedAt ?? DateTime.UtcNow);
        mock.AccumulatedContent.Returns(accumulatedContent);
        mock.LastChunkIndex.Returns(lastChunkIndex);
        mock.TotalChunksReceived.Returns(totalChunksReceived);
        return mock;
    }

    private static async IAsyncEnumerable<StreamingMessageResponse> AsyncEnumerableOf(
        params StreamingMessageResponse[] items)
    {
        foreach (var item in items)
        {
            yield return item;
        }
        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<StreamingMessageResponse> CancellingAsyncEnumerable(
        CancellationTokenSource cts,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        yield return new StreamingContentDeltaResponse
        {
            Index = 0,
            Delta = new TextDeltaContent { Value = "partial" }
        };
        await Task.Yield();
        cts.Cancel();
        // Check the token that was passed through the async enumerable
        cts.Token.ThrowIfCancellationRequested();
        yield break;
    }

    private static async IAsyncEnumerable<StreamingMessageResponse> ThrowingAsyncEnumerable(
        string errorMessage)
    {
        await Task.Yield();
        throw new InvalidOperationException(errorMessage);
#pragma warning disable CS0162 // Unreachable code detected
        yield break;
#pragma warning restore CS0162
    }

    #endregion
}
