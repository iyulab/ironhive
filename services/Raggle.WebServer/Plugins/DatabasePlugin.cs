using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Raggle.Server.Web.Plugins;

public class DatabasePlugin
{
    public DatabasePlugin()
    {
    }

    [KernelFunction, Description("")]
    public string GetSchemaInformation(Guid connectionId)
    {
        return "";
    }

    [KernelFunction, Description("")]
    public string QueryDatabase(Guid connectionId, string query)
    {
        return "";
    }

    [KernelFunction, Description("")]
    public string ExecuteDatabase(Guid connectionId, string query)
    {
        return "";
    }
}
