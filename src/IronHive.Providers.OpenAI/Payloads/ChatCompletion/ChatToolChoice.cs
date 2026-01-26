using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AllowedChatToolChoice), "allowed_tools")]
[JsonDerivedType(typeof(FunctionChatToolChoice), "function")]
[JsonDerivedType(typeof(CustomChatToolChoice), "custom")]
public abstract class ChatToolChoice
{ }

public class AllowedChatToolChoice : ChatToolChoice
{
    [JsonPropertyName("allowed_tools")]
    public required AllowedTools Tools { get; set; }

    public sealed class AllowedTools
    {
        /// <summary>
        /// "auto" or "required"
        /// </summary>
        [JsonPropertyName("mode")]
        public required string Mode { get; set; }

        [JsonPropertyName("tools")]
        public ICollection<ChatTool> Tools { get; set; } = [];
    }
}

public class FunctionChatToolChoice : ChatToolChoice
{
    [JsonPropertyName("function")]
    public required FunctionTool Function { get; set; }

    public sealed class FunctionTool
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }
    }
}

public class CustomChatToolChoice : ChatToolChoice
{
    [JsonPropertyName("custom")]
    public required CustomTool Custom { get; set; }

    public sealed class CustomTool
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }
    }
}
