using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Tools;
using Raggle.Server.Services;
using System.ComponentModel;

namespace Raggle.Server.ToolKits;

public class VectorSearchToolKit
{
    private readonly MemoryService _memory;

    public VectorSearchToolKit(IServiceProvider serviceProvider)
    {
        _memory = serviceProvider.GetRequiredService<MemoryService>();
    }

    [FunctionTool("vector_search", "search vectordb for internel information")]
    public async Task<IEnumerable<ScoredVectorPoint>> SearchVectorAsync(
        [Description("memory collectionName")] string collectionName,
        [Description("search query")] string query)
    {
        return await _memory.SearchDocumentAsync(collectionName, query);
    }
}
