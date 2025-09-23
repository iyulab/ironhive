using System.Text.Json.Serialization;

namespace ConsoleTest;

/// <summary>
/// OpenAPI 문서에서 정의된 보안 스키마에 대응하는 비밀(시크릿) 타입의 공통 인터페이스입니다.
/// ApiKey, HTTP, OAuth2, OpenID Connect, Mutual TLS 등의 인증 방식중, 일부를 지원합니다.
/// 참고: <see href="https://learn.openapis.org/specification/security.html"/>
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ApiKeyAuthSecret), "apiKey")]
[JsonDerivedType(typeof(HttpBasicAuthSecret), "basic")]
[JsonDerivedType(typeof(HttpBearerAuthSecret), "bearer")]
[JsonDerivedType(typeof(OAuth2AuthSecret), "oauth2")]
[JsonDerivedType(typeof(OpenIdConnectAuthSecret), "openIdConnect")]
public interface IOpenApiAuthSecret 
{ }

public sealed record ApiKeyAuthSecret(string Value) : IOpenApiAuthSecret;

public sealed record HttpBasicAuthSecret(string Username, string Password) : IOpenApiAuthSecret;

public sealed record HttpBearerAuthSecret(string Token) : IOpenApiAuthSecret;

public sealed record OAuth2AuthSecret(string AccessToken) : IOpenApiAuthSecret;

public sealed record OpenIdConnectAuthSecret(string AccessToken) : IOpenApiAuthSecret;
