namespace Raggle.Abstractions.Memory;

/// <summary>
/// IPipelineOrchestrator 인터페이스는 데이터 파이프라인을 조정하고, 핸들러 추가, 검색, 제거 및 파이프라인 실행을 위한 기능을 제공합니다.
/// </summary>
public interface IPipelineOrchestrator
{
    /// <summary>
    /// 제네릭 타입 T의 핸들러를 검색하고, 성공 시 out 매개변수로 반환합니다.
    /// </summary>
    /// <typeparam name="T">검색할 핸들러의 타입</typeparam>
    /// <param name="handler">찾은 핸들러</param>
    /// <returns>핸들러가 존재하면 true, 그렇지 않으면 false</returns>
    bool TryGetHandler<T>(out T handler) where T : IPipelineHandler;

    /// <summary>
    /// 핸들러 이름으로 핸들러를 검색하고, 성공 시 out 매개변수로 반환합니다.
    /// </summary>
    /// <param name="name">검색할 핸들러의 이름</param>
    /// <param name="handler">찾은 핸들러</param>
    /// <returns>핸들러가 존재하면 true, 그렇지 않으면 false</returns>
    bool TryGetHandler(string name, out IPipelineHandler handler);

    /// <summary>
    /// 지정한 단계 이름으로 핸들러를 추가합니다.
    /// </summary>
    /// <param name="stepName">단계 이름</param>
    /// <param name="handler">추가할 핸들러</param>
    /// <returns>핸들러가 성공적으로 추가되면 true, 그렇지 않으면 false</returns>
    bool TryAddHandler(string stepName, IPipelineHandler handler);

    /// <summary>
    /// 지정한 단계 이름에 대한 핸들러를 제거합니다.
    /// </summary>
    /// <param name="stepName">제거할 핸들러의 단계 이름</param>
    /// <returns>핸들러가 성공적으로 제거되면 true, 그렇지 않으면 false</returns>
    bool TryRemoveHandler(string stepName);

    /// <summary>
    /// 작업을 수행 가능한지 확인하는 메서드
    /// </summary>
    bool IsLocked();

    /// <summary>
    /// 지정된 데이터 파이프라인을 비동기적으로 실행합니다.
    /// </summary>
    /// <param name="pipeline">실행할 데이터 파이프라인</param>
    /// <param name="cancellationToken">작업 취소를 위한 토큰</param>
    Task ExecuteAsync(
        DataPipeline pipeline,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 컬렉션과 문서 ID에 대한 데이터 파이프라인을 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="collectionName">컬렉션의 이름</param>
    /// <param name="documentId">문서 ID</param>
    /// <param name="cancellationToken">작업 취소를 위한 토큰</param>
    /// <returns>찾은 데이터 파이프라인을 반환하는 Task</returns>
    Task<DataPipeline> GetPipelineAsync(
        string collectionName, 
        string documentId, 
        CancellationToken cancellationToken = default);
}
