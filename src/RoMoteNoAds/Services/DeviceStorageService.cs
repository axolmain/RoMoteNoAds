using System.Text.Json;
using RoMoteNoAds.Models;

namespace RoMoteNoAds.Services;

/// <summary>
/// Service for persisting Roku device information using Preferences.
/// </summary>
public class DeviceStorageService : IDeviceStorageService
{
    private const string DevicesKey = "saved_roku_devices";
    private const string LastUsedDeviceKey = "last_used_roku_device";

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<IEnumerable<RokuDevice>> GetSavedDevicesAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                var json = Preferences.Get(DevicesKey, string.Empty);
                if (string.IsNullOrEmpty(json))
                    return Enumerable.Empty<RokuDevice>();

                var devices = JsonSerializer.Deserialize<List<RokuDevice>>(json, _jsonOptions);
                return devices ?? Enumerable.Empty<RokuDevice>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading devices: {ex.Message}");
                return Enumerable.Empty<RokuDevice>();
            }
        });
    }

    public async Task SaveDeviceAsync(RokuDevice device)
    {
        await Task.Run(() =>
        {
            try
            {
                var devices = GetDevicesSync();

                // Update existing or add new
                var existing = devices.FirstOrDefault(d => d.SerialNumber == device.SerialNumber);
                if (existing != null)
                {
                    devices.Remove(existing);
                }
                devices.Add(device);

                var json = JsonSerializer.Serialize(devices, _jsonOptions);
                Preferences.Set(DevicesKey, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving device: {ex.Message}");
            }
        });
    }

    public async Task RemoveDeviceAsync(RokuDevice device)
    {
        await Task.Run(() =>
        {
            try
            {
                var devices = GetDevicesSync();
                var toRemove = devices.FirstOrDefault(d =>
                    d.SerialNumber == device.SerialNumber ||
                    d.IpAddress == device.IpAddress);

                if (toRemove != null)
                {
                    devices.Remove(toRemove);
                    var json = JsonSerializer.Serialize(devices, _jsonOptions);
                    Preferences.Set(DevicesKey, json);
                }

                // If this was the last used device, clear that too
                var lastUsed = GetLastUsedDeviceSync();
                if (lastUsed != null &&
                    (lastUsed.SerialNumber == device.SerialNumber ||
                     lastUsed.IpAddress == device.IpAddress))
                {
                    Preferences.Remove(LastUsedDeviceKey);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing device: {ex.Message}");
            }
        });
    }

    public async Task<RokuDevice?> GetLastUsedDeviceAsync()
    {
        return await Task.Run(GetLastUsedDeviceSync);
    }

    public async Task SetLastUsedDeviceAsync(RokuDevice device)
    {
        await Task.Run(() =>
        {
            try
            {
                var json = JsonSerializer.Serialize(device, _jsonOptions);
                Preferences.Set(LastUsedDeviceKey, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving last used device: {ex.Message}");
            }
        });
    }

    public async Task ClearAllDevicesAsync()
    {
        await Task.Run(() =>
        {
            Preferences.Remove(DevicesKey);
            Preferences.Remove(LastUsedDeviceKey);
        });
    }

    private List<RokuDevice> GetDevicesSync()
    {
        try
        {
            var json = Preferences.Get(DevicesKey, string.Empty);
            if (string.IsNullOrEmpty(json))
                return new List<RokuDevice>();

            return JsonSerializer.Deserialize<List<RokuDevice>>(json, _jsonOptions)
                   ?? new List<RokuDevice>();
        }
        catch
        {
            return new List<RokuDevice>();
        }
    }

    private RokuDevice? GetLastUsedDeviceSync()
    {
        try
        {
            var json = Preferences.Get(LastUsedDeviceKey, string.Empty);
            if (string.IsNullOrEmpty(json))
                return null;

            return JsonSerializer.Deserialize<RokuDevice>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
