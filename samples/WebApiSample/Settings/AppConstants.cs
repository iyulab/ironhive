namespace WebApiSample.Settings;

/// <summary>
/// 서비스 고정값 입니다.
/// </summary>
public static class AppConstants
{
    // 데이터 저장 경로
    public static readonly string BaseDirectoryPath = GetBaseDirectoryPath();
    public static readonly string FileDatabasePath = Path.Combine(BaseDirectoryPath, "file.db");
    public static readonly string VectorDatabasePath = Path.Combine(BaseDirectoryPath, "vector.db");
    public static readonly string QueueDirectoryPath = Path.Combine(BaseDirectoryPath, "queue");
    public static readonly string SettingsFilePath = Path.Combine("", "service_settings.json");
    
    // 파일 스토리지 커넥션
    public const string LocalFileStorage = "local";

    // AI 서비스 커넥션
    public const string OpenAIProvider = "openai";
    public const string LMStudioProvider = "lm-studio";
    public const string GPUStackProvider = "gpu-stack";

    // 파이프라인 스텝
    public const string ExtractStep = "extract";
    public const string ChunkStep = "chunk";
    public const string QnAStep = "gen_qa";
    public const string EmbeddingStep = "embedding";

    /// <summary>
    /// 데이터 저장용 디렉토리 경로를 가져옵니다
    /// </summary>
    private static string GetBaseDirectoryPath()
    {
        var special = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(special))
        {
            // LocalApplicationData 경로가 없는 OS의 경우
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        }
        else
        {
            // LocalApplicationData 경로를 가져올 수 있는 OS의 경우
            return Path.Combine(special, ".hive-sample");
        }
    }
}
