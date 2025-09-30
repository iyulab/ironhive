using IronHive.Providers.GoogleAI.GenerateContent.Models;
using IronHive.Providers.GoogleAI.Share.Models;
using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Tokens.Models;

/// <summary>
/// Union Type, Contents 또는 GenerateContentRequest 중 하나만 설정되어야 합니다.
/// </summary>
internal class CountTokensRequest
{
    [JsonPropertyName("contents")]
    public ICollection<Content>? Contents { get; set; }

    /// <summary> TODO: models가 포함 되어야 하는 오브젝트 </summary>
    [JsonPropertyName("generateContentRequest")]
    [Obsolete("GenerateContentRequest는 models필드가 없습니다. Contents를 사용하세요.")]
    public GenerateContentRequest? GenerateContentRequest { get; set; }
}
