using System.Globalization;
using System.Text.Json;
using Microsoft.OpenApi;

namespace IronHive.Plugins.OpenAPI;

/// <summary>
/// OpenAPI parameter의 style/explode/schema에 따른 직렬화 도우미 클래스입니다.
/// </summary>
internal static class OpenApiParameterSerializer
{
    /// <summary>
    /// path를 parameter에 따라 직렬화합니다.
    /// </summary>
    public static string SerializePath(IOpenApiParameter param, object? value)
    {
        if (value is null) return string.Empty;

        var type = param.Schema?.Type ?? JsonSchemaType.String;
        var style = param.Style ?? ParameterStyle.Simple; // path 기본: simple
        var explode = param.Explode;                      // bool (non-nullable)

        switch (style)
        {
            /* 
             * path의 label 스타일은 .(dot)으로 시작
             * - array: .value1,value2,value3 (explode=false)
             *          .value1.value2.value3 (explode=true)
             * - object: .key1,value1,key2,value2 (explode=false)
             *           .key1=value1.key2=value2 (explode=true)
             * - primitive: .value
             */
            case ParameterStyle.Label:
                if (type == JsonSchemaType.Array)
                    return "." + string.Join(explode ? "." : ",", Arrayify(value).Select(UrlEncode));
                if (type == JsonSchemaType.Object)
                    return "." + string.Join(explode ? "." : ",", Objectify(value).Select(kv => explode
                    ? $"{UrlEncode(kv.Key)}={UrlEncode(kv.Value)}"
                    : $"{UrlEncode(kv.Key)},{UrlEncode(kv.Value)}"));
                return "." + UrlEncode(Stringify(value));

            /*
             * Matrix 스타일은 ;(semicolon)으로 시작
             * - array: ;name=value1,value2,value3 (explode=false)
             *          ;name=value1;name=value2;name=value3 (explode=true)
             * - object: ;name=key1,value1,key2,value2 (explode=false)
             *           ;name=key1=value1;name=key2=value2 (explode=true)
             * - primitive: ;name=value
             */
            case ParameterStyle.Matrix:
                if (type == JsonSchemaType.Array)
                {
                    if (explode)
                        return string.Join(string.Empty, Arrayify(value).Select(v => $";{param.Name}={UrlEncode(v)}"));
                    else
                        return $";{param.Name}={string.Join(',', Arrayify(value).Select(UrlEncode))}";
                }
                if (type == JsonSchemaType.Object)
                {
                    if (explode)
                        return string.Join(string.Empty, Objectify(value).Select(kv => $";{UrlEncode(kv.Key)}={UrlEncode(kv.Value)}"));
                    else
                        return $";{param.Name}={string.Join(',', Objectify(value).SelectMany(kv => new[] { UrlEncode(kv.Key), UrlEncode(kv.Value) }))}";
                }
                return $";{param.Name}={UrlEncode(Stringify(value))}";


            /*
             * simple 스타일 (기본)
             * - array: value1,value2,value3 (explode=false)
             *          value1.value2.value3 (explode=true)
             * - object: key1,value1,key2,value2 (explode=false)
             *           key1=value1,key2=value2 (explode=true)
             * - primitive: value
             */
            default:
                if (type == JsonSchemaType.Array)
                    return string.Join(',', Arrayify(value).Select(UrlEncode));
                if (type == JsonSchemaType.Object)
                    return string.Join(',', Objectify(value).Select(kv => explode
                        ? new[] { $"{UrlEncode(kv.Key)}={UrlEncode(kv.Value)}" }
                        : new[] { UrlEncode(kv.Key), UrlEncode(kv.Value) }));
                return UrlEncode(Stringify(value));
        }
    }

    /// <summary>
    /// Query를 parameter에 따라 직렬화합니다.
    /// </summary>
    public static IEnumerable<KeyValuePair<string, string>> SerializeQuery(IOpenApiParameter p, object? value)
    {
        if (value is null) return Array.Empty<KeyValuePair<string, string>>();

        var type = p.Schema?.Type ?? JsonSchemaType.String;
        var style = p.Style ?? ParameterStyle.Form; // query 기본: form
        var explode = p.Explode;                    // bool (non-nullable)

        switch (style)
        {
            /*
             * SpaceDelimited: array만 허용, explode 무시
             * - array: name=value1 value2 value3
             */
            case ParameterStyle.SpaceDelimited:
                if (type != JsonSchemaType.Array) goto default;
                return [new(p.Name!, string.Join(' ', Arrayify(value)))];
            /*
             * PipeDelimited: array만 허용, explode 무시
             * - array: name=value1|value2|value3
             */
            case ParameterStyle.PipeDelimited:
                if (type != JsonSchemaType.Array) goto default;
                return [new(p.Name!, string.Join('|', Arrayify(value)))];
            /*
             * DeepObject: object만 허용, explode 무시
             * - object: name[key1]=value1&name[key2]=value2
             */
            case ParameterStyle.DeepObject:
                if (type != JsonSchemaType.Object) goto default;
                return Objectify(value).Select(kv => new KeyValuePair<string, string>($"{p.Name}[{kv.Key}]", kv.Value));
            /*
             * Form: 기본 스타일
             * - array: name=value1,value2,value3 (explode=false)
             *          name=value1&name=value2&name=value3 (explode=true)
             * - object: name=key1,value1,key2,value2 (explode=false)
             *           name=key1=value1&name=key2=value2 (explode=true)
             * - primitive: name=value
             */
            default:
                if (type == JsonSchemaType.Array)
                {
                    if (explode)
                        return Arrayify(value).Select(v => new KeyValuePair<string, string>(p.Name!, v));
                    else
                        return [new(p.Name!, string.Join(',', Arrayify(value)))];
                }
                if (type == JsonSchemaType.Object)
                {
                    if (explode)
                        return Objectify(value).Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value));
                    else
                        return [new(p.Name!, string.Join(',', Objectify(value).SelectMany(kv => new[] { kv.Key, kv.Value })))];
                }
                return [new(p.Name!, Stringify(value))];
        }
    }

    /// <summary>
    /// Header를 parameter에 따라 직렬화합니다.
    /// </summary>
    public static IEnumerable<KeyValuePair<string, string>> SerializeHeader(IOpenApiParameter p, object? value)
    {
        if (value is null) return Array.Empty<KeyValuePair<string, string>>();

        var type = p.Schema?.Type ?? JsonSchemaType.String;
        var explode = p.Explode; // header 기본: explode=false 가 스펙이지만 bool이므로 그대로 사용

        /*
         * header는 form 규칙을 기본으로 사용
         * - array: name=value1,value2,value3 (explode=false)
         *          name=value1&name=value2&name=value3 (explode=true)
         * - object: name=key1,value1,key2,value2 (explode=false)
         *           name=key1=value1&name=key2=value2 (explode=true)
         * - primitive: name=value
         */
        if (type == JsonSchemaType.Array)
        {
            if (explode)
                return Arrayify(value).Select(v => new KeyValuePair<string, string>(p.Name!, v));
            else
                return [new(p.Name!, string.Join(',', Arrayify(value)))];
        }
        else if (type == JsonSchemaType.Object)
        {
            if (explode)
                return Objectify(value).Select(kv => new KeyValuePair<string, string>(p.Name!, $"{kv.Key}={kv.Value}"));
            else
                return [new(p.Name!, string.Join(',', Objectify(value).SelectMany(kv => new[] { kv.Key, kv.Value })))];
        }
        else
        {
            return [new(p.Name!, Stringify(value))];
        }
    }

    /// <summary>
    /// Cookie를 parameter에 따라 직렬화합니다.
    /// </summary>
    public static IEnumerable<KeyValuePair<string, string>> SerializeCookie(IOpenApiParameter p, object? value)
    {
        if (value is null) return Array.Empty<KeyValuePair<string, string>>();

        var type = p.Schema?.Type ?? JsonSchemaType.String;
        var explode = p.Explode; // cookie: form 규칙. bool 그대로 사용

        /*
         * cookie는 form 규칙을 기본으로 사용
         * - array: name=value1,value2,value3 (explode=false)
         *          name=value1&name=value2&name=value3 (explode=true)
         * - object: name=key1,value1,key2,value2 (explode=false)
         *           name=key1=value1&name=key2=value2 (explode=true)
         * - primitive: name=value
         */
        if (type == JsonSchemaType.Array)
        {
            if (explode)
                return Arrayify(value).Select(v => new KeyValuePair<string, string>(p.Name!, v));
            else
                return [new(p.Name!, string.Join(',', Arrayify(value)))];
        }
        if (type == JsonSchemaType.Object)
        {
            if (explode)
                return Objectify(value).Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value));
            else
                return [new(p.Name!, string.Join(',', Objectify(value).SelectMany(kv => new[] { kv.Key, kv.Value })))];
        }
        return [new(p.Name!, Stringify(value))];
    }

    /// <summary> 다양한 객체를 문자열 딕셔너리로 변환 </summary>
    private static IEnumerable<KeyValuePair<string, string>> Objectify(object? value)
    {
        if (value.TryConvertTo<Dictionary<string, string>>(out var spair))
            return spair;
        if (value.TryConvertTo<Dictionary<string, object?>>(out var odict))
            return odict.Select(kv => new KeyValuePair<string, string>(kv.Key, Stringify(kv.Value)));

        return [new(string.Empty, Stringify(value))];
    }

    /// <summary> 다양한 객체를 문자열 배열로 변환 </summary>
    private static IEnumerable<string> Arrayify(object? value)
    {
        if (value.TryConvertTo<IEnumerable<string>>(out var seq))
            return seq;
        if (value.TryConvertTo<IEnumerable<object?>>(out var oseq))
            return oseq.Select(Stringify);
        return [Stringify(value)];
    }

    /// <summary> 다양한 객체를 문자열로 변환 </summary>
    private static string Stringify(object? value)
        => value switch
        {
            null => string.Empty,
            JsonElement je => je.ValueKind switch
            {
                JsonValueKind.Null => string.Empty,
                JsonValueKind.String => je.GetString() ?? string.Empty,
                JsonValueKind.Number => je.TryGetInt64(out var l) ? l.ToString(CultureInfo.InvariantCulture) : je.GetDouble().ToString(CultureInfo.InvariantCulture),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Array => string.Join(',', je.EnumerateArray().Select(e => Stringify(e))),
                JsonValueKind.Object => string.Join(',', je.EnumerateObject().Select(p => $"{p.Name},{Stringify(p.Value)}")),
                _ => je.ToString() ?? string.Empty
            },
            DateTime dt => dt.ToString("o", CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString("o", CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
            _ => value.ToString() ?? string.Empty
        };

    /// <summary> URL 인코딩 </summary>
    private static string UrlEncode(string s) => Uri.EscapeDataString(s);
}