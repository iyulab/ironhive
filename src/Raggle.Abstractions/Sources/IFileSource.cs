namespace Raggle.Source;

public interface IFileSource : IDataSource
{
    IEnumerable<string> GetFileNames(string index);
    void UpsertFile(string index, string filename, Stream content);
    void DeleteFile(string index, string filename);
}
