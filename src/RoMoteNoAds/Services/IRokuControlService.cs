using RoMoteNoAds.Models;

namespace RoMoteNoAds.Services;

/// <summary>
/// Service for controlling a Roku device via ECP.
/// </summary>
public interface IRokuControlService
{
    /// <summary>
    /// Gets or sets the currently active Roku device.
    /// </summary>
    RokuDevice? CurrentDevice { get; set; }

    /// <summary>
    /// Sends a keypress command to the Roku device.
    /// </summary>
    /// <param name="key">The key to press (use RokuKey constants).</param>
    /// <returns>True if successful.</returns>
    Task<bool> SendKeyPressAsync(string key);

    /// <summary>
    /// Sends a keydown command (for long press).
    /// </summary>
    /// <param name="key">The key to hold.</param>
    /// <returns>True if successful.</returns>
    Task<bool> SendKeyDownAsync(string key);

    /// <summary>
    /// Sends a keyup command (release held key).
    /// </summary>
    /// <param name="key">The key to release.</param>
    /// <returns>True if successful.</returns>
    Task<bool> SendKeyUpAsync(string key);

    /// <summary>
    /// Sends a text string as keyboard input.
    /// </summary>
    /// <param name="text">The text to type.</param>
    /// <returns>True if all characters were sent successfully.</returns>
    Task<bool> SendTextAsync(string text);

    /// <summary>
    /// Launches a channel by ID.
    /// </summary>
    /// <param name="channelId">The channel ID to launch.</param>
    /// <returns>True if successful.</returns>
    Task<bool> LaunchChannelAsync(string channelId);

    /// <summary>
    /// Gets the list of installed channels.
    /// </summary>
    /// <returns>Collection of installed channels.</returns>
    Task<IEnumerable<RokuChannel>> GetInstalledChannelsAsync();

    /// <summary>
    /// Gets the currently active channel/app.
    /// </summary>
    /// <returns>The active channel, or null if none.</returns>
    Task<RokuChannel?> GetActiveChannelAsync();

    /// <summary>
    /// Gets the channel icon as a byte array.
    /// </summary>
    /// <param name="channelId">The channel ID.</param>
    /// <returns>Icon bytes, or null if not found.</returns>
    Task<byte[]?> GetChannelIconAsync(string channelId);

    /// <summary>
    /// Gets the current media player state.
    /// </summary>
    /// <returns>Media player state XML, or null if not playing.</returns>
    Task<string?> GetMediaPlayerStateAsync();

    /// <summary>
    /// Event raised when a command fails.
    /// </summary>
    event EventHandler<string>? CommandFailed;
}
