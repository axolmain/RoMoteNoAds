using RoMoteNoAds.ViewModels;

namespace RoMoteNoAds.Views;

public partial class NeumorphicChannelsPage : ContentPage
{
    public NeumorphicChannelsPage(ChannelsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
