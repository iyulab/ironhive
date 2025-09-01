namespace IronHive.Abstractions.Memory;

/// <summary>
/// 메모리 작업자 구성을 나타내는 클래스입니다.
/// </summary>
public class MemoryWorkerConfig
{
    /// <summary>
    /// 작업자가 처리할 QueueStorage의 이름입니다.
    /// </summary>
    public required string QueueName { get; set; }

    /// <summary>
    /// 작업자가 큐에서 작업을 가져오는 간격(밀리초)입니다.
    /// 기본 값은 1000밀리초(1초)입니다.
    /// </summary>
    public int QueueInterval { get; set; } = 1000;

    /// <summary>
    /// 동적으로 생성되는 워커의 최소 수입니다. 
    /// 기본값은 1입니다.
    /// </summary>
    public int MinCount { get; set; } = 1;

    /// <summary>
    /// 동적으로 생성되는 워커의 최대 수입니다. 
    /// 기본값은 10입니다.
    /// </summary>
    public int MaxCount { get; set; } = 10;
}
