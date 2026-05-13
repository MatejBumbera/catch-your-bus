using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Catch_your_bus.Models.Entities;
using Catch_your_bus.Models.External.GooglePlaces;
using Catch_your_bus.Models.Services;

namespace Catch_your_bus.ViewModels;

public partial class RouteCalculationViewModel(
    IRouteCalculationService routeService,
    IGooglePlacesService placesService) : ObservableObject
{
    private const int DebounceDelayMs = 500;
    private const int MinQueryLength = 3;

    private CancellationTokenSource? _startSearchCts;
    private CancellationTokenSource? _endSearchCts;

    // These flags prevent the Two-Way binding from triggering an infinite search loop
    private bool _isSelectingStart;
    private bool _isSelectingEnd;

    [ObservableProperty] public partial string StartQuery { get; set; } = string.Empty;

    [ObservableProperty] public partial string EndQuery { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStartPredictions))]
    public partial ObservableCollection<PlacePrediction> StartPredictions { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEndPredictions))]
    public partial ObservableCollection<PlacePrediction> EndPredictions { get; set; } = [];

    [ObservableProperty] public partial PlaceLocation? SelectedStartLocation { get; set; }

    [ObservableProperty] public partial PlaceLocation? SelectedEndLocation { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResult))]
    [NotifyPropertyChangedFor(nameof(WinnerText))]
    [NotifyPropertyChangedFor(nameof(DifferenceText))]
    [NotifyPropertyChangedFor(nameof(IsRunningAlternative))]
    [NotifyPropertyChangedFor(nameof(TransitRouteSummary))]
    [NotifyPropertyChangedFor(nameof(FootRouteSummary))]
    public partial FastestModeResult? CurrentResult { get; set; }

    [ObservableProperty] public partial bool IsLoading { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string? ErrorMessage { get; set; }

    public bool HasStartPredictions => StartPredictions.Count > 0;
    public bool HasEndPredictions => EndPredictions.Count > 0;
    public bool HasResult => CurrentResult is not null;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public string WinnerText => CurrentResult?.Winner.Type switch
    {
        RouteType.PureRunning => "🏃 Just run there!",
        RouteType.PureWalking => "🚶 Just walk there!",
        RouteType.RunningAlternative => "🏃 Run to catch an earlier bus!",
        RouteType.StandardTransit => "🚌 Take the transit route!",
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
    private async Task SearchStartAsync(string query)
    {
        if (_isSelectingStart) return;

        CancelAndDispose(ref _startSearchCts);

        if (string.IsNullOrWhiteSpace(query) || query.Length < MinQueryLength)
        {
            StartPredictions = [];
            return;
        }

        var cts = _startSearchCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(DebounceDelayMs, cts.Token);

            var predictions = await placesService.GetPredictionsAsync(query);
            StartPredictions = new ObservableCollection<PlacePrediction>(predictions);
        }
        catch (OperationCanceledException)
        {
            // Expected — user kept typing
        }
        catch (Exception)
        {
            StartPredictions = [];
            ErrorMessage = "Could not get start location";
        }
    }

    [RelayCommand]
    private async Task SelectStartAsync(PlacePrediction prediction)
    {
        _isSelectingStart = true;
        StartQuery = prediction.DisplayText;
        StartPredictions = [];
        _isSelectingStart = false;

        try
        {
            var location = await placesService.GetPlaceCoordinatesAsync(prediction.PlaceId);
            SelectedStartLocation = location;
        }
        catch (Exception)
        {
            ErrorMessage = "Could not resolve origin location.";
        }
    }

    [RelayCommand]
    private async Task SearchEndAsync(string query)
    {
        if (_isSelectingEnd) return;

        CancelAndDispose(ref _endSearchCts);

        if (string.IsNullOrWhiteSpace(query) || query.Length < MinQueryLength)
        {
            EndPredictions = [];
            return;
        }

        var cts = _endSearchCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(DebounceDelayMs, cts.Token);

            var predictions = await placesService.GetPredictionsAsync(query);
            EndPredictions = new ObservableCollection<PlacePrediction>(predictions);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        catch (Exception)
        {
            EndPredictions = [];
            ErrorMessage = "Could not get end location";
        }
    }

    [RelayCommand]
    private async Task SelectEndAsync(PlacePrediction prediction)
    {
        _isSelectingEnd = true;
        EndQuery = prediction.DisplayText;
        EndPredictions = [];
        _isSelectingEnd = false;

        try
        {
            var location = await placesService.GetPlaceCoordinatesAsync(prediction.PlaceId);
            SelectedEndLocation = location;
        }
        catch (Exception)
        {
            ErrorMessage = "Could not resolve destination location.";
        }
    }

    [RelayCommand]
    private async Task CalculateBestRouteAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        CurrentResult = null;

        try
        {
            if (SelectedStartLocation is null || SelectedEndLocation is null)
            {
                ErrorMessage = "Please select valid locations from the dropdown.";
                return;
            }

            var result = await routeService.CompareFastestModeAsync(
                SelectedStartLocation.Latitude, SelectedStartLocation.Longitude,
                SelectedEndLocation.Latitude, SelectedEndLocation.Longitude);

            if (result is null)
            {
                ErrorMessage = "No routes found for this destination.";
                return;
            }

            CurrentResult = result;
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Network error — please check your connection.";
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

    private static void CancelAndDispose(ref CancellationTokenSource? cts)
    {
        if (cts is null) return;
        try
        {
            cts.Cancel();
            cts.Dispose();
        }
        catch (ObjectDisposedException)
        {
        }
        finally
        {
            cts = null;
        }
    }

    private static string FormatDuration(TimeSpan duration) => duration.TotalHours switch
    {
        >= 1 => $"{(int)duration.TotalHours}h {duration.Minutes}min",
        _ => $"{(int)duration.TotalMinutes} min"
    };
}