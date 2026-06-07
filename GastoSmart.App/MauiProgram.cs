using GastoSmart.App;
using Microsoft.Extensions.Logging;
using GastoSmart.App.Services;
using Microcharts.Maui;

namespace Gastosmart.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.UseMauiApp<App>()
            .UseMicrocharts()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug()
            .AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("System", LogLevel.Warning)
            .AddFilter("Gastosmart.App", LogLevel.Debug);
#endif
        builder.Services.AddHttpClient("GastoSmartClient", client =>
        {
            client.BaseAddress = new Uri(LocalConfig.ApiBaseUrl);
        });

        builder.Services.AddTransient<ApiService>(sp => 
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("GastoSmartClient");
            return new ApiService(client);
        });
        
        builder.Services.AddTransient<AuthService>(sp => 
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient();
            return new AuthService(client);
        });

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        
        builder.Services.AddTransient<GastoSmart.App.ViewModels.DashboardViewModel>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<SaveTransactionPage>();

        return builder.Build();
    }
}