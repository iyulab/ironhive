using Microsoft.OpenApi;
using System.Text.Json.Serialization;

namespace IronHive.Plugins.OpenAPI;

/// <summary>
/// OpenAPI 문서에서 정의된 보안 스키마에 대응하는 비밀(시크릿) 타입의 공통 인터페이스입니다.
/// ApiKey, HTTP, OAuth2, OpenID Connect, Mutual TLS 등의 인증 방식중, 일부를 지원합니다.
/// 참고: <see href="https://learn.openapis.org/specification/security.html"/>
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ApiKeyCredential), "apiKey")]
[JsonDerivedType(typeof(HttpBasicCredential), "basic")]
[JsonDerivedType(typeof(HttpBearerCredential), "bearer")]
[JsonDerivedType(typeof(OAuth2Credential), "oauth2")]
[JsonDerivedType(typeof(OpenIdConnectCredential), "openIdConnect")]
public interface IOpenApiCredential 
{
    /// <summary> 현재 인증 객체가 지정된 보안 스키마와 일치하는지 여부를 반환합니다. </summary>
    bool Match(OpenApiSecuritySchemeReference scheme);
}

/// <summary> API Key 인증을 위한 시크릿 타입입니다. </summary>
public sealed record ApiKeyCredential(string Value) : IOpenApiCredential
{
    /// <inheritdoc />
    public bool Match(OpenApiSecuritySchemeReference scheme)
        => scheme.Type == SecuritySchemeType.ApiKey && scheme.In != null && !string.IsNullOrEmpty(scheme.Name);
}

/// <summary> HTTP Basic 인증을 위한 시크릿 타입입니다. </summary>
public sealed record HttpBasicCredential(string Username, string Password) : IOpenApiCredential
{
    /// <inheritdoc />
    public bool Match(OpenApiSecuritySchemeReference scheme)
        => scheme.Type == SecuritySchemeType.Http && (scheme.Scheme?.Equals("basic", StringComparison.OrdinalIgnoreCase) ?? false);
}

/// <summary> HTTP Bearer 인증을 위한 시크릿 타입입니다. </summary>
public sealed record HttpBearerCredential(string Token) : IOpenApiCredential
{
    /// <inheritdoc />
    public bool Match(OpenApiSecuritySchemeReference scheme)
        => scheme.Type == SecuritySchemeType.Http && (scheme.Scheme?.Equals("bearer", StringComparison.OrdinalIgnoreCase) ?? false);
}

/// <summary> OAuth2 인증을 위한 시크릿 타입입니다. </summary>
public sealed record OAuth2Credential(string AccessToken) : IOpenApiCredential
{
    /// <inheritdoc />
    public bool Match(OpenApiSecuritySchemeReference scheme)
        => scheme.Type == SecuritySchemeType.OAuth2;
}

/// <summary> OpenID Connect 인증을 위한 시크릿 타입입니다. </summary>
public sealed record OpenIdConnectCredential(string AccessToken) : IOpenApiCredential
{
    /// <inheritdoc />
    public bool Match(OpenApiSecuritySchemeReference scheme)
        => scheme.Type == SecuritySchemeType.OpenIdConnect;
}
