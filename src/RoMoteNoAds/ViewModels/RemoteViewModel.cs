using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoMoteNoAds.Models;
using RoMoteNoAds.Services;

namespace RoMoteNoAds.ViewModels;

/// <summary>
/// ViewModel for the remote control page.
/// </summary>
public partial class RemoteViewModel : BaseViewModel
{
    private readonly IRokuControlService _controlService;
    private readonly IDeviceStorageService _storageService;

    [ObservableProperty]
    private RokuDevice? _currentDevice;

    [ObservableProperty]
    private bool _isKeyboardVisible;

    [ObservableProperty]
    private string _keyboardText = string.Empty;

    [ObservableProperty]
    private bool _supportsVolume;

    [ObservableProperty]
    private bool _supportsPower;

    public RemoteViewModel(
        IRokuControlService controlService,
        IDeviceStorageService storageService)
    {
        _controlService = controlService;
        _storageService = storageService;

        Title = "Remote";

        _controlService.CommandFailed += OnCommandFailed;
    }

    public async Task InitializeAsync()
    {
        var lastDevice = await _storageService.GetLastUsedDeviceAsync();
        if (lastDevice != null)
        {
            CurrentDevice = lastDevice;
            _controlService.CurrentDevice = lastDevice;
            UpdateCapabilities();
        }
    }

    public void RefreshDevice()
    {
        CurrentDevice = _controlService.CurrentDevice;
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
    private async Task PowerToggleAsync() => await SendKeyWithHapticAsync(RokuKey.Power);

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
            await _controlService.SendTextAsync(KeyboardText);
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

        var success = await _controlService.SendKeyPressAsync(key);
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

    private void OnCommandFailed(object? sender, string message)
    {
        MainThread.BeginInvokeOnMainThread(() => SetError(message));
    }
}
