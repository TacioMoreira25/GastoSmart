using GastoSmart.App;
using Microsoft.Extensions.Logging;
using GastoSmart.App.Services;
using Microsoft.Extensions.Hosting;
// using Microcharts.Maui;

namespace Gastosmart.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.AddServiceDefaults();

        builder.UseMauiApp<App>()
            // .UseMicrocharts()
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
            var safeStorage = sp.GetRequiredService<ISafeStorageService>();
            return new ApiService(client, safeStorage);
        });
        
        builder.Services.AddTransient<AuthService>(sp => 
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient();
            var safeStorage = sp.GetRequiredService<ISafeStorageService>();
            return new AuthService(client, safeStorage);
        });

        builder.Services.AddSingleton<ISafeStorageService, SafeStorageService>();

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        
        builder.Services.AddTransient<GastoSmart.App.ViewModels.DashboardViewModel>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<SaveTransactionPage>();
        builder.Services.AddTransient<GastoSmart.App.ViewModels.NovaTransacaoViewModel>();
        builder.Services.AddTransient<NovaTransacaoPage>();

        return builder.Build();
    }
}