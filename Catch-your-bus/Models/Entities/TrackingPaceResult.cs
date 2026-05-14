namespace Catch_your_bus.Models.Entities;

public record TrackingPaceResult(
    PaceStatus Status,
    double DistanceMeters,
    double RequiredSpeedKmH,
    double RemainingTimeSeconds
);