using IronHive.Abstractions.Tools;
using IronHive.Core.Mcp;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace IronHive.Core.Tools;

/// <summary>
/// 툴 생성기
/// </summary>
public static class ToolFactory
{
    /// <summary>
    /// Creates a tool collection from an object instance.
    /// </summary>
    public static ToolCollection CreateFromObject<T>(IServiceProvider? services = null)
        where T : class
    {
        var instance = services is null
            ? Activator.CreateInstance<T>()
            : ActivatorUtilities.CreateInstance<T>(services);
        return CreateFromObject(instance);
    }

    /// <summary>
    /// Creates a tool collection from an object instance.
    /// </summary>
    public static ToolCollection CreateFromObject(object instance)
    {
        var tools = new ToolCollection();
        var methods = instance.GetType().GetMethods(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        foreach (var method in methods)
        {
            var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
            var returnType = method.ReturnType;
            var functionType = returnType == typeof(void)
                ? Expression.GetActionType(parameterTypes)
                : Expression.GetFuncType([.. parameterTypes, returnType]);
            var function = method.CreateDelegate(functionType, instance);

            tools.Add(new FunctionTool(function));
        }

        return tools;
    }

    /// <summary>
    /// Creates a tool collection from an MCP server.
    /// </summary>
    public static async Task<ToolCollection> CreateFromMcpServer(IMcpServer server)
    {
        var tools = new ToolCollection();
        
        if (server is McpStdioServer stdio)
        {
            var mcp = await McpClientFactory.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions
            {
                Command = stdio.Command,
                Name = stdio.Name,
                Arguments = stdio.Arguments?.ToList(),
                EnvironmentVariables = stdio.EnvironmentVariables,
                ShutdownTimeout = stdio.ShutdownTimeout,
                WorkingDirectory = stdio.WorkingDirectory,
            }));
            var mcpTools = await mcp.ListToolsAsync();
            foreach (var mt in mcpTools)
            {
                tools.Add(new McpTool(mt));
            }
        }
        else if (server is McpSseServer sse)
        {
            var mcp = await McpClientFactory.CreateAsync(new SseClientTransport(new SseClientTransportOptions
            {
                Endpoint = sse.Endpoint,
                Name = sse.Name,
                AdditionalHeaders = sse.AdditionalHeaders,
                ConnectionTimeout = sse.ConnectionTimeout,
                MaxReconnectAttempts = sse.MaxReconnectAttempts,
                ReconnectDelay = sse.ReconnectDelay,
            }));
            var mcpTools = await mcp.ListToolsAsync();
            foreach (var mt in mcpTools)
            {
                tools.Add(new McpTool(mt));
            }
        }
        else
        {
            throw new NotSupportedException($"Server type {server.GetType().Name} is not supported.");
        }

        return tools;
    }
}
