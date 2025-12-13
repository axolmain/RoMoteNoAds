# RoMoteNoAds

Here's a macos/ios .NET 10 Maui app which works as a roku remote so you can use one on your phone/laptop without stupid ads.

If the dev materials are free, why should the experience to actually use them cost money... That's what leads to stupid 
products like [ai powered toothbrush health monitoring](https://feno.co/) (please do NOT purchase things like that. waste of money)

## Installation (macOS)

1. Download `RoMoteNoAds-macOS.dmg` from the [Releases](../../releases) page
2. Open the DMG and drag RoMoteNoAds to Applications

### First Launch (Required)

Since this app is not signed with an Apple Developer certificate, macOS will block it:

**Option 1 - Right-Click Open (Easiest):**
1. Right-click RoMoteNoAds in Applications
2. Select "Open"
3. Click "Open" in the dialog

**Option 2 - System Settings:**
1. Try to open the app (it will be blocked)
2. Go to System Settings → Privacy & Security
3. Click "Open Anyway" next to the blocked message

**Option 3 - Terminal:**
```bash
xattr -d com.apple.quarantine /Applications/RoMoteNoAds.app
```

## Building from Source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- macOS with Xcode installed (for iOS/Mac Catalyst builds):
  ```bash
  xcode-select --install
  # Also install Xcode from the App Store, then:
  sudo xcode-select -s /Applications/Xcode.app/Contents/Developer
  sudo xcodebuild -license accept
  ```
- .NET MAUI workload:
  ```bash
  dotnet workload install maui
  ```

## Building

```bash
# Restore dependencies
dotnet restore

# Build and run on Mac
dotnet run --project src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-maccatalyst

# Build for iOS Simulator
dotnet build src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-ios
```

## Usage

1. Make sure your device is on the same network as your Roku
2. Go to the **Devices** tab to discover Roku devices on your network
3. Select a device to control it
4. Use the **Remote** tab for navigation and playback controls
5. Use the **Channels** tab to launch installed apps

## How It Works

### Device Discovery (SSDP)

Roku devices are discovered using SSDP (Simple Service Discovery Protocol), a UPnP discovery mechanism. The app sends a UDP multicast M-SEARCH request to `239.255.255.250:1900` with the search target `roku:ecp`:

```
M-SEARCH * HTTP/1.1
HOST: 239.255.255.250:1900
MAN: "ssdp:discover"
ST: roku:ecp
MX: 3
```

Roku devices respond with their location URL (e.g., `http://192.168.1.100:8060/`) which is used for all subsequent control commands.

### Roku External Control Protocol (ECP)

Once discovered, the app communicates with Roku devices via the [External Control Protocol](https://developer.roku.com/docs/developer-program/dev-tools/external-control-api.md) on port 8060:

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/keypress/{key}` | POST | Send remote button press |
| `/launch/{channelId}` | POST | Launch a channel/app |
| `/query/device-info` | GET | Get device information (XML) |
| `/query/apps` | GET | List installed channels (XML) |
| `/query/active-app` | GET | Get currently running app (XML) |
| `/query/icon/{channelId}` | GET | Get channel icon (PNG) |

### iOS Network Requirements

For SSDP discovery to work on iOS, the app requires:

- **`NSLocalNetworkUsageDescription`** - Prompts user for local network access permission
- **`com.apple.developer.networking.multicast`** - Entitlement for UDP multicast (requires [Apple approval](https://developer.apple.com/contact/request/networking-multicast) for App Store distribution)

Full disclosure: This was built as a test of a claude clode plugin "[superpowers](https://github.com/obra/superpowers/tree/main)" which I wanted to try. The initial commit was a total of 4 prompts, and ~8 answered as part of its plan mode questioning.

Proof:
![Screenshot 2025-12-09 at 8.46.54 PM.png](Screenshot%202025-12-09%20at%208.46.54%E2%80%AFPM.png)
