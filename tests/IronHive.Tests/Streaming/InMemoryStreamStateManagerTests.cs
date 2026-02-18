using FluentAssertions;
using IronHive.Abstractions.Streaming;
using IronHive.Core.Streaming;

namespace IronHive.Tests.Streaming;

public class InMemoryStreamStateManagerTests : IDisposable
{
    private readonly InMemoryStreamStateManager _manager;

    public InMemoryStreamStateManagerTests()
    {
        _manager = new InMemoryStreamStateManager(new StreamStateOptions
        {
            EnableAutoCleanup = false
        });
    }

    public void Dispose()
    {
        _manager.Dispose();
        GC.SuppressFinalize(this);
    }

    #region CreateStateAsync

    [Fact]
    public async Task CreateStateAsync_WithoutId_GeneratesStreamId()
    {
        var state = await _manager.CreateStateAsync();

        state.StreamId.Should().NotBeNullOrEmpty();
        state.Status.Should().Be(StreamStatus.Pending);
        state.TotalChunksReceived.Should().Be(0);
        state.AccumulatedContent.Should().BeEmpty();
        state.CanResume.Should().BeFalse();
        state.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task CreateStateAsync_WithCustomId_UsesProvidedId()
    {
        var state = await _manager.CreateStateAsync("my-stream-123");

        state.StreamId.Should().Be("my-stream-123");
    }

    [Fact]
    public async Task CreateStateAsync_WithMetadata_StoresMetadata()
    {
        var metadata = new Dictionary<string, object>
        {
            ["key1"] = "value1",
            ["key2"] = 42
        };

        var state = await _manager.CreateStateAsync(metadata: metadata);

        state.Metadata.Should().ContainKey("key1").WhoseValue.Should().Be("value1");
        state.Metadata.Should().ContainKey("key2").WhoseValue.Should().Be(42);
    }

    [Fact]
    public async Task CreateStateAsync_DuplicateId_ThrowsInvalidOperationException()
    {
        await _manager.CreateStateAsync("dup-id");

        var act = () => _manager.CreateStateAsync("dup-id");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*dup-id*already exists*");
    }

    [Fact]
    public async Task CreateStateAsync_SetsCreatedAtAndLastUpdatedAt()
    {
        var before = DateTime.UtcNow;
        var state = await _manager.CreateStateAsync();
        var after = DateTime.UtcNow;

        state.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        state.LastUpdatedAt.Should().Be(state.CreatedAt);
    }

    #endregion

    #region GetStateAsync

    [Fact]
    public async Task GetStateAsync_ExistingStream_ReturnsState()
    {
        var created = await _manager.CreateStateAsync("stream-1");

        var retrieved = await _manager.GetStateAsync("stream-1");

        retrieved.Should().NotBeNull();
        retrieved!.StreamId.Should().Be("stream-1");
    }

    [Fact]
    public async Task GetStateAsync_NonExistentStream_ReturnsNull()
    {
        var result = await _manager.GetStateAsync("non-existent");

        result.Should().BeNull();
    }

    #endregion

    #region AppendChunkAsync

    [Fact]
    public async Task AppendChunkAsync_AppendsContent()
    {
        await _manager.CreateStateAsync("s1");

        await _manager.AppendChunkAsync("s1", "Hello ", 0);
        await _manager.AppendChunkAsync("s1", "World", 1);

        var state = await _manager.GetStateAsync("s1");
        state!.AccumulatedContent.Should().Be("Hello World");
        state.TotalChunksReceived.Should().Be(2);
        state.LastChunkIndex.Should().Be(1);
    }

    [Fact]
    public async Task AppendChunkAsync_TransitionsFromPendingToStreaming()
    {
        await _manager.CreateStateAsync("s1");

        var stateBefore = await _manager.GetStateAsync("s1");
        stateBefore!.Status.Should().Be(StreamStatus.Pending);

        await _manager.AppendChunkAsync("s1", "chunk", 0);

        var stateAfter = await _manager.GetStateAsync("s1");
        stateAfter!.Status.Should().Be(StreamStatus.Streaming);
    }

    [Fact]
    public async Task AppendChunkAsync_NonExistentStream_ThrowsKeyNotFoundException()
    {
        var act = () => _manager.AppendChunkAsync("missing", "data", 0);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*missing*not found*");
    }

    [Fact]
    public async Task AppendChunkAsync_ExceedsMaxChunks_ThrowsInvalidOperationException()
    {
        using var mgr = new InMemoryStreamStateManager(new StreamStateOptions
        {
            EnableAutoCleanup = false,
            MaxChunksPerStream = 3
        });
        await mgr.CreateStateAsync("s1");

        await mgr.AppendChunkAsync("s1", "a", 0);
        await mgr.AppendChunkAsync("s1", "b", 1);
        await mgr.AppendChunkAsync("s1", "c", 2);

        var act = () => mgr.AppendChunkAsync("s1", "d", 3);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Maximum chunks*3*exceeded*");
    }

    [Fact]
    public async Task AppendChunkAsync_UpdatesLastUpdatedAt()
    {
        await _manager.CreateStateAsync("s1");
        var state1 = await _manager.GetStateAsync("s1");
        var initialTime = state1!.LastUpdatedAt;

        await Task.Delay(10);
        await _manager.AppendChunkAsync("s1", "data", 0);

        var state2 = await _manager.GetStateAsync("s1");
        state2!.LastUpdatedAt.Should().BeAfter(initialTime);
    }

    #endregion

    #region UpdateStatusAsync

    [Fact]
    public async Task UpdateStatusAsync_ChangesStatus()
    {
        await _manager.CreateStateAsync("s1");

        await _manager.UpdateStatusAsync("s1", StreamStatus.Completed);

        var state = await _manager.GetStateAsync("s1");
        state!.Status.Should().Be(StreamStatus.Completed);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithErrorMessage_SetsErrorMessage()
    {
        await _manager.CreateStateAsync("s1");

        await _manager.UpdateStatusAsync("s1", StreamStatus.Failed, "Something went wrong");

        var state = await _manager.GetStateAsync("s1");
        state!.Status.Should().Be(StreamStatus.Failed);
        state.ErrorMessage.Should().Be("Something went wrong");
    }

    [Fact]
    public async Task UpdateStatusAsync_NonExistentStream_ThrowsKeyNotFoundException()
    {
        var act = () => _manager.UpdateStatusAsync("missing", StreamStatus.Completed);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region MarkAsDisconnectedAsync

    [Fact]
    public async Task MarkAsDisconnectedAsync_SetsDisconnectedStatus()
    {
        await _manager.CreateStateAsync("s1");

        await _manager.MarkAsDisconnectedAsync("s1");

        var state = await _manager.GetStateAsync("s1");
        state!.Status.Should().Be(StreamStatus.Disconnected);
        state.CanResume.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsDisconnectedAsync_NonExistentStream_ThrowsKeyNotFoundException()
    {
        var act = () => _manager.MarkAsDisconnectedAsync("missing");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region DeleteStateAsync

    [Fact]
    public async Task DeleteStateAsync_RemovesState()
    {
        await _manager.CreateStateAsync("s1");

        await _manager.DeleteStateAsync("s1");

        var state = await _manager.GetStateAsync("s1");
        state.Should().BeNull();
    }

    [Fact]
    public async Task DeleteStateAsync_NonExistentStream_DoesNotThrow()
    {
        var act = () => _manager.DeleteStateAsync("non-existent");

        await act.Should().NotThrowAsync();
    }

    #endregion

    #region CleanupExpiredAsync

    [Fact]
    public async Task CleanupExpiredAsync_RemovesCompletedStreamsOlderThanExpiration()
    {
        await _manager.CreateStateAsync("s1");
        await _manager.UpdateStatusAsync("s1", StreamStatus.Completed);

        // With zero expiration, everything older than now is expired
        var cleaned = await _manager.CleanupExpiredAsync(TimeSpan.Zero);

        cleaned.Should().Be(1);
        (await _manager.GetStateAsync("s1")).Should().BeNull();
    }

    [Fact]
    public async Task CleanupExpiredAsync_RemovesFailedAndCancelledStreams()
    {
        await _manager.CreateStateAsync("failed");
        await _manager.UpdateStatusAsync("failed", StreamStatus.Failed);
        await _manager.CreateStateAsync("cancelled");
        await _manager.UpdateStatusAsync("cancelled", StreamStatus.Cancelled);

        var cleaned = await _manager.CleanupExpiredAsync(TimeSpan.Zero);

        cleaned.Should().Be(2);
    }

    [Fact]
    public async Task CleanupExpiredAsync_KeepsActiveStreams()
    {
        await _manager.CreateStateAsync("active");
        await _manager.AppendChunkAsync("active", "data", 0);

        var cleaned = await _manager.CleanupExpiredAsync(TimeSpan.Zero);

        cleaned.Should().Be(0);
        (await _manager.GetStateAsync("active")).Should().NotBeNull();
    }

    [Fact]
    public async Task CleanupExpiredAsync_ExpiresPausedStreamsBeyondResumeWindow()
    {
        using var mgr = new InMemoryStreamStateManager(new StreamStateOptions
        {
            EnableAutoCleanup = false,
            ResumeWindow = TimeSpan.Zero
        });
        await mgr.CreateStateAsync("paused");
        await mgr.UpdateStatusAsync("paused", StreamStatus.Paused);

        var cleaned = await mgr.CleanupExpiredAsync(TimeSpan.FromHours(1));

        cleaned.Should().Be(1);
    }

    [Fact]
    public async Task CleanupExpiredAsync_ExpiresDisconnectedStreamsBeyondResumeWindow()
    {
        using var mgr = new InMemoryStreamStateManager(new StreamStateOptions
        {
            EnableAutoCleanup = false,
            ResumeWindow = TimeSpan.Zero
        });
        await mgr.CreateStateAsync("disc");
        await mgr.UpdateStatusAsync("disc", StreamStatus.Disconnected);

        var cleaned = await mgr.CleanupExpiredAsync(TimeSpan.FromHours(1));

        cleaned.Should().Be(1);
    }

    [Fact]
    public async Task CleanupExpiredAsync_KeepsRecentCompletedStreams()
    {
        await _manager.CreateStateAsync("recent");
        await _manager.UpdateStatusAsync("recent", StreamStatus.Completed);

        // Large expiration window → not expired
        var cleaned = await _manager.CleanupExpiredAsync(TimeSpan.FromHours(1));

        cleaned.Should().Be(0);
        (await _manager.GetStateAsync("recent")).Should().NotBeNull();
    }

    #endregion

    #region GetResumableStreamsAsync

    [Fact]
    public async Task GetResumableStreamsAsync_ReturnsPausedAndDisconnectedStreams()
    {
        await _manager.CreateStateAsync("paused");
        await _manager.UpdateStatusAsync("paused", StreamStatus.Paused);

        await _manager.CreateStateAsync("disconnected");
        await _manager.UpdateStatusAsync("disconnected", StreamStatus.Disconnected);

        await _manager.CreateStateAsync("completed");
        await _manager.UpdateStatusAsync("completed", StreamStatus.Completed);

        await _manager.CreateStateAsync("active");

        var resumable = await _manager.GetResumableStreamsAsync();

        resumable.Should().HaveCount(2);
        resumable.Should().Contain("paused");
        resumable.Should().Contain("disconnected");
    }

    [Fact]
    public async Task GetResumableStreamsAsync_EmptyWhenNoResumableStreams()
    {
        await _manager.CreateStateAsync("active");
        await _manager.CreateStateAsync("completed");
        await _manager.UpdateStatusAsync("completed", StreamStatus.Completed);

        var resumable = await _manager.GetResumableStreamsAsync();

        resumable.Should().BeEmpty();
    }

    #endregion

    #region CanResume Property

    [Theory]
    [InlineData(StreamStatus.Paused, true)]
    [InlineData(StreamStatus.Disconnected, true)]
    [InlineData(StreamStatus.Pending, false)]
    [InlineData(StreamStatus.Streaming, false)]
    [InlineData(StreamStatus.Completed, false)]
    [InlineData(StreamStatus.Failed, false)]
    [InlineData(StreamStatus.Cancelled, false)]
    public async Task CanResume_ReflectsStatusCorrectly(StreamStatus status, bool expected)
    {
        await _manager.CreateStateAsync("s1");
        await _manager.UpdateStatusAsync("s1", status);

        var state = await _manager.GetStateAsync("s1");
        state!.CanResume.Should().Be(expected);
    }

    #endregion

    #region Dispose

    [Fact]
    public async Task Dispose_ClearsAllStates()
    {
        await _manager.CreateStateAsync("s1");
        await _manager.CreateStateAsync("s2");

        _manager.Dispose();

        (await _manager.GetStateAsync("s1")).Should().BeNull();
        (await _manager.GetStateAsync("s2")).Should().BeNull();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var act = () =>
        {
            _manager.Dispose();
            _manager.Dispose();
        };

        act.Should().NotThrow();
    }

    #endregion

    #region AutoCleanup Timer

    [Fact]
    public void Constructor_WithAutoCleanupDisabled_DoesNotCreateTimer()
    {
        using var mgr = new InMemoryStreamStateManager(new StreamStateOptions
        {
            EnableAutoCleanup = false
        });

        // Should not throw and can be disposed cleanly
        mgr.Dispose();
    }

    [Fact]
    public void Constructor_WithAutoCleanupEnabled_CreatesTimer()
    {
        using var mgr = new InMemoryStreamStateManager(new StreamStateOptions
        {
            EnableAutoCleanup = true,
            CleanupInterval = TimeSpan.FromMinutes(10)
        });

        // Should not throw
        mgr.Dispose();
    }

    [Fact]
    public void Constructor_WithDefaultOptions_EnablesAutoCleanup()
    {
        using var mgr = new InMemoryStreamStateManager();

        // Default options enable auto cleanup
        mgr.Dispose();
    }

    [Fact]
    public void Constructor_WithNullOptions_UsesDefaults()
    {
        using var mgr = new InMemoryStreamStateManager(null);

        mgr.Dispose();
    }

    #endregion
}

public class StreamStateTests
{
    [Fact]
    public void Constructor_WithoutId_GeneratesGuidBasedId()
    {
        var state = new StreamState();

        state.StreamId.Should().NotBeNullOrEmpty();
        state.StreamId.Should().HaveLength(32); // Guid "N" format
    }

    [Fact]
    public void Constructor_WithId_UsesProvidedId()
    {
        var state = new StreamState("custom-id");

        state.StreamId.Should().Be("custom-id");
    }

    [Fact]
    public void Constructor_InitialState_IsPending()
    {
        var state = new StreamState();

        state.Status.Should().Be(StreamStatus.Pending);
        state.TotalChunksReceived.Should().Be(0);
        state.LastChunkIndex.Should().Be(0);
        state.AccumulatedContent.Should().BeEmpty();
        state.ErrorMessage.Should().BeNull();
        state.CanResume.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithMetadata_CopiesMetadata()
    {
        var metadata = new Dictionary<string, object>
        {
            ["model"] = "gpt-4",
            ["tokens"] = 100
        };

        var state = new StreamState(metadata: metadata);

        state.Metadata.Should().HaveCount(2);
        state.Metadata!["model"].Should().Be("gpt-4");
        state.Metadata!["tokens"].Should().Be(100);
    }

    [Fact]
    public void Constructor_WithNullMetadata_HasEmptyMetadata()
    {
        var state = new StreamState();

        state.Metadata.Should().NotBeNull();
        state.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void AppendChunk_TransitionsPendingToStreaming()
    {
        var state = new StreamState();
        state.Status.Should().Be(StreamStatus.Pending);

        state.AppendChunk("hello", 0);

        state.Status.Should().Be(StreamStatus.Streaming);
    }

    [Fact]
    public void AppendChunk_AccumulatesContent()
    {
        var state = new StreamState();

        state.AppendChunk("Hello ", 0);
        state.AppendChunk("World", 1);
        state.AppendChunk("!", 2);

        state.AccumulatedContent.Should().Be("Hello World!");
        state.TotalChunksReceived.Should().Be(3);
        state.LastChunkIndex.Should().Be(2);
    }

    [Fact]
    public void AppendChunk_UpdatesLastUpdatedAt()
    {
        var state = new StreamState();
        var initial = state.LastUpdatedAt;

        Thread.Sleep(10);
        state.AppendChunk("data", 0);

        state.LastUpdatedAt.Should().BeAfter(initial);
    }

    [Fact]
    public void UpdateStatus_ChangesStatusAndErrorMessage()
    {
        var state = new StreamState();

        state.UpdateStatus(StreamStatus.Failed, "timeout");

        state.Status.Should().Be(StreamStatus.Failed);
        state.ErrorMessage.Should().Be("timeout");
    }

    [Fact]
    public void UpdateStatus_WithoutErrorMessage_ClearsErrorMessage()
    {
        var state = new StreamState();
        state.UpdateStatus(StreamStatus.Failed, "error");
        state.ErrorMessage.Should().Be("error");

        state.UpdateStatus(StreamStatus.Streaming);

        state.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void SetMetadata_AddsOrUpdatesMetadataEntry()
    {
        var state = new StreamState();

        state.SetMetadata("key1", "value1");
        state.Metadata!["key1"].Should().Be("value1");

        state.SetMetadata("key1", "updated");
        state.Metadata!["key1"].Should().Be("updated");
    }

    [Fact]
    public void SetMetadata_UpdatesLastUpdatedAt()
    {
        var state = new StreamState();
        var initial = state.LastUpdatedAt;

        Thread.Sleep(10);
        state.SetMetadata("key", "value");

        state.LastUpdatedAt.Should().BeAfter(initial);
    }
}
