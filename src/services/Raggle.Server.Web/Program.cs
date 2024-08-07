using Raggle.Server.API.Assistant;
using Raggle.Server.API.Hubs;
using Raggle.Server.API.Repositories;
using Raggle.Server.API.Storages;
using Raggle.Server.API.Stores;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddSingleton<ConnectionStore>();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<FileStorage>();
builder.Services.AddSingleton<VectorStorage>();
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
app.MapHub<ChatHub>("/chat");

app.Run();
