namespace Raggle.Abstractions.Memories;

public interface IFileStorage : IDisposable
{
    Task<Stream> ReadFileAsync(string fileIdentifier, CancellationToken cancellationToken);
}
