using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using RoMoteNoAds.Models;

namespace RoMoteNoAds.Services;

/// <summary>
/// Service for discovering Roku devices using SSDP and querying device info.
/// </summary>
public partial class RokuDiscoveryService : IRokuDiscoveryService
{
    private readonly HttpClient _httpClient;
    private const string SsdpMulticastAddress = "239.255.255.250";
    private const int SsdpPort = 1900;
    private const int RokuEcpPort = 8060;

    // SSDP M-SEARCH request for Roku ECP devices
    private const string SsdpSearchRequest =
        "M-SEARCH * HTTP/1.1\r\n" +
        "Host: 239.255.255.250:1900\r\n" +
        "Man: \"ssdp:discover\"\r\n" +
        "ST: roku:ecp\r\n" +
        "\r\n";

    public event EventHandler<RokuDevice>? DeviceDiscovered;

    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    public RokuDiscoveryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<RokuDevice>> DiscoverDevicesAsync(
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        timeout ??= TimeSpan.FromSeconds(5);
        var devices = new Dictionary<string, RokuDevice>();

        try
        {
            using var udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));

            var multicastEndpoint = new IPEndPoint(IPAddress.Parse(SsdpMulticastAddress), SsdpPort);
            var requestBytes = Encoding.UTF8.GetBytes(SsdpSearchRequest);

            // Send M-SEARCH request
            await udpClient.SendAsync(requestBytes, requestBytes.Length, multicastEndpoint);

            // Listen for responses
            var endTime = DateTime.UtcNow.Add(timeout.Value);
            udpClient.Client.ReceiveTimeout = (int)timeout.Value.TotalMilliseconds;

            while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var receiveTask = udpClient.ReceiveAsync();
                    var delayTask = Task.Delay(endTime - DateTime.UtcNow, cancellationToken);

                    var completedTask = await Task.WhenAny(receiveTask, delayTask);

                    if (completedTask == delayTask)
                        break;

                    var result = await receiveTask;
                    var response = Encoding.UTF8.GetString(result.Buffer);

                    var device = await ParseSsdpResponseAsync(response, cancellationToken);
                    if (device != null && !devices.ContainsKey(device.SerialNumber))
                    {
                        devices[device.SerialNumber] = device;
                        DeviceDiscovered?.Invoke(this, device);
                    }
                }
                catch (SocketException)
                {
                    // Timeout or network error, continue
                    break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SSDP discovery error: {ex.Message}");
        }

        return devices.Values;
    }

    public async Task<RokuDevice?> ValidateDeviceAsync(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return null;

        // Clean up IP address
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
            // Check for 200 OK response
            if (!response.Contains("200 OK", StringComparison.OrdinalIgnoreCase))
                return null;

            // Extract Location header (case-insensitive per UPnP spec)
            var locationMatch = LocationRegex().Match(response);
            if (!locationMatch.Success)
                return null;

            var locationUrl = locationMatch.Groups[1].Value.Trim();

            // Extract USN for serial number
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

            // Parse URL to get IP and port
            if (!Uri.TryCreate(locationUrl, UriKind.Absolute, out var uri))
                return null;

            var device = new RokuDevice
            {
                IpAddress = uri.Host,
                Port = uri.Port > 0 ? uri.Port : RokuEcpPort,
                SerialNumber = serialNumber
            };

            // Fetch full device info
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

            // Parse capability flags
            device.IsTv = GetXmlBool(root, "is-tv");
            device.SupportsTvPowerControl = GetXmlBool(root, "supports-tv-power-control");
            device.SupportsAudioVolumeControl = GetXmlBool(root, "supports-audio-volume-control");
            device.SupportsFindRemote = GetXmlBool(root, "supports-find-remote");

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

    // Regex patterns for parsing SSDP responses (case-insensitive per UPnP spec)
    [GeneratedRegex(@"(?i)location:\s*(.+)", RegexOptions.IgnoreCase)]
    private static partial Regex LocationRegex();

    [GeneratedRegex(@"(?i)usn:\s*(.+)", RegexOptions.IgnoreCase)]
    private static partial Regex UsnRegex();

    [GeneratedRegex(@"uuid:roku:ecp:(.+)", RegexOptions.IgnoreCase)]
    private static partial Regex SerialRegex();
}
