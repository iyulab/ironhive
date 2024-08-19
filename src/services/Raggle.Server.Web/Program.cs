using Raggle.Server.Web;
using Raggle.Server.Web.Hubs;
using Raggle.Server.Web.Options;
using Raggle.Server.Web.Services;
using Raggle.Server.Web.Storages;
using Raggle.Server.Web.Stores;

var builder = WebApplication.CreateBuilder(args);

// Configuration 설정
builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAI"));

// 데이터베이스 설정
builder.Services.AddDatabaseServices(builder.Configuration);

// 파일, 벡터, 시그널R 저장소
builder.Services.AddSingleton<ConnectionStore>();
builder.Services.AddSingleton<FileStorage>();
builder.Services.AddSingleton<VectorStorage>();

// 서비스 설정
builder.Services.AddSingleton<ChatGenerateService>();
builder.Services.AddScoped<UserAssistantService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

var app = builder.Build();

// 데이터베이스 초기화
app.InitializeDatabase();

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
