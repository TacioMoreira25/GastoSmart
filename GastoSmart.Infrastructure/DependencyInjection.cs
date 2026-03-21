using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using GastoSmart.Infrastructure.Data;

namespace GastoSmart.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        var connBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = Environment.GetEnvironmentVariable("DB_HOST"),
            Database = Environment.GetEnvironmentVariable("DB_NAME"),
            Username = Environment.GetEnvironmentVariable("DB_USER"),
            Password = Environment.GetEnvironmentVariable("DB_PASSWORD")
        };

        if (int.TryParse(Environment.GetEnvironmentVariable("DB_PORT"), out int port))
        {
            connBuilder.Port = port;
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connBuilder.ConnectionString);
        });

        return services;
    }
}
