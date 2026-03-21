using DotNetEnv;
using GastoSmart.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

Env.TraversePath().Load();

builder.Services.AddInfrastructure();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

