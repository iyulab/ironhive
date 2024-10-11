using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raggle.Core.Memory;

public class SplitDocumentStep : IPipelineStep<List<string>, List<string>>
{
    public Task<List<string>> ProcessAsync(List<string> documents)
    {
        var words = new List<string>();

        foreach (var document in documents)
        {
            var tokens = document.Split(new char[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            words.AddRange(tokens);
        }

        return Task.FromResult(words);
    }
}
