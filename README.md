# RoMoteNoAds

Here's a macos/ios .NET 10 Maui app which works as a roku remote so you can use one on your phone/laptop without stupid ads.

If the dev materials are free, why should the experience to actually use them cost money... That's what leads to stupid 
products like [ai powered toothbrush health monitoring](https://feno.co/) (please do NOT purchase things like that. waste of money)

Anyway if you figure out how to get an installable build of this or if I decide to publish a build, here's a free roku remote app.

## Prerequisites

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

Full disclosure: This was built as a test of a claude clode plugin "[superpowers](https://github.com/obra/superpowers/tree/main)" which I wanted to try. The initial commit was a total of 4 prompts, and ~8 answered as part of its plan mode questioning.

Proof:
![Screenshot 2025-12-09 at 8.46.54 PM.png](Screenshot%202025-12-09%20at%208.46.54%E2%80%AFPM.png)
