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
    public static async Task<T> LoadAsync<T>(
        Assembly assembly,
        string resourceName,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using Stream stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"임베디드 리소스 '{resourceName}'를 찾을 수 없습니다.");
        
        // JSON 데이터를 제네릭 타입 T로 디시리얼라이즈
        var result = await JsonSerializer.DeserializeAsync<T>(
            utf8Json: stream, 
            options: options ?? JsonDefaultOptions.Options, 
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("JSON 파싱에 실패했습니다.");

        return result;
    }
}
