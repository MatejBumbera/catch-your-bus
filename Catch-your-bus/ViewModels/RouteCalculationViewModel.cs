using System.Collections.ObjectModel;
using Catch_your_bus.Models.Entities;
using Catch_your_bus.Models.External.GooglePlaces.Response;
using Catch_your_bus.Models.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Catch_your_bus.ViewModels;

public enum TravelTab
{
    Fastest,
    Transit,
    OnFoot
}

public partial class RouteCalculationViewModel(
    IRouteCalculationService routeService,
    IGooglePlacesService placesService) : ObservableObject
{
    private const int DebounceDelayMs = 500;
    private const int MinQueryLength = 3;

    private CancellationTokenSource? _startSearchCts;
    private CancellationTokenSource? _endSearchCts;

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

    public bool HasStartPredictions => StartPredictions.Count > 0;
    public bool HasEndPredictions => EndPredictions.Count > 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFastestTab))]
    [NotifyPropertyChangedFor(nameof(IsTransitTab))]
    [NotifyPropertyChangedFor(nameof(IsOnFootTab))]
    public partial TravelTab SelectedTab { get; set; } = TravelTab.Fastest;

    public bool IsFastestTab => SelectedTab == TravelTab.Fastest;
    public bool IsTransitTab => SelectedTab == TravelTab.Transit;
    public bool IsOnFootTab => SelectedTab == TravelTab.OnFoot;

    [RelayCommand]
    private void SwitchTab(TravelTab tab) => SelectedTab = tab;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBundle))]
    [NotifyPropertyChangedFor(nameof(WinnerText))]
    [NotifyPropertyChangedFor(nameof(TimeSavedByRunningAlternativeText))]
    [NotifyPropertyChangedFor(nameof(HasRunningAlternative))]
    [NotifyPropertyChangedFor(nameof(StandardTransitWarning))]
    public partial RouteOptionsBundle? CurrentBundle { get; set; }

    [ObservableProperty] public partial bool IsLoading { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string? ErrorMessage { get; set; }

    public bool HasBundle => CurrentBundle is not null;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public string WinnerText => CurrentBundle?.Winner?.Type switch
    {
        RouteType.PureRunning => "🏃 Just run there!",
        RouteType.PureWalking => "🚶 Just walk there!",
        RouteType.RunningAlternative => "🔥 Run to catch an earlier bus!",
        RouteType.StandardTransit => "🚌 Take the standard transit route.",
        _ => string.Empty
    };

    public bool HasRunningAlternative => CurrentBundle?.IsRunningAlternativeBetter == true;

    public string TimeSavedByRunningAlternativeText => CurrentBundle?.IsRunningAlternativeBetter == true
        ? $"Saves {FormatDuration(CurrentBundle.RunningAlternativeTimeSaved)} by running!"
        : string.Empty;

    /// <summary>
    /// Checks if the user needs to leave within the next 3 minutes for the very first step of their journey.
    /// </summary>
    public string StandardTransitWarning
    {
        get
        {
            var firstSegment = CurrentBundle?.StandardTransit?.Segments.FirstOrDefault();
            if (firstSegment?.StartTime is null) return string.Empty;

            var timeUntilStart = firstSegment.StartTime.Value - DateTimeOffset.UtcNow;

            return timeUntilStart.TotalMinutes switch
            {
                > 0 and <= 3 => $"Leave in {(int)timeUntilStart.TotalMinutes} mins!",
                <= 0 => "Leave NOW!",
                _ => string.Empty
            };
        }
    }

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
            SelectedStartLocation = await placesService.GetPlaceCoordinatesAsync(prediction.PlaceId);
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
            SelectedEndLocation = await placesService.GetPlaceCoordinatesAsync(prediction.PlaceId);
        }
        catch (Exception)
        {
            ErrorMessage = "Could not resolve destination location.";
        }
    }
    
    [RelayCommand]
    private async Task ViewRouteDetailsAsync(CalculatedRoute? selectedRoute)
    {
        if (selectedRoute is null) return;
        
        var navigationParameter = new Dictionary<string, object>
        {
            { "SelectedRoute", selectedRoute }
        };
        
        await Shell.Current.GoToAsync("RouteDetailsPage", navigationParameter);
    }

    [RelayCommand]
    private async Task CalculateBestRouteAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        CurrentBundle = null;
        
        SelectedTab = TravelTab.Fastest;

        try
        {
            if (SelectedStartLocation is null || SelectedEndLocation is null)
            {
                ErrorMessage = "Please select valid locations from the dropdown.";
                return;
            }
            
            var bundle = await routeService.CalculateAllRoutesAsync(
                SelectedStartLocation.Latitude, SelectedStartLocation.Longitude,
                SelectedEndLocation.Latitude, SelectedEndLocation.Longitude);

            if (bundle.Winner is null)
            {
                ErrorMessage = "No routes found for this destination.";
                return;
            }

            CurrentBundle = bundle;
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