using Raggle.Abstractions.Tools;

namespace Raggle.Abstractions.Assistant;

public interface IRaggleAssistant
{
    IServiceProvider Services { get; }

    string Name { get; set; }

    string? Description { get; set; }

    string? Instructions { get; set; }

    //IToolKit[]? ToolKitList { get; set; }


    //object Provider { get; set; }

    //string Model { get; set; }

    //public int? MaxTokens { get; set; }

    //public float? Temperature { get; set; }

    //public int? TopK { get; set; }

    //public float? TopP { get; set; }

    //public string[]? StopSequences { get; set; }

}
