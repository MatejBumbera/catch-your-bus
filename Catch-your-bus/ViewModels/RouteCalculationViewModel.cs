using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Catch_your_bus.Models.Entities;
using Catch_your_bus.Models.Services;

namespace Catch_your_bus.ViewModels;

public partial class RouteCalculationViewModel(
    IRouteCalculationService routeService) : ObservableObject
{
    [ObservableProperty]
    private double startLat;

    [ObservableProperty]
    private double startLng;

    [ObservableProperty]
    private double endLat;

    [ObservableProperty]
    private double endLng;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResult))]
    [NotifyPropertyChangedFor(nameof(WinnerText))]
    [NotifyPropertyChangedFor(nameof(DifferenceText))]
    [NotifyPropertyChangedFor(nameof(IsRunningAlternative))]
    [NotifyPropertyChangedFor(nameof(TransitRouteSummary))]
    [NotifyPropertyChangedFor(nameof(FootRouteSummary))]
    private FastestModeResult? currentResult;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;
    
    public bool HasResult => CurrentResult is not null;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public string WinnerText => CurrentResult?.Winner.Type switch
    {
        RouteType.PureRunning => "🏃 Just run there!",
        RouteType.PureWalking => "🚶 Just walk there!",
        RouteType.RunningAlternative => "🏃 Run to catch an earlier bus!",
        RouteType.StandardTransit => "Take the transit route!",
        _ => string.Empty
    };

    public string DifferenceText => CurrentResult switch
    {
        null => string.Empty,
        { Difference.TotalMinutes: <= 1 } => "Both options are about the same",
        _ => $"Saves {FormatDuration(CurrentResult.Difference)}"
    };

    public bool IsRunningAlternative =>
        CurrentResult?.TransitRoute.Type == RouteType.RunningAlternative;

    public string TransitRouteSummary =>
        CurrentResult?.TransitRoute.Summary ?? string.Empty;

    public string FootRouteSummary =>
        CurrentResult?.FootRoute.Summary ?? string.Empty;
    
    [RelayCommand]
    private async Task CalculateBestRouteAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        CurrentResult = null;

        try
        {
            var result = await routeService.CompareFastestModeAsync(
                StartLat, StartLng, EndLat, EndLng);

            if (result is null)
            {
                ErrorMessage = "No routes found for this destination.";
                return;
            }

            CurrentResult = result;
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Network error — please check your connection and try again.";
        }
        catch (Exception)
        {
            ErrorMessage = "An error occurred while calculating the route.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string FormatDuration(TimeSpan duration) => duration.TotalHours switch
    {
        >= 1 => $"{(int)duration.TotalHours}h {duration.Minutes}min",
        _ => $"{(int)duration.TotalMinutes} min"
    };
}