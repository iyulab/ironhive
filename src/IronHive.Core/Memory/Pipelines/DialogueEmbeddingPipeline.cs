using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Pipelines;
using IronHive.Abstractions.Storages;

namespace IronHive.Core.Memory.Pipelines;

/// <summary>
/// VectorEmbeddingHandler는 주어진 메모리 소스에서 벡터 임베딩을 생성하고,
/// 벡터 스토리지에 저장하는 메모리 파이프라인 핸들러입니다.
/// </summary>
public class DialogueEmbeddingPipeline : IPipeline<PipelineContext<IEnumerable<Dialogue>>, PipelineContext>
{
    private readonly IEmbeddingService _embedder;
    private readonly IVectorStorage _storage;

    public DialogueEmbeddingPipeline(IEmbeddingService embedder, IVectorStorage storage)
    {
        _embedder = embedder;
        _storage = storage;
    }

    /// <inheritdoc />
    public async Task<PipelineContext> InvokeAsync(
        PipelineContext<IEnumerable<Dialogue>> input, 
        CancellationToken cancellationToken = default)
    {
        if (input.Target is not VectorMemoryTarget target)
            throw new InvalidOperationException("target is not a MemoryVectorTarget");

        var embeddings = await _embedder.EmbedBatchAsync(
                target.EmbeddingProvider,
                target.EmbeddingModel,
                input.Payload!.Select(x => x.Question),
                cancellationToken);

        if (embeddings == null || embeddings.Count() != input.Payload?.Count())
            throw new InvalidOperationException("failed to get embeddings for dialogues");

        var points = new List<VectorRecord>();
        for (var i = 0; i < embeddings.Count(); i++)
        {
            var content = input.Payload.ElementAt(i);
            var vector = embeddings.ElementAt(i).Embedding;

            if (vector == null || content == null)
                throw new InvalidOperationException("failed to get embedding for dialogue");

            points.Add(new VectorRecord
            {
                Id = Guid.NewGuid().ToString(),
                Vectors = vector,
                Content = content,
                Source = input.Source,
            });
        }

        await _storage.UpsertVectorsAsync(target.CollectionName, points, cancellationToken);
        return new PipelineContext
        {
            Source = input.Source,
            Target = input.Target,
        };
    }
}
