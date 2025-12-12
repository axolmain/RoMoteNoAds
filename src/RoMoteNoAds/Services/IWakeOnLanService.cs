namespace RoMoteNoAds.Services;

/// <summary>
/// Service for sending Wake-on-LAN magic packets.
/// </summary>
public interface IWakeOnLanService
{
    /// <summary>
    /// Sends a Wake-on-LAN magic packet to wake a device.
    /// </summary>
    /// <param name="macAddress">The MAC address of the device to wake (format: XX:XX:XX:XX:XX:XX).</param>
    /// <returns>True if the packet was sent successfully, false otherwise.</returns>
    Task<bool> WakeAsync(string macAddress);
}
