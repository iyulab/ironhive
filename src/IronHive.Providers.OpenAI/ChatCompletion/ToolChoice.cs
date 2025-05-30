using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(NoneToolChoice), "none")]
[JsonDerivedType(typeof(AutoToolChoice), "auto")]
[JsonDerivedType(typeof(RequiredToolChoice), "required")]
internal abstract class ToolChoice
{ }

internal class NoneToolChoice : ToolChoice { }

internal class AutoToolChoice : ToolChoice { }

internal class RequiredToolChoice : ToolChoice
{
    [JsonPropertyName("function")]
    public FunctionChoice? Function { get; set; }
}

internal class FunctionChoice
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
