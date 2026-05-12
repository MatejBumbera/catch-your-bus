namespace Catch_your_bus.Models.Services;

/// <summary>
/// Service that wraps <see cref="Microsoft.Maui.Storage.Preferences"/>
/// to persist and retrieve user speed preferences.
/// </summary>
public class UserPreferencesService
{
    private const string WalkingSpeedKey = "WalkingSpeedKmH";
    private const string RunningSpeedKey = "RunningSpeedKmH";

    private const double DefaultWalkingSpeedKmH = 5.0;
    private const double DefaultRunningSpeedKmH = 12.0;
    
    public double WalkingSpeedKmH
    {
        get => Preferences.Default.Get(WalkingSpeedKey, DefaultWalkingSpeedKmH);
        set => Preferences.Default.Set(WalkingSpeedKey, value);
    }
    
    public double RunningSpeedKmH
    {
        get => Preferences.Default.Get(RunningSpeedKey, DefaultRunningSpeedKmH);
        set => Preferences.Default.Set(RunningSpeedKey, value);
    }
    
    public void ResetToDefaults()
    {
        WalkingSpeedKmH = DefaultWalkingSpeedKmH;
        RunningSpeedKmH = DefaultRunningSpeedKmH;
    }
}
