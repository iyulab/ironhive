namespace IronHive.Abstractions.Catalog;

/// <summary>
/// 모델 정보를 제공하는 서비스 인터페이스입니다.
/// </summary>
public interface IModelCatalogService
{
    /// <summary>
    /// 사용 가능한 모든 모델 목록을 비동기적으로 가져옵니다.
    /// provider가 지정되지 않은 경우 모든 공급자의 모델을 포함합니다.
    /// </summary>
    Task<IEnumerable<ModelSummary>> ListModelsAsync(
        string? provider = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 공급자와 모델 ID를 기준으로 모델 정보를 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="provider">모델 공급자 키</param>
    /// <param name="modelId">모델 ID</param>
    Task<ModelDetails?> FindModelAsync(
        string provider,
        string modelId,
        CancellationToken cancellationToken = default);
}
