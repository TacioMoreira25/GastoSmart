using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GastoSmart.Infrastructure.Data;
using GastoSmart.Infrastructure.Repositories;
using GastoSmart.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GastoSmart.Application.Services;
using GastoSmart.Infrastructure.Services;

namespace GastoSmart.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        
        var supabaseUrl = configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase URL is not configured.");        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"{supabaseUrl}/auth/v1";

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"{supabaseUrl}/auth/v1", 
                    
                    ValidateAudience = false, 
                    ValidateLifetime = true
                };
            });

        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddHttpClient<IReceiptAnalyzerService, GroqReceiptAnalyzerService>();

        return services;
    }
}
