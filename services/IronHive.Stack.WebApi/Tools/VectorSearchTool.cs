using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.Memory;
using IronHive.Core.ChatCompletion;
using IronHive.Stack.WebApi.Services;
using System.ComponentModel;
using IronHive.Abstractions.ChatCompletion.Tools;

namespace IronHive.Stack.WebApi.ChatCompletion;

public class VectorSearchTool
{
    private readonly IServiceProvider _provider;

    public VectorSearchTool(IServiceProvider provider)
    {
        _provider = provider;
    }

    [FunctionTool("vector_search")]
    [Description("search vector db for internel information")]
    public async Task<IEnumerable<ScoredVectorPoint>> SearchVectorAsync(
        [Description("memory collectionName")] string collectionName,
        [Description("search query")] string query)
    {
        using var scope = _provider.CreateScope();
        var memory = scope.ServiceProvider.GetRequiredService<MemoryService>();
        var points = await memory.SearchDocumentAsync(collectionName, query);
        return points;
    }
}
