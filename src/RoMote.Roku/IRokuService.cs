using RoMote.Roku.Models;

namespace RoMote.Roku;

/// <summary>
/// Service for discovering and controlling Roku devices.
/// Combines device discovery (SSDP), control (ECP), and Wake-on-LAN functionality.
/// </summary>
public interface IRokuService
{
    /// <summary>
    /// Gets or sets the currently active Roku device.
    /// </summary>
    RokuDevice? CurrentDevice { get; set; }

    #region Discovery

    /// <summary>
    /// Discovers Roku devices on the local network using SSDP.
    /// </summary>
    /// <param name="timeout">Discovery timeout (default: 5 seconds).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of discovered Roku devices.</returns>
    Task<IEnumerable<RokuDevice>> DiscoverDevicesAsync(
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and retrieves device info for a manually entered IP address.
    /// </summary>
    /// <param name="ipAddress">The IP address to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The device if valid, null otherwise.</returns>
    Task<RokuDevice?> ValidateDeviceAsync(
        string ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes device info for an existing device.
    /// </summary>
    /// <param name="device">The device to refresh.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated device info.</returns>
    Task<RokuDevice?> RefreshDeviceInfoAsync(
        RokuDevice device,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when a new device is discovered during scanning.
    /// </summary>
    event EventHandler<RokuDevice>? DeviceDiscovered;

    #endregion

    #region Control

    /// <summary>
    /// Sends a keypress command to the Roku device.
    /// </summary>
    /// <param name="key">The key to press (use RokuKey constants).</param>
    /// <returns>True if successful.</returns>
    Task<bool> SendKeyPressAsync(string key);

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
    /// Event raised when a command fails.
    /// </summary>
    event EventHandler<string>? CommandFailed;

    #endregion

    #region Wake-on-LAN

    /// <summary>
    /// Sends a Wake-on-LAN magic packet to wake a device.
    /// </summary>
    /// <param name="macAddress">The MAC address of the device to wake (format: XX:XX:XX:XX:XX:XX).</param>
    /// <returns>True if the packet was sent successfully.</returns>
    Task<bool> WakeAsync(string macAddress);

    #endregion
}
