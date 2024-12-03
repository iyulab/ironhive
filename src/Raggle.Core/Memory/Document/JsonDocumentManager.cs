using Raggle.Abstractions.Memory;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raggle.Core.Memory.Document;

public class JsonDocumentManager : IDocumentManager
{
    private const string extension = "json";
    private readonly JsonSerializerOptions _defaultJsonOptions = new()
    {
        WriteIndented = true,
        //PropertyNameCaseInsensitive = true,
        //PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
        //Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly IDocumentStorage _documentStorage;

    public JsonSerializerOptions JsonOptions { get; }
    
    public JsonDocumentManager(IDocumentStorage documentStorage, JsonSerializerOptions? jsonOptions = null)
    {
        _documentStorage = documentStorage;
        JsonOptions = jsonOptions ?? _defaultJsonOptions;
    }

    public async IAsyncEnumerable<T> GetDocumentFilesAsync<T>(
        string collectionName, 
        string documentId, 
        string suffix, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var filePath in _documentStorage.GetDocumentFilesAsync(
            collectionName: collectionName,
            documentId: documentId,
            cancellationToken: cancellationToken))
        {
            if (filePath.EndsWith($"{suffix}.{extension}"))
            {
                var stream = await _documentStorage.ReadDocumentFileAsync(
                    collectionName: collectionName,
                    documentId: documentId,
                    filePath: filePath,
                    cancellationToken: cancellationToken);

                var json = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken)
                    ?? throw new InvalidOperationException("Failed to deserialize the document.");

                yield return json;
            }
        }
    }

    public async Task<T> GetDocumentFileAsync<T>(
        string collectionName,
        string documentId,
        string suffix,
        CancellationToken cancellationToken = default)
    {
        return await GetDocumentFilesAsync<T>(collectionName, documentId, suffix, cancellationToken)
            .FirstAsync(cancellationToken);
    }

    public async Task UpsertDocumentFilesAsync<T>(
        string collectionName, 
        string documentId,
        string fileName,
        string suffix, 
        IEnumerable<T> values, 
        CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < values.Count(); i++)
        {
            var jsonString = JsonSerializer.Serialize(values.ElementAt(i), JsonOptions);
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync<T>(stream, values.ElementAt(i), JsonOptions, cancellationToken);
            stream.Position = 0;
            await _documentStorage.WriteDocumentFileAsync(
                collectionName: collectionName,
                documentId: documentId,
                filePath: $"{fileName}.{i:D3}.{suffix}.{extension}",
                data: stream,
                overwrite: true,
                cancellationToken: cancellationToken);
        }
    }

    public async Task UpsertDocumentFileAsync<T>(
        string collectionName, 
        string documentId,
        string fileName,
        string suffix,
        T value,
        CancellationToken cancellationToken = default)
    {
        var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync<T>(stream, value, JsonOptions, cancellationToken);
        stream.Position = 0;
        await _documentStorage.WriteDocumentFileAsync(
            collectionName: collectionName,
            documentId: documentId,
            filePath: $"{fileName}.{suffix}.{extension}",
            data: stream,
            overwrite: true,
            cancellationToken: cancellationToken);
    }
}
