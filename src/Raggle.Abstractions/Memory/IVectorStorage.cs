namespace Raggle.Abstractions.Memories;

public interface IVectorStorage
{
    string IndexCollectionName { get; set; }

    Task CreateDocumentCollection(string collectionName);

    Task FindDocument();

    Task FindDocuments();

    Task UpsertDocument();

    Task DeleteDocument();

    Task CreateMemoryCollection(string collectionName);

    Task UpsertRecord();
    
    Task UpsertRecords();
    
    Task DeleteRecord();
    
    Task FindRecord();
}
