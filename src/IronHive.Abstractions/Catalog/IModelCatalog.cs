using IronHive.Abstractions.Registries;

namespace IronHive.Abstractions.Catalog;

/// <summary>
/// 모델 메타데이터(스펙)를 조회하는 서비스입니다.
/// </summary>
public interface IModelCatalog : IProviderItem
{
    /// <summary>
    /// 현재 사용 가능한 모든 모델 스펙을 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="cancellationToken">작업 취소 토큰.</param>
    /// <returns>모델 스펙의 나열.</returns>
    Task<IEnumerable<IModelSpec>> ListModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정한 ID의 모델 스펙을 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="modelId">조회할 모델의 ID.</param>
    /// <param name="cancellationToken">작업 취소 토큰.</param>
    /// <returns>일치하는 스펙. 없으면 <c>null</c>.</returns>
    Task<IModelSpec?> FindModelAsync(
        string modelId,
        CancellationToken cancellationToken = default);
}