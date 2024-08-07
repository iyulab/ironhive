namespace Raggle.Source;

public interface IHttpSource : IDataSource
{
    string Method { get; set; }
    string Url { get; set; }
    IDictionary<string, string>? Headers { get; set; }
    string? Body { get; set; }

    string GetContent();
}
