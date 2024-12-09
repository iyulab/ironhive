using Microsoft.AspNetCore.Http.Features;
using Raggle.Server;
using Raggle.Server.WebApi;
using Raggle.Server.WebApi.Development;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

# region For Raggle
var config = TempConfigManager.Make(false);
builder.Services.AddRaggleServices(config);
#endregion

Console.WriteLine("Default Options");
var json = JsonSerializer.Serialize(new JsonSerializerOptions(), new JsonSerializerOptions
{
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter() }
});
Console.WriteLine(json);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        var json = JsonSerializer.Serialize(options, new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        });
        Console.WriteLine(json);
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
