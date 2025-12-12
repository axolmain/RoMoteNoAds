using RoMoteNoAds.Models;

namespace RoMoteNoAds.Services;

/// <summary>
/// Service for managing, recording, and executing custom shortcuts.
/// </summary>
public interface IShortcutService
{
    #region Persistence

    /// <summary>
    /// Gets all saved shortcuts.
    /// </summary>
    Task<IEnumerable<Shortcut>> GetShortcutsAsync();

    /// <summary>
    /// Saves a shortcut (creates or updates).
    /// </summary>
    Task SaveShortcutAsync(Shortcut shortcut);

    /// <summary>
    /// Removes a shortcut.
    /// </summary>
    Task RemoveShortcutAsync(Shortcut shortcut);

    /// <summary>
    /// Reorders shortcuts.
    /// </summary>
    Task ReorderShortcutsAsync(IEnumerable<Shortcut> shortcuts);

    #endregion

    #region Recording

    /// <summary>
    /// Whether the service is currently recording key presses.
    /// </summary>
    bool IsRecording { get; }

    /// <summary>
    /// Starts recording key presses.
    /// </summary>
    void StartRecording();

    /// <summary>
    /// Stops recording and returns the captured key sequence.
    /// </summary>
    IReadOnlyList<string> StopRecording();

    #endregion

    #region Execution

    /// <summary>
    /// Sends a key press to the Roku device. If recording, also captures the key.
    /// ViewModels should call this instead of IRokuService.SendKeyPressAsync()
    /// when they want recording to work.
    /// </summary>
    Task<bool> SendKeyPressAsync(string key);

    /// <summary>
    /// Executes a shortcut (launches channel or plays key sequence).
    /// </summary>
    Task<bool> ExecuteShortcutAsync(Shortcut shortcut);

    #endregion
}
