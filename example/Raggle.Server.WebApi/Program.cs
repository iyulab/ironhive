using Microsoft.AspNetCore.Http.Features;
using Raggle.Core.Extensions;
using Raggle.Server.Configurations;
using Raggle.Server.Extensions;
using Raggle.Server.Tools;
using Raggle.Server.WebApi;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

# region For Raggle
var cm = new JsonConfigManager(Path.Combine(Directory.GetCurrentDirectory(),"appservicesettings.json"));
if (cm.Config == null)
    throw new Exception("Failed to load raggle_settings.json");
Directory.CreateDirectory("/var/db");

builder.Services.AddDefaultServices(cm.Config);
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
}
else
{
    //builder.Services.AddAuthentication();
    //builder.Services.AddAuthorization();
}

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10_737_418_240; // 10GB
});

var app = builder.Build();

# region For Raggle
app.Services.EnsureRaggleServices();
#endregion

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
    //app.UseAuthentication();
    //app.UseAuthorization();

    //app.UseHttpsRedirection();
    //app.UseHsts();

    app.Urls.Clear();
    app.Urls.Add("http://*:80");
}

app.Run();
