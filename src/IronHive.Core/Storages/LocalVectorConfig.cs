namespace IronHive.Core.Storages;

/// <summary>
/// 로컬 벡터 데이터베이스 LiteDB 설정 클래스입니다.
/// </summary>
public class LocalVectorConfig
{
    /// <summary>
    /// 벡터 데이터베이스 파일 경로입니다.
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// 파일 암호 (암호화 필요 시 설정)
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 파일 잠금 해제 (다중 프로세스 공유 가능, default: true)
    /// </summary>
    public bool Shared { get; set; } = true;

    /// <summary>
    /// 파일 손상 시 자동 복구 여부 (default: false)
    /// </summary>
    public bool AutoRebuild { get; set; } = false;

    /// <summary>
    /// LiteDB 버전 업그레이드 허용 여부 (default: false)
    /// </summary>
    public bool Upgrade { get; set; } = false;
}