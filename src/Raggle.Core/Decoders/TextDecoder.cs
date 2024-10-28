using Raggle.Abstractions.Memory;

namespace Raggle.Core.Extractors;

public class TextDecoder : IContentDecoder
{
    public string[] SupportTypes =>
    [
        "text/plain"
    ];

    public Task<IEnumerable<DocumentSection>> DecodeAsync(Stream data, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(data);
        var text = reader.ReadToEnd();
        //var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        var results = new List<DocumentSection>
        {
            new DocumentSection(1, text)
        };
        return Task.FromResult(results.AsEnumerable());
    }
}
