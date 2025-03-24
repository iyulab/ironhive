namespace IronHive.Abstractions.ChatCompletion.Tools;

/// <summary>
/// Tool 등록 및 실행 핸들링을 위한 인터페이스
/// </summary>
public interface IToolService
{
    /// <summary>
    /// 툴을 사용하기 위한 초기화 작업
    /// </summary>
    Task InitializeToolExecutionAsync(object? options);

    /// <summary>
    /// 툴 실행 후 결과 값을 재가공
    /// </summary>
    Task FinalizeToolExecutionAsync(object? options);
}
