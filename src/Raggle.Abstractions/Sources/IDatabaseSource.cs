namespace Raggle.Source;

public interface IDatabaseSource : IDataSource
{
    string ConnectionString { get; set; }

    string GetContent(string query);
}

