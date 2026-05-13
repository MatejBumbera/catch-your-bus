using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catch_your_bus.Models.Services;

public class FavoriteRoutesService(AppDbContext dbContext)
{
    public async Task<List<FavoriteRoute>> GetFavoritesAsync()
    {
        return await dbContext.FavoriteRoutes.ToListAsync();
    }

    public async Task<FavoriteRoute> AddFavoriteAsync(
        string name, 
        string startName, double startLat, double startLng, 
        string endName, double endLat, double endLng)
    {
        var favorite = new FavoriteRoute
        {
            Name = name,
            StartLocationName = startName,
            StartLat = startLat,
            StartLng = startLng,
            EndLocationName = endName,
            EndLat = endLat,
            EndLng = endLng
        };

        dbContext.FavoriteRoutes.Add(favorite);
        await dbContext.SaveChangesAsync();
        return favorite;
    }

    public async Task DeleteFavoriteAsync(int id)
    {
        var favorite = await dbContext.FavoriteRoutes.FindAsync(id);
        if (favorite is not null)
        {
            dbContext.FavoriteRoutes.Remove(favorite);
            await dbContext.SaveChangesAsync();
        }
    }
}