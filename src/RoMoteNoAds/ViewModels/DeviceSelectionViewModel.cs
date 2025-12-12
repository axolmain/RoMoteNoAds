using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoMote.Roku;
using RoMoteNoAds.Models;
using RoMoteNoAds.Services;

namespace RoMoteNoAds.ViewModels;

/// <summary>
/// ViewModel for the device selection page.
/// </summary>
public partial class DeviceSelectionViewModel : BaseViewModel, IDisposable
{
    private readonly IRokuService _rokuService;
    private readonly IStorageService _storageService;
    private readonly PeriodicTimer _autoScanTimer;
    private CancellationTokenSource? _autoScanCts;
    private bool _disposed;

    private static readonly TimeSpan AutoScanInterval = TimeSpan.FromSeconds(30);

    [ObservableProperty]
    private ObservableCollection<RokuDevice> _devices = new();

    [ObservableProperty]
    private RokuDevice? _selectedDevice;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private string _manualIpAddress = string.Empty;

    public DeviceSelectionViewModel(
        IRokuService rokuService,
        IStorageService storageService)
    {
        _rokuService = rokuService;
        _storageService = storageService;
        _autoScanTimer = new PeriodicTimer(AutoScanInterval);

        Title = "Devices";

        _rokuService.DeviceDiscovered += OnDeviceDiscovered;
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

        // Start background auto-scan
        StartAutoScan();
    }

    private void StartAutoScan()
    {
        _autoScanCts = new CancellationTokenSource();
        _ = RunAutoScanLoopAsync(_autoScanCts.Token);
    }

    private async Task RunAutoScanLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _autoScanTimer.WaitForNextTickAsync(cancellationToken))
            {
                await ScanForDevicesInBackgroundAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
    }

    private async Task ScanForDevicesInBackgroundAsync(CancellationToken cancellationToken)
    {
        // Skip if manual scan is in progress
        if (IsScanning)
            return;

        try
        {
            var discoveredDevices = await _rokuService.DiscoverDevicesAsync(
                TimeSpan.FromSeconds(5), cancellationToken);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                foreach (var libDevice in discoveredDevices)
                {
                    var device = RokuDevice.FromLibrary(libDevice);
                    if (!Devices.Any(d => d.SerialNumber == device.SerialNumber))
                    {
                        Devices.Add(device);
                        await _storageService.SaveDeviceAsync(device);
                    }
                }
            });
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelling
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Background scan error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _autoScanCts?.Cancel();
        _autoScanCts?.Dispose();
        _autoScanTimer.Dispose();
        _rokuService.DeviceDiscovered -= OnDeviceDiscovered;
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

            var discoveredDevices = await _rokuService.DiscoverDevicesAsync(
                TimeSpan.FromSeconds(5));

            foreach (var libDevice in discoveredDevices)
            {
                var device = RokuDevice.FromLibrary(libDevice);
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

            var libDevice = await _rokuService.ValidateDeviceAsync(ManualIpAddress);

            if (libDevice == null)
            {
                SetError("Could not connect to Roku at this address");
                return;
            }

            var device = RokuDevice.FromLibrary(libDevice);
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
        _rokuService.CurrentDevice = device.ToLibrary();

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
            _rokuService.CurrentDevice = SelectedDevice?.ToLibrary();
        }
    }

    [RelayCommand]
    private async Task RefreshDeviceAsync(RokuDevice? device)
    {
        if (device == null)
            return;

        try
        {
            var libUpdated = await _rokuService.RefreshDeviceInfoAsync(device.ToLibrary());
            if (libUpdated != null)
            {
                var updated = RokuDevice.FromLibrary(libUpdated);
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
                        _rokuService.CurrentDevice = updated.ToLibrary();
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

    private void OnDeviceDiscovered(object? sender, RoMote.Roku.Models.RokuDevice libDevice)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var device = RokuDevice.FromLibrary(libDevice);
            if (!Devices.Any(d => d.SerialNumber == device.SerialNumber))
            {
                Devices.Add(device);
            }
        });
    }
}
