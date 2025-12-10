using RoMoteNoAds.Models;

namespace RoMoteNoAds.Services;

/// <summary>
/// Service for persisting Roku device information.
/// </summary>
public interface IDeviceStorageService
{
    /// <summary>
    /// Gets all saved devices.
    /// </summary>
    /// <returns>Collection of saved devices.</returns>
    Task<IEnumerable<RokuDevice>> GetSavedDevicesAsync();

    /// <summary>
    /// Saves a device to storage.
    /// </summary>
    /// <param name="device">The device to save.</param>
    Task SaveDeviceAsync(RokuDevice device);

    /// <summary>
    /// Removes a device from storage.
    /// </summary>
    /// <param name="device">The device to remove.</param>
    Task RemoveDeviceAsync(RokuDevice device);

    /// <summary>
    /// Gets the last used device.
    /// </summary>
    /// <returns>The last used device, or null if none.</returns>
    Task<RokuDevice?> GetLastUsedDeviceAsync();

    /// <summary>
    /// Sets the last used device.
    /// </summary>
    /// <param name="device">The device that was last used.</param>
    Task SetLastUsedDeviceAsync(RokuDevice device);

    /// <summary>
    /// Clears all saved devices.
    /// </summary>
    Task ClearAllDevicesAsync();
}
