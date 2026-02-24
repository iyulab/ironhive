using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Videos;

/// <summary>
/// OpenAI 비디오 생성 응답 (POST /v1/videos 및 GET /v1/videos/{id})
/// </summary>
public class CreateVideoResponse : OpenAIPayloadBase
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("completed_at")]
    public long? CompletedAt { get; set; }

    [JsonPropertyName("created_at")]
    public long? CreatedAt { get; set; }

    [JsonPropertyName("error")]
    public CreateVideoError? Error { get; set; }

    [JsonPropertyName("expires_at")]
    public long? ExpiresAt { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("object")]
    public string ObjectType { get; } = "video";

    [JsonPropertyName("progress")]
    public int? Progress { get; set; }

    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    [JsonPropertyName("remixed_from_video_id")]
    public string? RemixedFromVideoId { get; set; }

    [JsonPropertyName("seconds")]
    public string? Seconds { get; set; }

    [JsonPropertyName("size")]
    public string? Size { get; set; }

    [JsonPropertyName("status")]
    public CreateVideoStatus? Status { get; set; }
}

public class CreateVideoError
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public enum CreateVideoStatus
{
    Queued,
    InProgress,
    Completed,
    Failed
}