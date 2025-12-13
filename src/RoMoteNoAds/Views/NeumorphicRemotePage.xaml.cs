using RoMoteNoAds.ViewModels;

namespace RoMoteNoAds.Views;

public partial class NeumorphicRemotePage : ContentPage
{
    public NeumorphicRemotePage(RemoteViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
