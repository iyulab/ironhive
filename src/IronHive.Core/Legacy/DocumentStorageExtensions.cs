//using IronHive.Abstractions.Json;
//using IronHive.Abstractions.Memory;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Text.Json;

//namespace IronHive.Core.Legacy;

//public static class DocumentStorageExtensions
//{
//    private const string JsonExtension = "json";

//    public struct JsonFileName
//    {
//        public string Name { get; set; }
//        public string? Suffix { get; set; }
//        public int? Index { get; set; }
//    }

//    public static async IAsyncEnumerable<T> GetDocumentJsonAsync<T>(
//        this IFileStorage documentStorage,
//        string collectionName,
//        string documentId,
//        string suffix,
//        Range? range = null,
//        JsonSerializerOptions? options = null,
//        [EnumeratorCancellation] CancellationToken cancellationToken = default)
//    {
//        await foreach (var filePath in documentStorage.GetDocumentFilesAsync(
//            collectionName: collectionName,
//            documentId: documentId,
//            cancellationToken: cancellationToken))
//        {
//            if (filePath.EndsWith($"{suffix}.{JsonExtension}", StringComparison.OrdinalIgnoreCase))
//            {
//                var index = ParseJsonFileName(filePath).Index;

//                if (range is not null && index is not null)
//                {
//                    if (!range.Value.WithInRange(index.Value))
//                    {
//                        continue;
//                    }
//                }

//                await using var stream = await documentStorage.ReadDocumentFileAsync(
//                    collectionName: collectionName,
//                    documentId: documentId,
//                    filePath: filePath,
//                    cancellationToken: cancellationToken);

//                var json = await JsonSerializer.DeserializeAsync<T>(
//                    utf8Json: stream,
//                    options: options ?? JsonDefaultOptions.Options,
//                    cancellationToken: cancellationToken)
//                    ?? throw new InvalidOperationException("문서의 역직렬화에 실패했습니다.");

//                yield return json;
//            }
//        }
//    }

//    public static async Task<T> GetDocumentJsonFirstAsync<T>(
//        this IFileStorage documentStorage,
//        string collectionName,
//        string documentId,
//        string suffix,
//        JsonSerializerOptions? options = null,
//        CancellationToken cancellationToken = default)
//    {
//        await foreach (var filePath in documentStorage.GetDocumentFilesAsync(
//            collectionName: collectionName,
//            documentId: documentId,
//            cancellationToken: cancellationToken))
//        {
//            if (filePath.EndsWith($"{suffix}.{JsonExtension}", StringComparison.OrdinalIgnoreCase))
//            {
//                await using var stream = await documentStorage.ReadDocumentFileAsync(
//                    collectionName: collectionName,
//                    documentId: documentId,
//                    filePath: filePath,
//                    cancellationToken: cancellationToken);

//                var json = await JsonSerializer.DeserializeAsync<T>(
//                    utf8Json: stream,
//                    options: options ?? JsonDefaultOptions.Options,
//                    cancellationToken: cancellationToken)
//                    ?? throw new InvalidOperationException("문서의 역직렬화에 실패했습니다.");

//                return json;
//            }
//        }
//        throw new InvalidOperationException("문서를 찾을 수 없습니다.");
//    }

//    public static async Task UpsertDocumentJsonAsync<T>(
//        this IFileStorage documentStorage,
//        string collectionName,
//        string documentId,
//        string fileName,
//        string suffix,
//        IEnumerable<T> values,
//        JsonSerializerOptions? options = null,
//        CancellationToken cancellationToken = default)
//    {
//        var valueList = values.ToList();
//        for (var i = 0; i < valueList.Count; i++)
//        {
//            await using var stream = new MemoryStream();
//            await JsonSerializer.SerializeAsync(
//                utf8Json: stream,
//                value: valueList[i],
//                options: options ?? JsonDefaultOptions.Options,
//                cancellationToken: cancellationToken);
//            stream.Position = 0;

//            var filePath = BuildJsonFileName(fileName, suffix, i);
//            await documentStorage.WriteDocumentFileAsync(
//                collectionName: collectionName,
//                documentId: documentId,
//                filePath: filePath,
//                data: stream,
//                overwrite: true,
//                cancellationToken: cancellationToken);
//        }
//    }

//    public static async Task UpsertDocumentJsonAsync<T>(
//        this IFileStorage documentStorage,
//        string collectionName,
//        string documentId,
//        string fileName,
//        string suffix,
//        T value,
//        int? index = null,
//        JsonSerializerOptions? options = null,
//        CancellationToken cancellationToken = default)
//    {
//        await using var stream = new MemoryStream();
//        await JsonSerializer.SerializeAsync(
//            utf8Json: stream,
//            value: value,
//            options: options ?? JsonDefaultOptions.Options,
//            cancellationToken: cancellationToken);
//        stream.Position = 0;

//        var filePath = BuildJsonFileName(fileName, suffix, index);
//        await documentStorage.WriteDocumentFileAsync(
//            collectionName: collectionName,
//            documentId: documentId,
//            filePath: filePath,
//            data: stream,
//            overwrite: true,
//            cancellationToken: cancellationToken);
//    }

//    private static JsonFileName ParseJsonFileName(string filePath)
//    {
//        var fileName = Path.GetFileName(filePath)
//            ?? throw new InvalidOperationException("파일 이름을 가져올 수 없습니다.");
//        if (!fileName.EndsWith($".{JsonExtension}"))
//            throw new InvalidOperationException("JSON 파일이 아닙니다.");

//        var baseName = fileName.Substring(0, fileName.Length - JsonExtension.Length - 1);
//        var parts = baseName.Split('.');

//        if (parts.Length < 1)
//            throw new InvalidOperationException("파일 이름 형식이 올바르지 않습니다.");

//        string name;
//        string? suffix = null;
//        int? index = null;

//        // 이름, 인덱스, 접미사 파싱
//        if (parts.Length >= 2)
//        {
//            if (int.TryParse(parts[^2], out int parsedIndex))
//            {
//                index = parsedIndex;
//                suffix = parts.Length >= 3 ? parts[^3] : null;
//            }
//            else
//            {
//                suffix = parts[^2];
//            }
//        }

//        // 기본 이름 조립 (복수의 점이 포함된 이름 고려)
//        name = string.Join(".", parts.Take(parts.Length - (index.HasValue ? 2 : suffix != null ? 1 : 0)));

//        return new JsonFileName
//        {
//            Name = name,
//            Index = index,
//            Suffix = suffix
//        };
//    }

//    private static string BuildJsonFileName(
//        string fileName,
//        string? suffix = null,
//        int? index = null)
//    {
//        var builder = new StringBuilder(fileName);

//        if (index.HasValue)
//            builder.Append($".{index.Value:D3}");

//        if (!string.IsNullOrEmpty(suffix))
//            builder.Append($".{suffix}");

//        builder.Append($".{JsonExtension}");

//        return builder.ToString();
//    }

//    public static bool WithInRange(this Range range, int value)
//    {
//        int start = range.Start.IsFromEnd
//            ? throw new ArgumentException("FromEnd는 지원되지 않습니다.", nameof(range))
//            : range.Start.Value;
//        int end = range.End.IsFromEnd
//            ? throw new ArgumentException("FromEnd는 지원되지 않습니다.", nameof(range))
//            : range.End.Value;

//        return value >= start && value < end;
//    }
//}
