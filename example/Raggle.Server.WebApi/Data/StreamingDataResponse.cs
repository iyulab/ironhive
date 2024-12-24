namespace Raggle.Server.WebApi.Data;

public enum StreamingDataState
{
    Start,
    Progress,
    End,
    Error
}

public class StreamingDataResponse<T>
{
    public StreamingDataState State { get; set; }

    public T? Data { get; set; }

    public string? Error { get; set; }
}
