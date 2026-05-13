using Catch_your_bus.Models.External.GooglePlaces;
using Catch_your_bus.Models.External.GooglePlaces.Response;
using Catch_your_bus.ViewModels;

namespace Catch_your_bus.Views;

public partial class RouteCalculationPage : ContentPage
{
    private readonly RouteCalculationViewModel _viewModel;

    /// <summary>
    /// Guards against firing SearchStart/End when we programmatically set the Entry text
    /// (e.g. after a selection). Without this, selecting a prediction would immediately
    /// re-trigger the autocomplete search.
    /// </summary>
    private bool _suppressTextChanged;

    public RouteCalculationPage(RouteCalculationViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadFavoritesCommand.ExecuteAsync(null);
    }

    private async void OnStartTextChanged(object? sender, TextChangedEventArgs e)
    {
        try
        {
            if (_suppressTextChanged) return;
            await _viewModel.SearchStartCommand.ExecuteAsync(e.NewTextValue);
        }
        catch (Exception exception)
        {
            throw; // TODO handle exception
        }
    }

    private async void OnEndTextChanged(object? sender, TextChangedEventArgs e)
    {
        try
        {
            if (_suppressTextChanged) return;
            await _viewModel.SearchEndCommand.ExecuteAsync(e.NewTextValue);
        }
        catch (Exception exception)
        {
            throw; // TODO handle exception
        }
    }

    private async void OnStartPredictionSelected(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.CurrentSelection.FirstOrDefault() is not PlacePrediction prediction)
                return;

            _suppressTextChanged = true;
            await _viewModel.SelectStartCommand.ExecuteAsync(prediction);
            _suppressTextChanged = false;

            if (sender is CollectionView cv)
                cv.SelectedItem = null;
        }
        catch (Exception)
        {
            _suppressTextChanged = false;
        }
    }

    private async void OnEndPredictionSelected(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.CurrentSelection.FirstOrDefault() is not PlacePrediction prediction)
                return;

            _suppressTextChanged = true;
            await _viewModel.SelectEndCommand.ExecuteAsync(prediction);
            _suppressTextChanged = false;

            if (sender is CollectionView cv)
                cv.SelectedItem = null;
        }
        catch (Exception)
        {
            _suppressTextChanged = false;
        }
    }
}
