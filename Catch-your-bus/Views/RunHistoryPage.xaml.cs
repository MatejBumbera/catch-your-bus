using Catch_your_bus.ViewModels;

namespace Catch_your_bus.Views;

public partial class RunHistoryPage : ContentPage
{
    private readonly RunHistoryViewModel _viewModel;

    public RunHistoryPage(RunHistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        await _viewModel.LoadHistoryAsync();
    }
}