namespace IronHive.Abstractions.Catalog;

/// <summary>
/// 모델의 공급자와 사양 정보를 함께 담는 레코드입니다.
/// </summary>
public sealed record ModelSpecItem
{
    /// <summary>
    /// 모델 공급자 이름입니다.
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    /// 모델의 구체적인 사양 정보입니다.
    /// </summary>
    public required IModelSpec Model { get; init; }
}

/// <summary>
/// 모델의 공급자와 지정된 타입의 모델 사양 정보를 함께 담는 레코드입니다.
/// </summary>
public sealed record ModelSpecItem<T> where T : class, IModelSpec
{
    /// <summary>
    /// 모델 공급자 이름입니다.
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    /// 모델의 구체적인 사양 정보입니다.
    /// </summary>
    public required T Model { get; init; }
}