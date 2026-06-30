namespace IronHive.Abstractions.Messages;

/// <summary>
/// 구조화 출력 설정입니다.
/// MessageRequest.Output에 null이 아닌 값을 지정하면 활성화됩니다.
/// </summary>
public sealed class OutputOptions
{
    private OutputOptions() { }

    /// <summary>
    /// C# 타입 기반 스키마입니다.
    /// </summary>
    public Type? Type { get; private init; }

    /// <summary>
    /// JSON 스키마 문자열입니다.
    /// </summary>
    public string? Schema { get; private init; }

    /// <summary>
    /// C# 타입으로부터 JSON 스키마를 생성해 출력을 구조화합니다.
    /// </summary>
    public static OutputOptions For<T>() => new() 
    { 
        Type = typeof(T) 
    };

    /// <summary>
    /// JSON 스키마 문자열을 직접 지정해 출력을 구조화합니다.
    /// </summary>
    public static OutputOptions For(string jsonSchema) => new() 
    { 
        Schema = jsonSchema 
    };
}
