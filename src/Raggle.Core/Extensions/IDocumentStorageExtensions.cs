using Raggle.Abstractions.Memory;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raggle.Core.Extensions;

public static class IDocumentStorageExtensions
{
    public static JsonSerializerOptions DefaultJsonSerializerOptions { get; } = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower) }
    };

    public static async Task<T> ReadJsonDocumentFileAsync<T>(
        this IDocumentStorage storage,
        string collectionName,
        string documentId,
        string filePath,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= DefaultJsonSerializerOptions;
        var stream = await storage.ReadDocumentFileAsync(collectionName, documentId, filePath, cancellationToken)
            ?? throw new InvalidOperationException($"Document file '{filePath}' not found.");
        var document = await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize document file.");
        return document;
    }

    public static async Task WriteJsonDocumentFileAsync<T>(
        this IDocumentStorage storage,
        string collectionName,
        string documentId,
        string filePath,
        T json,
        JsonSerializerOptions? options = null,
        bool overwrite = true,
        CancellationToken cancellationToken = default)
    {
        options ??= DefaultJsonSerializerOptions;
        var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync<T>(stream, json, options, cancellationToken);
        stream.Position = 0;
        await storage.WriteDocumentFileAsync(collectionName, documentId, filePath, stream, overwrite, cancellationToken);
    }
}
