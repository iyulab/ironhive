namespace IronHive.Core.Utilities;

/// <summary>
/// <see cref="TextCompactor"/>의 동작 설정입니다.
/// </summary>
public class TextCompactorOptions
{
    /// <summary>
    /// JSON 배열 출력을 CSV 형식으로 변환할지 여부입니다.
    /// 평탄한(nested 없는) 객체로 이루어진 JSON 배열은 CSV로 변환 시 약 40~50%의 문자 절감이 있습니다.
    /// 기본값: true.
    /// </summary>
    public bool EnableJsonToCsv { get; set; } = true;

    /// <summary>
    /// JSON→CSV 변환을 적용할 최소 배열 요소 수입니다. 이보다 적으면 변환하지 않습니다.
    /// 기본값: 3.
    /// </summary>
    public int JsonToCsvMinElements { get; set; } = 3;

    /// <summary>
    /// 과도한 공백을 정규화할지 여부입니다.
    /// 3개 이상 연속된 빈 줄을 2개로 줄이고, 각 줄의 trailing 공백을 제거합니다.
    /// 기본값: true.
    /// </summary>
    public bool EnableWhitespaceNormalization { get; set; } = true;

    /// <summary>
    /// 이 문자 수를 초과하면 잘라냅니다.
    /// 기본값: 50,000.
    /// </summary>
    public int MaxResultChars { get; set; } = 50_000;

    /// <summary>
    /// 잘라낼 때 앞에서 유지할 줄 수입니다.
    /// 기본값: 100.
    /// </summary>
    public int KeepHeadLines { get; set; } = 100;

    /// <summary>
    /// 잘라낼 때 뒤에서 유지할 줄 수입니다.
    /// 기본값: 30.
    /// </summary>
    public int KeepTailLines { get; set; } = 30;
}
