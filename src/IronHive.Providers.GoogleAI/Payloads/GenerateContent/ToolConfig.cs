using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.GenerateContent;

/// <summary>도구 설정(특히 함수 호출 동작).</summary>
internal sealed class ToolConfig
{
    [JsonPropertyName("functionCallingConfig")]
    public Config? FunctionConfig { get; set; }

    /// <summary>함수 호출 동작 설정.</summary>
    internal sealed class Config
    {
        /// <summary> 함수 호출 모드. 미지정 시 AUTO.</summary>
        [JsonPropertyName("mode")]
        public FunctionMode? Mode { get; set; }

        /// <summary>호출을 허용할 함수명 화이트리스트.</summary>
        [JsonPropertyName("allowedFunctionNames")]
        public ICollection<string>? FunctionNames { get; set; }
    }

    internal enum FunctionMode
    {
        MODE_UNSPECIFIED,
        AUTO,
        ANY,
        NONE,
        VALIDATED
    }
}