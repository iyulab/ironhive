using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.GenerateContent.Models;

/// <summary>모델이 사용하길 원하는 도구 묶음.</summary>
internal sealed class Tool
{
    /// <summary>함수 선언 목록(함수 호출 도구).</summary>
    [JsonPropertyName("functionDeclarations")]
    public ICollection<FunctionTool>? Functions { get; set; }

    /// <summary>구글 검색 기반 그라운딩 도구.</summary>
    [JsonPropertyName("googleSearchRetrieval")]
    public GoogleRetrievalTool? GoogleRetrieval { get; set; }

    /// <summary>코드 실행 도구 사용 시 Empty JSON Object.</summary>
    [JsonPropertyName("codeExecution")]
    public JsonObject? CodeExecution { get; set; }

    /// <summary>구글 검색 기반 그라운딩 도구.</summary>
    [JsonPropertyName("googleSearch")]
    public GoogleSearchTool? GoogleSearch { get; set; }

    /// <summary>URL 컨텍스트(페이지 내용 자동 참조) 도구 사용 시 Empty JSON Object.</summary>
    [JsonPropertyName("urlContext")]
    public JsonObject? UrlContext { get; set; }
}