using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Catch_your_bus.Models.Services;
using DAL.Entities;

namespace Catch_your_bus.ViewModels;

public partial class ProfileViewModel(
    UserPreferencesService preferences,
    FavoriteRoutesService favoritesService,
    CalibrationService calibrationService) : ObservableObject
{
    public const double MinWalkingSpeedKmH = 2;
    public const double MaxWalkingSpeedKmH = 10;
    public const double MinRunningSpeedKmH = 6;
    public const double MaxRunningSpeedKmH = 25;

    private IDispatcherTimer? _stopwatchTimer;

    [ObservableProperty] public partial double WalkingSpeed { get; set; } = preferences.WalkingSpeedKmH;
    [ObservableProperty] public partial double RunningSpeed { get; set; } = preferences.RunningSpeedKmH;

    [ObservableProperty] public partial ObservableCollection<FavoriteRoute> Favorites { get; set; } = [];

    [ObservableProperty] public partial bool IsCalibrating { get; set; }
    [ObservableProperty] public partial bool IsWalkingCalibration { get; set; }
    [ObservableProperty] public partial string CalibrationMessage { get; set; } = "Ready to test.";
    [ObservableProperty] public partial TimeSpan ElapsedTime { get; set; }
    [ObservableProperty] public partial double CurrentTestDistanceMeters { get; set; }

    [RelayCommand]
    public async Task LoadFavoritesAsync()
    {
        var favs = await favoritesService.GetFavoritesAsync();
        Favorites = new ObservableCollection<FavoriteRoute>(favs);
    }

    [RelayCommand]
    private async Task DeleteFavoriteAsync(FavoriteRoute route)
    {
        await favoritesService.DeleteFavoriteAsync(route.Id);
        Favorites.Remove(route);
    }

    [RelayCommand]
    private void SaveSpeeds()
    {
        preferences.WalkingSpeedKmH = WalkingSpeed;
        preferences.RunningSpeedKmH = RunningSpeed;
    }

    [RelayCommand]
    private async Task StartCalibrationAsync(string mode)
    {
        CalibrationMessage = "Fetching GPS signal...";
        IsWalkingCalibration = mode == "Walk";

        calibrationService.DistanceUpdated += OnDistanceUpdated;
        
        var started = await calibrationService.StartListeningAsync();

        if (!started)
        {
            CalibrationMessage = "Failed to get GPS. Make sure Location is enabled.";
            calibrationService.DistanceUpdated -= OnDistanceUpdated;
            return;
        }

        IsCalibrating = true;
        CalibrationMessage = $"Test started! {mode} normally for ~50 meters, then press Stop.";
        CurrentTestDistanceMeters = 0;

        ElapsedTime = TimeSpan.Zero;
        _stopwatchTimer = Application.Current!.Dispatcher.CreateTimer();
        _stopwatchTimer.Interval = TimeSpan.FromSeconds(1);
        _stopwatchTimer.Tick += (s, e) => ElapsedTime = ElapsedTime.Add(TimeSpan.FromSeconds(1));
        _stopwatchTimer.Start();
    }
    
    private void OnDistanceUpdated(object? sender, double totalDistanceKm)
    {
        MainThread.BeginInvokeOnMainThread(() => {
            CurrentTestDistanceMeters = totalDistanceKm * 1000;
        });
    }

    [RelayCommand]
    private void StopCalibration()
    {
        CalibrationMessage = "Calculating speed...";
        _stopwatchTimer?.Stop();
        
        calibrationService.DistanceUpdated -= OnDistanceUpdated;

        var calculatedSpeed = calibrationService.StopListening(); 

        if (calculatedSpeed.HasValue)
        {
            var roundedSpeed = Math.Round(calculatedSpeed.Value, 1);

            if (IsWalkingCalibration)
            {
                WalkingSpeed = Math.Clamp(roundedSpeed, MinWalkingSpeedKmH, MaxWalkingSpeedKmH);
            }
            else
            {
                RunningSpeed = Math.Clamp(roundedSpeed, MinRunningSpeedKmH, MaxRunningSpeedKmH);
            }
            SaveSpeeds();
            CalibrationMessage = $"Success! Your average speed is {roundedSpeed} km/h.";
        }
        else
        {
            CalibrationMessage = "Test failed. Not enough distance covered.";
        }

        IsCalibrating = false;
    }

    [RelayCommand]
    private void CancelCalibration()
    {
        _stopwatchTimer?.Stop();
        calibrationService.DistanceUpdated -= OnDistanceUpdated;
        calibrationService.StopListening();
        
        IsCalibrating = false;
        CalibrationMessage = "Test cancelled.";
    }
}