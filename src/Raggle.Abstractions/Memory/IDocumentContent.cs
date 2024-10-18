using Raggle.Abstractions.Utils;

namespace Raggle.Abstractions.Memory;

[JsonDiscriminatorName("type")]
public interface IDocumentContent { }

[JsonDiscriminatorValue("text")]
public class TextDocumentContent : IDocumentContent
{
    public string Text { get; set; } = string.Empty;
}

[JsonDiscriminatorValue("image")]
public class ImageDocumentContent : IDocumentContent
{
    public string ImagePath { get; set; } = string.Empty;
}

[JsonDiscriminatorValue("table")]
public class TableDocumentContent : IDocumentContent
{
    public List<string> Columns { get; set; } = [];
    public List<List<string>> Rows { get; set; } = [];
}
