using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.EmbedContent;

internal class EmbedContentRequest
{
    [JsonIgnore]
    public required string Model { get; set; }

    /// <summary>
    /// Part.Text만 임베딩이 지원됩니다.
    /// </summary>
    [JsonPropertyName("content")]
    public required Content Content { get; set; }

    /// <summary>
    /// 임베딩 모델에게 어떤 용도로 사용할 것인지 알려주는 '힌트' 또는 '지시사항'입니다.
    /// (Not supported: embedding-001)
    /// </summary>
    [JsonPropertyName("taskType")]
    public EmbedTaskType? TaskType { get; set; }

    /// <summary>
    /// Text의 제목입니다. 이 필드는 선택 사항이며, 오직 "RETRIEVAL_DOCUMENT" TaskType에만 적용됩니다.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 생성된 임베딩의 차원수를 지정한 크기로 잘라서 반환합니다.(2024년 이후 모델만 지원)
    /// </summary>
    [JsonPropertyName("outputDimensionality")]
    public int? OutputDimensionality { get; set; }
}

internal class BatchEmbedContentRequest
{
    [JsonPropertyName("requests")]
    public ICollection<EmbedContentRequest> Requests { get; set; } = [];
}