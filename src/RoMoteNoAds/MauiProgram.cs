using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using RoMoteNoAds.Services;
using RoMoteNoAds.ViewModels;
using RoMoteNoAds.Views;

namespace RoMoteNoAds;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register Services
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<IDeviceStorageService, DeviceStorageService>();
        builder.Services.AddSingleton<IRokuDiscoveryService, RokuDiscoveryService>();
        builder.Services.AddSingleton<IRokuControlService, RokuControlService>();
        builder.Services.AddSingleton<IShortcutService, ShortcutService>();

        // Register ViewModels
        builder.Services.AddSingleton<DeviceSelectionViewModel>();
        builder.Services.AddSingleton<RemoteViewModel>();
        builder.Services.AddSingleton<ChannelsViewModel>();

        // Register Views
        builder.Services.AddSingleton<DeviceSelectionPage>();
        builder.Services.AddSingleton<RemotePage>();
        builder.Services.AddSingleton<ChannelsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
