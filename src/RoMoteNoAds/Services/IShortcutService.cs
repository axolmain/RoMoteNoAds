using RoMoteNoAds.Models;

namespace RoMoteNoAds.Services;

/// <summary>
/// Service for managing and executing custom shortcuts.
/// </summary>
public interface IShortcutService
{
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
    /// Executes a shortcut (launches channel or plays key sequence).
    /// </summary>
    Task<bool> ExecuteShortcutAsync(Shortcut shortcut);

    /// <summary>
    /// Reorders shortcuts.
    /// </summary>
    Task ReorderShortcutsAsync(IEnumerable<Shortcut> shortcuts);
}
