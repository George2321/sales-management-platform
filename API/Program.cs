using SalesManagementPlatform.Data;
using SalesManagementPlatform.Data.Repositories;
using SalesManagementPlatform.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var sqlitePath = builder.Configuration["Database:SqlitePath"]
    ?? throw new InvalidOperationException("Configure Database:SqlitePath in the API settings.");
var databasePath = Path.GetFullPath(sqlitePath, builder.Environment.ContentRootPath);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<IDbConnectionFactory>(new SqliteConnectionFactory(databasePath));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
