using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoMote.Roku;
using RoMoteNoAds.Models;

namespace RoMoteNoAds.ViewModels;

/// <summary>
/// ViewModel for the channels page.
/// </summary>
public partial class ChannelsViewModel : BaseViewModel
{
    private readonly IRokuService _rokuService;

    [ObservableProperty]
    private ObservableCollection<RokuChannel> _channels = new();

    [ObservableProperty]
    private RokuChannel? _activeChannel;

    [ObservableProperty]
    private bool _isRefreshing;

    public ChannelsViewModel(IRokuService rokuService)
    {
        _rokuService = rokuService;
        Title = "Channels";
    }

    public async Task InitializeAsync()
    {
        await RefreshChannelsAsync();
    }

    [RelayCommand]
    private async Task RefreshChannelsAsync()
    {
        if (_rokuService.CurrentDevice == null)
        {
            SetError("No device connected");
            return;
        }

        try
        {
            IsRefreshing = true;
            ClearError();

            var libChannels = await _rokuService.GetInstalledChannelsAsync();

            // Get active channel
            var libActive = await _rokuService.GetActiveChannelAsync();
            ActiveChannel = libActive != null ? RokuChannel.FromLibrary(libActive) : null;

            Channels.Clear();
            foreach (var libChannel in libChannels.OrderBy(c => c.Name))
            {
                var channel = RokuChannel.FromLibrary(libChannel);
                channel.IsActive = libActive?.Id == channel.Id;
                await LoadChannelIconAsync(channel);
                Channels.Add(channel);
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load channels: {ex.Message}");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task LaunchChannelAsync(RokuChannel? channel)
    {
        if (channel == null)
            return;

        if (_rokuService.CurrentDevice == null)
        {
            SetError("No device connected");
            return;
        }

        try
        {
            IsBusy = true;
            TriggerHaptic();

            var success = await _rokuService.LaunchChannelAsync(channel.Id);
            if (success)
            {
                // Update active state
                foreach (var c in Channels)
                {
                    c.IsActive = c.Id == channel.Id;
                }
                ActiveChannel = channel;

                // Refresh the collection to update UI
                var temp = new ObservableCollection<RokuChannel>(Channels);
                Channels = temp;
            }
            else
            {
                SetError($"Failed to launch {channel.Name}");
            }
        }
        catch (Exception ex)
        {
            SetError($"Error launching channel: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadChannelIconAsync(RokuChannel channel)
    {
        try
        {
            if (!string.IsNullOrEmpty(channel.IconUrl))
            {
                channel.IconSource = ImageSource.FromUri(new Uri(channel.IconUrl));
            }
        }
        catch
        {
            // Icon loading failed, use placeholder
            channel.IconSource = null;
        }

        await Task.CompletedTask;
    }

    private void TriggerHaptic()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch
        {
            // Haptics not available on all platforms
        }
    }
}
