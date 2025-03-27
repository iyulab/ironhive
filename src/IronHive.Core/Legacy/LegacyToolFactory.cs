using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace IronHive.Core.Legacy;

/// <summary>
/// 툴 생성기
/// </summary>
public class LegacyToolFactory
{
    private readonly IServiceProvider _services;

    public LegacyToolFactory(IServiceProvider services)
    {
        _services = services;
    }

    public ICollection<object> CreateFromObject<T>(params object[] parameters)
        where T : class
    {
        T instance;

        if (_services is not null)
        {
            instance = ActivatorUtilities.CreateInstance<T>(_services, parameters);
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

    public ICollection<object> CreateFromObject(object instance)
    {
        var tools = new List<object>();
        var methods = instance.GetType().GetMethods(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        foreach (var method in methods)
        {
            var name = "function name";
            var description = method.GetCustomAttribute<DescriptionAttribute>()?.Description;

            var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
            var returnType = method.ReturnType;
            var functionType = returnType == typeof(void)
                ? Expression.GetActionType(parameterTypes)
                : Expression.GetFuncType([.. parameterTypes, returnType]);
            var function = method.CreateDelegate(functionType, instance);

            var tool = new
            {
                Name = name,
                Description = description
            };
            tools.Add(tool);
        }

        return tools;
    }
}
