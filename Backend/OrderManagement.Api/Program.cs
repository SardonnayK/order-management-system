using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Data;
using OrderManagement.Api.Endpoints;

// NoClobber: variables already set by the host (Aspire, Docker, launchSettings)
// take precedence over the .env file.
DotNetEnv.Env.TraversePath().NoClobber().Load();

var builder = WebApplication.CreateBuilder(args);

// When no host (Aspire, docker-compose, launchSettings) supplies ASPNETCORE_URLS,
// fall back to the port configured in .env.
var apiPort = builder.Configuration["API_PORT"];
if (!string.IsNullOrEmpty(apiPort) &&
    string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls($"http://localhost:{apiPort}");
}

builder.AddServiceDefaults();

var connectionString = builder.Configuration["DB_CONNECTION_STRING"]
    ?? throw new InvalidOperationException(
        "DB_CONNECTION_STRING is not set. Define it in the .env file at the repository root (see .env.example).");

// SQLite does not create missing directories, so make sure the db folder exists.
var dbPath = new SqliteConnectionStringBuilder(connectionString).DataSource;
var dbDirectory = Path.GetDirectoryName(Path.GetFullPath(dbPath));
if (!string.IsNullOrEmpty(dbDirectory))
{
    Directory.CreateDirectory(dbDirectory);
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

// The Angular dev server runs on a different origin than the API.
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

app.MapDefaultEndpoints();

app.MapOrderEndpoints();

app.Run();
