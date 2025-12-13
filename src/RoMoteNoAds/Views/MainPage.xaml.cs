using RoMoteNoAds.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace RoMoteNoAds.Views;

public partial class MainPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;
    private ContentPage? _currentPage;
    private string _currentTab = "remote";

    public string CurrentTab
    {
        get => _currentTab;
        set
        {
            if (_currentTab != value)
            {
                _currentTab = value;
                OnPropertyChanged();
                NavigateToTab(value);
            }
        }
    }

    public MainPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        BindingContext = this;

        // Setup layout based on device type
        SetupLayout();

        // Navigate to initial tab
        NavigateToTab("remote");
    }

    private void SetupLayout()
    {
        var isTablet = DeviceInfo.Idiom == DeviceIdiom.Tablet ||
                       DeviceInfo.Idiom == DeviceIdiom.Desktop;

        if (isTablet)
        {
            // Tablet: Sidebar on left, content on right
            RootGrid.ColumnDefinitions.Clear();
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(100)));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            Sidebar.IsVisible = true;
            TabBar.IsVisible = false;

            Grid.SetColumn(Sidebar, 0);
            Grid.SetColumn(ContentArea, 1);
        }
        else
        {
            // Phone: Content with bottom tab bar
            RootGrid.ColumnDefinitions.Clear();
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            Sidebar.IsVisible = false;
            TabBar.IsVisible = true;

            Grid.SetColumn(ContentArea, 0);
        }
    }

    private void OnSidebarItemSelected(object? sender, string tabId)
    {
        CurrentTab = tabId;
    }

    private void OnTabSelected(object? sender, string tabId)
    {
        CurrentTab = tabId;
    }

    private void NavigateToTab(string tabId)
    {
        ContentPage? newPage = tabId switch
        {
            "remote" => _serviceProvider.GetService<NeumorphicRemotePage>(),
            "channels" => _serviceProvider.GetService<NeumorphicChannelsPage>(),
            "shortcuts" => _serviceProvider.GetService<NeumorphicShortcutsPage>(),
            _ => null
        };

        if (newPage != null && newPage != _currentPage)
        {
            _currentPage = newPage;
            PageContent.Content = newPage.Content;
            PageContent.BindingContext = newPage.BindingContext;
        }
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        // Re-evaluate layout on size change (orientation, window resize)
        SetupLayout();
    }
}
