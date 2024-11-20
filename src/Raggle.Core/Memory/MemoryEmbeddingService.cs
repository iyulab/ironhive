using Raggle.Abstractions.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raggle.Core.Memory;

public class MemoryEmbeddingService
{
    private readonly string modelName;
    private readonly IEmbeddingService _service;

    public MemoryEmbeddingService(string modelName, IEmbeddingService service)
    {
        this.modelName = modelName;
        _service = service;
    }

    public async Task<IEnumerable<EmbeddingResponse>> EmbeddingsAsync(
        string[] input, 
        CancellationToken cancellationToken = default)
    {
        var request = new EmbeddingRequest
        {
            Model = modelName,
            Input = input,
        };
        return await _service.EmbeddingsAsync(request, cancellationToken);
    }

}
