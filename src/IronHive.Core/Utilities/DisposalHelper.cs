namespace IronHive.Core.Utilities;

/// <summary>
/// Dispose 관련 헬퍼.
/// </summary>
public static class DisposalHelper
{
    /// <summary>
    /// 아이템이 IDisposable 또는 IAsyncDisposable을 구현하는 경우 Dispose를 호출합니다.
    /// - IAsyncDisposable이 있으면 우선적으로 DisposeAsync를 blocking 호출
    /// - 예외는 모두 무시 (silently)
    /// </summary>
    public static void DisposeSafely<T>(T item)
    {
        try
        {
            if (item is IAsyncDisposable ad)
            {
                ad.DisposeAsync().AsTask().GetAwaiter().GetResult();
                return;
            }

            if (item is IDisposable d)
            {
                d.Dispose();
            }
        }
        catch
        {
            // Silently
        }
    }

    /// <summary>
    /// 아이템이 IDisposable 또는 IAsyncDisposable을 구현하는 경우 비동기로 Dispose를 호출합니다.
    /// - IAsyncDisposable이 있으면 우선적으로 DisposeAsync 호출
    /// - 예외는 모두 무시 (silently)
    /// </summary>
    public static async ValueTask DisposeSafelyAsync<T>(T item)
    {
        try
        {
            if (item is IAsyncDisposable ad)
            {
                await ad.DisposeAsync().ConfigureAwait(false);
                return;
            }

            if (item is IDisposable d)
            {
                d.Dispose();
            }
        }
        catch
        {
            // Silently
        }
    }
}
