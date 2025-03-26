namespace IronHive.Abstractions.Json;

public class JsonResource<T>
{
    public string? Name { get; set; }

    public string? Version { get; set; }

    public string? LastUpdated { get; set; }

    public T? Data { get; set; }
}
