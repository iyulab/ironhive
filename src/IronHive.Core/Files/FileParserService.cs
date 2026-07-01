using System.Text;
using IronHive.Abstractions.Files;
using IronHive.Core.Utilities;

namespace IronHive.Core.Files;

/// <inheritdoc />
public class FileParserService : IFileParserService
{
    private readonly IReadOnlyList<IFileParser> _parsers;

    public FileParserService(IEnumerable<IFileParser>? parsers = null)
    {
        _parsers = (parsers ?? []).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FileBlock>> ParseAsync(
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Empty file name.", nameof(fileName));

        if (data.CanSeek) data.Position = 0;

        var parser = _parsers.FirstOrDefault(p => p.CanParse(fileName));
        if (parser != null)
            return await parser.ParseAsync(fileName, data, cancellationToken).ConfigureAwait(false);

        // 전용 파서가 없을 때: git과 동일한 방식으로 앞 8192바이트에 null byte(0x00)가
        // 있으면 binary, 없으면 UTF-8 텍스트로 판별합니다. (content_inspector 휴리스틱)
        using var ms = new MemoryStream();
        await data.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        var bytes = ms.ToArray();

        var sample = bytes.AsSpan(0, Math.Min(bytes.Length, 8192));
        if (sample.Contains((byte)0))
            return [new BinaryBlock { Data = bytes }];

        var text = TextCleaner.Clean(Encoding.UTF8.GetString(bytes));
        return [new TextBlock { Label = fileName, Text = text }];
    }
}
