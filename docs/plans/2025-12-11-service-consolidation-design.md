# Service Consolidation & Class Library Design

## Overview

Simplify the RoMoteNoAds codebase by:
1. Extracting Roku protocol logic into a reusable class library (`RoMote.Roku`)
2. Consolidating 5 services down to 2 app services + 1 library service

This enables future reuse (e.g., ASP.NET Core web API for home server automation).

---

## Current State (5 services)

| Service | Lines | Purpose |
|---------|-------|---------|
| `DeviceStorageService` | 163 | Store/retrieve saved devices (JSON in Preferences) |
| `ShortcutService` | 185 | Store/retrieve shortcuts (JSON in Preferences) + execute |
| `RokuControlService` | 243 | Send ECP commands to Roku |
| `RokuDiscoveryService` | 253 | SSDP discovery + fetch device info |
| `WakeOnLanService` | 95 | Send WoL magic packets |

**Problems:**
- Duplicate JSON/Preferences boilerplate in storage services
- WakeOnLanService is tiny (1 method) for a standalone service
- Too many services to reason about

---

## Target State (3 services)

| Layer | Service | Responsibility |
|-------|---------|----------------|
| Library | `IRokuService` | Roku protocol only (discovery, control, WoL) |
| App | `IStorageService` | Device persistence only |
| App | `IShortcutService` | Shortcut persistence + recording + execution |

---

## Class Library: RoMote.Roku

**Project:** `RoMote.Roku` (targeting `net10.0`)

```
src/RoMote.Roku/
├── RoMote.Roku.csproj
├── IRokuService.cs
├── RokuService.cs
└── Models/
    ├── RokuDevice.cs
    ├── RokuChannel.cs
    └── RokuKey.cs
```

### IRokuService Interface

```csharp
public interface IRokuService
{
    // Device state
    RokuDevice? CurrentDevice { get; set; }

    // Discovery
    Task<IEnumerable<RokuDevice>> DiscoverDevicesAsync(TimeSpan? timeout = null, CancellationToken ct = default);
    Task<RokuDevice?> ValidateDeviceAsync(string ipAddress, CancellationToken ct = default);
    Task<RokuDevice?> RefreshDeviceInfoAsync(RokuDevice device, CancellationToken ct = default);
    event EventHandler<RokuDevice>? DeviceDiscovered;

    // Control
    Task<bool> SendKeyPressAsync(string key);
    Task<bool> SendTextAsync(string text);
    Task<bool> LaunchChannelAsync(string channelId);
    Task<IEnumerable<RokuChannel>> GetInstalledChannelsAsync();
    Task<RokuChannel?> GetActiveChannelAsync();
    Task<byte[]?> GetChannelIconAsync(string channelId);

    // Wake-on-LAN
    Task<bool> WakeAsync(string macAddress);

    // Events
    event EventHandler<string>? CommandFailed;
}
```

**Note:** `SendKeyDownAsync` and `SendKeyUpAsync` removed (unused in current codebase).

### Project Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

No external dependencies - uses built-in `System.Net.Http`, `System.Net.Sockets`, `System.Text.Json`, `System.Xml.Linq`.

---

## App Service: IStorageService

Device persistence only (shortcuts handled separately due to recording behavior).

```csharp
public interface IStorageService
{
    // Devices
    Task<IEnumerable<RokuDevice>> GetSavedDevicesAsync();
    Task SaveDeviceAsync(RokuDevice device);
    Task RemoveDeviceAsync(RokuDevice device);
    Task<RokuDevice?> GetLastUsedDeviceAsync();
    Task SetLastUsedDeviceAsync(RokuDevice device);
    Task ClearAllDevicesAsync();
}
```

Implementation uses generic JSON/Preferences helpers to eliminate boilerplate.

---

## App Service: IShortcutService

Handles shortcut persistence, recording, and execution.

```csharp
public interface IShortcutService
{
    // Persistence
    Task<IEnumerable<Shortcut>> GetShortcutsAsync();
    Task SaveShortcutAsync(Shortcut shortcut);
    Task RemoveShortcutAsync(Shortcut shortcut);
    Task ReorderShortcutsAsync(IEnumerable<Shortcut> shortcuts);

    // Recording
    bool IsRecording { get; }
    void StartRecording();
    IReadOnlyList<string> StopRecording();  // Returns captured keys

    // Execution (wraps IRokuService)
    Task<bool> SendKeyPressAsync(string key);  // Captures if recording
    Task<bool> ExecuteShortcutAsync(Shortcut shortcut);
}
```

**Recording flow:**
1. ViewModel calls `StartRecording()`
2. User presses buttons - ViewModel calls `SendKeyPressAsync()` as normal
3. ShortcutService forwards to RokuService AND captures each key
4. ViewModel calls `StopRecording()` to get captured sequence
5. ViewModel calls `SaveShortcutAsync()` with new shortcut

---

## Dependency Graph

```
┌─────────────────────────────────────────────────────────┐
│                      MAUI App                           │
│                                                         │
│  ┌─────────────┐     ┌─────────────────┐               │
│  │ ViewModels  │────▶│ IShortcutService │               │
│  └─────────────┘     └────────┬────────┘               │
│         │                     │                         │
│         │            ┌────────▼────────┐               │
│         └───────────▶│  IRokuService   │◀──────────┐   │
│                      └────────┬────────┘           │   │
│                               │                    │   │
│  ┌─────────────┐              │             ┌──────┴──────┐
│  │ ViewModels  │──────────────┼────────────▶│IStorageService│
│  └─────────────┘              │             └─────────────┘
└───────────────────────────────┼─────────────────────────────┘
                                │
                    ┌───────────▼───────────┐
                    │    RoMote.Roku        │
                    │    (Class Library)    │
                    └───────────────────────┘
```

---

## DI Registration

```csharp
// MauiProgram.cs

// Library service
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<IRokuService, RokuService>();

// App services
builder.Services.AddSingleton<IStorageService, StorageService>();
builder.Services.AddSingleton<IShortcutService, ShortcutService>();
```

---

## File Changes

### New Files

```
src/
├── RoMote.Roku/                         (NEW project)
│   ├── RoMote.Roku.csproj
│   ├── IRokuService.cs
│   ├── RokuService.cs
│   └── Models/
│       ├── RokuDevice.cs                (moved from app)
│       ├── RokuChannel.cs               (moved from app)
│       └── RokuKey.cs                   (moved from app)
```

### Modified Files

```
src/RoMoteNoAds/
├── RoMoteNoAds.csproj                   Add reference to RoMote.Roku
├── MauiProgram.cs                       Update DI registrations
├── Services/
│   ├── IStorageService.cs               NEW - device persistence only
│   ├── StorageService.cs                NEW - consolidated implementation
│   ├── IShortcutService.cs              MODIFY - add recording API
│   └── ShortcutService.cs               MODIFY - add recording + wrap RokuService
├── ViewModels/
│   ├── RemoteViewModel.cs               Change to use IShortcutService for keys
│   ├── DeviceSelectionViewModel.cs      Update service references
│   ├── ChannelsViewModel.cs             Update service references
│   └── ShortcutsViewModel.cs            Update service references
```

### Deleted Files

```
src/RoMoteNoAds/
├── Services/
│   ├── IRokuControlService.cs           (merged into library)
│   ├── RokuControlService.cs            (merged into library)
│   ├── IRokuDiscoveryService.cs         (merged into library)
│   ├── RokuDiscoveryService.cs          (merged into library)
│   ├── IWakeOnLanService.cs             (merged into library)
│   ├── WakeOnLanService.cs              (merged into library)
│   ├── IDeviceStorageService.cs         (replaced by IStorageService)
│   └── DeviceStorageService.cs          (replaced by StorageService)
├── Models/
│   ├── RokuDevice.cs                    (moved to library)
│   ├── RokuChannel.cs                   (moved to library)
│   └── RokuKey.cs                       (moved to library)
```

**Summary:** Delete 11 files, create 6 library files, create 2 app service files, modify 5-6 existing files.

---

## Solution Structure

```
RoMote.sln
├── src/
│   ├── RoMote.Roku/
│   │   └── RoMote.Roku.csproj
│   └── RoMoteNoAds/
│       └── RoMoteNoAds.csproj
```

**Namespace conventions:**

| Project | Namespace |
|---------|-----------|
| RoMote.Roku | `RoMote.Roku`, `RoMote.Roku.Models` |
| RoMoteNoAds | `RoMoteNoAds`, `RoMoteNoAds.Services`, `RoMoteNoAds.Models` |

---

## Implementation Order

### Phase 1: Create the library (no app changes yet)

1. Create `RoMote.Roku` project in `src/`
2. Copy `RokuDevice.cs`, `RokuChannel.cs`, `RokuKey.cs` to library's `Models/`
3. Update namespaces to `RoMote.Roku.Models`
4. Create `IRokuService.cs` interface
5. Create `RokuService.cs` - combine logic from `RokuControlService`, `RokuDiscoveryService`, `WakeOnLanService`
6. Build library - ensure it compiles standalone

### Phase 2: Wire library into app

7. Add project reference from `RoMoteNoAds` → `RoMote.Roku`
8. Update `MauiProgram.cs` - register `IRokuService`
9. Update ViewModels to use `IRokuService` instead of the three old services
10. Delete old services: `RokuControlService`, `RokuDiscoveryService`, `WakeOnLanService` (+ interfaces)
11. Delete old models from app (now in library)
12. Build & test - app should work identically

### Phase 3: Consolidate storage

13. Create `IStorageService.cs` and `StorageService.cs` (devices only)
14. Update `DeviceSelectionViewModel` to use `IStorageService`
15. Delete `DeviceStorageService` (+ interface)
16. Build & test

### Phase 4: Update ShortcutService

17. Update `IShortcutService` - add recording API
18. Update `ShortcutService` - inject `IRokuService`, add recording logic, wrap key presses
19. Update `RemoteViewModel` to use `IShortcutService.SendKeyPressAsync()`
20. Build & test recording flow

### Phase 5: Cleanup

21. Remove any dead code / unused usings
22. Run full app test
23. Commit

**Each phase leaves the app in a working state.**

---

## Future: Web API

The class library enables a future ASP.NET Core web API:

```
src/RoMote.Api/                          (future)
├── RoMote.Api.csproj
├── Controllers/
│   └── RokuController.cs                Calls IRokuService
└── Program.cs                           Register IRokuService
```

Example endpoints:
- `POST /api/roku/keypress/{key}`
- `POST /api/roku/launch/{channelId}`
- `GET /api/roku/devices`
- `POST /api/roku/wake`
