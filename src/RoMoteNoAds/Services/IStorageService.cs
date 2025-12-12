using RoMoteNoAds.Models;

namespace RoMoteNoAds.Services;

/// <summary>
/// Service for persisting device information.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Gets all saved devices.
    /// </summary>
    Task<IEnumerable<RokuDevice>> GetSavedDevicesAsync();

    /// <summary>
    /// Saves a device to storage.
    /// </summary>
    Task SaveDeviceAsync(RokuDevice device);

    /// <summary>
    /// Removes a device from storage.
    /// </summary>
    Task RemoveDeviceAsync(RokuDevice device);

    /// <summary>
    /// Gets the last used device.
    /// </summary>
    Task<RokuDevice?> GetLastUsedDeviceAsync();

    /// <summary>
    /// Sets the last used device.
    /// </summary>
    Task SetLastUsedDeviceAsync(RokuDevice device);

    /// <summary>
    /// Clears all saved devices.
    /// </summary>
    Task ClearAllDevicesAsync();
}
