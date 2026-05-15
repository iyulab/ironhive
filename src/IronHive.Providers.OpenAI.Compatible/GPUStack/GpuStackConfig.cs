namespace IronHive.Providers.OpenAI.Compatible.GpuStack;

/// <summary>
/// GPUStack service configuration. GPUStack serves OpenAI-compatible APIs at /v1-openai/.
/// </summary>
public class GpuStackConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "http://localhost:8080";
    private const string ApiPath = "/v1-openai/";

    /// <summary>
    /// GPUStack server base URL (without path). Default: http://localhost:8080
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <inheritdoc/>
    public override OpenAIConfig ToOpenAI() => new()
    {
        BaseUrl = (string.IsNullOrEmpty(BaseUrl) ? DefaultBaseUrl : BaseUrl).TrimEnd('/') + ApiPath,
        ApiKey = ApiKey ?? string.Empty,
    };
}
