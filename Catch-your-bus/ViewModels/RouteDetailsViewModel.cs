using CommunityToolkit.Mvvm.ComponentModel;
using Catch_your_bus.Models.Entities;

namespace Catch_your_bus.ViewModels;

[QueryProperty(nameof(Route), "SelectedRoute")]
public partial class RouteDetailsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial CalculatedRoute? Route { get; set; }
}