using System.Diagnostics;

namespace Catch_your_bus.Models.Services;

public class CalibrationService
{
    private double _totalDistanceKm;
    private Location? _lastLocation;
    private DateTimeOffset _startTime;
    private bool _isTracking;

    public event EventHandler<double>? DistanceUpdated;

    public async Task<bool> StartListeningAsync()
    {
        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted) return false;
        
        var initialLocation = await GetAccurateLocationAsync(TimeSpan.FromSeconds(4));
        
        if (initialLocation == null) 
        {
            System.Diagnostics.Debug.WriteLine("Failed to get initial GPS anchor.");
            return false;
        }

        _totalDistanceKm = 0;
        _lastLocation = null;
        _startTime = DateTimeOffset.UtcNow;
        _isTracking = true;
        
        System.Diagnostics.Debug.WriteLine("Initial GPS Anchor set successfully.");

        _ = TrackLocationLoopAsync();

        return true;
    }

    private async Task TrackLocationLoopAsync()
    {
        while (_isTracking)
        {
            await Task.Delay(2000);
            
            if (!_isTracking) break;
            var newLocation = await GetAccurateLocationAsync(TimeSpan.FromSeconds(2));

                if (newLocation != null && _lastLocation != null)
                {
                    var segmentDistance =
                        Location.CalculateDistance(_lastLocation, newLocation, DistanceUnits.Kilometers);

                    _totalDistanceKm += segmentDistance;
                    DistanceUpdated?.Invoke(this, _totalDistanceKm);

                    _lastLocation = newLocation;
                }
            
            await Task.Delay(2000);
        }
    }
    
    private static async Task<Location?> GetAccurateLocationAsync(TimeSpan timeout)
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Best, timeout);
            return await Geolocation.Default.GetLocationAsync(request);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GPS Pull Failed: {ex.Message}");
            return null;
        }
    }

    public double? StopListening()
    {
        _isTracking = false;
        var durationHours = (DateTimeOffset.UtcNow - _startTime).TotalHours;

        if (durationHours <= 0 || _totalDistanceKm < 0.01) return null;

        var speedKmH = _totalDistanceKm / durationHours;
        return Math.Min(speedKmH, 35.0);
    }
}