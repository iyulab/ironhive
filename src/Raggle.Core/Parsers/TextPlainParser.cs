using Raggle.Core.Document;
using Raggle.Core.Utils;

namespace Raggle.Core.Parsers;

public class TextPlainParser : IDocumentParser
{
    public string[] SupportTypes =>
    [
        "text/plain"
    ];

    public Task<IEnumerable<DocumentSection>> ParseAsync(Stream data, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(data);
        var text = reader.ReadToEnd();
        text = TextCleaner.Clean(text);

        var results = new List<DocumentSection>
        {
            new DocumentSection(1, text)
        };
        return Task.FromResult(results.AsEnumerable());
    }
}
