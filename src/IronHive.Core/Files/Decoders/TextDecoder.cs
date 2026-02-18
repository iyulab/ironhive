using IronHive.Abstractions.Files;
using IronHive.Core.Utilities;

namespace IronHive.Core.Files.Decoders;

/// <summary>
/// 텍스트 파일 디코더 클래스입니다.
/// </summary>
public class TextDecoder : IFileDecoder<string>
{
    /// <inheritdoc />
    public bool SupportsMimeType(string mimeType)
    {
        return mimeType.StartsWith("text/", StringComparison.Ordinal);
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
