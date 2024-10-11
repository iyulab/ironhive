using Raggle.Abstractions.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raggle.Core.Memory;

public class EmbeddingDocumentStep : IPipelineStep<List<string>, List<(string Text, float[] Embedding)>>
{
    private readonly IEmbeddingEngine _engine;

    public EmbeddingDocumentStep(IEmbeddingEngine embeddingEngine)
    {
        _engine = embeddingEngine;
    }

    public async Task<List<(string Text, float[] Embedding)>> ProcessAsync(List<string> documents)
    {
        var embeddings = new List<(string Text, float[] Embedding)>();

        foreach (var document in documents)
        {
            var embedding = await _engine.EmbeddingAsync(document, new EmbeddingOptions
            {
                ModelId = ""
            });
            embeddings.Add((document, embedding));
        }

        return embeddings;
    }
}
