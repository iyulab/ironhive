namespace IronHive.Storages.AmazonS3;

/// <summary>
/// Amazon S3 스토리지에 연결하기 위한 구성 클래스입니다.
/// AWS IAM 서비스의 자격증명을 사용하여 접속합니다.
/// </summary>
public class AmazonS3Config
{
    /// <summary>
    /// AWS 자격증명 IAM Access Key (키 이름).
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// AWS 자격증명 IAM Secret Access Key (비밀번호).
    /// </summary>
    public string SecretAccessKey { get; set; } = string.Empty;

    /// <summary>
    /// AWS 서비스의 지역 코드입니다.
    /// 예) us-east-1, ap-northeast-2, eu-west-1
    /// </summary>
    public string RegionCode { get; set; } = string.Empty;

    /// <summary>
    /// S3 버킷 이름입니다.
    /// </summary>
    public string BucketName { get; set; } = string.Empty;
}
