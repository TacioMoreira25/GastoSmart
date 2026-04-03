using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GastoSmart.Infrastructure.Data;
using GastoSmart.Infrastructure.Repositories;
using GastoSmart.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GastoSmart.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        var jwtSecret = configuration["Supabase:JwtSecret"] ?? throw new InvalidOperationException("JWT secret is not configured.");
        var validAudience = configuration["Supabase:ValidAudience"] ?? "authenticated";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidateAudience = true,
                    ValidAudience = validAudience,
                    ValidateIssuer = false,
                    ValidateLifetime = true

                };
            });

        services.AddScoped<ITransactionRepository, TransactionRepository>();

        return services;
    }
}
