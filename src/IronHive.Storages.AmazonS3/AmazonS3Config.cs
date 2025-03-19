using System.Text.Json.Serialization;

namespace IronHive.Storages.AmazonS3;

/// <summary>
/// Amazon S3에 연결하기 위한 다양한 인증 유형을 나타냅니다.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AmazonS3AuthTypes
{
    /// <summary>
    /// AWS IAM Access Key와 Secret Access Key를 사용한 인증 방식입니다.
    /// </summary>
    AccessKey,

    /// <summary>
    /// AWS Credential Chain을 사용한 인증 방식입니다.
    /// </summary>
    CredentialChain,
}

/// <summary>
/// Amazon S3 스토리지에 연결하기 위한 구성 클래스입니다.
/// 선택한 <see cref="Auth"/>에 따라 필요한 필드가 달라집니다.
/// </summary>
public class AmazonS3Config
{
    /// <summary>
    /// 사용될 인증 방법을 결정합니다.
    /// 기본값은 <see cref="AmazonS3AuthTypes.AccessKey"/>입니다.
    /// </summary>
    public AmazonS3AuthTypes Auth { get; set; } = AmazonS3AuthTypes.AccessKey;

    /// <summary>
    /// AWS IAM Access Key (키 이름).
    /// <see cref="Auth"/>가 <see cref="AmazonS3AuthTypes.AccessKey"/>로 설정된 경우 필요합니다.
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// AWS IAM Secret Access Key (비밀번호).
    /// <see cref="Auth"/>가 <see cref="AmazonS3AuthTypes.AccessKey"/>로 설정된 경우 필요합니다.
    /// </summary>
    public string SecretAccessKey { get; set; } = string.Empty;

    /// <summary>
    /// AWS S3 엔드포인트, 예: https://s3.us-west-2.amazonaws.com.
    /// S3 호환 서비스나 개발 도구에서도 사용할 수 있습니다.
    /// </summary>
    public string Endpoint { get; set; } = "https://s3.amazonaws.com";

    /// <summary>
    /// S3 버킷 이름입니다.
    /// </summary>
    public string BucketName { get; set; } = string.Empty;
}
