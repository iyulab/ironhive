using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace IronHive.Core.Services;

public class ToolHandlerManager : IToolHandlerManager
{
    private readonly IServiceProvider _services;

    public ToolHandlerManager(IServiceProvider services)
    {
        _services = services;
    }

    /// <inheritdoc />
    public async Task<string> HandleSetInstructionsAsync(string serviceKey, object? options)
    {
        var handler = GetToolHandler(serviceKey);
        return await handler.HandleSetInstructionsAsync(options);
    }

    /// <inheritdoc />
    public async Task HandleInitializedAsync(string serviceKey, object? options)
    {
        var handler = GetToolHandler(serviceKey);
        await handler.HandleInitializedAsync(options);
    }

    /// <inheritdoc />
    public ICollection<ITool> CreateToolCollection(string serviceKey)
    {
        var handler = GetToolHandler(serviceKey);
        var methods = handler.GetType().GetMethods(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        var tools = new List<ITool>();
        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<FunctionToolAttribute>();
            if (attr is null) continue;

            var name = attr.Name ?? method.Name;
            var approval = attr.RequiresApproval;
            var description = method.GetCustomAttribute<DescriptionAttribute>()?.Description;

            var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
            var returnType = method.ReturnType;
            var functionType = returnType == typeof(void)
                ? Expression.GetActionType(parameterTypes)
                : Expression.GetFuncType([.. parameterTypes, returnType]);
            var function = method.CreateDelegate(functionType, handler);

            var tool = new FunctionTool(function)
            {
                Name = name,
                Description = description,
                RequiresApproval = approval,
            };
            tools.Add(tool);
        }

        return tools;
    }

    private IToolHandler GetToolHandler(string serviceKey)
    {
        // 등록된 서비스의 라이프 타임을 알 수 없으므로 Scope를 생성하여 서비스를 가져옵니다.
        using var scope = _services.CreateScope();
        return scope.ServiceProvider.GetRequiredKeyedService<IToolHandler>(serviceKey);
    }
}
