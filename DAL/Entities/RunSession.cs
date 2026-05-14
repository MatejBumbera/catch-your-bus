using System.ComponentModel.DataAnnotations;

namespace DAL.Entities;

public class RunSession
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public string? TargetBusStop { get; set; } = null;
    public bool Success { get; set; }
    public double DistanceMeters { get; set; }
    public int DurationSeconds { get; set; }
}