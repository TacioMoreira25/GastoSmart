using GastoSmart.Services;
using GastoSmart.ViewModels;
using GastoSmart.Views;
using Microsoft.Extensions.Logging;

namespace GastoSmart;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug()
            .AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("System", LogLevel.Warning)
            .AddFilter("GastoSmart", LogLevel.Debug);
#endif

        // Services
        builder.Services.AddSingleton(new System.Net.Http.HttpClient());
        builder.Services.AddSingleton<ILocalDatabaseService, LocalDatabaseService>();
        builder.Services.AddSingleton<IUserSessionService, UserSessionService>();
        builder.Services.AddTransient<IGroqVisionService, GroqVisionService>();

        // ViewModels
        builder.Services.AddTransient<PerfilSelectionViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<NovaTransacaoViewModel>();

        // Views
        builder.Services.AddTransient<PerfilSelectionPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<NovaTransacaoPage>();

        return builder.Build();
    }
}