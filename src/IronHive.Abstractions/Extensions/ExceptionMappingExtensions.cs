using System.Runtime.CompilerServices;

namespace IronHive.Abstractions.Extensions;

/// <summary>
/// Provider-neutral helpers for translating low-level exceptions (SDK/HTTP) into
/// IronHive domain exceptions at the call boundary. The mapping rules themselves
/// live in each provider assembly; these helpers only own the wrapping mechanics.
/// </summary>
public static class ExceptionMappingExtensions
{
    /// <summary>
    /// Awaits <paramref name="task"/>, rethrowing as the mapped exception when
    /// <paramref name="mapper"/> returns non-null for a thrown exception.
    /// </summary>
    public static async Task<T> MapExceptions<T>(this Task<T> task, Func<Exception, Exception?> mapper)
    {
        try
        {
            return await task.ConfigureAwait(false);
        }
        catch (Exception ex) when (mapper(ex) is { } mapped)
        {
            throw mapped;
        }
    }

    /// <summary>
    /// Enumerates <paramref name="source"/>, rethrowing as the mapped exception when
    /// <paramref name="mapper"/> returns non-null for an exception thrown by enumeration
    /// (including the initial request of a streaming call).
    /// </summary>
    public static async IAsyncEnumerable<T> MapExceptions<T>(
        this IAsyncEnumerable<T> source,
        Func<Exception, Exception?> mapper,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var enumerator = source.GetAsyncEnumerator(cancellationToken);
        while (true)
        {
            T current;
            try
            {
                if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                    yield break;
                current = enumerator.Current;
            }
            catch (Exception ex) when (mapper(ex) is { } mapped)
            {
                throw mapped;
            }

            yield return current;
        }
    }
}
