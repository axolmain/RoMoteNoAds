using RoMoteNoAds.ViewModels;

namespace RoMoteNoAds.Views;

public partial class DeviceSelectionPage : ContentPage
{
    private readonly DeviceSelectionViewModel _viewModel;

    public DeviceSelectionPage(DeviceSelectionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
