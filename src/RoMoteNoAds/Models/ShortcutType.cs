namespace RoMoteNoAds.Models;

/// <summary>
/// Types of shortcuts that can be created.
/// </summary>
public enum ShortcutType
{
    /// <summary>
    /// Launches a channel, optionally with a deep link.
    /// </summary>
    Channel,

    /// <summary>
    /// Executes a recorded sequence of key presses.
    /// </summary>
    KeySequence
}
