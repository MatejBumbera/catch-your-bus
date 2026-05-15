using CommunityToolkit.Mvvm.ComponentModel;
using Catch_your_bus.Models.Entities;
using CommunityToolkit.Mvvm.Input;

namespace Catch_your_bus.ViewModels;

public partial class RouteDetailsViewModel(ActiveTrackingState trackingState, ActiveRouteDetailsState routeDetailsState)
    : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanStartLiveTracking))]
    public partial CalculatedRoute? Route { get; set; } = routeDetailsState.Route;

    [ObservableProperty]
    public partial DateTime? StandardArrivalTime { get; set; } =
        routeDetailsState.StandardRouteArrivalTime?.DateTime;
    
    public bool CanStartLiveTracking => 
        Route?.Type is RouteType.StandardTransit or RouteType.RunningAlternative;
    
    [RelayCommand]
    private async Task StartLiveTrackingAsync()
    {
        if (Route == null || !Route.Segments.Any()) return;
        var firstTransitSegment = Route.Segments.FirstOrDefault(s => s.Mode == TravelMode.Transit);

        if (firstTransitSegment is null) 
        {
            return;
        }
        
        var departureTime = firstTransitSegment.StartTime;
        var departureLocation = firstTransitSegment.StartLocation;
        if (departureTime is null || departureLocation is null)
        {
            return;
        }
        
        trackingState.TargetBusStop = firstTransitSegment.DepartureStopName ?? "Next Stop";
        trackingState.DepartureTime = departureTime.Value;
        trackingState.DestinationLocation = departureLocation;
        trackingState.FullRoute = Route;
        trackingState.StandardRouteArrivalTime = StandardArrivalTime;

        await Shell.Current.GoToAsync(nameof(Views.TrackingPage));
    }
}