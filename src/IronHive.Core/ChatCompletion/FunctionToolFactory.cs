using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace IronHive.Core.ChatCompletion;

public static class FunctionToolFactory
{
    public static ICollection<FunctionTool> CreateFromObject<T>(
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

        return CreateFromObject(instance);
    }

    public static ICollection<FunctionTool> CreateFromObject(object instance)
    {
        var tools = new List<FunctionTool>();
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

            var tool = new FunctionTool(function, name, description);
            tools.Add(tool);
        }

        return tools;
    }
}
