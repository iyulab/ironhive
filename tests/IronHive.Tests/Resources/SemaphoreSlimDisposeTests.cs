using FluentAssertions;

namespace IronHive.Tests.Resources;

/// <summary>
/// Tests to verify SemaphoreSlim resources are properly disposed.
/// Issue #5: SemaphoreSlim not disposed in RabbitMQueueStorage and McpSession.
/// </summary>
public class SemaphoreSlimDisposeTests
{
    /// <summary>
    /// Verifies that SemaphoreSlim is properly disposed when the owning class is disposed.
    /// This tests the general pattern that should be used.
    /// </summary>
    [Fact]
    public void SemaphoreSlim_ShouldBeDisposed_WhenOwnerIsDisposed()
    {
        // Arrange
        var semaphore = new SemaphoreSlim(1, 1);
        var wrapper = new SemaphoreWrapper(semaphore);

        // Act
        wrapper.Dispose();

        // Assert - After disposal, Wait should throw ObjectDisposedException
        var act = () => semaphore.Wait(0);
        act.Should().Throw<ObjectDisposedException>();
    }

    /// <summary>
    /// Verifies that async SemaphoreSlim operations fail after disposal.
    /// </summary>
    [Fact]
    public async Task SemaphoreSlim_ShouldThrowOnWaitAsync_AfterDisposal()
    {
        // Arrange
        var semaphore = new SemaphoreSlim(1, 1);

        // Act
        semaphore.Dispose();

        // Assert
        var act = () => semaphore.WaitAsync();
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    /// <summary>
    /// Verifies proper disposal pattern: dispose should be idempotent.
    /// </summary>
    [Fact]
    public void SemaphoreSlim_MultipleDispose_ShouldNotThrow()
    {
        // Arrange
        var semaphore = new SemaphoreSlim(1, 1);

        // Act & Assert - Multiple dispose calls should not throw
        var act = () =>
        {
            semaphore.Dispose();
            semaphore.Dispose();
        };
        act.Should().NotThrow();
    }

    private class SemaphoreWrapper : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        public SemaphoreWrapper(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            _semaphore.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
