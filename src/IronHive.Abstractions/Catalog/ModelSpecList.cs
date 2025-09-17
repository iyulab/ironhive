namespace IronHive.Abstractions.Catalog;

/// <summary>
/// 공급자와 모든 모델의 사양 정보를 함께 담는 레코드입니다.
/// </summary>
public sealed class ModelSpecList
{
    /// <summary>
    /// 모델 공급자 이름입니다.
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    /// 공급자에 속한 모든 모델의 구체적인 사양 정보입니다.
    /// </summary>
    public required IEnumerable<IModelSpec> Models { get; init; }
}

/// <summary>
/// 공급자와 지정된 타입의 모든 모델의 사양 정보를 함께 담는 레코드입니다.
/// </summary>
public sealed class ModelSpecList<T> where T : class, IModelSpec
{
    /// <summary>
    /// 모델 공급자 이름입니다.
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    /// 공급자에 속한 지정된 타입의 모델의 구체적인 사양 정보입니다.
    /// </summary>
    public required IEnumerable<T> Models { get; init; }
}