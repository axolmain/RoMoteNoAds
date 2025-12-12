using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoMote.Roku;
using RoMoteNoAds.Models;
using RoMoteNoAds.Services;

namespace RoMoteNoAds.ViewModels;

/// <summary>
/// ViewModel for managing shortcuts.
/// </summary>
public partial class ShortcutsViewModel : BaseViewModel
{
    private readonly IShortcutService _shortcutService;
    private readonly IRokuService _rokuService;

    [ObservableProperty]
    private ObservableCollection<Shortcut> _shortcuts = new();

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isRecording;

    [ObservableProperty]
    private List<string> _recordedKeys = new();

    // Editor properties
    [ObservableProperty]
    private bool _isEditorVisible;

    [ObservableProperty]
    private int _editorStep = 1;

    [ObservableProperty]
    private ObservableCollection<RokuChannel> _availableChannels = new();

    [ObservableProperty]
    private RokuChannel? _selectedChannel;

    [ObservableProperty]
    private string _newShortcutName = string.Empty;

    [ObservableProperty]
    private bool _isLoadingChannels;

    public ShortcutsViewModel(
        IShortcutService shortcutService,
        IRokuService rokuService)
    {
        _shortcutService = shortcutService;
        _rokuService = rokuService;
        Title = "Shortcuts";
    }

    public async Task InitializeAsync()
    {
        await LoadShortcutsAsync();
    }

    [RelayCommand]
    private async Task LoadShortcutsAsync()
    {
        try
        {
            IsBusy = true;
            var shortcuts = await _shortcutService.GetShortcutsAsync();
            Shortcuts = new ObservableCollection<Shortcut>(shortcuts);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load shortcuts: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExecuteShortcutAsync(Shortcut? shortcut)
    {
        if (shortcut == null || IsEditing)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var success = await _shortcutService.ExecuteShortcutAsync(shortcut);
            if (!success)
            {
                SetError("Failed to execute shortcut");
            }
        }
        catch (Exception ex)
        {
            SetError($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteShortcutAsync(Shortcut? shortcut)
    {
        if (shortcut == null)
            return;

        try
        {
            await _shortcutService.RemoveShortcutAsync(shortcut);
            Shortcuts.Remove(shortcut);
        }
        catch (Exception ex)
        {
            SetError($"Failed to delete: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ToggleEditing()
    {
        IsEditing = !IsEditing;
    }

    [RelayCommand]
    private void StartRecording()
    {
        RecordedKeys = new List<string>();
        IsRecording = true;
    }

    [RelayCommand]
    private void StopRecording()
    {
        IsRecording = false;
    }

    public void RecordKey(string key)
    {
        if (IsRecording)
        {
            RecordedKeys.Add(key);
        }
    }

    // Editor commands
    [RelayCommand]
    private async Task OpenEditorAsync()
    {
        EditorStep = 1;
        SelectedChannel = null;
        NewShortcutName = string.Empty;
        IsEditorVisible = true;
        await LoadChannelsAsync();
    }

    [RelayCommand]
    private async Task LoadChannelsAsync()
    {
        try
        {
            IsLoadingChannels = true;
            var libChannels = await _rokuService.GetInstalledChannelsAsync();
            AvailableChannels = new ObservableCollection<RokuChannel>(
                libChannels.Select(RokuChannel.FromLibrary));
        }
        catch (Exception ex)
        {
            SetError($"Failed to load channels: {ex.Message}");
        }
        finally
        {
            IsLoadingChannels = false;
        }
    }

    [RelayCommand]
    private void SelectChannel(RokuChannel? channel)
    {
        if (channel == null)
            return;

        SelectedChannel = channel;
        NewShortcutName = channel.Name;
        EditorStep = 2;
    }

    [RelayCommand]
    private void BackToChannelSelection()
    {
        EditorStep = 1;
    }

    [RelayCommand]
    private async Task SaveShortcutAsync()
    {
        if (SelectedChannel == null || string.IsNullOrWhiteSpace(NewShortcutName))
            return;

        try
        {
            var shortcut = new Shortcut
            {
                Name = NewShortcutName.Trim(),
                Type = ShortcutType.Channel,
                ChannelId = SelectedChannel.Id,
                IconUrl = SelectedChannel.IconUrl
            };

            await _shortcutService.SaveShortcutAsync(shortcut);
            Shortcuts.Add(shortcut);
            CloseEditor();
        }
        catch (Exception ex)
        {
            SetError($"Failed to save shortcut: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CloseEditor()
    {
        IsEditorVisible = false;
        EditorStep = 1;
        SelectedChannel = null;
        NewShortcutName = string.Empty;
    }
}
