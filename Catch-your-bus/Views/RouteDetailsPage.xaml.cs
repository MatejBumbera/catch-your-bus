using Catch_your_bus.ViewModels;

namespace Catch_your_bus.Views;

public partial class RouteDetailsPage : ContentPage
{
    public RouteDetailsPage(RouteDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}