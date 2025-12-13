using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoMote.Roku;
using RoMoteNoAds.Models;
using RoMoteNoAds.Services;
using System.Collections.ObjectModel;

namespace RoMoteNoAds.ViewModels;

/// <summary>
/// ViewModel for the remote control page.
/// </summary>
public partial class RemoteViewModel : BaseViewModel
{
    private readonly IRokuService _rokuService;
    private readonly IStorageService _storageService;
    private readonly IVolumeButtonService _volumeButtonService;

    [ObservableProperty]
    private RokuDevice? _currentDevice;

    [ObservableProperty]
    private RokuChannel? _activeChannel;

    [ObservableProperty]
    private bool _isKeyboardVisible;

    [ObservableProperty]
    private string _keyboardText = string.Empty;

    [ObservableProperty]
    private bool _supportsVolume;

    [ObservableProperty]
    private bool _supportsPower;

    [ObservableProperty]
    private ObservableCollection<RokuDevice> _savedDevices = new();

    [ObservableProperty]
    private bool _isConnected;

    public RemoteViewModel(
        IRokuService rokuService,
        IStorageService storageService,
        IVolumeButtonService volumeButtonService)
    {
        _rokuService = rokuService;
        _storageService = storageService;
        _volumeButtonService = volumeButtonService;

        Title = "Remote";

        _rokuService.CommandFailed += OnCommandFailed;

        // Subscribe to hardware volume button events
        _volumeButtonService.VolumeUpPressed += OnHardwareVolumeUp;
        _volumeButtonService.VolumeDownPressed += OnHardwareVolumeDown;
    }

    /// <summary>
    /// Start listening for hardware volume buttons when the view appears.
    /// </summary>
    public void OnAppearing()
    {
        _volumeButtonService.StartListening();
    }

    /// <summary>
    /// Stop listening for hardware volume buttons when the view disappears.
    /// </summary>
    public void OnDisappearing()
    {
        _volumeButtonService.StopListening();
    }

    private async void OnHardwareVolumeUp(object? sender, EventArgs e)
    {
        if (CurrentDevice != null && SupportsVolume)
        {
            await VolumeUpAsync();
        }
    }

    private async void OnHardwareVolumeDown(object? sender, EventArgs e)
    {
        if (CurrentDevice != null && SupportsVolume)
        {
            await VolumeDownAsync();
        }
    }

    public async Task InitializeAsync()
    {
        var lastDevice = await _storageService.GetLastUsedDeviceAsync();
        if (lastDevice != null)
        {
            CurrentDevice = lastDevice;
            _rokuService.CurrentDevice = lastDevice.ToLibrary();
            IsConnected = true;
            UpdateCapabilities();
            await RefreshActiveChannelAsync();
        }
        else
        {
            IsConnected = false;
        }
    }

    public async Task RefreshActiveChannelAsync()
    {
        if (_rokuService.CurrentDevice == null)
            return;

        try
        {
            var libActive = await _rokuService.GetActiveChannelAsync();
            if (libActive != null)
            {
                var channel = RokuChannel.FromLibrary(libActive);
                // Load icon
                if (!string.IsNullOrEmpty(channel.IconUrl))
                {
                    channel.IconSource = ImageSource.FromUri(new Uri(channel.IconUrl));
                }
                ActiveChannel = channel;
            }
            else
            {
                ActiveChannel = null;
            }
        }
        catch
        {
            // Silently fail - active channel is not critical
            ActiveChannel = null;
        }
    }

    public void RefreshDevice()
    {
        var libDevice = _rokuService.CurrentDevice;
        CurrentDevice = libDevice != null ? RokuDevice.FromLibrary(libDevice) : null;
        UpdateCapabilities();
    }

    private void UpdateCapabilities()
    {
        SupportsVolume = CurrentDevice?.SupportsAudioVolumeControl ?? false;
        SupportsPower = CurrentDevice?.SupportsTvPowerControl ?? false;
    }

    // Navigation Commands
    [RelayCommand]
    private async Task PressUpAsync() => await SendKeyWithHapticAsync(RokuKey.Up);

    [RelayCommand]
    private async Task PressDownAsync() => await SendKeyWithHapticAsync(RokuKey.Down);

    [RelayCommand]
    private async Task PressLeftAsync() => await SendKeyWithHapticAsync(RokuKey.Left);

    [RelayCommand]
    private async Task PressRightAsync() => await SendKeyWithHapticAsync(RokuKey.Right);

    [RelayCommand]
    private async Task PressSelectAsync() => await SendKeyWithHapticAsync(RokuKey.Select);

    [RelayCommand]
    private async Task PressBackAsync() => await SendKeyWithHapticAsync(RokuKey.Back);

    [RelayCommand]
    private async Task PressHomeAsync() => await SendKeyWithHapticAsync(RokuKey.Home);

    // Playback Commands
    [RelayCommand]
    private async Task PressPlayAsync() => await SendKeyWithHapticAsync(RokuKey.Play);

    [RelayCommand]
    private async Task PressRewindAsync() => await SendKeyWithHapticAsync(RokuKey.Rev);

    [RelayCommand]
    private async Task PressFastForwardAsync() => await SendKeyWithHapticAsync(RokuKey.Fwd);

    [RelayCommand]
    private async Task PressReplayAsync() => await SendKeyWithHapticAsync(RokuKey.InstantReplay);

    // Info/Search Commands
    [RelayCommand]
    private async Task PressInfoAsync() => await SendKeyWithHapticAsync(RokuKey.Info);

    [RelayCommand]
    private async Task PressSearchAsync() => await SendKeyWithHapticAsync(RokuKey.Search);

    // Volume Commands
    [RelayCommand]
    private async Task VolumeUpAsync() => await SendKeyWithHapticAsync(RokuKey.VolumeUp);

    [RelayCommand]
    private async Task VolumeDownAsync() => await SendKeyWithHapticAsync(RokuKey.VolumeDown);

    [RelayCommand]
    private async Task VolumeMuteAsync() => await SendKeyWithHapticAsync(RokuKey.VolumeMute);

    // Power Commands
    [RelayCommand]
    private async Task PowerToggleAsync()
    {
        if (CurrentDevice == null)
        {
            SetError("No device connected");
            return;
        }

        TriggerHaptic();
        ClearError();

        // Send Wake-on-LAN if we have a MAC address (works even when TV is fully off)
        if (!string.IsNullOrEmpty(CurrentDevice.WifiMacAddress))
        {
            System.Diagnostics.Debug.WriteLine($"[Power] Sending WoL to {CurrentDevice.WifiMacAddress}");
            await _rokuService.WakeAsync(CurrentDevice.WifiMacAddress);
        }

        // Just send the Power command - don't check result or show errors
        // Many Roku TVs return 403 but still toggle power via CEC
        _ = _rokuService.SendKeyPressAsync(RokuKey.Power);

        System.Diagnostics.Debug.WriteLine("[Power] Power command sent");
    }

    [RelayCommand]
    private async Task PowerOffAsync() => await SendKeyWithHapticAsync(RokuKey.PowerOff);

    // Keyboard Commands
    [RelayCommand]
    private void ShowKeyboard()
    {
        KeyboardText = string.Empty;
        IsKeyboardVisible = true;
    }

    [RelayCommand]
    private void HideKeyboard()
    {
        IsKeyboardVisible = false;
    }

    [RelayCommand]
    private async Task SendKeyboardTextAsync()
    {
        if (string.IsNullOrEmpty(KeyboardText))
            return;

        try
        {
            IsBusy = true;
            await _rokuService.SendTextAsync(KeyboardText);
            KeyboardText = string.Empty;
            IsKeyboardVisible = false;
            TriggerHaptic();
        }
        catch (Exception ex)
        {
            SetError($"Failed to send text: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SendBackspaceAsync()
    {
        await SendKeyWithHapticAsync(RokuKey.Backspace);
    }

    [RelayCommand]
    private async Task SendEnterAsync()
    {
        await SendKeyWithHapticAsync(RokuKey.Enter);
        IsKeyboardVisible = false;
    }

    private async Task SendKeyWithHapticAsync(string key)
    {
        if (CurrentDevice == null)
        {
            SetError("No device connected");
            return;
        }

        TriggerHaptic();

        var success = await _rokuService.SendKeyPressAsync(key);
        if (!success)
        {
            // Error already set by control service event
        }
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

    // Device Management Commands
    [RelayCommand]
    private async Task SelectDevice(RokuDevice device)
    {
        if (device != null)
        {
            CurrentDevice = device;
            _rokuService.CurrentDevice = device.ToLibrary();
            await _storageService.SetLastUsedDeviceAsync(device);
            IsConnected = CurrentDevice != null;
            UpdateCapabilities();
            await RefreshActiveChannelAsync();
        }
    }

    [RelayCommand]
    private async Task ScanDevices()
    {
        try
        {
            IsBusy = true;
            var libDevices = await _rokuService.DiscoverDevicesAsync();
            SavedDevices.Clear();
            foreach (var libDevice in libDevices)
            {
                var device = RokuDevice.FromLibrary(libDevice);
                SavedDevices.Add(device);
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to scan for devices: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnCommandFailed(object? sender, string message)
    {
        MainThread.BeginInvokeOnMainThread(() => SetError(message));
    }
}
