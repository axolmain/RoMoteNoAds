using RoMoteNoAds.ViewModels;

namespace RoMoteNoAds.Views;

public partial class NeumorphicShortcutsPage : ContentPage
{
    public NeumorphicShortcutsPage(ShortcutsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
