using Raggle.Abstractions.Memory;

namespace Raggle.Core.Extractors;

public class TextDecoder : IContentDecoder
{
    public string[] SupportTypes =>
    [
        "text/plain"
    ];

    public Task<IDocumentContent[]> DecodeAsync(Stream data, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(data);
        var text = reader.ReadToEnd();
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var results = new List<IDocumentContent>();
        foreach (var line in lines)
        {
            results.Add(new TextDocumentContent
            {
                Text = line
            });
        }
        return Task.FromResult(results.ToArray());
    }
}
