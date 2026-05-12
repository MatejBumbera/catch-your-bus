using Catch_your_bus.Models.Services;
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
        
        builder.Services.AddHttpClient<IGoogleRoutesService, GoogleRoutesService>(client => new GoogleRoutesService(client, Config.GoogleRoutesApiKey));

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}