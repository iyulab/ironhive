using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;

namespace IronHive.Core.Extensions;

/// <summary>
/// IToolCollection에 FunctionTool을 추가하는 확장 메서드입니다.
/// </summary>
public static class ToolCollectionExtensions
{
    /// <summary>
    /// 타입 T의 [FunctionTool] 메서드를 찾아 툴로 등록합니다.
    /// DI가 필요한 경우 services를 전달합니다.
    /// </summary>
    public static IToolCollection AddFunctionTool<T>(
        this IToolCollection tools,
        IServiceProvider? services = null)
        where T : class
    {
        foreach (var tool in FunctionToolFactory.CreateFrom<T>(services))
            tools.Add(tool);
        return tools;
    }

    /// <summary>
    /// 인스턴스의 [FunctionTool] 메서드를 찾아 툴로 등록합니다.
    /// </summary>
    public static IToolCollection AddFunctionTool(
        this IToolCollection tools,
        object instance)
    {
        foreach (var tool in FunctionToolFactory.CreateFrom(instance))
            tools.Add(tool);
        return tools;
    }

    /// <summary>
    /// 단일 Delegate를 툴로 등록합니다.
    /// </summary>
    public static IToolCollection AddFunctionTool(
        this IToolCollection tools,
        Delegate function,
        DelegateDescriptor descriptor,
        IServiceProvider? services = null)
    {
        tools.Add(FunctionToolFactory.CreateFrom(function, descriptor, services));
        return tools;
    }
}
