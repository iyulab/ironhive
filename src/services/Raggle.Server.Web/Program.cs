using Microsoft.EntityFrameworkCore;
using Raggle.Server.Web.Database;
using Raggle.Server.Web.Hubs;
using Raggle.Server.Web.Models;
using Raggle.Server.Web.Options;
using Raggle.Server.Web.Services;
using Raggle.Server.Web.Storages;
using Raggle.Server.Web.Stores;

var builder = WebApplication.CreateBuilder(args);

// Configuration 설정
builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAI"));

// 데이터베이스 설정
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")));
builder.Services.AddScoped<AppRepository<User>>();
builder.Services.AddScoped<AppRepository<Assistant>>();
builder.Services.AddScoped<AppRepository<Knowledge>>();
builder.Services.AddScoped<AppRepository<Connection>>();
builder.Services.AddScoped<AppRepository<OpenAPI>>();

// 파일, 벡터, 시그널R 저장소
builder.Services.AddSingleton<ConnectionStore>();
builder.Services.AddSingleton<FileStorage>();
builder.Services.AddSingleton<VectorStorage>();

// 서비스 설정
builder.Services.AddScoped<UserAssistantService>();
builder.Services.AddScoped<ChatGenerateService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

var app = builder.Build();

// 데이터베이스 초기화
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();  // 테이블 생성
}

// 미들웨어 설정
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();

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
app.MapHub<AppHub>("/stream");

app.Run();
