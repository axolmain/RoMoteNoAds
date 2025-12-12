namespace RoMote.Roku.Models;

/// <summary>
/// Represents a Roku device discovered on the network.
/// </summary>
public class RokuDevice
{
    /// <summary>
    /// The IP address of the Roku device.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// The port for ECP commands (default: 8060).
    /// </summary>
    public int Port { get; set; } = 8060;

    /// <summary>
    /// The friendly name of the device (e.g., "Living Room Roku").
    /// </summary>
    public string FriendlyName { get; set; } = string.Empty;

    /// <summary>
    /// The device serial number from USN header.
    /// </summary>
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// The model name (e.g., "Roku Ultra", "Roku TV").
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// The model number.
    /// </summary>
    public string ModelNumber { get; set; } = string.Empty;

    /// <summary>
    /// Whether this device is a Roku TV (vs streaming stick/box).
    /// </summary>
    public bool IsTv { get; set; }

    /// <summary>
    /// Whether the device supports TV power control.
    /// </summary>
    public bool SupportsTvPowerControl { get; set; }

    /// <summary>
    /// Whether the device supports audio volume control.
    /// </summary>
    public bool SupportsAudioVolumeControl { get; set; }

    /// <summary>
    /// Whether the device supports the "Find Remote" feature.
    /// </summary>
    public bool SupportsFindRemote { get; set; }

    /// <summary>
    /// Whether the device supports Wake-on-WLAN.
    /// </summary>
    public bool SupportsWakeOnWlan { get; set; }

    /// <summary>
    /// The WiFi MAC address of the device.
    /// </summary>
    public string? WifiMacAddress { get; set; }

    /// <summary>
    /// The software version running on the device.
    /// </summary>
    public string SoftwareVersion { get; set; } = string.Empty;

    /// <summary>
    /// The base URL for ECP commands.
    /// </summary>
    public string BaseUrl => $"http://{IpAddress}:{Port}";

    /// <summary>
    /// User-assigned custom name for the device.
    /// </summary>
    public string? CustomName { get; set; }

    /// <summary>
    /// Display name (custom name if set, otherwise friendly name).
    /// </summary>
    public string DisplayName => !string.IsNullOrEmpty(CustomName) ? CustomName : FriendlyName;

    /// <summary>
    /// Last time this device was successfully contacted.
    /// </summary>
    public DateTime? LastSeen { get; set; }

    public override string ToString() => $"{DisplayName} ({IpAddress})";

    public override bool Equals(object? obj)
    {
        if (obj is RokuDevice other)
        {
            return SerialNumber == other.SerialNumber || IpAddress == other.IpAddress;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return !string.IsNullOrEmpty(SerialNumber)
            ? SerialNumber.GetHashCode()
            : IpAddress.GetHashCode();
    }
}
