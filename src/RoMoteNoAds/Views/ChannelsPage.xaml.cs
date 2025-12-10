using RoMoteNoAds.ViewModels;

namespace RoMoteNoAds.Views;

public partial class ChannelsPage : ContentPage
{
    private readonly ChannelsViewModel _viewModel;

    public ChannelsPage(ChannelsViewModel viewModel)
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
