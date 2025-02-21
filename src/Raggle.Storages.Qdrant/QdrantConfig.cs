namespace Raggle.Storages.Qdrant;

public class QdrantConfig
{
    public string Host { get; set; } = "http://localhost";
    public int Port { get; set; } = 6333;
    public string ApiKey { get; set; } = string.Empty;

    public bool Https { get; set; } = false;
    public TimeSpan GrpcTimeout { get; set; } = TimeSpan.FromSeconds(100);
}
