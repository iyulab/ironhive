using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raggle.Core.Memory;

public class ParseDocumentStep : IPipelineStep<string, List<string>>
{
    public Task<List<string>> ProcessAsync(string folderPath)
    {
        var texts = new List<string>();
        var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            if (Path.GetExtension(file).Equals(".txt", StringComparison.OrdinalIgnoreCase))
            {
                var content = File.ReadAllText(file);
                texts.Add(content);
            }
        }

        return Task.FromResult(texts);
    }
}
