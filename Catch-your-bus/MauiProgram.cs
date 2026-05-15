using Catch_your_bus.Models.Entities;
using Catch_your_bus.Models.Services;
using Catch_your_bus.ViewModels;
using Catch_your_bus.Views;
using CommunityToolkit.Maui;
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
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "catchyourbus.db3");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        builder.Services.AddHttpClient<IGoogleRoutesService, GoogleRoutesService>(client =>
            new GoogleRoutesService(client, Config.GoogleApiKey));
        
        builder.Services.AddHttpClient<IGooglePlacesService, GooglePlacesService>(client =>
            new GooglePlacesService(client, Config.GoogleApiKey));
        
        builder.Services.AddSingleton<ActiveTrackingState>();
        builder.Services.AddSingleton<ActiveRouteDetailsState>();
        
        builder.Services.AddSingleton<UserPreferencesService>();
        builder.Services.AddTransient<FavoriteRoutesService>();
        builder.Services.AddTransient<IRouteCalculationService, RouteCalculationService>();
        builder.Services.AddTransient<RunSessionService>();
        builder.Services.AddTransient<CalibrationService>();
        builder.Services.AddTransient<TrackingService>();

        builder.Services.AddTransient<RouteCalculationViewModel>();
        builder.Services.AddTransient<RouteCalculationPage>();
        
        builder.Services.AddTransient<RouteDetailsViewModel>();
        builder.Services.AddTransient<RouteDetailsPage>();

        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<ProfilePage>();
        
        builder.Services.AddTransient<TrackingViewModel>();
        builder.Services.AddTransient<TrackingPage>();
        
        builder.Services.AddTransient<RunHistoryViewModel>();
        builder.Services.AddTransient<RunHistoryPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}