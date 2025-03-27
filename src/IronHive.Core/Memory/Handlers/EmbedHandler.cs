using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;

namespace IronHive.Core.Memory.Handlers;

public class EmbedHandler : IPipelineHandler
{
    private readonly IEmbeddingService _service;
    private readonly IVectorStorage _storage;

    public EmbedHandler(IEmbeddingService emeddings, IVectorStorage storage)
    {
        _service = emeddings;
        _storage = storage;
    }

    public class Options
    {
        public required string Collection { get; set; }
        public required string Provider { get; set; }
        public required string Model { get; set; }
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        if (pipeline.Payload.TryConvertTo<IEnumerable<Dialogue>>(out var dialogues))
        {
            var options = pipeline.GetCurrentOptions<Options>()
                ?? throw new InvalidOperationException("must provide options for embeddings handler");

            var embeddings = await _service.EmbedBatchAsync(
                options.Provider,
                options.Model,
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
                    Source = pipeline.Source,
                    Vectors = vector,
                    Payload = content,
                    LastUpdatedAt = DateTime.UtcNow,
                });
            }

            await _storage.UpsertVectorsAsync(options.Collection, points, cancellationToken);
            pipeline.Payload = null;
            return pipeline;
        }
        else
        {
            throw new InvalidOperationException("The document content is not a dialogue.");
        }
    }
}
