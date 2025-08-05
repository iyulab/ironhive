namespace IronHive.Abstractions.Agent;

/// <summary>
/// 프롬프트 템플릿 렌더링을 위한 인터페이스입니다.
/// </summary>
public interface IPromptProvider
{
    /// <summary>
    /// 주어진 컨텍스트를 기반으로 템플릿을 렌더링합니다.
    /// </summary>
    /// <param name="expression">렌더링할 템플릿 문자열</param>
    /// <param name="model">렌더링에 사용할 모델 객체</param>
    /// <returns>렌더링된 문자열</returns>
    string Render(string expression, object? model);

    /// <summary>
    /// 파일로부터 템플릿을 읽어와 주어진 컨텍스트로 렌더링합니다.
    /// </summary>
    /// <param name="filePath">템플릿 파일의 경로</param>
    /// <param name="model">렌더링에 사용할 모델 객체</param>
    /// <returns>렌더링된 문자열</returns>
    string RenderFromFile(string filePath, object? model);
}