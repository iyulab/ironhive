using IronHive.Abstractions.Tools;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace IronHive.Core.Tools;

public class ToolManager : IToolManager
{
    private readonly IServiceProvider _services;

    public ToolManager(IServiceProvider services)
    {
        _services = services;
    }

    public IToolHandler GetToolService(string key)
    {
        using var scope = _services.CreateScope();
        return scope.ServiceProvider.GetRequiredKeyedService<IToolHandler>(key);
    }

    public ICollection<ITool> CreateFromObject<T>(params object[] parameters)
        where T : class
    {
        Activator.CreateInstance(typeof(T));
        var instance = ActivatorUtilities.CreateInstance<T>(_services, parameters);
        return CreateFromObject(instance);
    }

    public ICollection<ITool> CreateFromObject(object instance)
    {
        var tools = new List<ITool>();
        var methods = instance.GetType().GetMethods(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<FunctionToolAttribute>();
            if (attr is null) continue;

            var name = attr.Name ?? method.Name;
            var description = method.GetCustomAttribute<DescriptionAttribute>()?.Description;

            var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
            var returnType = method.ReturnType;
            var functionType = returnType == typeof(void)
                ? Expression.GetActionType(parameterTypes)
                : Expression.GetFuncType([.. parameterTypes, returnType]);
            var function = method.CreateDelegate(functionType, instance);

            var tool = CreateFromFunction(name, description, function);
            tools.Add(tool);
        }

        return tools;
    }

    public ITool CreateFromFunction(string name, string? description, Delegate function)
    {
        return new FunctionTool(function)
        {
            Name = name,
            Description = description
        };
    }
}
