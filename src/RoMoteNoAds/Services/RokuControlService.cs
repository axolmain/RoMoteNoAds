using System.Xml.Linq;
using RoMoteNoAds.Models;

namespace RoMoteNoAds.Services;

/// <summary>
/// Service for controlling a Roku device via ECP REST API.
/// </summary>
public class RokuControlService : IRokuControlService
{
    private readonly HttpClient _httpClient;

    public RokuDevice? CurrentDevice { get; set; }

    public event EventHandler<string>? CommandFailed;

    public RokuControlService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
    }

    public async Task<bool> SendKeyPressAsync(string key)
    {
        return await SendPostCommandAsync($"keypress/{key}");
    }

    public async Task<bool> SendKeyDownAsync(string key)
    {
        return await SendPostCommandAsync($"keydown/{key}");
    }

    public async Task<bool> SendKeyUpAsync(string key)
    {
        return await SendPostCommandAsync($"keyup/{key}");
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

            // Small delay between characters to avoid overwhelming the device
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

    public async Task<string?> GetMediaPlayerStateAsync()
    {
        try
        {
            if (CurrentDevice == null)
                return null;

            var url = $"{CurrentDevice.BaseUrl}/query/media-player";
            return await _httpClient.GetStringAsync(url);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching media player state: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine("[RokuControl] Command failed: No device connected");
                OnCommandFailed("No device connected");
                return false;
            }

            url = $"{CurrentDevice.BaseUrl}/{command}";
            System.Diagnostics.Debug.WriteLine($"[RokuControl] Sending command: POST {url}");

            var response = await _httpClient.PostAsync(url, null);

            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                var reasonPhrase = response.ReasonPhrase ?? "Unknown";
                var responseBody = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[RokuControl] Command failed:");
                System.Diagnostics.Debug.WriteLine($"  URL: {url}");
                System.Diagnostics.Debug.WriteLine($"  Status: {statusCode} {reasonPhrase}");
                System.Diagnostics.Debug.WriteLine($"  Response body: {responseBody}");
                System.Diagnostics.Debug.WriteLine($"  Response headers:");
                foreach (var header in response.Headers)
                {
                    System.Diagnostics.Debug.WriteLine($"    {header.Key}: {string.Join(", ", header.Value)}");
                }

                OnCommandFailed($"Command failed: {response.StatusCode}");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"[RokuControl] Command succeeded: {command}");
            return true;
        }
        catch (TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine($"[RokuControl] Command timed out: {url ?? command}");
            OnCommandFailed("Command timed out");
            return false;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RokuControl] Network error for {url ?? command}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[RokuControl] Inner exception: {ex.InnerException?.Message}");
            OnCommandFailed($"Network error: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RokuControl] Unexpected error for {url ?? command}: {ex.GetType().Name} - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[RokuControl] Stack trace: {ex.StackTrace}");
            OnCommandFailed($"Error: {ex.Message}");
            return false;
        }
    }

    private void OnCommandFailed(string message)
    {
        CommandFailed?.Invoke(this, message);
    }
}
