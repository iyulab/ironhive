using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.GenerateContent;

internal sealed class ModelStatus
{
    [JsonPropertyName("modelStage")]
    public ModelStage? ModelStage { get; set; }

    [JsonPropertyName("retirementTime")]
    public DateTime? RetirementTime { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

internal enum ModelStage
{
    MODEL_STAGE_UNSPECIFIED,
    EXPERIMENTAL,
    PREVIEW,
    STABLE,
    LEGACY,
    RETIRED
}
