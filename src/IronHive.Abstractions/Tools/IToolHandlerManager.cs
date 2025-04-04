namespace IronHive.Abstractions.Tools;

/// <summary>
/// 툴 핸들러 매니저 인터페이스
/// </summary>
public interface IToolHandlerManager
{
    /// <summary>
    /// 핸들러에 속한 툴을 사용하기 위한 지침을 반환합니다.
    /// </summary>
    /// <param name="serviceKey">핸들러의 서비스키</param>
    /// <param name="options">핸들러의 사용자 옵션</param>
    /// <returns>툴 사용시 안내 지침</returns>
    Task<string> HandleSetInstructionsAsync(string serviceKey, object? options);

    /// <summary>
    /// 핸들러가 생성될때 호출됩니다.
    /// </summary>
    /// <param name="serviceKey">핸들러의 서비스키</param>
    /// <param name="options">핸들러의 사용자 옵션</param>
    Task HandleInitializedAsync(string serviceKey, object? options);

    /// <summary>
    /// 핸들러에서 툴 컬렉션을 생성합니다.
    /// </summary>
    /// <param name="serviceKey">핸들러의 서비스 키</param>
    ICollection<ITool> CreateToolCollection(string serviceKey);
}
