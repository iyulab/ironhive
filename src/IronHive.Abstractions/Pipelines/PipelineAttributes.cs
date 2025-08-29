namespace IronHive.Abstractions.Pipelines;

/// <summary>
/// 지정된 파이프라인 구현에 고유 이름을 부여하는 데 사용되는 특성입니다.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class PipelineNameAttribute : Attribute
{
    public string Name { get; }
    
    public PipelineNameAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Pipeline name must be a non-empty string.", nameof(name));
        Name = name;
    }
}
