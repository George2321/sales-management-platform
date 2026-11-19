using SalesManagementPlatform.Api.Configuration;
using SalesManagementPlatform.Data.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabaseAndRepositories(builder.Configuration, builder.Environment);
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

var app = builder.Build();

await app.Services.GetRequiredService<SqliteDatabaseInitializer>()
    .InitializeAsync(app.Configuration.GetValue<bool>("Database:SeedDemoData"));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var serveUi = app.Environment.WebRootFileProvider.GetFileInfo("index.html").Exists;
if (serveUi)
{
    app.UseStaticFiles();
}

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/api/health");

if (serveUi)
{
    app.MapFallback("/api/{**path}", () => Results.NotFound());
    app.MapFallbackToFile("index.html");
}

app.Run();
