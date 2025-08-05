using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Storages;

namespace IronHive.Core.Memory.Handlers;

/// <summary>
/// VectorEmbeddingHandler는 주어진 메모리 소스에서 벡터 임베딩을 생성하고 벡터 스토리지에 저장하는 메모리 파이프라인 핸들러입니다.
/// </summary>
public class VectorEmbeddingHandler : IMemoryPipelineHandler
{
    private readonly IMemoryEmbedder _embedder;
    private readonly IVectorStorage _storage;

    public VectorEmbeddingHandler(IMemoryEmbedder embedder, IVectorStorage storage)
    {
        _embedder = embedder;
        _storage = storage;
    }

    public async Task<MemoryPipelineContext> ProcessAsync(MemoryPipelineContext context, CancellationToken cancellationToken)
    {
        var target = context.Target.ConvertTo<VectorMemoryTarget>()
            ?? throw new InvalidOperationException("target is not a VectorMemoryTarget");

        if (context.Payload.TryConvertTo<IEnumerable<Dialogue>>(out var dialogues))
        {
            var embeddings = await _embedder.EmbedBatchAsync(dialogues.Select(x => x.Question), cancellationToken);

            if (embeddings == null || embeddings.Count() != dialogues.Count())
                throw new InvalidOperationException("failed to get embeddings for dialogues");

            var points = new List<VectorRecord>();
            for (var i = 0; i < embeddings.Count(); i++)
            {
                var content = dialogues.ElementAt(i);
                var vector = embeddings.ElementAt(i).Embedding;

                if (vector == null || content == null)
                    throw new InvalidOperationException("failed to get embedding for dialogue");

                points.Add(new VectorRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceId = context.Source.Id,
                    Vectors = vector,
                    Source = context.Source,
                    Content = content,
                    LastUpdatedAt = DateTime.UtcNow,
                });
            }

            await _storage.UpsertVectorsAsync(target.CollectionName, points, cancellationToken);
            context.Payload = null;
            return context;
        }
        else if (context.Payload.TryConvertTo<IEnumerable<string>>(out var chunks))
        {
            var embeddings = await _embedder.EmbedBatchAsync(chunks, cancellationToken);

            if (embeddings == null || embeddings.Count() != chunks.Count())
                throw new InvalidOperationException("failed to get embeddings for chunks");

            var points = new List<VectorRecord>();
            for (var i = 0; i < embeddings.Count(); i++)
            {
                var content = chunks.ElementAt(i);
                var vector = embeddings.ElementAt(i).Embedding;

                if (vector == null || content == null)
                    throw new InvalidOperationException("failed to get embedding for chunk");

                points.Add(new VectorRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceId = context.Source.Id,
                    Source = context.Source,
                    Vectors = vector,
                    Content = content,
                    LastUpdatedAt = DateTime.UtcNow,
                });
            }

            await _storage.UpsertVectorsAsync(target.CollectionName, points, cancellationToken);
            context.Payload = null;
            return context;
        }
        else
        {
            throw new NotSupportedException($"Unsupported payload type: {context.Payload?.GetType().Name}.");
        }
    }
}
