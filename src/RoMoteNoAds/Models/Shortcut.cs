using CommunityToolkit.Mvvm.ComponentModel;

namespace RoMoteNoAds.Models;

/// <summary>
/// Represents a user-created shortcut for quick actions.
/// </summary>
public partial class Shortcut : ObservableObject
{
    /// <summary>
    /// Unique identifier for the shortcut.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User-defined name for the shortcut.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL to the icon (for channel shortcuts).
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// Type of shortcut (Channel or KeySequence).
    /// </summary>
    public ShortcutType Type { get; set; }

    /// <summary>
    /// Channel ID (for Channel type shortcuts).
    /// </summary>
    public string? ChannelId { get; set; }

    /// <summary>
    /// Deep link URL (for Channel type shortcuts).
    /// </summary>
    public string? DeepLink { get; set; }

    /// <summary>
    /// Recorded key sequence (for KeySequence type shortcuts).
    /// </summary>
    public List<string>? KeySequence { get; set; }

    /// <summary>
    /// Sort order for display.
    /// </summary>
    public int SortOrder { get; set; }
}
