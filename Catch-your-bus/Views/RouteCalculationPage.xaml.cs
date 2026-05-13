using Catch_your_bus.ViewModels;

namespace Catch_your_bus.Views;

public partial class RouteCalculationPage : ContentPage
{
    public RouteCalculationPage(RouteCalculationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
