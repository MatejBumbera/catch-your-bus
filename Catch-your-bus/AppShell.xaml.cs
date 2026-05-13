namespace Catch_your_bus;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("RouteDetailsPage", typeof(Views.RouteDetailsPage));
    }
}