namespace RoMoteNoAds.Models;

/// <summary>
/// Represents an installed channel/app on a Roku device.
/// </summary>
public class RokuChannel
{
    /// <summary>
    /// The unique channel ID used for launching.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The display name of the channel.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The channel type (e.g., "appl", "menu").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The version of the installed channel.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// The URL to fetch the channel icon.
    /// </summary>
    public string IconUrl { get; set; } = string.Empty;

    /// <summary>
    /// Cached icon image source.
    /// </summary>
    public ImageSource? IconSource { get; set; }

    /// <summary>
    /// Whether this channel is currently active/running.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Whether this channel is pinned as a favorite.
    /// </summary>
    public bool IsFavorite { get; set; }

    public override string ToString() => Name;

    public override bool Equals(object? obj)
    {
        if (obj is RokuChannel other)
        {
            return Id == other.Id;
        }
        return false;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
