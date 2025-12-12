using System.Net;
using System.Net.Sockets;

namespace RoMoteNoAds.Services;

/// <summary>
/// Service for sending Wake-on-LAN magic packets.
/// </summary>
public class WakeOnLanService : IWakeOnLanService
{
    private const int WolPort = 9;

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

            // Send to broadcast address
            var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, WolPort);
            await udpClient.SendAsync(magicPacket, magicPacket.Length, broadcastEndpoint);

            // Also try sending to the subnet broadcast (255.255.255.255)
            var globalBroadcast = new IPEndPoint(IPAddress.Parse("255.255.255.255"), WolPort);
            await udpClient.SendAsync(magicPacket, magicPacket.Length, globalBroadcast);

            System.Diagnostics.Debug.WriteLine($"[WoL] Magic packet sent to {macAddress}");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WoL] Failed to send magic packet: {ex.Message}");
            return false;
        }
    }

    private static byte[]? ParseMacAddress(string macAddress)
    {
        try
        {
            // Support formats: XX:XX:XX:XX:XX:XX, XX-XX-XX-XX-XX-XX, XXXXXXXXXXXX
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
        // Magic packet: 6 bytes of 0xFF followed by MAC address repeated 16 times
        var packet = new byte[6 + (6 * 16)];

        // First 6 bytes are 0xFF
        for (int i = 0; i < 6; i++)
        {
            packet[i] = 0xFF;
        }

        // Repeat MAC address 16 times
        for (int i = 0; i < 16; i++)
        {
            Array.Copy(macBytes, 0, packet, 6 + (i * 6), 6);
        }

        return packet;
    }
}
