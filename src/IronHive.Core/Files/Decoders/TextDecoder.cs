using IronHive.Abstractions.Files;
using IronHive.Abstractions.Memory;

namespace IronHive.Core.Files.Decoders;

public class TextDecoder : IFileDecoder
{
    /// <inheritdoc />
    public bool SupportsMimeType(string mimeType)
    {
        return mimeType.StartsWith("text/");
    }

    /// <inheritdoc />
    public async Task<string> DecodeAsync(
        Stream data,
        CancellationToken cancellationToken = default)
    {
        var content = new List<string>();

        using var reader = new StreamReader(data);
        while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
        {
            cancellationToken.ThrowIfCancellationRequested();

            line = TextCleaner.Clean(line);
            if (string.IsNullOrWhiteSpace(line))
                continue;
            content.Add(line);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return string.Join(Environment.NewLine, content);
    }
}
