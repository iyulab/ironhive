using AwesomeAssertions;
using IronHive.Core.Storages;

namespace IronHive.Tests.Storages;

public class LocalQueueStorageTests : IDisposable
{
    private readonly string _testDir;
    private readonly LocalQueueStorage _storage;

    public LocalQueueStorageTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"ironhive_queue_test_{Guid.NewGuid():N}");
        _storage = new LocalQueueStorage(_testDir);
    }

    public void Dispose()
    {
        _storage.Dispose();
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, recursive: true); } catch { }
        }
        GC.SuppressFinalize(this);
    }

    #region Constructor

    [Fact]
    public void Constructor_CreatesDirectory()
    {
        Directory.Exists(_testDir).Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithConfig_InvalidCacheSize_Throws()
    {
        var act = () => new LocalQueueStorage(new LocalQueueConfig
        {
            DirectoryPath = _testDir,
            CacheSize = 0
        });

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DirectoryPath_ReturnsConfiguredPath()
    {
        _storage.DirectoryPath.Should().Be(_testDir);
    }

    #endregion

    #region EnqueueAsync / CountAsync

    [Fact]
    public async Task EnqueueAsync_SingleMessage_IncreasesCount()
    {
        var initialCount = await _storage.CountAsync(TestContext.Current.CancellationToken);
        initialCount.Should().Be(0);

        await _storage.EnqueueAsync("hello", TestContext.Current.CancellationToken);

        var count = await _storage.CountAsync(TestContext.Current.CancellationToken);
        count.Should().Be(1);
    }

    [Fact]
    public async Task EnqueueAsync_MultipleMessages_IncreasesCount()
    {
        await _storage.EnqueueAsync("msg1", TestContext.Current.CancellationToken);
        await _storage.EnqueueAsync("msg2", TestContext.Current.CancellationToken);
        await _storage.EnqueueAsync("msg3", TestContext.Current.CancellationToken);

        var count = await _storage.CountAsync(TestContext.Current.CancellationToken);
        count.Should().Be(3);
    }

    [Fact]
    public async Task EnqueueAsync_ComplexObject_Serializes()
    {
        var obj = new TestPayload { Name = "test", Value = 42 };

        await _storage.EnqueueAsync(obj, TestContext.Current.CancellationToken);

        var count = await _storage.CountAsync(TestContext.Current.CancellationToken);
        count.Should().Be(1);
    }

    #endregion

    #region DequeueAsync

    [Fact]
    public async Task DequeueAsync_EmptyQueue_ReturnsNull()
    {
        var msg = await _storage.DequeueAsync<string>(TestContext.Current.CancellationToken);

        msg.Should().BeNull();
    }

    [Fact]
    public async Task DequeueAsync_AfterEnqueue_ReturnsMessage()
    {
        await _storage.EnqueueAsync("hello world", TestContext.Current.CancellationToken);

        var msg = await _storage.DequeueAsync<string>(TestContext.Current.CancellationToken);

        msg.Should().NotBeNull();
        msg!.Body.Should().Be("hello world");
    }

    [Fact]
    public async Task DequeueAsync_FIFO_Order()
    {
        await _storage.EnqueueAsync("first", TestContext.Current.CancellationToken);
        await Task.Delay(10, TestContext.Current.CancellationToken); // Ensure different ticks
        await _storage.EnqueueAsync("second", TestContext.Current.CancellationToken);
        await Task.Delay(10, TestContext.Current.CancellationToken);
        await _storage.EnqueueAsync("third", TestContext.Current.CancellationToken);

        var msg1 = await _storage.DequeueAsync<string>(TestContext.Current.CancellationToken);
        var msg2 = await _storage.DequeueAsync<string>(TestContext.Current.CancellationToken);
        var msg3 = await _storage.DequeueAsync<string>(TestContext.Current.CancellationToken);

        msg1!.Body.Should().Be("first");
        msg2!.Body.Should().Be("second");
        msg3!.Body.Should().Be("third");
    }

    [Fact]
    public async Task DequeueAsync_ComplexObject_Deserializes()
    {
        var original = new TestPayload { Name = "test", Value = 42 };
        await _storage.EnqueueAsync(original, TestContext.Current.CancellationToken);

        var msg = await _storage.DequeueAsync<TestPayload>(TestContext.Current.CancellationToken);

        msg.Should().NotBeNull();
        msg!.Body.Name.Should().Be("test");
        msg.Body.Value.Should().Be(42);
    }

    [Fact]
    public async Task DequeueAsync_LocksFile_PreventsDoubleDequeueing()
    {
        await _storage.EnqueueAsync("only-one", TestContext.Current.CancellationToken);

        var msg1 = await _storage.DequeueAsync<string>(TestContext.Current.CancellationToken);
        var msg2 = await _storage.DequeueAsync<string>(TestContext.Current.CancellationToken);

        msg1.Should().NotBeNull();
        msg2.Should().BeNull();
    }

    [Fact]
    public async Task DequeueAsync_WithTtl_ExpiredMessage_ReturnsNull()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"ironhive_queue_ttl_{Guid.NewGuid():N}");
        using var storage = new LocalQueueStorage(new LocalQueueConfig
        {
            DirectoryPath = dir,
            TimeToLive = TimeSpan.FromMilliseconds(1)
        });

        await storage.EnqueueAsync("expires-fast", TestContext.Current.CancellationToken);
        await Task.Delay(50, TestContext.Current.CancellationToken); // Wait for expiration

        var msg = await storage.DequeueAsync<string>(TestContext.Current.CancellationToken);
        msg.Should().BeNull();

        if (Directory.Exists(dir))
            Directory.Delete(dir, recursive: true);
    }

    #endregion

    #region CompleteAsync / RequeueAsync

    [Fact]
    public async Task CompleteAsync_RemovesLockedFile()
    {
        await _storage.EnqueueAsync("ack-test", TestContext.Current.CancellationToken);

        var msg = await _storage.DequeueAsync<string>(TestContext.Current.CancellationToken);
        msg.Should().NotBeNull();

        await msg!.CompleteAsync(TestContext.Current.CancellationToken);

        // After complete, file should be deleted — count of message files is 0
        var count = await _storage.CountAsync(TestContext.Current.CancellationToken);
        count.Should().Be(0);
    }

    [Fact]
    public async Task RequeueAsync_RestoresMessageFile()
    {
        await _storage.EnqueueAsync("requeue-test", TestContext.Current.CancellationToken);

        var msg = await _storage.DequeueAsync<string>(TestContext.Current.CancellationToken);
        msg.Should().NotBeNull();

        await msg!.RequeueAsync(TestContext.Current.CancellationToken);

        // After requeue, message should be available again
        var count = await _storage.CountAsync(TestContext.Current.CancellationToken);
        count.Should().Be(1);
    }

    #endregion

    #region ClearAsync

    [Fact]
    public async Task ClearAsync_RemovesAllMessages()
    {
        await _storage.EnqueueAsync("a", TestContext.Current.CancellationToken);
        await _storage.EnqueueAsync("b", TestContext.Current.CancellationToken);
        await _storage.EnqueueAsync("c", TestContext.Current.CancellationToken);
        (await _storage.CountAsync(TestContext.Current.CancellationToken)).Should().Be(3);

        await _storage.ClearAsync(TestContext.Current.CancellationToken);

        (await _storage.CountAsync(TestContext.Current.CancellationToken)).Should().Be(0);
    }

    [Fact]
    public async Task ClearAsync_OnEmptyQueue_DoesNotThrow()
    {
        var act = () => _storage.ClearAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ClearAsync_RecreatesDirectory()
    {
        await _storage.ClearAsync(TestContext.Current.CancellationToken);

        Directory.Exists(_testDir).Should().BeTrue();
    }

    #endregion

    #region CreateConsumerAsync

    [Fact]
    public async Task CreateConsumerAsync_ReturnsConsumer()
    {
        var consumer = await _storage.CreateConsumerAsync<string>(msg => Task.CompletedTask, TestContext.Current.CancellationToken);

        consumer.Should().NotBeNull();
    }

    #endregion

    #region RestoreAsync

    [Fact]
    public async Task RestoreAsync_RestoresEnqueuedMessages()
    {
        await _storage.EnqueueAsync("msg1", TestContext.Current.CancellationToken);
        await _storage.EnqueueAsync("msg2", TestContext.Current.CancellationToken);

        var restored = await _storage.RestoreAsync<string>();

        restored.Should().HaveCount(2);
    }

    #endregion

    private sealed class TestPayload
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }
}
