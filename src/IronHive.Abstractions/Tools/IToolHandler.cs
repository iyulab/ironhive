namespace IronHive.Abstractions.Tools;

/// <summary>
/// Tool의 실행을 핸들링하기 위한 인터페이스입니다.
/// 이 인터페이스는 도구 실행 전 초기화와 지침 설정을 관리합니다.
/// </summary>
public interface IToolHandler
{
    /// <summary>
    /// 도구 실행을 위한 지침을 제공합니다.
    /// </summary>
    /// <param name="options">사용자의 도구 실행 옵션입니다. 지침 설정에 필요한 추가 정보를 제공합니다.</param>
    /// <returns>도구 실행 지침이 포함된 문자열을 비동기적으로 반환합니다.</returns>
    Task<string> HandleSetInstructionsAsync(object? options);

    /// <summary>
    /// 도구 실행을 위한 초기화 작업을 수행합니다.
    /// </summary>
    /// <param name="options">사용자의 도구 실행 옵션입니다. 초기화에 필요한 추가 정보를 제공합니다.</param>
    /// <returns>초기화 작업 완료 후 비동기적으로 작업을 반환합니다.</returns>
    Task HandleInitializedAsync(object? options);

    ///// <summary>
    ///// 도구 실행 전에 수행할 사전 작업을 처리합니다.
    ///// </summary>
    ///// <param name="name">실행할 도구의 이름입니다.</param>
    ///// <param name="options">사용자의 도구 실행 옵션입니다.</param>
    ///// <param name="parameters">도구 실행에 필요한 매개변수입니다.</param>
    ///// <returns>사전 작업 완료 후 비동기적으로 작업을 반환합니다.</returns>
    //Task HandleBeforeInvokeAsync(string name, object? options, object? parameters);

    ///// <summary>
    ///// 도구 실행 후 수행할 후처리 작업을 처리합니다.
    ///// </summary>
    ///// <param name="name">실행된 도구의 이름입니다.</param>
    ///// <param name="options">사용자의 도구 실행 옵션입니다.</param>
    ///// <param name="result">도구 실행 결과입니다.</param>
    ///// <returns>후처리 작업 완료 후 비동기적으로 작업을 반환합니다.</returns>
    //Task HandleAfterInvokeAsync(string name, object? options, FunctionResult result);
}
