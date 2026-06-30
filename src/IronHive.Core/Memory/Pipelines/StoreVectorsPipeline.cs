using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Vector;
using IronHive.Abstractions.Workflow;

namespace IronHive.Core.Memory.Pipelines;

/// <summary>
/// 벡터 스토리지에 벡터들을 저장하는 메모리 파이프라인 핸들러입니다.
/// </summary>
public class StoreVectorsPipeline : IMemoryPipeline
{
    private readonly IReadOnlyDictionary<string, IVectorStorage> _storages;

    public StoreVectorsPipeline(IReadOnlyDictionary<string, IVectorStorage> storages)
    {
        _storages = storages;
    }

    /// <inheritdoc />
    public async Task<TaskStepResult> ExecuteAsync(
        MemoryContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.Target is not VectorMemoryTarget target)
            throw new InvalidOperationException("target is not a MemoryVectorTarget");
        if (!_storages.TryGetValue(target.StorageName, out var storage))
            throw new InvalidOperationException($"Vector storage '{target.StorageName}' is not registered.");
        if (!context.Payload.TryGetValue<IEnumerable<VectorRecord>>("vectors", out var vectors))
            throw new InvalidOperationException("vectors not found in context items");

        await storage.UpsertVectorsAsync(target.CollectionName, vectors, cancellationToken);
        return TaskStepResult.Success();
    }
}
