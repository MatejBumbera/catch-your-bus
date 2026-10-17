using Catch_your_bus.Models.Entities;

namespace Catch_your_bus.Models.Services;

public class TrackingService
{
    public const double ArrivalThresholdMeters = 15;

    public TrackingPaceResult CalculatePace(
        Location currentLoc,
        Location destinationLoc,
        DateTimeOffset departureTime,
        double userWalkSpeed,
        double userRunSpeed)
    {
        var distanceKm = Location.CalculateDistance(currentLoc, destinationLoc, DistanceUnits.Kilometers);
        var distanceMeters = distanceKm * 1000;

        if (distanceMeters <= ArrivalThresholdMeters)
        {
            return new TrackingPaceResult(PaceStatus.Green, distanceMeters, 0, 0);
        }

        var remainingTimeSeconds = (departureTime - DateTime.Now).TotalSeconds;

        if (remainingTimeSeconds <= 0)
        {
            return new TrackingPaceResult(PaceStatus.Red, distanceMeters, 999, remainingTimeSeconds);
        }

        var requiredSpeed = (distanceMeters / remainingTimeSeconds) * 3.6;

        var status = requiredSpeed switch
        {
            _ when requiredSpeed <= userWalkSpeed => PaceStatus.Green,
            _ when requiredSpeed <= userRunSpeed => PaceStatus.Orange,
            _ => PaceStatus.Red
        };

        return new TrackingPaceResult(status, distanceMeters, requiredSpeed, remainingTimeSeconds);
    }
}