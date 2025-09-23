using IronHive.Abstractions.Tools;
using Json.Schema;
using Json.Schema.Generation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace IronHive.Core.Tools;

/// <summary>
/// FunctionTool을 생성하는 팩토리.
/// - 특정 타입의 메서드에서 생성
/// - 임의의 Delegate에서 생성
/// </summary>
public static class FunctionToolFactory
{
    /// <summary>
    /// public/non-public 인스턴스/정적 메서드 중 FunctionToolAttribute가 붙은 메서드를 찾아 툴 모음으로 만듭니다.
    /// </summary>
    public static IEnumerable<ITool> CreateFrom<T>()
        where T : class
    {
        return CreateFrom(typeof(T));
    }

    /// <summary>
    /// public/non-public 인스턴스/정적 메서드 중 FunctionToolAttribute가 붙은 메서드를 찾아 툴 모음으로 만듭니다.
    /// </summary>
    public static IEnumerable<ITool> CreateFrom(Type type)
    {
        var tools = new List<ITool>();
        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<FunctionToolAttribute>();
            if (attr is null) continue;

            var name = attr.Name ?? method.Name;
            var desc = attr.Description ?? method.GetCustomAttribute<DescriptionAttribute>()?.Description;
            var requires = attr.RequiresApproval;
            var timeout = attr.Timeout;
            var parameters = BuildJsonSchemaParameters(method);
            
            tools.Add(new FunctionTool(method)
            {
                Name = name,
                Description = desc,
                Parameters = parameters,
                RequiresApproval = requires,
                Timeout = timeout
            });
        }
        return tools;
    }

    /// <summary>
    /// 단일 Delegate에서 툴을 생성합니다.
    /// </summary>
    public static ITool CreateFrom(
        Delegate function,
        DelegateDescriptor descriptor)
    {
        if (string.IsNullOrWhiteSpace(descriptor.Name))
            throw new ArgumentException("툴 이름은 비어있을 수 없습니다.", nameof(descriptor.Name));

        var method = function.Method;
        var parameters = BuildJsonSchemaParameters(method);

        return new FunctionTool(function)
        {
            Name = descriptor.Name,
            Description = descriptor.Description,
            Parameters = parameters,
            RequiresApproval = descriptor.RequiresApproval,
            Timeout = descriptor.Timeout
        };
    }

    /// <summary>
    /// 메서드의 파라미터 정보를 JSON 스키마로 변환합니다.
    /// </summary>
    private static JsonSchema? BuildJsonSchemaParameters(MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length == 0) return null;

        var properties = new Dictionary<string, JsonSchema>();
        var required = new List<string>();

        foreach (var param in parameters)
        {
            // 이름이 없거나 out 매개변수인 경우
            if (string.IsNullOrEmpty(param.Name) || param.IsOut)
                continue;

            // 취소 토큰인 경우
            if (param.ParameterType == typeof(CancellationToken))
                continue;
            
            // 특정 서비스의 경우
            if (param.GetCustomAttributes().Any(a => a is FromOptionsAttribute || 
                    a is FromServicesAttribute || a is FromKeyedServicesAttribute))
                continue;

            // JSON 스키마 생성
            var pb = new JsonSchemaBuilder().FromType(param.ParameterType);

            // DescriptionAttribute 또는 DisplayAttribute에서 설명 추출
            if (param.GetCustomAttribute<DescriptionAttribute>() is { } descAttr)
            {
                pb = pb.Description(descAttr.Description);
            }
            else if (param.GetCustomAttribute<DisplayAttribute>() is { } displayAttr
                     && !string.IsNullOrWhiteSpace(displayAttr.Description))
            {
                pb = pb.Description(displayAttr.Description!);
            }

            // 프로퍼티 추가
            if (!properties.TryAdd(param.Name!, pb.Build()))
                throw new InvalidOperationException($"동일한 이름의 매개변수 '{param.Name}'가 이미 존재합니다.");

            // 필수 여부: 파라미터가 선택적이 아니면 required에 추가
            if (!param.IsOptional)
                required.Add(param.Name!);

            // 선택 매개변수의 기본값, 필수적이지 않으므로 생략
            //if (param.IsOptional && param.HasDefaultValue)
            //    pb = pb.Default();
        }

        return new JsonSchemaBuilder().Type(SchemaValueType.Object)
                                      .Properties(properties)
                                      .Required(required)
                                      .Build();
    }
}

/// <summary>
/// Delegate 함수의 메타데이터를 설명하는 객체.
/// </summary>
public record DelegateDescriptor
{
    public required string Name { get; set; }
    
    public string? Description { get; set; }
    
    public bool RequiresApproval { get; set; } = false;

    public long Timeout { get; set; } = 60;
}