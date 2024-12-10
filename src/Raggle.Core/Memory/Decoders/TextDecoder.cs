using Raggle.Abstractions.Memory;
using Raggle.Core.Utils;

namespace Raggle.Core.Memory.Decoders;

public class TextDecoder : IDocumentDecoder
{
    /// <inheritdoc />
    public bool IsSupportMimeType(string mimeType)
    {
        return mimeType.StartsWith("text/");
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> DecodeAsync(
        Stream data,
        CancellationToken cancellationToken = default)
    {
        var contents = new List<string>();

        using var reader = new StreamReader(data);
        while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
        {
            cancellationToken.ThrowIfCancellationRequested();

            line = TextCleaner.Clean(line);
            if (string.IsNullOrWhiteSpace(line))
                continue;
            contents.Add(line);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return contents;
    }
}
