namespace IronHive.Abstractions.Catalog;

/// <summary>
/// 전체 모델 정보를 제공하는 서비스 입니다.
/// </summary>
public interface IModelCatalogService
{
    /// <summary>
    /// 사용 가능한 모든 모델 목록을 비동기적으로 가져옵니다.
    /// provider가 지정되지 않은 경우 모든 공급자의 모델을 포함합니다.
    /// </summary>
    Task<IEnumerable<ModelSpecItem>> ListModelsAsync(
        string? provider = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 타입의 모든 모델 목록을 비동기적으로 가져옵니다.
    /// provider가 지정되지 않은 경우 모든 공급자에서 해당 타입의 모델을 포함합니다.
    /// </summary>
    Task<IEnumerable<ModelSpecItem<T>>> ListModelsAsync<T>(
        string? provider = null,
        CancellationToken cancellationToken = default)
        where T : class, IModelSpec;

    /// <summary>
    /// 지정된 공급자와 모델 ID를 기준으로 모델 정보를 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="provider">모델 공급자 키</param>
    /// <param name="modelId">모델 ID</param>
    Task<ModelSpecItem?> FindModelAsync(
        string provider,
        string modelId,
        CancellationToken cancellationToken = default);
}
