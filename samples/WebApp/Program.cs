using dotenv.net;
using IronHive.Providers.Anthropic;
using IronHive.Providers.OpenAI;
using IronHive.Abstractions;
using IronHive.Providers.GoogleAI;
using WebApp.Components;
using IronHive.Providers.OpenAI.Compatible;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "wwwroot"
});

#region For Services
DotEnv.Load(new DotEnvOptions(
    envFilePaths: [".env"],
    trimValues: true,
    overwriteExistingVars: false
));
var openaiKey = Environment.GetEnvironmentVariable("OPENAI");
var anthropicKey = Environment.GetEnvironmentVariable("ANTHROPIC");
var googleKey = Environment.GetEnvironmentVariable("GOOGLE");
var xaiKey = Environment.GetEnvironmentVariable("XAI");
var localKey = Environment.GetEnvironmentVariable("LOCAL");

// 각 프로바이더는 대응하는 API 키가 설정된 경우에만 등록됩니다.
// 클라이언트 생성자가 빈 키를 거부하므로, 미설정 프로바이더를 등록하면 앱이 기동조차 하지 못합니다.
builder.Services.AddHiveService((builder, sp) =>
{
    if (!string.IsNullOrEmpty(openaiKey))
        builder.AddOpenAIProviders("openai", new OpenAIConfig { ApiKey = openaiKey });

    if (!string.IsNullOrEmpty(anthropicKey))
        builder.AddAnthropicProviders("anthropic", new AnthropicConfig { ApiKey = anthropicKey });

    if (!string.IsNullOrEmpty(googleKey))
        builder.AddGoogleAIProviders("google", new GoogleAIConfig { ApiKey = googleKey });

    if (!string.IsNullOrEmpty(xaiKey))
        builder.AddOpenAIProviders("xai", new OpenAIConfig { BaseUrl = "https://api.x.ai/v1/", ApiKey = xaiKey });

    // OpenAI 호환 서버는 보통 자격증명이 필요 없으므로 키 유무로 등록을 막지 않는다.
    // 주소는 LOCAL_BASE_URL 로 바꾼다.
    builder.AddOpenAICompatibleProviders("local", new OpenAICompatibleConfig
    {
        BaseUrl = Environment.GetEnvironmentVariable("LOCAL_BASE_URL") ?? "http://localhost:8080",
        ApiKey = localKey ?? string.Empty
    });
    
    return builder.Build();
});
#endregion

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.UseDeveloperExceptionPage();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
