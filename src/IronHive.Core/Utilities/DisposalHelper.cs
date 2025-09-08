namespace IronHive.Core.Utilities;

/// <summary>
/// Dispose 관련 헬퍼.
/// </summary>
public static class DisposalHelper
{
    /// <summary>
    /// 아이템이 IDisposable 또는 IAsyncDisposable을 구현하는 경우 Dispose를 호출합니다.
    /// - IAsyncDisposable이 있으면 우선적으로 DisposeAsync 호출
    /// - 예외는 모두 무시 (silently)
    /// </summary>
    public static void DisposeSafely<T>(T item)
    {
        try
        {
            if (item is IAsyncDisposable ad)
            {
                // 비차단: 흘려보내되, 예외는 내부에서 처리
                _ = ad.DisposeAsync().AsTask().ContinueWith(_ => { /* swallow */ });
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
