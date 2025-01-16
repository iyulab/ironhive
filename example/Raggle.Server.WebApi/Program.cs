using Microsoft.AspNetCore.Http.Features;
using Raggle.Abstractions;
using Raggle.Core.Extensions;
using Raggle.Server;
using Raggle.Server.Configurations;
using Raggle.Server.Extensions;
using Raggle.Server.Tools;
using Raggle.Server.WebApi.Development;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

# region For Raggle
var cm = new JsonConfigManager(Path.Combine(Directory.GetCurrentDirectory(),"raggle_settings.json"));
if (cm.Config == null)
    throw new Exception("Failed to load raggle_settings.json");

builder.Services.AddRaggleServices(cm.Config);
builder.Services.AddToolService<DatabaseTool>("database_search");
#endregion

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
                //.AllowCredentials();
        });
    });
    builder.Services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 10_737_418_240; // 10GB
    });
}
else
{
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();
}

var app = builder.Build();

# region For Raggle
app.Services.EnsureRaggleServices();
#endregion

app.MapControllers();
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseMiddleware<ControllerMiddleware>();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseHttpsRedirection();
    app.UseHsts();

    app.Urls.Add("https://*:7297");
    app.Urls.Add("http://*:5075");
}

app.Run();
