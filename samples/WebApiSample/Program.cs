/*
 * 패키지
 * 1. IronHive.Core
 * 2. IronHive.Connectors.OpenAI
 * 3. Microsoft.EntityFrameworkCore
 * 4. Microsoft.EntityFrameworkCore.Sqlite
 */

using WebApiSample;

var builder = WebApplication.CreateBuilder(args);

#region 서비스 설정

builder.UseSampleServices();

#endregion

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
