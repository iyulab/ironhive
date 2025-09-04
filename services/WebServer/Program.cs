using Microsoft.AspNetCore.Http.Features;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using WebServer.Dev;
using WebServer;
using dotenv.net;
using WebServer.Controllers;
using IronHive.Core.Files.Decoders;
using IronHive.Abstractions.Files;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
#if DEBUG
    WebRootPath = "wwwroot"
#else
    WebRootPath = "/var/app/wwwroot",
#endif
});

#region For Services
DotEnv.Load(new DotEnvOptions(
    envFilePaths: [".env",".env.development"],
    trimValues: true,
    overwriteExistingVars: false
));
builder.Services.AddMainSerivces();
#endregion

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10_737_418_240; // 10GB
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}
else
{
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();
}

var app = builder.Build();

app.UseRouting();
app.UseStaticFiles();
app.MapControllers();
app.MapFallbackToFile("index.html");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(builder => builder.AllowAnyOrigin()
                                  .AllowAnyMethod()
                                  //.AllowCredentials()
                                  .AllowAnyHeader());

    app.UseMiddleware<Middleware>();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseAuthentication();
    app.UseAuthorization();

    //app.UseHttpsRedirection();
    //app.UseHsts();

    app.Urls.Clear();
    app.Urls.Add("http://*:80");
}

app.Run();
