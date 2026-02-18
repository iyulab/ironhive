using FluentAssertions;
using IronHive.Core.Utilities;

namespace IronHive.Tests.Utilities;

public class DisposalHelperTests
{
    [Fact]
    public void DisposeSafely_Disposable_CallsDispose()
    {
        var disposable = new TrackingDisposable();

        DisposalHelper.DisposeSafely(disposable);

        disposable.Disposed.Should().BeTrue();
    }

    [Fact]
    public void DisposeSafely_AsyncDisposable_CallsDisposeAsync()
    {
        var disposable = new TrackingAsyncDisposable();

        DisposalHelper.DisposeSafely(disposable);

        disposable.DisposedAsync.Should().BeTrue();
    }

    [Fact]
    public void DisposeSafely_NonDisposable_DoesNotThrow()
    {
        var act = () => DisposalHelper.DisposeSafely("just a string");
        act.Should().NotThrow();
    }

    [Fact]
    public void DisposeSafely_Null_DoesNotThrow()
    {
        var act = () => DisposalHelper.DisposeSafely<object?>(null);
        act.Should().NotThrow();
    }

    [Fact]
    public void DisposeSafely_ThrowingDisposable_DoesNotThrow()
    {
        var disposable = new ThrowingDisposable();

        var act = () => DisposalHelper.DisposeSafely(disposable);

        act.Should().NotThrow();
    }

    [Fact]
    public async Task DisposeSafelyAsync_Disposable_CallsDispose()
    {
        var disposable = new TrackingDisposable();

        await DisposalHelper.DisposeSafelyAsync(disposable);

        disposable.Disposed.Should().BeTrue();
    }

    [Fact]
    public async Task DisposeSafelyAsync_AsyncDisposable_CallsDisposeAsync()
    {
        var disposable = new TrackingAsyncDisposable();

        await DisposalHelper.DisposeSafelyAsync(disposable);

        disposable.DisposedAsync.Should().BeTrue();
    }

    [Fact]
    public async Task DisposeSafelyAsync_ThrowingAsyncDisposable_DoesNotThrow()
    {
        var disposable = new ThrowingAsyncDisposable();

        var act = async () => await DisposalHelper.DisposeSafelyAsync(disposable);

        await act.Should().NotThrowAsync();
    }

    #region Helpers

    private sealed class TrackingDisposable : IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
    }

    private sealed class TrackingAsyncDisposable : IAsyncDisposable
    {
        public bool DisposedAsync { get; private set; }
        public ValueTask DisposeAsync()
        {
            DisposedAsync = true;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ThrowingDisposable : IDisposable
    {
        public void Dispose() => throw new InvalidOperationException("disposal failed");
    }

    private sealed class ThrowingAsyncDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => throw new InvalidOperationException("async disposal failed");
    }

    #endregion
}
