namespace IronHive.Abstractions.Catalog;

/// <summary>
/// 전체 모델 정보를 제공하는 서비스 입니다.
/// </summary>
public interface IModelCatalogService
{
    /// <summary>
    /// 사용 가능한 모든 모델 목록을 비동기적으로 가져옵니다.
    /// </summary>
    Task<IEnumerable<ModelSpecList>> ListModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 공급자의 모든 모델 목록을 비동기적으로 가져옵니다.
    /// 찾는 공급자가 없으면 빈 목록을 반환합니다.
    /// </summary>
    Task<ModelSpecList?> ListModelsAsync(
        string provider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 공급자와 모델 ID를 기준으로 모델 정보를 비동기적으로 가져옵니다.
    /// 찾는 모델이 없으면 null을 반환합니다.
    /// </summary>
    /// <param name="provider">모델 공급자 키</param>
    /// <param name="modelId">모델 ID</param>
    Task<IModelSpec?> FindModelAsync(
        string provider,
        string modelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 타입의 모든 모델 목록을 비동기적으로 가져옵니다.
    /// </summary>
    Task<IEnumerable<ModelSpecList<T>>> ListModelsAsync<T>(
        CancellationToken cancellationToken = default)
        where T : class, IModelSpec;

    /// <summary>
    /// 지정된 공급자와 타입의 모든 모델 목록을 비동기적으로 가져옵니다.
    /// 찾는 공급자가 없으면 null을 반환합니다.
    /// </summary>
    Task<ModelSpecList<T>?> ListModelsAsync<T>(
        string provider,
        CancellationToken cancellationToken = default)
        where T : class, IModelSpec;

    /// <summary>
    /// 지정된 공급자와 모델 ID와 타입을 기준으로 모델 정보를 비동기적으로 가져옵니다.
    /// 찾는 모델이 없으면 null을 반환합니다.
    /// </summary>
    /// <param name="provider">모델 공급자 키</param>
    /// <param name="modelId">모델 ID</param>
    Task<T?> FindModelAsync<T>(
        string provider,
        string modelId,
        CancellationToken cancellationToken = default)
        where T : class, IModelSpec;
}
