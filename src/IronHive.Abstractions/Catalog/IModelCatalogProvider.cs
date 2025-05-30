namespace IronHive.Abstractions.Catalog;

/// <summary>
/// 모델 정보를 제공하는 프로바이더 인터페이스입니다.
/// </summary>
public interface IModelCatalogProvider
{
    /// <summary>
    /// 공급자의 이름을 가져옵니다.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// 사용 가능한 모델 목록을 비동기적으로 가져옵니다.
    /// </summary>
    Task<IEnumerable<ModelSummary>> ListModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 ID에 해당하는 모델 정보를 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="modelId">모델 ID</param>
    Task<ModelDetails?> FindModelAsync(
        string modelId,
        CancellationToken cancellationToken = default);
}
