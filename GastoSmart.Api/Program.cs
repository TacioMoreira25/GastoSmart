using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using GastoSmart.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

Env.TraversePath().Load();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

connectionString = connectionString
    .Replace("{DB_HOST}", Environment.GetEnvironmentVariable("DB_HOST"))
    .Replace("{DB_PORT}", Environment.GetEnvironmentVariable("DB_PORT"))
    .Replace("{DB_NAME}", Environment.GetEnvironmentVariable("DB_NAME"))
    .Replace("{DB_USER}", Environment.GetEnvironmentVariable("DB_USER"))
    .Replace("{DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD"));

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

