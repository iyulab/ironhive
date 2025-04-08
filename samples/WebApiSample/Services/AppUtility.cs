using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace WebApiSample.Services;

/// <summary>
/// 서비스 유틸리티 객체입니다.
/// </summary>
public static partial class AppUtility
{
    private static readonly char _modelSeparator = '/';
    private static readonly string _collectionPrefix = "v_";
    private static readonly string _collectionSeparator = "__sep__";

    /// <summary>
    /// 문자열을 (Provider, Model)로 변환합니다.
    /// </summary>
    /// <param name="identifier">식별자 입니다.</param>
    public static (string, string) ParseModelIdentifier(string modelIdentifier)
    {
        var split = modelIdentifier.Split(_modelSeparator, 2, StringSplitOptions.TrimEntries);

        if (split.Length != 2)
            throw new ArgumentException($"Identifier must contain exactly one '{_modelSeparator}' character.");
        if (string.IsNullOrWhiteSpace(split[0]) || string.IsNullOrWhiteSpace(split[1]))
            throw new ArgumentException("Identifier must not contain empty parts.");

        return (split[0], split[1]);
    }

    [GeneratedRegex(@"[^a-zA-Z0-9]")]
    private static partial Regex CollectionSanitizedRegex();

    /// <summary>
    /// 포맷된 식별자를 생성합니다.
    /// </summary>
    /// <param name="provider">모델 제공자 키</param>
    /// <param name="model">모델 이름</param>
    public static string FormatModelIdentifier(string provider, string model)
    {
        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(model))
        {
            throw new ArgumentException("Provider and model must not be null or whitespace.");
        }
        return $"{provider.Trim()}{_modelSeparator}{model.Trim()}";
    }

    /// <summary>
    /// 모델 식별자를 사용하여, DB 컬렉션의 이름을 생성합니다.
    /// </summary>
    public static string ConvertCollectionName(string modelIdentifier)
    {
        if (string.IsNullOrWhiteSpace(modelIdentifier))
        {
            throw new ArgumentException("Model identifier must not be null or whitespace.");
        }

        var (provider, model) = ParseModelIdentifier(modelIdentifier);

        // 영문자, 숫자를 제외한 모든 문자를 '_'로 변환합니다.
        provider = CollectionSanitizedRegex().Replace(provider, "_");
        model = CollectionSanitizedRegex().Replace(model, "_");

        return $"{_collectionPrefix}{provider}{_collectionSeparator}{model}";
    }
}

