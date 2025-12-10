using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoMoteNoAds.Models;
using RoMoteNoAds.Services;

namespace RoMoteNoAds.ViewModels;

/// <summary>
/// ViewModel for the device selection page.
/// </summary>
public partial class DeviceSelectionViewModel : BaseViewModel
{
    private readonly IRokuDiscoveryService _discoveryService;
    private readonly IRokuControlService _controlService;
    private readonly IDeviceStorageService _storageService;

    [ObservableProperty]
    private ObservableCollection<RokuDevice> _devices = new();

    [ObservableProperty]
    private RokuDevice? _selectedDevice;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private string _manualIpAddress = string.Empty;

    public DeviceSelectionViewModel(
        IRokuDiscoveryService discoveryService,
        IRokuControlService controlService,
        IDeviceStorageService storageService)
    {
        _discoveryService = discoveryService;
        _controlService = controlService;
        _storageService = storageService;

        Title = "Devices";

        _discoveryService.DeviceDiscovered += OnDeviceDiscovered;
    }

    public async Task InitializeAsync()
    {
        await LoadSavedDevicesAsync();

        // Auto-select last used device
        var lastUsed = await _storageService.GetLastUsedDeviceAsync();
        if (lastUsed != null)
        {
            var device = Devices.FirstOrDefault(d => d.SerialNumber == lastUsed.SerialNumber);
            if (device != null)
            {
                await SelectDeviceAsync(device);
            }
        }
    }

    [RelayCommand]
    private async Task LoadSavedDevicesAsync()
    {
        var savedDevices = await _storageService.GetSavedDevicesAsync();
        Devices.Clear();
        foreach (var device in savedDevices)
        {
            Devices.Add(device);
        }
    }

    [RelayCommand]
    private async Task ScanForDevicesAsync()
    {
        if (IsScanning)
            return;

        try
        {
            IsScanning = true;
            ClearError();

            var discoveredDevices = await _discoveryService.DiscoverDevicesAsync(
                TimeSpan.FromSeconds(5));

            foreach (var device in discoveredDevices)
            {
                if (!Devices.Any(d => d.SerialNumber == device.SerialNumber))
                {
                    Devices.Add(device);
                    await _storageService.SaveDeviceAsync(device);
                }
            }

            if (!discoveredDevices.Any())
            {
                SetError("No Roku devices found on your network");
            }
        }
        catch (Exception ex)
        {
            SetError($"Scan failed: {ex.Message}");
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private async Task AddManualDeviceAsync()
    {
        if (string.IsNullOrWhiteSpace(ManualIpAddress))
        {
            SetError("Please enter an IP address");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var device = await _discoveryService.ValidateDeviceAsync(ManualIpAddress);

            if (device == null)
            {
                SetError("Could not connect to Roku at this address");
                return;
            }

            if (!Devices.Any(d => d.SerialNumber == device.SerialNumber))
            {
                Devices.Add(device);
            }

            await _storageService.SaveDeviceAsync(device);
            ManualIpAddress = string.Empty;

            // Auto-select the newly added device
            await SelectDeviceAsync(device);
        }
        catch (Exception ex)
        {
            SetError($"Failed to add device: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectDeviceAsync(RokuDevice? device)
    {
        if (device == null)
            return;

        // Update selection state
        foreach (var d in Devices)
        {
            d.IsSelected = d.SerialNumber == device.SerialNumber;
        }

        SelectedDevice = device;
        _controlService.CurrentDevice = device;

        await _storageService.SetLastUsedDeviceAsync(device);
    }

    [RelayCommand]
    private async Task RemoveDeviceAsync(RokuDevice? device)
    {
        if (device == null)
            return;

        Devices.Remove(device);
        await _storageService.RemoveDeviceAsync(device);

        if (SelectedDevice?.SerialNumber == device.SerialNumber)
        {
            SelectedDevice = Devices.FirstOrDefault();
            _controlService.CurrentDevice = SelectedDevice;
        }
    }

    [RelayCommand]
    private async Task RefreshDeviceAsync(RokuDevice? device)
    {
        if (device == null)
            return;

        try
        {
            var updated = await _discoveryService.RefreshDeviceInfoAsync(device);
            if (updated != null)
            {
                var index = Devices.IndexOf(device);
                if (index >= 0)
                {
                    updated.IsSelected = device.IsSelected;
                    updated.CustomName = device.CustomName;
                    Devices[index] = updated;
                    await _storageService.SaveDeviceAsync(updated);

                    if (SelectedDevice?.SerialNumber == updated.SerialNumber)
                    {
                        SelectedDevice = updated;
                        _controlService.CurrentDevice = updated;
                    }
                }
            }
            else
            {
                SetError("Device not responding");
            }
        }
        catch (Exception ex)
        {
            SetError($"Refresh failed: {ex.Message}");
        }
    }

    private void OnDeviceDiscovered(object? sender, RokuDevice device)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!Devices.Any(d => d.SerialNumber == device.SerialNumber))
            {
                Devices.Add(device);
            }
        });
    }
}
