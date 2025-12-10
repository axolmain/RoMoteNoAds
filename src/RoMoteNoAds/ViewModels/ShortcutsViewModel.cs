using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoMoteNoAds.Models;
using RoMoteNoAds.Services;

namespace RoMoteNoAds.ViewModels;

/// <summary>
/// ViewModel for managing shortcuts.
/// </summary>
public partial class ShortcutsViewModel : BaseViewModel
{
    private readonly IShortcutService _shortcutService;
    private readonly IRokuControlService _controlService;

    [ObservableProperty]
    private ObservableCollection<Shortcut> _shortcuts = new();

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isRecording;

    [ObservableProperty]
    private List<string> _recordedKeys = new();

    public ShortcutsViewModel(
        IShortcutService shortcutService,
        IRokuControlService controlService)
    {
        _shortcutService = shortcutService;
        _controlService = controlService;
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
}
