using AutoCenterPlus.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Параметры ИС: строка подключения из appsettings.json (Раздел 8, Шаг 5)
DbSettings.ConnectionString = builder.Configuration.GetConnectionString("Default")
    ?? @"Server=(localdb)\mssqllocaldb;Database=AutoCenterPlusDB;Trusted_Connection=True;";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger доступен во всех средах — для демонстрации и автотестов Postman (Раздел 9)
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
