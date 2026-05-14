using System.Diagnostics;
using Catch_your_bus.Models.Entities;
using Catch_your_bus.Models.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAL.Entities;

namespace Catch_your_bus.ViewModels;

public partial class TrackingViewModel(
    TrackingService trackingService,
    RunSessionService runSessionService,
    UserPreferencesService preferences,
    ActiveTrackingState trackingState) : ObservableObject
{
    private IDispatcherTimer? _countdownTimer;
    private readonly Location? _destinationLocation = trackingState.DestinationLocation;
    private DateTime _startTime;
    private double _finalDistanceMeters;

    [ObservableProperty] public partial string TargetBusStop { get; set; } = trackingState.TargetBusStop;
    [ObservableProperty] public partial DateTimeOffset DepartureTime { get; set; } = trackingState.DepartureTime;
    [ObservableProperty] public partial Color BackgroundColor { get; set; } = Colors.Gray;
    [ObservableProperty] public partial string CountdownText { get; set; } = "--:--";
    [ObservableProperty] public partial string DistanceText { get; set; } = "Locating satellites...";
    [ObservableProperty] public partial string SpeedText { get; set; } = "Hold tight...";
    [ObservableProperty] public partial bool IsTracking { get; set; }

    public async Task StartTrackingAsync()
    {
        if (IsTracking) return;

        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                DistanceText = "GPS Permission Denied :(";
                SpeedText = "Cannot track without location.";
                return;
            }
        }

        IsTracking = true;
        _startTime = DateTime.Now;

        _countdownTimer = Application.Current!.Dispatcher.CreateTimer();
        _countdownTimer.Interval = TimeSpan.FromSeconds(1);
        _countdownTimer.Tick += OnCountdownTick;
        _countdownTimer.Start();

        _ = GpsTrackingLoopAsync();
    }

    private void OnCountdownTick(object? sender, EventArgs e)
    {
        var remaining = DepartureTime - DateTime.Now;
        if (remaining.TotalSeconds <= 0)
        {
            CountdownText = "00:00";
            return;
        }

        CountdownText = $"{(int)remaining.TotalMinutes:D2}:{remaining.Seconds:D2}";
    }

    private async Task GpsTrackingLoopAsync()
    {
        while (IsTracking)
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(2));
                var currentLoc = await Geolocation.Default.GetLocationAsync(request);


                if (currentLoc != null && _destinationLocation != null)
                {
                    var result = trackingService.CalculatePace(
                        currentLoc,
                        _destinationLocation,
                        DepartureTime,
                        preferences.WalkingSpeedKmH,
                        preferences.RunningSpeedKmH
                    );

                    _finalDistanceMeters = result.DistanceMeters;

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        DistanceText = $"{Math.Round(result.DistanceMeters)} meters left";
                        SpeedText = $"Required pace: {Math.Round(result.RequiredSpeedKmH, 1)} km/h";

                        BackgroundColor = result.Status switch
                        {
                            PaceStatus.Green => Colors.MediumSeaGreen,
                            PaceStatus.Orange => Colors.DarkOrange,
                            PaceStatus.Red => Colors.IndianRed,
                            _ => Colors.Gray
                        };

                        if (result.DistanceMeters <= 15) SpeedText = "You arrived!";
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Live Track GPS Failed: {ex.Message}");
            }

            await Task.Delay(3000);
        }
    }

    [RelayCommand]
    private async Task MadeItAsync() => await FinalizeRunAsync(success: true);

    [RelayCommand]
    private async Task MissedItAsync() => await FinalizeRunAsync(success: false);

    [RelayCommand]
    private async Task CancelAsync()
    {
        IsTracking = false;
        _countdownTimer?.Stop();
        await Shell.Current.GoToAsync("..");
    }

    private async Task FinalizeRunAsync(bool success)
    {
        IsTracking = false;
        _countdownTimer?.Stop();

        var totalDuration = (int)(DateTime.Now - _startTime).TotalSeconds;

        var session = new RunSession
        {
            Date = DateTime.Now,
            TargetBusStop = TargetBusStop,
            Success = success,
            DurationSeconds = totalDuration,
            DistanceMeters = _finalDistanceMeters
        };

        await runSessionService.AddSessionAsync(session);

        await Shell.Current.GoToAsync("..");
    }
}