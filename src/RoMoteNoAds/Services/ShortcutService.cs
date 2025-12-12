using System.Text.Json;
using RoMote.Roku;
using RoMoteNoAds.Models;

namespace RoMoteNoAds.Services;

/// <summary>
/// Service for managing, recording, and executing custom shortcuts.
/// </summary>
public class ShortcutService : IShortcutService
{
    private const string ShortcutsKey = "saved_shortcuts";
    private const int KeySequenceDelayMs = 100;

    private readonly IRokuService _rokuService;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly List<string> _recordedKeys = [];
    private bool _isRecording;

    public ShortcutService(IRokuService rokuService)
    {
        _rokuService = rokuService;
    }

    #region Recording

    public bool IsRecording => _isRecording;

    public void StartRecording()
    {
        _recordedKeys.Clear();
        _isRecording = true;
        System.Diagnostics.Debug.WriteLine("[ShortcutService] Recording started");
    }

    public IReadOnlyList<string> StopRecording()
    {
        _isRecording = false;
        var result = _recordedKeys.ToList();
        System.Diagnostics.Debug.WriteLine($"[ShortcutService] Recording stopped. Captured {result.Count} keys: {string.Join(", ", result)}");
        return result;
    }

    public async Task<bool> SendKeyPressAsync(string key)
    {
        // Capture key if recording
        if (_isRecording)
        {
            _recordedKeys.Add(key);
            System.Diagnostics.Debug.WriteLine($"[ShortcutService] Recorded key: {key}");
        }

        // Forward to Roku service
        return await _rokuService.SendKeyPressAsync(key);
    }

    #endregion

    #region Persistence

    public async Task<IEnumerable<Shortcut>> GetShortcutsAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                var json = Preferences.Get(ShortcutsKey, string.Empty);
                if (string.IsNullOrEmpty(json))
                    return Enumerable.Empty<Shortcut>();

                var shortcuts = JsonSerializer.Deserialize<List<Shortcut>>(json, _jsonOptions);
                return shortcuts?.OrderBy(s => s.SortOrder) ?? Enumerable.Empty<Shortcut>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading shortcuts: {ex.Message}");
                return Enumerable.Empty<Shortcut>();
            }
        });
    }

    public async Task SaveShortcutAsync(Shortcut shortcut)
    {
        await Task.Run(() =>
        {
            try
            {
                var shortcuts = GetShortcutsSync();

                var existing = shortcuts.FirstOrDefault(s => s.Id == shortcut.Id);
                if (existing != null)
                {
                    shortcuts.Remove(existing);
                }

                if (shortcut.SortOrder == 0)
                {
                    shortcut.SortOrder = shortcuts.Count > 0
                        ? shortcuts.Max(s => s.SortOrder) + 1
                        : 1;
                }

                shortcuts.Add(shortcut);
                SaveShortcutsSync(shortcuts);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving shortcut: {ex.Message}");
            }
        });
    }

    public async Task RemoveShortcutAsync(Shortcut shortcut)
    {
        await Task.Run(() =>
        {
            try
            {
                var shortcuts = GetShortcutsSync();
                var toRemove = shortcuts.FirstOrDefault(s => s.Id == shortcut.Id);

                if (toRemove != null)
                {
                    shortcuts.Remove(toRemove);
                    SaveShortcutsSync(shortcuts);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing shortcut: {ex.Message}");
            }
        });
    }

    public async Task ReorderShortcutsAsync(IEnumerable<Shortcut> shortcuts)
    {
        await Task.Run(() =>
        {
            try
            {
                var list = shortcuts.ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].SortOrder = i + 1;
                }
                SaveShortcutsSync(list);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reordering shortcuts: {ex.Message}");
            }
        });
    }

    #endregion

    #region Execution

    public async Task<bool> ExecuteShortcutAsync(Shortcut shortcut)
    {
        try
        {
            return shortcut.Type switch
            {
                ShortcutType.Channel => await ExecuteChannelShortcutAsync(shortcut),
                ShortcutType.KeySequence => await ExecuteKeySequenceAsync(shortcut),
                _ => false
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error executing shortcut: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> ExecuteChannelShortcutAsync(Shortcut shortcut)
    {
        if (string.IsNullOrEmpty(shortcut.ChannelId))
            return false;

        return await _rokuService.LaunchChannelAsync(shortcut.ChannelId);
    }

    private async Task<bool> ExecuteKeySequenceAsync(Shortcut shortcut)
    {
        if (shortcut.KeySequence == null || shortcut.KeySequence.Count == 0)
            return false;

        foreach (var key in shortcut.KeySequence)
        {
            var success = await _rokuService.SendKeyPressAsync(key);
            if (!success)
                return false;

            await Task.Delay(KeySequenceDelayMs);
        }

        return true;
    }

    #endregion

    #region Private Helpers

    private List<Shortcut> GetShortcutsSync()
    {
        try
        {
            var json = Preferences.Get(ShortcutsKey, string.Empty);
            if (string.IsNullOrEmpty(json))
                return [];

            return JsonSerializer.Deserialize<List<Shortcut>>(json, _jsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private void SaveShortcutsSync(List<Shortcut> shortcuts)
    {
        var json = JsonSerializer.Serialize(shortcuts, _jsonOptions);
        Preferences.Set(ShortcutsKey, json);
    }

    #endregion
}
