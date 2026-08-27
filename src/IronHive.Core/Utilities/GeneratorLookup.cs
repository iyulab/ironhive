using System.Globalization;

namespace IronHive.Core.Utilities;

/// <summary>
/// provider 이름으로 등록된 generator를 찾는 공용 규칙입니다. provider를 지정하지 않으면
/// 단일 등록된 generator가 자동 선택되고, 둘 이상 등록돼 있으면 명시를 요구하는 예외가
/// 발생합니다. <see cref="Services.MessageService"/>의 내부 라우팅과
/// <see cref="HiveService"/>의 raw generator 조회(<c>GetMessageGenerator</c>/
/// <c>GetEmbeddingGenerator</c>)가 이 규칙을 공유합니다 — 두 곳이 각자 구현하면 조용히
/// 갈라질 수 있는 자리입니다.
/// </summary>
internal static class GeneratorLookup
{
    public static TGenerator GetRequired<TGenerator>(
        IReadOnlyDictionary<string, TGenerator> generators,
        string? provider,
        string kind,
        string? disambiguationHint = null)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            var entries = generators.ToList();
            if (entries.Count == 0)
            {
                var kindPascal = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(kind);
                throw new InvalidOperationException(
                    $"No {kind} generators are registered. Call Add{kindPascal}Generator() during setup.");
            }
            if (entries.Count > 1)
            {
                var hint = disambiguationHint is null ? "." : $" via {disambiguationHint}.";
                throw new InvalidOperationException(
                    $"Multiple {kind} generators are registered ({string.Join(", ", entries.Select(e => e.Key))}). " +
                    $"Specify a provider{hint}");
            }
            return entries[0].Value;
        }

        if (!generators.TryGetValue(provider, out var generator))
            throw new KeyNotFoundException($"{kind} generator '{provider}' is not registered.");
        return generator;
    }
}
