using System.Collections.ObjectModel;
using Catch_your_bus.Models.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAL.Entities;

namespace Catch_your_bus.ViewModels;

public enum RunFilter
{
    All,
    Caught,
    Missed
}

public partial class RunHistoryViewModel(RunSessionService runSessionService) : ObservableObject
{
    private List<RunSession> _allRuns = [];

    [ObservableProperty] public partial ObservableCollection<RunSession> DisplayedRuns { get; set; } = [];

    [ObservableProperty] public partial bool IsLoading { get; set; }

    [ObservableProperty] public partial double TotalCalories { get; set; }
    [ObservableProperty] public partial double TotalDistanceKm { get; set; }
    [ObservableProperty] public partial int TotalTimeSavedMinutes { get; set; }

    [RelayCommand]
    public async Task LoadHistoryAsync()
    {
        IsLoading = true;

        _allRuns = await runSessionService.GetAllSessionsAsync();

        CalculateStatistics();
        MarkPersonalBests();

        FilterRuns(RunFilter.All);

        IsLoading = false;
    }

    private void CalculateStatistics()
    {
        TotalCalories = Math.Round(_allRuns.Sum(r => r.CaloriesBurned), 1);
        TotalDistanceKm = Math.Round(_allRuns.Sum(r => r.DistanceMeters) / 1000.0, 2);
        TotalTimeSavedMinutes = _allRuns.Sum(r => r.TimeSavedSeconds) / 60;
    }

    private void MarkPersonalBests()
    {
        foreach (var run in _allRuns) run.IsPersonalBest = false;

        var personalBestIds = _allRuns
            .Where(r => r.Success && !string.IsNullOrEmpty(r.RouteIdentifier))
            .GroupBy(r => r.RouteIdentifier)
            .Select(group => group.OrderBy(r => r.DurationSeconds).First().Id)
            .ToHashSet();

        foreach (var run in _allRuns.Where(r => personalBestIds.Contains(r.Id)))
        {
            run.IsPersonalBest = true;
        }
    }

    [RelayCommand]
    public void FilterRuns(RunFilter filterType)
    {
        DisplayedRuns.Clear();
        
        var filtered = filterType switch
        {
            RunFilter.Caught => _allRuns.Where(r => r.Success),
            RunFilter.Missed => _allRuns.Where(r => !r.Success),
            RunFilter.All => _allRuns,
            _ => _allRuns
        };

        foreach (var run in filtered)
        {
            DisplayedRuns.Add(run);
        }
    }

    [RelayCommand]
    public async Task ClearHistoryAsync()
    {
        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Clear History",
            "Are you sure you want to delete all run data? This cannot be undone.",
            "Yes, Delete", "Cancel");

        if (confirm)
        {
            await runSessionService.ClearHistoryAsync();
            _allRuns.Clear();
            DisplayedRuns.Clear();
            CalculateStatistics();
        }
    }
}