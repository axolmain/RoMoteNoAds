using RoMoteNoAds.Models;

namespace RoMoteNoAds.Services;

/// <summary>
/// Service for discovering Roku devices on the local network.
/// </summary>
public interface IRokuDiscoveryService
{
    /// <summary>
    /// Discovers Roku devices on the local network using SSDP.
    /// </summary>
    /// <param name="timeout">Discovery timeout.</param>
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
}
