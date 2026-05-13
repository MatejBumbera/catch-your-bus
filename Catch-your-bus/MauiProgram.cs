using Catch_your_bus.Models.Services;
using Catch_your_bus.ViewModels;
using Catch_your_bus.Views;
using DAL;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Catch_your_bus;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "catchyourbus.db3");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        builder.Services.AddHttpClient<IGoogleRoutesService, GoogleRoutesService>(client =>
            new GoogleRoutesService(client, Config.GoogleRoutesApiKey));
        
        builder.Services.AddSingleton<UserPreferencesService>();
        builder.Services.AddTransient<IRouteCalculationService, RouteCalculationService>();
        
        builder.Services.AddTransient<RouteCalculationViewModel>();
        
        builder.Services.AddTransient<RouteCalculationPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}