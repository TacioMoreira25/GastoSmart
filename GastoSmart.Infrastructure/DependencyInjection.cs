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
        
        var supabaseUrl = configuration["Supabase:Url"]?.TrimEnd('/');
        if (!string.IsNullOrEmpty(supabaseUrl) && !supabaseUrl.StartsWith("http"))
        {
            supabaseUrl = "https://" + supabaseUrl;
        }

        // Download manual forçado das chaves públicas (JWKS) com bypass de SSL (UntrustedRoot)
        var jwksUrl = $"{supabaseUrl}/auth/v1/.well-known/jwks.json";
        var handler = new System.Net.Http.HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        using var httpClient = new System.Net.Http.HttpClient(handler);
        var jwksJson = httpClient.GetStringAsync(jwksUrl).GetAwaiter().GetResult();
        var jwks = new Microsoft.IdentityModel.Tokens.JsonWebKeySet(jwksJson);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = jwks.Keys, // Injeção direta das chaves ECC
                    ValidateIssuer = true,
                    ValidIssuer = $"{supabaseUrl}/auth/v1",
                    ValidateAudience = true,
                    ValidAudience = "authenticated",
                    ValidateLifetime = true
                };
            });

        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddHttpClient<IReceiptAnalyzerService, GroqReceiptAnalyzerService>();

        return services;
    }
}
