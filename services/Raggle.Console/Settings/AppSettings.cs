using Raggle.Core.Options.Platforms;
using Raggle.Core.Options.Prompts;
using Raggle.Core.Options.VectorDB;
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
    public OpenAIOption OpenAI { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VectorDBTypes
{
    File
}

public class VectorDBOptions
{
    public VectorDBTypes Type { get; set; } = VectorDBTypes.File;
    public FileVectorDBOption FileVectorDB { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PromptTypes
{
    Simple
}

public class PromptOptions
{
    public PromptTypes Type { get; set; } = PromptTypes.Simple;
    public SimplePromptOption SimplePrompt { get; set; } = new();
}