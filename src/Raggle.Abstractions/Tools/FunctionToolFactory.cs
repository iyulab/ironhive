using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
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
        ArgumentNullException.ThrowIfNull(instance);
        var tools = new List<FunctionTool>();

        var methods = instance.GetType().GetMethods(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        foreach (var method in methods)
        {
            if (method.GetCustomAttribute<FunctionToolAttribute>() is not null)
            {
                tools.Add(CreateFromMethod(method, instance));
            }
        }
        return tools;
    }

    public static FunctionTool CreateFromMethod(MethodInfo methodInfo, object instance)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);
        ArgumentNullException.ThrowIfNull(instance);

        var attribute = methodInfo.GetCustomAttribute<FunctionToolAttribute>();
        if (attribute is null)
        {
            throw new ArgumentException("Method is not a function tool.");
        }

        var tool = new FunctionTool
        {
            Method = methodInfo,
            Name = attribute.Name ?? methodInfo.Name,
            Description = attribute.Description,
            Parameters = FunctionToolConverter.Convert(methodInfo),
        };
        return tool;
    }
}
