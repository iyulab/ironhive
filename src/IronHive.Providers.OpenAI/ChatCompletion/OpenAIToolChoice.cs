using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(NoneOpenAIToolChoice), "none")]
[JsonDerivedType(typeof(AutoOpenAIToolChoice), "auto")]
[JsonDerivedType(typeof(RequiredOpenAIToolChoice), "required")]
public abstract class OpenAIToolChoice
{ }

public class NoneOpenAIToolChoice : OpenAIToolChoice { }

public class AutoOpenAIToolChoice : OpenAIToolChoice { }

public class RequiredOpenAIToolChoice : OpenAIToolChoice
{
    [JsonPropertyName("function")]
    public FunctionChoice? Function { get; set; }

    public class FunctionChoice
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
