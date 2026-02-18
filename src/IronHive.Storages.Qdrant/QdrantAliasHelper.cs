using IronHive.Abstractions.Vector;

namespace IronHive.Storages.Qdrant;

/// <summary>
/// Qdrant의 컬렉션 별칭을 사용하여 컬렉션 메타데이터를 저장하고 추출하는 헬퍼 클래스입니다.
/// </summary>
internal sealed class QdrantAliasHelper
{
    // 컬렉션 별칭의 접두사와 구분자를 정의합니다.
    // Qdrant에서 컬렉션 별칭은 커스텀 메타데이터를 저장하는 데 사용됩니다.
    internal const string Prefix = "vc_meta";
    internal const string Separator = "|";

    /// <summary>
    /// 컬렉션 별칭에서 컬렉션 정보를 추출하여 VectorCollection 객체로 반환합니다.
    /// </summary>
    internal static VectorCollectionInfo Parse(string alias)
    {
        var parts = alias.Split(Separator);
        if (parts.Length != 5 || !long.TryParse(parts[4].Trim(), out var dimensions))
            throw new ArgumentException("Invalid collection alias format", nameof(alias));

        return new VectorCollectionInfo
        {
            Name = parts[1],
            EmbeddingProvider = parts[2],
            EmbeddingModel = parts[3],
            Dimensions = dimensions
        };
    }

    /// <summary>
    /// 컬렉션 관련정보를 저장하기 위한 용도로 Qdrant의 컬렉션 별칭을 생성합니다.
    /// </summary>
    internal static string Create(VectorCollectionInfo collection)
    {
        var alias = string.Join(Separator,
            Prefix,
            collection.Name,
            collection.EmbeddingProvider,
            collection.EmbeddingModel,
            collection.Dimensions);

        return alias;
    }
}
