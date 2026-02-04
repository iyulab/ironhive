using System.Text.Json.Nodes;

namespace IronHive.Providers.OpenAI.Compatible.Adapters;

/// <summary>
/// Provider 어댑터의 기본 구현입니다.
/// </summary>
public abstract class BaseProviderAdapter : IProviderAdapter
{
    /// <inheritdoc />
    public abstract CompatibleProvider Provider { get; }

    /// <summary>
    /// 제공자의 기본 URL입니다.
    /// </summary>
    protected abstract string DefaultBaseUrl { get; }

    /// <inheritdoc />
    public virtual string GetBaseUrl(CompatibleConfig config)
    {
        return !string.IsNullOrEmpty(config.BaseUrl)
            ? config.BaseUrl
            : DefaultBaseUrl;
    }

    /// <inheritdoc />
    public virtual IDictionary<string, string> GetAdditionalHeaders(CompatibleConfig config)
    {
        return new Dictionary<string, string>();
    }

    /// <inheritdoc />
    public virtual JsonObject TransformRequest(JsonObject request, CompatibleConfig config)
    {
        RemoveUnsupportedParameters(request);
        return request;
    }

    /// <inheritdoc />
    public virtual JsonObject TransformResponse(JsonObject response)
    {
        return response;
    }

    /// <inheritdoc />
    public virtual JsonObject TransformStreamingChunk(JsonObject chunk)
    {
        return chunk;
    }

    /// <inheritdoc />
    public virtual void RemoveUnsupportedParameters(JsonObject request)
    {
        // 기본적으로 아무것도 제거하지 않음
    }

    /// <summary>
    /// JSON 객체에서 지정된 속성들을 제거합니다.
    /// </summary>
    protected static void RemoveProperties(JsonObject obj, params string[] properties)
    {
        foreach (var prop in properties)
        {
            obj.Remove(prop);
        }
    }

    /// <summary>
    /// JSON 객체에 속성이 없으면 추가합니다.
    /// </summary>
    protected static void SetIfNotExists(JsonObject obj, string property, JsonNode? value)
    {
        if (!obj.ContainsKey(property))
        {
            obj[property] = value;
        }
    }
}
