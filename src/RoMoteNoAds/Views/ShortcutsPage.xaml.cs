using RoMoteNoAds.ViewModels;

namespace RoMoteNoAds.Views;

public partial class ShortcutsPage : ContentPage
{
    private readonly ShortcutsViewModel _viewModel;

    public ShortcutsPage(ShortcutsViewModel viewModel)
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
