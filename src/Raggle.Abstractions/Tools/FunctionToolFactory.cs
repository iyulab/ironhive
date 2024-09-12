using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Raggle.Abstractions.Tools;

public static class FunctionToolFactory
{
    public static ICollection<FunctionTool> CreateFromType<T>(IServiceProvider serviceProvider, params object[] parameters)
        where T : class
    {
        var instance = ActivatorUtilities.CreateInstance<T>(serviceProvider, parameters);
        return CreateFromInstance(instance);
    }

    public static ICollection<FunctionTool> CreateFromInstance<T>(T instance)
        where T : class
    {
        var tools = new List<FunctionTool>();
        var methods = instance.GetType().GetMethods(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        foreach (var method in methods)
        {
            var funcAttr = method.GetCustomAttribute<FunctionToolAttribute>();
            if (funcAttr is null) continue;

            var tool = CreateFromMethod(
                method: method,
                instance: instance,
                name: funcAttr.Name,
                description: funcAttr.Description);
            tools.Add(tool);
        }

        return tools;
    }

    public static FunctionTool CreateFromMethod(MethodInfo method, object instance, string? name = null, string? description = null)
    {
        name ??= method.Name;
        description ??= method.GetCustomAttribute<DescriptionAttribute>()?.Description;

        var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
        var returnType = method.ReturnType;
        var functionType = returnType == typeof(void)
            ? Expression.GetActionType(parameterTypes)
            : Expression.GetFuncType([.. parameterTypes, returnType]);
        var function = method.CreateDelegate(functionType, instance);
        
        return new FunctionTool(function)
        {
            Name = name,
            Description = description,
        };
    }
}
