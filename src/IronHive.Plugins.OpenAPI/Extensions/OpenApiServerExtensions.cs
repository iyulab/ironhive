using System.Text.RegularExpressions;

namespace Microsoft.OpenApi;

public static partial class OpenApiServerExtensions
{
    /// <summary>
    /// OpenApiServer.url의 {변수}와 variables(enum/default)을 조합하여 가능한 모든 절대 URL을 생성합니다.
    /// enum이 있으면 enum 전부를 사용하고, enum이 없으면 default만 사용합니다.
    /// </summary>
    public static IEnumerable<Uri> ExtractServerUrls(this OpenApiServer server)
    {
        var url = server.Url;
        if (string.IsNullOrWhiteSpace(url))
            yield break;

        // URL 안에 실제로 쓰인 플레이스홀더만 뽑기
        var placeholders = ServerVariableRegex().Matches(url)
                                                .Select(m => m.Groups["name"].Value)
                                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                                .ToList();

        // 변수 치환할 게 없으면 원본을 그대로 검증/반환
        if (placeholders.Count == 0)
        {
            yield return Uri.TryCreate(url, UriKind.Absolute, out var uri) 
                ? uri 
                : throw new InvalidOperationException($"서버 URL '{url}'이 올바른 절대 URL이 아닙니다.");
            
            yield break;
        }

        // variables에서 값 목록 만들기( enum ∪ default ). 사용된 변수만 필터
        var valueMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        if (server.Variables is { Count: > 0 })
        {
            foreach (var (key, variable) in server.Variables)
            {
                if (!placeholders.Contains(key))
                    continue;

                var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // default 추가
                if (!string.IsNullOrWhiteSpace(variable.Default))
                    set.Add(variable.Default);

                // enum 추가
                if (variable.Enum is { Count: > 0 })
                {
                    foreach (var v in variable.Enum)
                        if (!string.IsNullOrWhiteSpace(v))
                            set.Add(v);
                }

                if (set.Count > 0)
                    valueMap[key] = set.ToList();
            }
        }

        // 카테시안 곱으로 모든 조합 만들기
        var combos = new List<Dictionary<string, string>> { new(StringComparer.OrdinalIgnoreCase) };
        foreach (var name in placeholders)
        {
            var next = new List<Dictionary<string, string>>();
            foreach (var combo in combos)
            {
                foreach (var value in valueMap[name])
                {
                    var clone = new Dictionary<string, string>(combo, StringComparer.OrdinalIgnoreCase)
                    {
                        [name] = value
                    };
                    next.Add(clone);
                }
            }
            combos = next;
        }

        // 치환 & 중복 제거
        var urls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var combo in combos)
        {
            var resolved = url;
            foreach (var (k, v) in combo)
                resolved = resolved.Replace("{" + k + "}", v);

            urls.Add(resolved);
        }

        // 검증 & 반환
        foreach (var u in urls)
        {
            if (Uri.TryCreate(u, UriKind.Absolute, out var uri) && 
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                yield return uri;
            }
        }
    }

    /// <summary> OpenApiServer.url의 {변수}를 찾는 정규식 </summary>
    [GeneratedRegex(@"\{(?<name>[^{}]+)\}", RegexOptions.Compiled)]
    private static partial Regex ServerVariableRegex();
}
