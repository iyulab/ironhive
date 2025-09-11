using DocumentFormat.OpenXml.VariantTypes;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Vector;
using IronHive.Abstractions.Workflow;

namespace IronHive.Core.Memory.Pipelines;

/// <summary>
/// VectorEmbeddingHandler는 주어진 메모리 소스에서 벡터 임베딩을 생성하고,
/// 벡터 스토리지에 저장하는 메모리 파이프라인 핸들러입니다.
/// </summary>
public class EmbeddingToStoragePipeline : IMemoryPipeline
{
    private readonly IStorageRegistry _storages;
    private readonly IEmbeddingService _embedder;

    public EmbeddingToStoragePipeline(IStorageRegistry storages, IEmbeddingService embedder)
    {
        _embedder = embedder;
        _storages = storages;
    }

    /// <inheritdoc />
    public async Task<TaskStepResult> ExecuteAsync(
        MemoryContext context, 
        CancellationToken cancellationToken = default)
    {
        if (context.Target is not VectorMemoryTarget target)
            throw new InvalidOperationException("target is not a MemoryVectorTarget");

        var points = new List<VectorRecord>();
        if (context.Payload is IEnumerable<Dialogue> dialogues)
        {
            var embeddings = await _embedder.EmbedBatchAsync(
                    target.EmbeddingProvider,
                    target.EmbeddingModel,
                    dialogues.Select(x => x.Question),
                    cancellationToken);

            if (embeddings == null || embeddings.Count() != dialogues.Count())
                throw new InvalidOperationException("failed to get embeddings for dialogues");

            for (var i = 0; i < embeddings.Count(); i++)
            {
                var content = dialogues.ElementAt(i);
                var vector = embeddings.ElementAt(i).Embedding;

                if (vector == null || content == null)
                    throw new InvalidOperationException("failed to get embedding for dialogue");

                points.Add(new VectorRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    Vectors = vector,
                    Content = content,
                    Source = context.Source,
                });
            }
        }
        else if (context.Payload is IEnumerable<string> texts)
        {
            var embeddings = await _embedder.EmbedBatchAsync(
               target.EmbeddingProvider,
               target.EmbeddingModel,
               texts,
               cancellationToken);

            if (embeddings == null || embeddings.Count() != texts.Count())
                throw new InvalidOperationException("failed to get embeddings for texts");

            for (var i = 0; i < embeddings.Count(); i++)
            {
                var content = texts.ElementAt(i);
                var vector = embeddings.ElementAt(i).Embedding;

                if (vector == null || content == null)
                    throw new InvalidOperationException("failed to get embedding for dialogue");

                points.Add(new VectorRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    Vectors = vector,
                    Content = content,
                    Source = context.Source,
                });
            }
        }
        else
        {
            return TaskStepResult.Fail(new InvalidOperationException("payload is not a IEnumerable<Dialogue> or IEnumerable<string>"));
        }

        if (!_storages.TryGet<IVectorStorage>(target.StorageName, out var storage))
            throw new InvalidOperationException($"Vector storage '{target.StorageName}' is not registered.");

        await storage.UpsertVectorsAsync(target.CollectionName, points, cancellationToken);
        return TaskStepResult.Success();
    }
}
