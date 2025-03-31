namespace IronHive.Abstractions.Memory;

// <summary>
/// RAG 파이프라인의 중간 처리 결과물을 저장하기 위한 인터페이스.
/// Redis, InMemory 등 다양한 저장소 구현체에 맞춰 확장됩니다.
/// </summary>
public interface IPipelineStorage : IDisposable
{
    /// <summary>
    /// 저장된 키 목록을 불러옵니다.
    /// </summary>
    /// <returns>저장된 키 목록</returns>
    Task<IEnumerable<string>> GetKeysAsync(
        string? prefix = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 키에 데이터가 존재하는지 확인합니다.
    /// </summary>
    /// <param name="key">데이터 식별을 위한 키</param>
    /// <returns>데이터가 존재하면 true, 아니면 false</returns>
    Task<bool> ContainsKeyAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 키에 해당하는 데이터를 불러옵니다.
    /// </summary>
    /// <typeparam name="T">불러올 데이터의 타입</typeparam>
    /// <param name="key">데이터 식별을 위한 키</param>
    /// <returns>저장된 값</returns>
    Task<T> GetValueAsync<T>(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 키를 사용하여 데이터를 저장합니다.
    /// </summary>
    /// <typeparam name="T">저장할 데이터의 타입</typeparam>
    /// <param name="key">데이터 식별을 위한 키</param>
    /// <param name="value">저장할 값</param>
    Task SetValueAsync<T>(
        string key, 
        T value, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 키에 해당하는 데이터를 삭제합니다.
    /// </summary>
    /// <param name="key">데이터 식별을 위한 키</param>
    Task DeleteKeyAsync(
        string key,
        CancellationToken cancellationToken = default);
}
