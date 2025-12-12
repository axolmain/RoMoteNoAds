using System.Text.Json;
using RoMoteNoAds.Models;

namespace RoMoteNoAds.Services;

/// <summary>
/// Service for persisting device information using Preferences.
/// </summary>
public class StorageService : IStorageService
{
    private const string DevicesKey = "saved_roku_devices";
    private const string LastUsedDeviceKey = "last_used_roku_device";

    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<IEnumerable<RokuDevice>> GetSavedDevicesAsync()
    {
        return await Task.Run(() => Load<List<RokuDevice>>(DevicesKey) ?? []);
    }

    public async Task SaveDeviceAsync(RokuDevice device)
    {
        await Task.Run(() =>
        {
            var devices = Load<List<RokuDevice>>(DevicesKey) ?? [];

            // Update existing or add new
            var existing = devices.FirstOrDefault(d => d.SerialNumber == device.SerialNumber);
            if (existing != null)
            {
                devices.Remove(existing);
            }
            devices.Add(device);

            Save(DevicesKey, devices);
        });
    }

    public async Task RemoveDeviceAsync(RokuDevice device)
    {
        await Task.Run(() =>
        {
            var devices = Load<List<RokuDevice>>(DevicesKey) ?? [];
            var toRemove = devices.FirstOrDefault(d =>
                d.SerialNumber == device.SerialNumber ||
                d.IpAddress == device.IpAddress);

            if (toRemove != null)
            {
                devices.Remove(toRemove);
                Save(DevicesKey, devices);
            }

            // If this was the last used device, clear that too
            var lastUsed = Load<RokuDevice>(LastUsedDeviceKey);
            if (lastUsed != null &&
                (lastUsed.SerialNumber == device.SerialNumber ||
                 lastUsed.IpAddress == device.IpAddress))
            {
                Preferences.Remove(LastUsedDeviceKey);
            }
        });
    }

    public async Task<RokuDevice?> GetLastUsedDeviceAsync()
    {
        return await Task.Run(() => Load<RokuDevice>(LastUsedDeviceKey));
    }

    public async Task SetLastUsedDeviceAsync(RokuDevice device)
    {
        await Task.Run(() => Save(LastUsedDeviceKey, device));
    }

    public async Task ClearAllDevicesAsync()
    {
        await Task.Run(() =>
        {
            Preferences.Remove(DevicesKey);
            Preferences.Remove(LastUsedDeviceKey);
        });
    }

    private T? Load<T>(string key)
    {
        try
        {
            var json = Preferences.Get(key, string.Empty);
            if (string.IsNullOrEmpty(json))
                return default;

            return JsonSerializer.Deserialize<T>(json, _json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading {key}: {ex.Message}");
            return default;
        }
    }

    private void Save<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, _json);
            Preferences.Set(key, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving {key}: {ex.Message}");
        }
    }
}
