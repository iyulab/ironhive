using ConsoleTest;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Vector;
using System.Text.Json;

var dbPath = @"C:\TEMP\vectors.db";
var storage = new SqliteVecVectorStorage(dbPath);

var collection = new VectorCollectionInfo
{
    Name = "demo",
    Dimensions = 4, // 벡터 차원 (예시)
    EmbeddingProvider = "test",
    EmbeddingModel = "dummy"
};

// 1. 컬렉션 생성 (이미 있으면 생략)
if (!await storage.CollectionExistsAsync(collection.Name))
{
    Console.WriteLine("컬렉션 생성중...");
    await storage.CreateCollectionAsync(collection);
}

// 2. 벡터 업서트 (임의 벡터)
var rnd = new Random();
var vectors = new[]
{
                new VectorRecord
                {
                    Id = "vec1",
                    Source = new TextMemorySource { Id = "src1", Value = "source text" },
                    Content = new { Note = "첫 번째 벡터" },
                    Vectors = Enumerable.Range(0, (int)collection.Dimensions).Select(_ => (float)rnd.NextDouble()).ToArray()
                },
                new VectorRecord
                {
                    Id = "vec2",
                    Source = new TextMemorySource { Id = "src2", Value = "source text" },
                    Content = new { Note = "두 번째 벡터" },
                    Vectors = Enumerable.Range(0, (int)collection.Dimensions).Select(_ => (float)rnd.NextDouble()).ToArray()
                }
            };

Console.WriteLine("벡터 저장중...");
await storage.UpsertVectorsAsync(collection.Name, vectors);

// 3. 벡터 검색 (쿼리 벡터도 랜덤)
var queryVector = Enumerable.Range(0, (int)collection.Dimensions).Select(_ => (float)rnd.NextDouble()).ToArray();

Console.WriteLine("쿼리 벡터:");
Console.WriteLine(JsonSerializer.Serialize(queryVector));

var results = await storage.SearchVectorsAsync(
    collection.Name,
    queryVector,
    limit: 5
);

Console.WriteLine("\n검색 결과:");
foreach (var r in results)
{
    Console.WriteLine($"ID={r.VectorId}, Score={r.Score:F4}, Content={JsonSerializer.Serialize(r.Content)}");
}

Console.WriteLine("\n테스트 완료");