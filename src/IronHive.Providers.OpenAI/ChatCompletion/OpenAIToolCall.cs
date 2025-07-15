using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion;

//[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
//[JsonDerivedType(typeof(OpenAIFunctionToolCall), "function")]
//public abstract class OpenAIToolCall
//{ }

public class OpenAIFunctionToolCall
{
    /// <summary>
    /// "type"이 json의 첫번째 속성으로 오지 않아 다형성 객체를 사용할 수 없음
    /// .net9 부터 AllowOutOfOrderMetadataProperties 옵션으로 사용가능 하지만 성능저하로 보류
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("function")]
    public FunctionSchema? Function { get; set; }

    public class FunctionSchema
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("arguments")]
        public string? Arguments { get; set; }
    }
}
