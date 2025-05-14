using IronHive.Abstractions.Tools;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Reflection;

namespace IronHive.Core.Tools;

/// <summary>
/// 툴 생성기
/// </summary>
public static class FunctionToolFactory
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
            var isFunctionTool = method.GetCustomAttribute<FunctionToolAttribute>() != null;
            if (!isFunctionTool)
                continue;

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
}
