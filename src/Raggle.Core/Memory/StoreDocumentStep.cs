using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raggle.Core.Memory;

public class StoreDocumentStep : IPipelineStep<List<(string Text, float[] Embedding)>, bool>
{
    public Task<bool> ProcessAsync(List<(string Text, float[] Embedding)> documents)
    {
        var result = false;

        foreach (var document in documents)
        {
            result = true;
        }

        return Task.FromResult(result);
    }
}
