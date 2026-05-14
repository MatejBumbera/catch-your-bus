using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public sealed class AppDbContext : DbContext
{
    public DbSet<FavoriteRoute> FavoriteRoutes { get; set; }
    public DbSet<RunSession> RunSessions { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FavoriteRoute>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.StartLocationName).IsRequired().HasMaxLength(300);
            entity.Property(e => e.EndLocationName).IsRequired().HasMaxLength(300);
        });
        
        modelBuilder.Entity<RunSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.TargetBusStop).HasMaxLength(300);
            entity.Property(e => e.Success).IsRequired();
            entity.Property(e => e.DistanceMeters).IsRequired();
            entity.Property(e => e.DurationSeconds).IsRequired();
        });
    }
}
