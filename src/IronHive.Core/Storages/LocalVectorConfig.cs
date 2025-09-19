using IronHive.Core.Utilities;

namespace IronHive.Core.Storages;

/// <summary>
/// SqliteVec(vec0)을 사용하는 로컬 벡터 스토리지 설정입니다.
/// </summary>
public class LocalVectorConfig
{
    /// <summary>
    /// Sqlite 데이터 베이스의 파일 경로 
    /// </summary>
    public required string DatabasePath { get; set; }

    /// <summary>
    /// sqlite-vec 모듈 버전 (기본값: 최신) 
    /// </summary>
    public string Version { get; set; } = SqliteVecInstaller.DefaultVersion;
}
