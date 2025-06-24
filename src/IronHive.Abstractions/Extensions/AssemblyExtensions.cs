using IronHive.Abstractions.Json;
using System.Text.Json;

namespace System.Reflection;

public static class AssemblyExtensions
{
    /// <summary>
    /// 지정된 어셈블리의 "Resources" 경로에서 임베디드 리소스를 가져옵니다.
    /// </summary>
    /// <param name="assembly">어셈블리</param>
    /// <param name="name">"Resources" 경로의 리소스 이름 (예: "models.json")</param>
    /// <param name="jsonOptions">JSON 직렬화 옵션 (선택 사항)</param>
    public static async Task<T> GetJsonResourceAsync<T>(
        this Assembly assembly,
        string name,
        JsonSerializerOptions? jsonOptions = null,
        CancellationToken cancellationToken = default)
    {
        var fullName = $"{assembly.GetName().Name}.Resources.{name}";
        using Stream stream = assembly.GetManifestResourceStream(fullName)
            ?? throw new FileNotFoundException($"임베디드 리소스 '{fullName}'를 찾을 수 없습니다.");

        // JSON 데이터를 제네릭 타입 T로 디시리얼라이즈
        var result = await JsonSerializer.DeserializeAsync<T>(
            utf8Json: stream,
            options: jsonOptions ?? JsonDefaultOptions.Options,
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("JSON 파싱에 실패했습니다.");

        return result;
    }
}
