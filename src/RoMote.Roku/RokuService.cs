using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using RoMote.Roku.Models;

namespace RoMote.Roku;

/// <summary>
/// Service for discovering and controlling Roku devices.
/// Combines device discovery (SSDP), control (ECP), and Wake-on-LAN functionality.
/// </summary>
public partial class RokuService : IRokuService
{
    private readonly HttpClient _httpClient;

    private const string SsdpMulticastAddress = "239.255.255.250";
    private const int SsdpPort = 1900;
    private const int RokuEcpPort = 8060;
    private const int WolPort = 9;

    private const string SsdpSearchRequest =
        "M-SEARCH * HTTP/1.1\r\n" +
        "HOST: 239.255.255.250:1900\r\n" +
        "MAN: \"ssdp:discover\"\r\n" +
        "ST: roku:ecp\r\n" +
        "MX: 3\r\n" +
        "\r\n";

    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    public RokuDevice? CurrentDevice { get; set; }

    public event EventHandler<RokuDevice>? DeviceDiscovered;
    public event EventHandler<string>? CommandFailed;

    public RokuService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
    }

    #region Discovery

    public async Task<IEnumerable<RokuDevice>> DiscoverDevicesAsync(
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        timeout ??= DefaultTimeout;
        var devices = new Dictionary<string, RokuDevice>();

        System.Diagnostics.Debug.WriteLine($"[SSDP] Starting discovery with timeout: {timeout.Value.TotalSeconds}s");

        try
        {
            using var udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));

            var localEndpoint = udpClient.Client.LocalEndPoint as IPEndPoint;
            System.Diagnostics.Debug.WriteLine($"[SSDP] Bound to local endpoint: {localEndpoint}");

            var multicastEndpoint = new IPEndPoint(IPAddress.Parse(SsdpMulticastAddress), SsdpPort);
            var requestBytes = Encoding.UTF8.GetBytes(SsdpSearchRequest);

            System.Diagnostics.Debug.WriteLine($"[SSDP] Sending M-SEARCH to {multicastEndpoint}");
            System.Diagnostics.Debug.WriteLine($"[SSDP] Request:\n{SsdpSearchRequest}");

            await udpClient.SendAsync(requestBytes, requestBytes.Length, multicastEndpoint);
            System.Diagnostics.Debug.WriteLine($"[SSDP] M-SEARCH sent successfully ({requestBytes.Length} bytes)");

            var endTime = DateTime.UtcNow.Add(timeout.Value);
            udpClient.Client.ReceiveTimeout = (int)timeout.Value.TotalMilliseconds;

            var responseCount = 0;
            while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var receiveTask = udpClient.ReceiveAsync();
                    var delayTask = Task.Delay(endTime - DateTime.UtcNow, cancellationToken);

                    var completedTask = await Task.WhenAny(receiveTask, delayTask);

                    if (completedTask == delayTask)
                    {
                        System.Diagnostics.Debug.WriteLine($"[SSDP] Timeout reached, received {responseCount} responses");
                        break;
                    }

                    var result = await receiveTask;
                    responseCount++;
                    var response = Encoding.UTF8.GetString(result.Buffer);

                    System.Diagnostics.Debug.WriteLine($"[SSDP] Response #{responseCount} from {result.RemoteEndPoint}:");
                    System.Diagnostics.Debug.WriteLine(response);

                    var device = await ParseSsdpResponseAsync(response, cancellationToken);
                    if (device != null && !devices.ContainsKey(device.SerialNumber))
                    {
                        System.Diagnostics.Debug.WriteLine($"[SSDP] Found device: {device.FriendlyName} ({device.IpAddress})");
                        devices[device.SerialNumber] = device;
                        DeviceDiscovered?.Invoke(this, device);
                    }
                    else if (device == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[SSDP] Response #{responseCount} was not a valid Roku device");
                    }
                }
                catch (SocketException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[SSDP] SocketException: {ex.SocketErrorCode} - {ex.Message}");
                    break;
                }
                catch (OperationCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine($"[SSDP] Cancelled");
                    break;
                }
            }

            System.Diagnostics.Debug.WriteLine($"[SSDP] Discovery complete. Found {devices.Count} devices from {responseCount} responses");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SSDP] Discovery error: {ex.GetType().Name} - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SSDP] Stack trace: {ex.StackTrace}");
        }

        return devices.Values;
    }

    public async Task<RokuDevice?> ValidateDeviceAsync(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return null;

        ipAddress = ipAddress.Trim();
        if (!IPAddress.TryParse(ipAddress, out _))
            return null;

        try
        {
            var device = new RokuDevice
            {
                IpAddress = ipAddress,
                Port = RokuEcpPort
            };

            return await FetchDeviceInfoAsync(device, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<RokuDevice?> RefreshDeviceInfoAsync(
        RokuDevice device,
        CancellationToken cancellationToken = default)
    {
        return await FetchDeviceInfoAsync(device, cancellationToken);
    }

    private async Task<RokuDevice?> ParseSsdpResponseAsync(
        string response,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!response.Contains("200 OK", StringComparison.OrdinalIgnoreCase))
                return null;

            var locationMatch = LocationRegex().Match(response);
            if (!locationMatch.Success)
                return null;

            var locationUrl = locationMatch.Groups[1].Value.Trim();

            var usnMatch = UsnRegex().Match(response);
            var serialNumber = string.Empty;
            if (usnMatch.Success)
            {
                var usn = usnMatch.Groups[1].Value;
                var serialMatch = SerialRegex().Match(usn);
                if (serialMatch.Success)
                {
                    serialNumber = serialMatch.Groups[1].Value;
                }
            }

            if (!Uri.TryCreate(locationUrl, UriKind.Absolute, out var uri))
                return null;

            var device = new RokuDevice
            {
                IpAddress = uri.Host,
                Port = uri.Port > 0 ? uri.Port : RokuEcpPort,
                SerialNumber = serialNumber
            };

            return await FetchDeviceInfoAsync(device, cancellationToken);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error parsing SSDP response: {ex.Message}");
            return null;
        }
    }

    private async Task<RokuDevice?> FetchDeviceInfoAsync(
        RokuDevice device,
        CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(DefaultTimeout);

            var url = $"{device.BaseUrl}/query/device-info";
            var response = await _httpClient.GetStringAsync(url, cts.Token);

            var xml = XDocument.Parse(response);
            var root = xml.Root;

            if (root == null)
                return null;

            device.FriendlyName = GetXmlValue(root, "friendly-device-name")
                               ?? GetXmlValue(root, "user-device-name")
                               ?? GetXmlValue(root, "model-name")
                               ?? "Roku Device";
            device.ModelName = GetXmlValue(root, "model-name") ?? string.Empty;
            device.ModelNumber = GetXmlValue(root, "model-number") ?? string.Empty;
            device.SerialNumber = GetXmlValue(root, "serial-number") ?? device.SerialNumber;
            device.SoftwareVersion = GetXmlValue(root, "software-version") ?? string.Empty;

            device.IsTv = GetXmlBool(root, "is-tv");
            device.SupportsTvPowerControl = GetXmlBool(root, "supports-tv-power-control");
            device.SupportsAudioVolumeControl = GetXmlBool(root, "supports-audio-volume-control");
            device.SupportsFindRemote = GetXmlBool(root, "supports-find-remote");
            device.SupportsWakeOnWlan = GetXmlBool(root, "supports-wake-on-wlan");
            device.WifiMacAddress = GetXmlValue(root, "wifi-mac");

            device.LastSeen = DateTime.UtcNow;

            return device;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching device info: {ex.Message}");
            return null;
        }
    }

    private static string? GetXmlValue(XElement root, string elementName)
    {
        return root.Element(elementName)?.Value;
    }

    private static bool GetXmlBool(XElement root, string elementName)
    {
        var value = root.Element(elementName)?.Value;
        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex(@"(?i)location:\s*(.+)", RegexOptions.IgnoreCase)]
    private static partial Regex LocationRegex();

    [GeneratedRegex(@"(?i)usn:\s*(.+)", RegexOptions.IgnoreCase)]
    private static partial Regex UsnRegex();

    [GeneratedRegex(@"uuid:roku:ecp:(.+)", RegexOptions.IgnoreCase)]
    private static partial Regex SerialRegex();

    #endregion

    #region Control

    public async Task<bool> SendKeyPressAsync(string key)
    {
        return await SendPostCommandAsync($"keypress/{key}");
    }

    public async Task<bool> SendTextAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
            return true;

        foreach (var c in text)
        {
            var key = RokuKey.Literal(c);
            var success = await SendKeyPressAsync(key);
            if (!success)
                return false;

            await Task.Delay(50);
        }

        return true;
    }

    public async Task<bool> LaunchChannelAsync(string channelId)
    {
        return await SendPostCommandAsync($"launch/{channelId}");
    }

    public async Task<IEnumerable<RokuChannel>> GetInstalledChannelsAsync()
    {
        var channels = new List<RokuChannel>();

        try
        {
            if (CurrentDevice == null)
                return channels;

            var url = $"{CurrentDevice.BaseUrl}/query/apps";
            var response = await _httpClient.GetStringAsync(url);

            var xml = XDocument.Parse(response);
            var apps = xml.Root?.Elements("app");

            if (apps == null)
                return channels;

            foreach (var app in apps)
            {
                var id = app.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(id))
                    continue;

                channels.Add(new RokuChannel
                {
                    Id = id,
                    Name = app.Value?.Trim() ?? string.Empty,
                    Type = app.Attribute("type")?.Value ?? string.Empty,
                    Version = app.Attribute("version")?.Value ?? string.Empty,
                    IconUrl = $"{CurrentDevice.BaseUrl}/query/icon/{id}"
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching channels: {ex.Message}");
            OnCommandFailed("Failed to fetch installed channels");
        }

        return channels;
    }

    public async Task<RokuChannel?> GetActiveChannelAsync()
    {
        try
        {
            if (CurrentDevice == null)
                return null;

            var url = $"{CurrentDevice.BaseUrl}/query/active-app";
            var response = await _httpClient.GetStringAsync(url);

            var xml = XDocument.Parse(response);
            var app = xml.Root?.Element("app");

            if (app == null)
                return null;

            var id = app.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(id))
                return null;

            return new RokuChannel
            {
                Id = id,
                Name = app.Value?.Trim() ?? string.Empty,
                Type = app.Attribute("type")?.Value ?? string.Empty,
                Version = app.Attribute("version")?.Value ?? string.Empty,
                IconUrl = $"{CurrentDevice.BaseUrl}/query/icon/{id}",
                IsActive = true
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching active app: {ex.Message}");
            return null;
        }
    }

    public async Task<byte[]?> GetChannelIconAsync(string channelId)
    {
        try
        {
            if (CurrentDevice == null)
                return null;

            var url = $"{CurrentDevice.BaseUrl}/query/icon/{channelId}";
            return await _httpClient.GetByteArrayAsync(url);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching icon for {channelId}: {ex.Message}");
            return null;
        }
    }

    private async Task<bool> SendPostCommandAsync(string command)
    {
        string? url = null;
        try
        {
            if (CurrentDevice == null)
            {
                System.Diagnostics.Debug.WriteLine("[RokuService] Command failed: No device connected");
                OnCommandFailed("No device connected");
                return false;
            }

            url = $"{CurrentDevice.BaseUrl}/{command}";
            System.Diagnostics.Debug.WriteLine($"[RokuService] Sending command: POST {url}");

            var response = await _httpClient.PostAsync(url, null);

            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                var reasonPhrase = response.ReasonPhrase ?? "Unknown";
                var responseBody = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[RokuService] Command failed:");
                System.Diagnostics.Debug.WriteLine($"  URL: {url}");
                System.Diagnostics.Debug.WriteLine($"  Status: {statusCode} {reasonPhrase}");
                System.Diagnostics.Debug.WriteLine($"  Response body: {responseBody}");

                OnCommandFailed($"Command failed: {response.StatusCode}");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"[RokuService] Command succeeded: {command}");
            return true;
        }
        catch (TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine($"[RokuService] Command timed out: {url ?? command}");
            OnCommandFailed("Command timed out");
            return false;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RokuService] Network error for {url ?? command}: {ex.Message}");
            OnCommandFailed($"Network error: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RokuService] Unexpected error for {url ?? command}: {ex.GetType().Name} - {ex.Message}");
            OnCommandFailed($"Error: {ex.Message}");
            return false;
        }
    }

    private void OnCommandFailed(string message)
    {
        CommandFailed?.Invoke(this, message);
    }

    #endregion

    #region Wake-on-LAN

    public async Task<bool> WakeAsync(string macAddress)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
            return false;

        try
        {
            var macBytes = ParseMacAddress(macAddress);
            if (macBytes == null)
                return false;

            var magicPacket = BuildMagicPacket(macBytes);

            using var udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;

            var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, WolPort);
            await udpClient.SendAsync(magicPacket, magicPacket.Length, broadcastEndpoint);

            var globalBroadcast = new IPEndPoint(IPAddress.Parse("255.255.255.255"), WolPort);
            await udpClient.SendAsync(magicPacket, magicPacket.Length, globalBroadcast);

            System.Diagnostics.Debug.WriteLine($"[RokuService] Magic packet sent to {macAddress}");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RokuService] Failed to send magic packet: {ex.Message}");
            return false;
        }
    }

    private static byte[]? ParseMacAddress(string macAddress)
    {
        try
        {
            var cleanMac = macAddress
                .Replace(":", "")
                .Replace("-", "")
                .Replace(" ", "")
                .ToUpperInvariant();

            if (cleanMac.Length != 12)
                return null;

            var bytes = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                bytes[i] = Convert.ToByte(cleanMac.Substring(i * 2, 2), 16);
            }

            return bytes;
        }
        catch
        {
            return null;
        }
    }

    private static byte[] BuildMagicPacket(byte[] macBytes)
    {
        var packet = new byte[6 + (6 * 16)];

        for (int i = 0; i < 6; i++)
        {
            packet[i] = 0xFF;
        }

        for (int i = 0; i < 16; i++)
        {
            Array.Copy(macBytes, 0, packet, 6 + (i * 6), 6);
        }

        return packet;
    }

    #endregion
}
