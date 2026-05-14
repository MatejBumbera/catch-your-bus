using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catch_your_bus.Models.Services;

public class RunSessionService(AppDbContext dbContext)
{
    public async Task AddSessionAsync(RunSession session)
    {
        dbContext.RunSessions.Add(session);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<RunSession>> GetAllSessionsAsync()
    {
        return await dbContext.RunSessions
            .OrderByDescending(r => r.Date)
            .ToListAsync();
    }
    
    public async Task ClearHistoryAsync()
    {
        var allSessions = await dbContext.RunSessions.ToListAsync();
        dbContext.RunSessions.RemoveRange(allSessions);
        await dbContext.SaveChangesAsync();
    }
}