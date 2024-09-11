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
            var functionAttribute = method.GetCustomAttribute<FunctionToolAttribute>();
            if (functionAttribute is not null)
            {
                var name = functionAttribute.Name 
                    ?? method.Name;
                var description = functionAttribute.Description 
                    ?? method.GetCustomAttribute<DescriptionAttribute>()?.Description;
                
                var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
                var returnType = method.ReturnType;
                var functionType = returnType == typeof(void) 
                    ? Expression.GetActionType(parameterTypes)
                    : Expression.GetFuncType([.. parameterTypes, returnType]);
                
                var function = method.CreateDelegate(functionType, instance);
                var tool = CreateFromFunction(name, description, function);
                tools.Add(tool);
            }
        }

        return tools;
    }

    public static FunctionTool CreateFromFunction(string name, string? description, Delegate function)
    {
        return new FunctionTool(function)
        {
            Name = name,
            Description = description,
        };
    }
}
