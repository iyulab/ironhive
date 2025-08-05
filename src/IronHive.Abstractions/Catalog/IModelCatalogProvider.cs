namespace IronHive.Abstractions.Catalog;

/// <summary>
/// 모델 정보를 제공하는 서비스의 인터페이스입니다.
/// </summary>
public interface IModelCatalogProvider
{
    /// <summary>
    /// 공급자의 이름을 가져옵니다.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// 사용 가능한 모델 목록을 비동기적으로 반환합니다.
    /// </summary>
    /// <returns>모델 요약 정보 목록</returns>
    Task<IEnumerable<ModelSummary>> ListModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 ID를 가진 모델의 세부 정보를 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="modelId">조회할 모델의 ID</param>
    /// <returns>모델의 세부 정보, 없으면 null</returns>
    Task<ModelDetails?> FindModelAsync(
        string modelId,
        CancellationToken cancellationToken = default);
}