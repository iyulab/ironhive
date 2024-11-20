using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Tools;
using System.ComponentModel;

namespace Raggle.Core.ToolKit;

public class VectorSearchToolKit
{
    private readonly IRaggleMemory _memory;

    public VectorSearchToolKit(IRaggleMemory memory)
    {
        _memory = memory;
    }

    [FunctionTool("Search", "Search for a vector in a collection")]
    public async Task Search(
        [Description("Collection name search for")] string collectionName,
        [Description("The query to search for")] string query)
    {
        await _memory.SearchDocumentMemoryAsync(collectionName, query);
    }
}
