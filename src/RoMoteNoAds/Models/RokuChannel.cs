using LibRokuChannel = RoMote.Roku.Models.RokuChannel;

namespace RoMoteNoAds.Models;

/// <summary>
/// App-specific wrapper around library RokuChannel with UI-bindable properties.
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
    /// Cached icon image source (UI-specific).
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

    /// <summary>
    /// Creates an app model from a library model.
    /// </summary>
    public static RokuChannel FromLibrary(LibRokuChannel lib)
    {
        return new RokuChannel
        {
            Id = lib.Id,
            Name = lib.Name,
            Type = lib.Type,
            Version = lib.Version,
            IconUrl = lib.IconUrl,
            IsActive = lib.IsActive
        };
    }

    /// <summary>
    /// Converts to a library model.
    /// </summary>
    public LibRokuChannel ToLibrary()
    {
        return new LibRokuChannel
        {
            Id = Id,
            Name = Name,
            Type = Type,
            Version = Version,
            IconUrl = IconUrl,
            IsActive = IsActive
        };
    }
}
