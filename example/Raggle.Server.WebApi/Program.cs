using Microsoft.AspNetCore.Http.Features;
using Raggle.Server.WebApi;
using Raggle.Server.WebApi.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var config = TempConfigManager.Make(false);
builder.Services.AddRaggleServices(config);

builder.Services.AddControllers();
builder.Services.AddScoped<AssistantService>();
builder.Services.AddScoped<MemoryService>();

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

app.Services.EnsureRaggleDB();
app.MapControllers();
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
