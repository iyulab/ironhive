using Microsoft.AspNetCore.Http.Features;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using WebServer.Dev;
using WebServer;
using IronHive.Core.Mcp;

var builder = WebApplication.CreateBuilder(args);

# region For Services
builder.Services.AddMainSerivces();
builder.Services.AddSingleton<JobObject>();
builder.Services.AddSingleton<ProcessStore>();
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

    app.UseHttpsRedirection();
    app.UseHsts();

    app.Urls.Clear();
    app.Urls.Add("http://*:8080");
}

//var manager = new McpServerManager();
//await manager.StartAsync("test", new McpSseServer
//{
//    Endpoint = new Uri("http://localhost:8000/sse"),
//});

//var tools = await manager.ListToolsAsync("test");
//foreach (var tool in tools)
//{
//    var res = await tool.InvokeAsync(new
//    {
//        location = "seoul",
//    });
//}


app.Run();
