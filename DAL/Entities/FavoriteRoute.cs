using System.ComponentModel.DataAnnotations;

namespace DAL.Entities;

/// <summary>
/// Entity representing a user's saved favorite route.
/// Stores both human-readable location names and precise GPS coordinates.
/// </summary>
public class FavoriteRoute
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// User-defined name for the favorite route (e.g. "Home → Work").
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    public string StartLocationName { get; set; } = string.Empty;
    
    public double StartLat { get; set; }
    
    public double StartLng { get; set; }
    
    public string EndLocationName { get; set; } = string.Empty;
    
    public double EndLat { get; set; }
    
    public double EndLng { get; set; }
}
