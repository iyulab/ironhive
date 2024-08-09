using Raggle.Server.API.Assistant;
using Raggle.Server.API.Hubs;
using Raggle.Server.API.Repositories;
using Raggle.Server.API.Storages;
using Raggle.Server.API.Stores;
using Raggle.Server.Web.Repositories;
using Raggle.Server.Web.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

var connectionString = builder.Configuration.GetConnectionString("Sqlite");
DatabaseService.InitializeSqlite(connectionString).Wait();

// 데이터베이스기반 저장소
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<SourceRepository>();
builder.Services.AddSingleton<ConnectionStore>();

// 파일기반 저장소
builder.Services.AddSingleton<FileStorage>();
builder.Services.AddSingleton<VectorStorage>();

// AI채널
builder.Services.AddScoped<DescriptionAssistant>();
builder.Services.AddScoped<SearchAssistant>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(builder =>
    {
        builder.WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/stream");

app.Run();
