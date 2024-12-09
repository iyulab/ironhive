using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Json;
using Raggle.Abstractions.Memory;
using Raggle.Core.Extensions;
using Raggle.Core.Memory.Document;

namespace Raggle.Core.Memory.Handlers;

public class EmbeddingsHandler : IPipelineHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentStorage _documentStorage;
    private readonly IVectorStorage _vectorStorage;

    public EmbeddingsHandler(IServiceProvider service)
    {
        _serviceProvider = service;
        _documentStorage = service.GetRequiredService<IDocumentStorage>();
        _vectorStorage = service.GetRequiredService<IVectorStorage>();
    }

    public class Options
    {
        public string ServiceKey { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var options = pipeline.GetCurrentOptions<Options>()
            ?? throw new InvalidOperationException("must provide options for embeddings handler");
        var embedder = _serviceProvider.GetRequiredKeyedService<IEmbeddingService>(options.ServiceKey);
        
        var points = new List<VectorPoint>();

        await foreach (var section in _documentStorage.GetDocumentJsonAsync<DocumentSection>(
            collectionName: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            suffix: pipeline.GetPreviousStep() ?? "unknown",
            cancellationToken: cancellationToken))
        {
            if (section.Content == null)
                throw new InvalidOperationException($"No content found in the document");

            if (JsonObjectConverter.TryConvertTo<string>(section.Content, out string str))
            {
                var response = await embedder.EmbeddingAsync(options.ModelName, str, cancellationToken);
                points.Add(new VectorPoint
                {
                    VectorId = Guid.NewGuid(),
                    Vectors = response.Embedding,
                    DocumentId = pipeline.DocumentId,
                    Tags = pipeline.Tags?.ToArray(),
                    Payload = section
                });
            }
            else if (JsonObjectConverter.TryConvertTo<IEnumerable<Tuple<string,string>>>(section.Content, out var dialogues))
            {
                var questions = dialogues.Select(x => x.Item1).ToArray();
                var request = new EmbeddingRequest
                {
                    Model = options.ModelName,
                    Input = questions
                };
                var response = await embedder.EmbeddingsAsync(request, cancellationToken);
                for (var i = 0; i < response.Count(); i++)
                {
                    section.Content = dialogues.ElementAt(i);
                    points.Add(new VectorPoint
                    {
                        VectorId = Guid.NewGuid(),
                        Vectors = response.ElementAt(i).Embedding,
                        DocumentId = pipeline.DocumentId,
                        Tags = pipeline.Tags?.ToArray(),
                        Payload = section
                    });
                }
            }
            else
            {
                throw new InvalidOperationException($"No content found in the document");
            }
        }

        await _vectorStorage.UpsertVectorsAsync(pipeline.CollectionName, points, cancellationToken);
        return pipeline;
    }
}
