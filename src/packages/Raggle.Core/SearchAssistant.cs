namespace Raggle.Core;

public class SearchAssistant
{
    public string Name { get; set; } = string.Empty;
    public string? Instruction { get; set; }

    public SearchAssistant()
    {
        Name = "Search Assistant";
        Instruction = "Search for information using the search assistant.";
    }

}
