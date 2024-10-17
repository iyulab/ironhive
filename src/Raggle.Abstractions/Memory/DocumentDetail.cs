namespace Raggle.Abstractions.Memory;

public class DocumentDetail : DocumentRecord
{
    public DataPipeline? DataPipeline { get; set; }

    public IEnumerable<string> Chunks { get; set; } = [];
}
