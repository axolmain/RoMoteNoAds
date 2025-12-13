namespace RoMoteNoAds;

public partial class App : Application
{
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        // Use MainPage instead of AppShell for neomorphic navigation
        MainPage = serviceProvider.GetRequiredService<Views.MainPage>();
    }
}
