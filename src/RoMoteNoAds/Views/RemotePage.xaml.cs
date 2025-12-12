using RoMoteNoAds.ViewModels;
#if MACCATALYST
using UIKit;
using RoMoteNoAds.Platforms.MacCatalyst;
#endif

namespace RoMoteNoAds.Views;

public partial class RemotePage : ContentPage
{
    private readonly RemoteViewModel _viewModel;
#if MACCATALYST
    private Dictionary<UIKeyboardHidUsage, Func<Task>>? _keyMappings;
    private Dictionary<string, Func<Task>>? _characterMappings;
    private bool _isSubscribed;
#endif

    public RemotePage(RemoteViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();

#if MACCATALYST
        SetupKeyboardHandling();
#endif
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

#if MACCATALYST
        TeardownKeyboardHandling();
#endif
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        _viewModel.RefreshDevice();
    }

#if MACCATALYST
    private void SetupKeyboardHandling()
    {
        if (_isSubscribed)
            return;

        // Map hardware key codes to commands
        _keyMappings = new Dictionary<UIKeyboardHidUsage, Func<Task>>
        {
            // Arrow keys → D-pad
            { UIKeyboardHidUsage.KeyboardUpArrow, () => _viewModel.PressUpCommand.ExecuteAsync(null) },
            { UIKeyboardHidUsage.KeyboardDownArrow, () => _viewModel.PressDownCommand.ExecuteAsync(null) },
            { UIKeyboardHidUsage.KeyboardLeftArrow, () => _viewModel.PressLeftCommand.ExecuteAsync(null) },
            { UIKeyboardHidUsage.KeyboardRightArrow, () => _viewModel.PressRightCommand.ExecuteAsync(null) },

            // Enter/Return → Select
            { UIKeyboardHidUsage.KeyboardReturnOrEnter, () => _viewModel.PressSelectCommand.ExecuteAsync(null) },

            // Escape → Back
            { UIKeyboardHidUsage.KeyboardEscape, () => _viewModel.PressBackCommand.ExecuteAsync(null) },

            // Space → Play/Pause
            { UIKeyboardHidUsage.KeyboardSpacebar, () => _viewModel.PressPlayCommand.ExecuteAsync(null) },
        };

        // Map character keys to commands
        _characterMappings = new Dictionary<string, Func<Task>>(StringComparer.OrdinalIgnoreCase)
        {
            // H → Home
            { "h", () => _viewModel.PressHomeCommand.ExecuteAsync(null) },

            // S → Search
            { "s", () => _viewModel.PressSearchCommand.ExecuteAsync(null) },

            // I → Info
            { "i", () => _viewModel.PressInfoCommand.ExecuteAsync(null) },

            // R → Replay
            { "r", () => _viewModel.PressReplayCommand.ExecuteAsync(null) },

            // B → Back (alternative)
            { "b", () => _viewModel.PressBackCommand.ExecuteAsync(null) },

            // P → Power
            { "p", () => _viewModel.PowerToggleCommand.ExecuteAsync(null) },

            // M → Mute
            { "m", () => _viewModel.VolumeMuteCommand.ExecuteAsync(null) },

            // +/= → Volume Up
            { "=", () => _viewModel.VolumeUpCommand.ExecuteAsync(null) },
            { "+", () => _viewModel.VolumeUpCommand.ExecuteAsync(null) },

            // - → Volume Down
            { "-", () => _viewModel.VolumeDownCommand.ExecuteAsync(null) },

            // [ → Rewind
            { "[", () => _viewModel.PressRewindCommand.ExecuteAsync(null) },

            // ] → Fast Forward
            { "]", () => _viewModel.PressFastForwardCommand.ExecuteAsync(null) },
        };

        KeyboardEventService.KeyPressed += OnKeyPressed;
        _isSubscribed = true;

        System.Diagnostics.Debug.WriteLine("[Keyboard] Keyboard shortcuts enabled");
    }

    private void TeardownKeyboardHandling()
    {
        if (!_isSubscribed)
            return;

        KeyboardEventService.KeyPressed -= OnKeyPressed;
        _isSubscribed = false;
    }

    private async void OnKeyPressed(object? sender, KeyboardEventArgs e)
    {
        // Skip if keyboard overlay is visible (user is typing text)
        if (_viewModel.IsKeyboardVisible)
            return;

        // Check hardware key mappings first
        if (_keyMappings != null && _keyMappings.TryGetValue(e.KeyCode, out var keyAction))
        {
            await keyAction();
            return;
        }

        // Check character mappings
        if (!string.IsNullOrEmpty(e.Characters) &&
            _characterMappings != null &&
            _characterMappings.TryGetValue(e.Characters, out var charAction))
        {
            await charAction();
        }
    }
#endif
}
