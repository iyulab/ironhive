using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;

namespace IronHive.Core.Handlers;

public class VectorEmbeddingHandler : IPipelineHandler
{
    private readonly IEmbeddingService _service;
    private readonly IVectorStorage _storage;

    public VectorEmbeddingHandler(IEmbeddingService emeddings, IVectorStorage storage)
    {
        _service = emeddings;
        _storage = storage;
    }

    public async Task<PipelineContext> ProcessAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var target = context.Target.ConvertTo<VectorMemoryTarget>()
            ?? throw new InvalidOperationException("target is not a VectorMemoryTarget");

        if (context.Payload.TryConvertTo<IEnumerable<Dialogue>>(out var dialogues))
        {
            var embeddings = await _service.EmbedBatchAsync(
                target.EmbedProvider,
                target.EmbedModel,
                dialogues.Select(x => x.Question),
                cancellationToken);

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
            var embeddings = await _service.EmbedBatchAsync(
                target.EmbedProvider,
                target.EmbedModel,
                chunks,
                cancellationToken);

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
