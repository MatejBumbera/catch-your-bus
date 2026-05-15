using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities;

public class RunSession
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public string? TargetBusStop { get; set; } = null;
    public bool Success { get; set; }
    public double DistanceMeters { get; set; }
    public int DurationSeconds { get; set; }
    public double CaloriesBurned { get; set; }
    public int TimeSavedSeconds { get; set; }
    public string? RouteIdentifier { get; set; }
    public string? RouteJson { get; set; }
    
    [NotMapped] 
    public bool IsPersonalBest { get; set; }
}