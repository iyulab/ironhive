namespace IronHive.Abstractions.Models;

/// <summary>
/// 모델 카드를 조회하는 서비스입니다.
/// </summary>
public interface IModelFinder : IDisposable
{
    /// <summary>
    /// 현재 사용 가능한 모든 모델 카드를 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="cancellationToken">작업 취소 토큰.</param>
    /// <returns>모델 카드의 나열.</returns>
    Task<IEnumerable<IModelCard>> ListModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정한 ID의 모델 카드를 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="modelId">조회할 모델의 ID.</param>
    /// <param name="cancellationToken">작업 취소 토큰.</param>
    /// <returns>일치하는 카드. 서버가 그런 모델이 없다고 답하면 <c>null</c>.</returns>
    /// <remarks>
    /// <c>null</c> 은 «그런 모델 없음» 만 뜻한다. 조회 자체가 실패하면(거부된 키, 서버 오류, 닿지 않는 서버)
    /// <see cref="ListModelsAsync"/> 와 같은 예외를 던진다.
    /// </remarks>
    Task<IModelCard?> FindModelAsync(
        string modelId,
        CancellationToken cancellationToken = default);
}