using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Raggle.Abstractions.Tools;

public static class FunctionToolFactory
{
    public static ICollection<FunctionTool> CreateFromType<T>(
        IServiceProvider? serviceProvider = null, 
        params object[] parameters)
        where T : class
    {
        T instance;

        if (serviceProvider is not null)
        {
            instance = ActivatorUtilities.CreateInstance<T>(serviceProvider, parameters);
        }
        else if (parameters.Length > 0)
        {
            var constructor = typeof(T).GetConstructors()
                                       .FirstOrDefault(c => c.GetParameters()
                                                             .Select(p => p.ParameterType)
                                                             .SequenceEqual(parameters.Select(p => p.GetType())))
                               ?? throw new InvalidOperationException("Not found matching constructor.");
            instance = (T)constructor.Invoke(parameters);
        }
        else
        {
            instance = Activator.CreateInstance<T>();
        }

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

    public static FunctionTool CreateFromMethod(
        MethodInfo method, 
        object instance, 
        string? name = null, 
        string? description = null)
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
