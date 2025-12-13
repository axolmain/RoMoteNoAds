using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using RoMote.Roku;
using RoMoteNoAds.Services;
using RoMoteNoAds.ViewModels;
using RoMoteNoAds.Views;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace RoMoteNoAds;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register Services
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<IRokuService, RokuService>();
        builder.Services.AddSingleton<IStorageService, StorageService>();
        builder.Services.AddSingleton<IShortcutService, ShortcutService>();

        // Register platform-specific volume button service
#if IOS
        builder.Services.AddSingleton<IVolumeButtonService, RoMoteNoAds.Platforms.iOS.VolumeButtonService>();
#else
        builder.Services.AddSingleton<IVolumeButtonService, NullVolumeButtonService>();
#endif

        // Register ViewModels
        builder.Services.AddSingleton<DeviceSelectionViewModel>();
        builder.Services.AddSingleton<RemoteViewModel>();
        builder.Services.AddSingleton<ChannelsViewModel>();
        builder.Services.AddSingleton<ShortcutsViewModel>();

        // Register Views
        builder.Services.AddSingleton<DeviceSelectionPage>();
        builder.Services.AddSingleton<RemotePage>();
        builder.Services.AddSingleton<NeumorphicRemotePage>();
        builder.Services.AddSingleton<ChannelsPage>();
        builder.Services.AddSingleton<NeumorphicChannelsPage>();
        builder.Services.AddSingleton<ShortcutsPage>();
        builder.Services.AddSingleton<NeumorphicShortcutsPage>();
        builder.Services.AddSingleton<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
