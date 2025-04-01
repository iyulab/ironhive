namespace IronHive.Abstractions.Memory;

// <summary>
/// RAG 파이프라인의 중간 처리 결과물을 저장하기 위한 인터페이스.
/// Redis, InMemory 등 다양한 저장소 구현체에 맞춰 확장됩니다.
/// </summary>
public interface IPipelineStorage : IDisposable
{
    /// <summary>
    /// 모든 파이프라인을 불러옵니다.
    /// </summary>
    Task<IEnumerable<DataPipeline>> ListAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 id에 해당하는 파이프라인이 존재하는지 확인합니다.
    /// </summary>
    Task<bool> ContainsAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 id에 해당하는 파이프라인을 불러옵니다.
    /// </summary>
    Task<DataPipeline> GetAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 파이프라인을 저장합니다.
    /// </summary>
    Task SetAsync(
        DataPipeline pipeline,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 id에 해당하는 파이프라인을 삭제합니다.
    /// </summary>
    Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);
}
