using Catch_your_bus.ViewModels;

namespace Catch_your_bus.Views;

public partial class TrackingPage : ContentPage
{
    private readonly TrackingViewModel _viewModel;
    public TrackingPage(TrackingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        await _viewModel.StartTrackingAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        _viewModel.StopTracking();
    }
}