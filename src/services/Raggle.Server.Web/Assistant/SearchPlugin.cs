using Microsoft.SemanticKernel;
using Raggle.Server.API.Storages;
using Raggle.Server.Web.Repositories;
using System.ComponentModel;

namespace Raggle.Server.Web.Assistant;

public class SearchPlugin
{
    private readonly SourceRepository _source;
    private readonly VectorStorage _vector;

    public SearchPlugin(SourceRepository sourceRepository, VectorStorage vectorStorage)
    {
        _source = sourceRepository;
        _vector = vectorStorage;
    }

    [KernelFunction, Description("")]
    public string SearchSources(Guid userId)
    {
        return "";
    }

    [KernelFunction, Description("")]
    private string SearchVectorFile(Guid sourceId)
    {
        return "";
    }

    [KernelFunction, Description("")]
    public string SearchDatabaseSchema(Guid sourceId)
    {
        return "";
    }

    [KernelFunction, Description("")]
    public string QueryDatabase(Guid sourceId, string query)
    {
        return "";
    }

    [KernelFunction, Description("")]
    public string CallOpenApi(Guid sourceId)
    {
        return "";
    }
}
