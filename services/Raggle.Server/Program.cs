var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.UseUrls([
//        "http://localhost:5000",
//        "https://localhost:5001",
//        "http://localhost:5002",
//        "https://localhost:5003",
//        "http://localhost:5004",
//        "https://localhost:5005"
//    ]);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run("http://localhost:5000");
