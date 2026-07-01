using IronHive.Abstractions.Json;
using IronHive.Abstractions.Tools;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;

namespace IronHive.Core.Tools;

/// <summary>
/// FunctionTool을 생성하는 팩토리.
/// - 특정 타입의 메서드에서 생성
/// - 임의의 Delegate에서 생성
/// </summary>
public static class FunctionToolFactory
{
    // 파라미터는 nullable 허용, description 어트리뷰트 반영
    private static readonly JsonSchemaExporterOptions _propSchemaOptions = new()
    {
        TreatNullObliviousAsNonNullable = false,
        TransformSchemaNode = JsonSchemaFactory.HandleTransform,
    };

    /// <summary>
    /// public/non-public 인스턴스/정적 메서드 중 FunctionToolAttribute가 붙은 메서드를 찾아 툴 모음으로 만듭니다.
    /// </summary>
    public static IEnumerable<ITool> CreateFrom<T>(IServiceProvider? services = null)
        where T : class
    {
        return CreateFromObject(typeof(T), instance: null, services);
    }

    /// <summary>
    /// 인스턴스의 FunctionToolAttribute가 붙은 메서드를 찾아 툴 모음으로 만듭니다.
    /// 인스턴스가 직접 바인딩되므로 DI 없이 호출됩니다.
    /// </summary>
    public static IEnumerable<ITool> CreateFrom(object instance, IServiceProvider? services = null)
    {
        return CreateFromObject(instance.GetType(), instance, services);
    }

    /// <summary>
    /// 단일 Delegate에서 툴을 생성합니다.
    /// </summary>
    public static ITool CreateFrom(Delegate function, DelegateDescriptor descriptor, IServiceProvider? services = null)
    {
        if (string.IsNullOrWhiteSpace(descriptor.Name))
            throw new ArgumentException("툴 이름은 비어있을 수 없습니다.", nameof(descriptor));

        var method = function.Method;
        var parameters = BuildJsonSchemaParameters(method);

        return new FunctionTool(function, services)
        {
            Name = descriptor.Name,
            Description = descriptor.Description,
            Parameters = parameters,
            RequiresApproval = descriptor.RequiresApproval,
            Timeout = descriptor.Timeout
        };
    }

    private static List<ITool> CreateFromObject(Type type, object? instance, IServiceProvider? services = null)
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

            FunctionTool tool;
            if (instance is not null && !method.IsStatic)
            {
                tool = new FunctionTool(method, instance, services)
                {
                    Name = name,
                    Description = desc,
                    Parameters = parameters,
                    RequiresApproval = requires,
                    Timeout = timeout
                };
            }
            else
            {
                tool = new FunctionTool(method, services)
                {
                    Name = name,
                    Description = desc,
                    Parameters = parameters,
                    RequiresApproval = requires,
                    Timeout = timeout
                };
            }

            tools.Add(tool);
        }
        return tools;
    }

    /// <summary>
    /// 메서드의 파라미터 정보를 JSON 스키마로 변환합니다.
    /// </summary>
    private static JsonObject? BuildJsonSchemaParameters(MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length == 0) return null;

        var root = new JsonObject
        {
            ["type"] = "object",
            ["additionalProperties"] = false
        };
        var properties = new JsonObject();
        var required = new JsonArray();

        foreach (var param in parameters)
        {
            // 이름이 없거나 out 매개변수인 경우 무시
            if (string.IsNullOrEmpty(param.Name) || param.IsOut)
                continue;
            // 취소 토큰인 경우 무시
            if (param.ParameterType == typeof(CancellationToken))
                continue;
            // 특정 서비스의 경우 무시
            if (param.GetCustomAttributes().Any(a => a is FromServicesAttribute || a is FromKeyedServicesAttribute))
                continue;

            // JSON 스키마 생성 (파라미터는 nullable 허용 + 프로퍼티 description 반영)
            var node = JsonSchemaFactory.Build(param.ParameterType, _propSchemaOptions);

            // 파라미터 자체의 [Description] / [Display] 반영 (ctx.PropertyInfo로 접근 불가한 영역)
            if (TryGetDescription(param.GetCustomAttributes(true), out var description))
            {
                node.Root["description"] = description;
            }

            // 프로퍼티 추가
            if (!properties.TryAdd(param.Name!, node.Root))
                throw new InvalidOperationException($"동일한 이름의 매개변수 '{param.Name}'가 이미 존재합니다.");

            // 필수 여부: 파라미터가 선택적이 아니면 required에 추가
            if (!param.IsOptional)
                required.Add(param.Name!);
        }
        
        if (properties.Count > 0)
            root["properties"] = properties;
        if (required.Count > 0)
            root["required"] = required;

        return root;
    }

    /// <summary> 어트리뷰트 배열에서 설명을 추출합니다. </summary>
    private static bool TryGetDescription(object[] attributes, out string description)
    {
        var descAttr = attributes.OfType<DescriptionAttribute>().FirstOrDefault();
        var displayAttr = attributes.OfType<DisplayAttribute>().FirstOrDefault();
        description = descAttr?.Description ?? displayAttr?.Description ?? string.Empty;
        return !string.IsNullOrWhiteSpace(description);
    }
}

/// <summary>
/// Delegate 함수의 메타데이터를 설명하는 객체.
/// </summary>
public record DelegateDescriptor
{
    public required string Name { get; set; }
    
    public string? Description { get; set; }
    
    public bool RequiresApproval { get; set; }

    public long Timeout { get; set; } = 60;
}