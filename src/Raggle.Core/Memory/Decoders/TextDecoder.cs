using Raggle.Abstractions.Memory;
using Raggle.Core.Memory.Document;
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
    public async Task<DocumentSource> DecodeAsync(
        DataPipeline pipeline,
        Stream data,
        CancellationToken cancellationToken = default)
    {
        var lines = new List<string>();

        using var reader = new StreamReader(data);
        while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
        {
            cancellationToken.ThrowIfCancellationRequested();

            line = TextCleaner.Clean(line);
            if (string.IsNullOrWhiteSpace(line))
                continue;
            lines.Add(line);
        }

        return new DocumentSource
        {
            Source = pipeline.Source,
            Section = new DocumentSegment
            {
                Unit = "line",
                From = 1,
                To = lines.Count,
            },
            Content = lines,
        };
    }
}
