# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RoMoteNoAds is a .NET MAUI application that serves as a remote control for Roku devices. It targets iOS and macOS (Mac Catalyst) platforms and communicates with Roku devices using the Roku External Control Protocol (ECP) over HTTP.

## Build Commands

```bash
# Restore dependencies
dotnet restore

# Build for Mac Catalyst (development)
dotnet build src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-maccatalyst

# Build for iOS Simulator
dotnet build src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-ios

# Run on Mac Catalyst
dotnet run --project src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-maccatalyst
```

## Architecture

### MVVM Pattern
The app follows the MVVM pattern using CommunityToolkit.Mvvm:
- **Models** (`Models/`): Data classes like `RokuDevice`, `RokuChannel`, `RokuKey`
- **ViewModels** (`ViewModels/`): Inherit from `BaseViewModel`, use `[ObservableProperty]` and `[RelayCommand]` source generators
- **Views** (`Views/`): XAML pages with code-behind, bound to ViewModels via `x:DataType`

### Dependency Injection
All services, ViewModels, and Views are registered as singletons in `MauiProgram.cs`. Services are injected via constructors.

### Core Services
- **IRokuDiscoveryService**: Discovers Roku devices on the local network using SSDP (Simple Service Discovery Protocol). Sends M-SEARCH requests to `239.255.255.250:1900` and parses responses.
- **IRokuControlService**: Sends ECP commands to control Roku devices. All commands are HTTP POST requests to `http://{ip}:8060/{command}`. Maintains `CurrentDevice` state.
- **IDeviceStorageService**: Persists saved device list using MAUI Preferences.

### Roku ECP API
The app communicates with Roku devices on port 8060:
- `POST /keypress/{key}` - Send remote button press (keys defined in `RokuKey.cs`)
- `POST /launch/{channelId}` - Launch a channel/app
- `GET /query/device-info` - Get device information (XML)
- `GET /query/apps` - List installed channels (XML)
- `GET /query/active-app` - Get currently running app (XML)
- `GET /query/icon/{channelId}` - Get channel icon (PNG)

### Navigation
Uses Shell navigation with a TabBar containing three tabs:
1. **Remote** - D-pad, playback controls, volume, power, keyboard input
2. **Channels** - Grid of installed apps with icons
3. **Devices** - Device discovery and selection

## Key Implementation Details

- `RokuDevice.BaseUrl` computes `http://{IpAddress}:{Port}` for API calls
- `RokuKey.Literal(char)` URL-encodes characters for keyboard input as `Lit_{encoded}`
- Device discovery uses UDP multicast with 5-second timeout
- Commands use 5-second HTTP timeout
- Models use `ObservableObject` for property change notifications
