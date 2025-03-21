using MessagePack;
using MessagePack.Resolvers;

namespace IronHive.Abstractions.Memory;

/// <summary>
/// RAG 파이프라인의 중간 처리 결과물을 저장하기 위한 기본 추상 클래스.
/// 데이터 압축 직렬화를 위한 메서드를 제공합니다.
/// </summary>
public abstract class PipelineStorageBase : IPipelineStorage
{
    private readonly MessagePackSerializerOptions _options = ContractlessStandardResolver.Options
        .WithCompression(MessagePackCompression.Lz4Block);

    /// <summary>
    /// 값을 압축 직렬화합니다.
    /// </summary>
    protected virtual byte[] Serialize<T>(
        T value, 
        CancellationToken cancellationToken = default)
    {
        return MessagePackSerializer.Serialize(value, _options, cancellationToken);
    }

    /// <summary>
    /// 값을 역직렬화합니다.
    /// </summary>
    protected virtual T Deserialize<T>(
        byte[] value, 
        CancellationToken cancellationToken = default)
    {
        return MessagePackSerializer.Deserialize<T>(value, _options, cancellationToken);
    }

    /// <inheritdoc />
    public abstract void Dispose();

    /// <inheritdoc />
    public abstract Task<bool> ContainsKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract Task<IEnumerable<string>> GetKeysAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract Task<T> GetValueAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract Task SetValueAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract Task DeleteKeyAsync(string key, CancellationToken cancellationToken = default);
}
