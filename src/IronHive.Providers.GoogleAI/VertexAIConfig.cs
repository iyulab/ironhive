using Google.Apis.Auth.OAuth2;
using Google.GenAI.Types;

namespace IronHive.Providers.GoogleAI;

/// <summary>
/// Google Vertex AI 서비스 연결을 위한 구성 클래스입니다.
/// </summary>
public class VertexAIConfig
{
    /// <summary>
    /// Google Cloud 인증 정보를 가져오거나 설정합니다.
    /// 이 속성은 필수이며 객체 초기화 시 반드시 제공되어야 합니다.
    /// </summary>
    public ICredential? Credential { get; set; }

    /// <summary>
    /// Google Cloud 프로젝트 ID를 가져오거나 설정합니다.
    /// Vertex AI 리소스가 속한 프로젝트를 식별하는 데 사용됩니다.
    /// </summary>
    public string? Project { get; set; }
    
    /// <summary>
    /// Vertex AI 서비스 리전(예: "us-central1", "asia-northeast3")을 가져오거나 설정합니다.
    /// 서비스가 실행될 지리적 위치를 지정합니다.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// HTTP 요청에 대한 추가 옵션을 가져오거나 설정합니다.
    /// 타임아웃, 재시도 정책 등을 구성할 수 있습니다.
    /// </summary>
    public HttpOptions? HttpOptions { get; set; }

    /// <summary>
    /// 구성이 유효한지 검증합니다.
    /// </summary>
    /// <returns>
    /// 모든 필수 속성(Credential, Project, Location)이 올바르게 설정되어 있으면 true,
    /// 그렇지 않으면 false를 반환합니다.
    /// </returns>
    public bool Validate()
    {
        return Credential != null 
            && !string.IsNullOrEmpty(Project) 
            && !string.IsNullOrEmpty(Location);
    }
}
