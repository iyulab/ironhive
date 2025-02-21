using System.Text.Json.Serialization;

namespace Raggle.Abstractions.ChatCompletion.Tools;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AutoToolChoice), "auto")]
[JsonDerivedType(typeof(ManualToolChoice), "manual")]
[JsonDerivedType(typeof(DisabledToolChoice), "disabled")]
public interface IToolChoice
{ }

public class AutoToolChoice : IToolChoice
{ }

public class ManualToolChoice : IToolChoice
{
    public string ToolName { get; set; } = string.Empty;
}

public class DisabledToolChoice : IToolChoice
{ }
