using Raggle.Abstractions.AI;

namespace Raggle.Abstractions.Memory;

public class MemoryEmbedder
{
    public string ModelName { get; set; }

    public IEmbeddingService Service { get; set; }

    public MemoryEmbedder(string modelName, IEmbeddingService service)
    {
        ModelName = modelName;
        Service = service;
    }

    public async Task<IEnumerable<EmbeddingResponse>> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        var request = new EmbeddingRequest
        {
            Model = ModelName,
            Input = [text],
        };
        return await Service.EmbeddingsAsync(request, cancellationToken);
    }
}
