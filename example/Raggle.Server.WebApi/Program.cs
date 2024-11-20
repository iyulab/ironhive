using Microsoft.EntityFrameworkCore;
using Raggle.Server.WebApi;
using Raggle.Server.WebApi.Data;
using Raggle.Server.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
    options.AddInterceptors(new AppDbIntercepter());
});

builder.Services.AddControllers();
builder.Services.AddRaggle();
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
            builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
    });
}
else
{
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

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
