using System.Reflection;
using System.Text.Json;

namespace IronHive.Abstractions.Json;

public static class JsonResourceLoader
{
    /// <summary>
    /// 지정된 임베디드 JSON 리소스를 파싱하여 제네릭 타입 T로 반환합니다.
    /// </summary>
    /// <typeparam name="T">디시리얼라이즈할 타입</typeparam>
    /// <param name="resourceName">
    /// 임베디드 리소스의 전체 이름 (예: "IronHive.models.json")
    /// </param>
    public static async Task<JsonResource<T>> LoadAsync<T>(
        Assembly assembly,
        string resourceName,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using Stream stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"임베디드 리소스 '{resourceName}'를 찾을 수 없습니다.");
        
        // JSON 데이터를 제네릭 타입 T로 디시리얼라이즈
        var result = await JsonSerializer.DeserializeAsync<JsonResource<T>>(stream, 
            options ?? JsonDefaultOptions.Options, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("JSON 파싱에 실패했습니다.");

        return result;
    }

    /// <summary>
    /// JsonResource<T> 객체를 JSON 파일로 저장합니다.
    /// </summary>
    /// <typeparam name="T">직렬화할 데이터 타입</typeparam>
    /// <param name="resource">저장할 JsonResource 객체</param>
    /// <param name="filePath">저장할 파일 경로</param>
    public static async Task SaveAsync<T>(
        JsonResource<T> resource, 
        string filePath, 
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // 디렉토리가 존재하지 않으면 생성합니다.
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 파일 스트림을 열어 JSON 직렬화를 수행합니다.
        using FileStream stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, resource, 
            options ?? JsonDefaultOptions.Options, cancellationToken: cancellationToken);
    }
}
