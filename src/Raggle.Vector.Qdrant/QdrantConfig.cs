namespace Raggle.Vector.Qdrant;

public class QdrantConfig
{
    public required string Host { get; set; }
    public required int Port { get; set; }
    public string? ApiKey { get; set; }

    public bool Https { get; set; } = false;
    public TimeSpan GrpcTimeout { get; set; } = TimeSpan.FromSeconds(10);
}
