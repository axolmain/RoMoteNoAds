using RoMoteNoAds.ViewModels;

namespace RoMoteNoAds.Views;

public partial class RemotePage : ContentPage
{
    private readonly RemoteViewModel _viewModel;

    public RemotePage(RemoteViewModel viewModel)
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

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        _viewModel.RefreshDevice();
    }
}
