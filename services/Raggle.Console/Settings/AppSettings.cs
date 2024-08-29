using System.Text.Json.Serialization;

namespace Raggle.Console.Settings;

public class AppSettings
{
    public required string WorkingDirectory { get; set; }
    public PlatformOptions Platforms { get; set; } = new();
    public PromptOptions Prompts { get; set; } = new();
    public VectorDBOptions VectorDB { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlatformTypes
{
    OpenAI
}

public class PlatformOptions
{
    public PlatformTypes Type { get; set; } = PlatformTypes.OpenAI;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VectorDBTypes
{
    File
}

public class VectorDBOptions
{
    public VectorDBTypes Type { get; set; } = VectorDBTypes.File;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PromptTypes
{
    Simple
}

public class PromptOptions
{
    public PromptTypes Type { get; set; } = PromptTypes.Simple;
}