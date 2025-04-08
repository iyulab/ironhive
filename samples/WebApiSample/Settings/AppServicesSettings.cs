using System.Text.Json;
using System.Threading.Tasks;

namespace WebApiSample.Settings;

/// <summary>
/// 서비스 설정 인터페이스
/// </summary>
public interface IServiceSettings
{
    /// <summary>
    /// 서비스 설정을 검증합니다.
    /// </summary>
    /// <param name="reason">설정이 유효하지 않은 경우 그 이유를 설명합니다.</param>
    /// <returns>true: 유효한 설정, false: 유효하지 않은 설정</returns>
    bool TryValidate(out string? reason);
}

/// <summary>
/// 서비스 설정
/// </summary>
public class AppServicesSettings : IServiceSettings
{
    /// <summary>
    /// AI 서비스 설정
    /// </summary>
    public ConnectorsSettings Connectors { get; set; } = new ConnectorsSettings();

    /// <summary>
    /// 메모리 서비스 설정
    /// </summary>
    public MemorySettings Memory { get; set; } = new MemorySettings();

    /// <inheritdoc />
    public bool TryValidate(out string? reason)
    {
        reason = null;
        if (Connectors.TryValidate(out var connectorsReason) == false)
            reason = connectorsReason;
        else if (Memory.TryValidate(out var memoryReason) == false)
            reason = memoryReason;
        return string.IsNullOrEmpty(reason);
    }

    /// <summary>
    /// 설정을 지정된 파일 경로에 저장합니다.
    /// </summary>
    public async Task SaveAsync(string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = new
        {
            Services = this,
        };
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(json, options));
    }
}

/// <summary>
/// AI 서비스 설정
/// </summary>
public class ConnectorsSettings : IServiceSettings
{
    /// <summary>
    /// OpenAI 서비스 설정
    /// </summary>
    public OpenAIConnectorSettings OpenAI { get; set; } = new OpenAIConnectorSettings();

    /// <summary>
    /// LM Studio 서비스 설정
    /// </summary>
    public LMStudioConnectorSettings LMStudio { get; set; } = new LMStudioConnectorSettings();

    /// <summary>
    /// GPU Stack 서비스 설정
    /// </summary>
    public GPUStackConnectorSettings GPUStack { get; set; } = new GPUStackConnectorSettings();

    /// <summary>
    /// 하나의 커넥션이라도 유효하면 true 입니다.
    /// </summary>
    public bool TryValidate(out string? reason)
    {
        if (OpenAI.TryValidate(out reason))
            return true;   
        if (LMStudio.TryValidate(out reason))
            return true;
        if (GPUStack.TryValidate(out reason))
            return true;
        return false;
    }

    public class OpenAIConnectorSettings : IServiceSettings, IEquatable<OpenAIConnectorSettings>
    {
        public string ApiKey { get; set; } = string.Empty;

        public string Organization { get; set; } = string.Empty;

        public string Project { get; set; } = string.Empty;

        /// <inheritdoc />
        public bool TryValidate(out string? reason)
        {
            reason = null;
            if (string.IsNullOrWhiteSpace(ApiKey))
                reason = "OpenAI API Key is required.";
            return string.IsNullOrEmpty(reason);
        }

        public bool Equals(OpenAIConnectorSettings? other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(ApiKey, other.ApiKey) &&
                   string.Equals(Organization, other.Organization) &&
                   string.Equals(Project, other.Project);
        }
    }

    public class LMStudioConnectorSettings : IServiceSettings, IEquatable<LMStudioConnectorSettings>
    {
        public string BaseUrl { get; set; } = "http://localhost:1234/v1";

        public string ApiKey { get; set; } = string.Empty;

        /// <inheritdoc />
        public bool TryValidate(out string? reason)
        {
            reason = null;
            if (string.IsNullOrWhiteSpace(BaseUrl))
                reason = "LM Studio Base URL is required.";
            if (string.IsNullOrWhiteSpace(ApiKey))
                reason = "LM Studio API Key is required.";
            return string.IsNullOrEmpty(reason);
        }

        public bool Equals(LMStudioConnectorSettings? other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(BaseUrl, other.BaseUrl) &&
                   string.Equals(ApiKey, other.ApiKey);
        }
    }

    public class GPUStackConnectorSettings : IServiceSettings, IEquatable<GPUStackConnectorSettings>
    {
        public string BaseUrl { get; set; } = "http://localhost:8000/v1-openai/";

        public string ApiKey { get; set; } = string.Empty;

        /// <inheritdoc />
        public bool TryValidate(out string? reason)
        {
            reason = null;
            if (string.IsNullOrWhiteSpace(BaseUrl))
                reason = "GPU Stack Base URL is required.";
            if (string.IsNullOrWhiteSpace(ApiKey))
                reason = "GPU Stack API Key is required.";
            return string.IsNullOrEmpty(reason);
        }

        public bool Equals(GPUStackConnectorSettings? other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(BaseUrl, other.BaseUrl) &&
                   string.Equals(ApiKey, other.ApiKey);
        }
    }
}

/// <summary>
/// 메모리 서비스 설정
/// </summary>
public class MemorySettings : IServiceSettings, IEquatable<MemorySettings>
{
    /// <summary>
    /// 사용할 임베딩 모델 식별자 (ex. "openai/text-embedding-ada-002")
    /// </summary>
    public string EmbeddingModel { get; set; } = string.Empty;

    /// <summary>
    /// 파일 텍스트를 벡터화하기 위한 청크 크기
    /// </summary>
    public int ChunkSize { get; set; } = 800;

    /// <summary>
    /// QnA 생성에 사용할 채팅 모델 식별자 (ex. "openai/gpt-4o-mini")
    /// </summary>
    public string QnAGenerationModel { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool TryValidate(out string? reason)
    {
        reason = null;
        if (string.IsNullOrWhiteSpace(EmbeddingModel))
            reason = "Embedding model is required.";
        if (ChunkSize < 1 || ChunkSize > 4096)
            reason = "Chunk size must be between 1 and 4096.";
        return string.IsNullOrEmpty(reason);
    }

    public bool Equals(MemorySettings? other)
    {
        if (other == null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return string.Equals(EmbeddingModel, other.EmbeddingModel) &&
               ChunkSize == other.ChunkSize &&
               string.Equals(QnAGenerationModel, other.QnAGenerationModel);
    }
}
