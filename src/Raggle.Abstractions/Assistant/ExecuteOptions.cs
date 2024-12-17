namespace Raggle.Abstractions.Assistant;

public class ExecuteOptions
{
    public int? MaxTokens { get; set; }

    public float? Temperature { get; set; }

    public int? TopK { get; set; }

    public float? TopP { get; set; }

    public string[]? StopSequences { get; set; }
}
