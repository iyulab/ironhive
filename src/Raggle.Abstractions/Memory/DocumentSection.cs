namespace Raggle.Abstractions.Memory;

public class DocumentSection
{
    public int Number { get; set; }

    public string? Text { get; set; }

    public DocumentSection(int number, string? text)
    {
        Number = number;
        Text = text;
    }
}
