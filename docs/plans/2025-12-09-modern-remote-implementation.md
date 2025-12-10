# Modern Remote Redesign - Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Redesign the remote control interface with a sleek glass/floating aesthetic and add custom shortcut functionality.

**Architecture:** The remote gets a visual refresh with frosted glass styling while keeping the existing MVVM pattern. Shortcuts are stored as JSON in Preferences (matching DeviceStorageService pattern), with platform-adaptive UI (swipe-up panel on iOS, dedicated tab on Mac).

**Tech Stack:** .NET MAUI, CommunityToolkit.Mvvm, CommunityToolkit.Maui, System.Text.Json

**Working Directory:** `/Users/sebastiandunn/Documents/Coding/rokuApp/.worktrees/modern-remote`

---

## Phase 1: Visual Refresh - Glass Styling

### Task 1.1: Add Frost/Glass Colors

**Files:**
- Modify: `src/RoMoteNoAds/Resources/Styles/Colors.xaml`

**Step 1: Add glass overlay colors**

Add these colors after the Dark Theme section (line 41):

```xml
<!-- Glass/Frost Effects -->
<Color x:Key="FrostLight">#E8FFFFFF</Color>
<Color x:Key="FrostDark">#E82D2D2D</Color>
<Color x:Key="FrostBorderLight">#40000000</Color>
<Color x:Key="FrostBorderDark">#40FFFFFF</Color>
```

**Step 2: Verify build**

Run: `dotnet build src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-maccatalyst --no-restore`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add src/RoMoteNoAds/Resources/Styles/Colors.xaml
git commit -m "feat: add glass/frost color palette"
```

---

### Task 1.2: Add Glass Button Styles

**Files:**
- Modify: `src/RoMoteNoAds/Resources/Styles/Styles.xaml`

**Step 1: Add glass button styles**

Add after the Entry Style section (after line 100):

```xml
<!-- Glass Remote Card -->
<Style x:Key="GlassCard" TargetType="Frame">
    <Setter Property="BackgroundColor" Value="{AppThemeBinding Light={StaticResource FrostLight}, Dark={StaticResource FrostDark}}" />
    <Setter Property="CornerRadius" Value="24" />
    <Setter Property="Padding" Value="20" />
    <Setter Property="HasShadow" Value="True" />
    <Setter Property="BorderColor" Value="{AppThemeBinding Light={StaticResource FrostBorderLight}, Dark={StaticResource FrostBorderDark}}" />
</Style>

<!-- Glass Button Base -->
<Style x:Key="GlassButton" TargetType="Button">
    <Setter Property="BackgroundColor" Value="{AppThemeBinding Light=#20000000, Dark=#20FFFFFF}" />
    <Setter Property="TextColor" Value="{AppThemeBinding Light={StaticResource TextPrimaryLight}, Dark={StaticResource TextPrimaryDark}}" />
    <Setter Property="FontSize" Value="20" />
    <Setter Property="CornerRadius" Value="12" />
    <Setter Property="HeightRequest" Value="56" />
    <Setter Property="WidthRequest" Value="56" />
    <Setter Property="Padding" Value="0" />
    <Setter Property="BorderColor" Value="Transparent" />
</Style>

<!-- Power Button -->
<Style x:Key="PowerButton" TargetType="Button" BasedOn="{StaticResource GlassButton}">
    <Setter Property="BackgroundColor" Value="#40E74C3C" />
    <Setter Property="TextColor" Value="{StaticResource Error}" />
    <Setter Property="HeightRequest" Value="44" />
    <Setter Property="WidthRequest" Value="44" />
    <Setter Property="CornerRadius" Value="22" />
</Style>

<!-- Volume Pill Container (apply to Frame) -->
<Style x:Key="VolumePillFrame" TargetType="Frame">
    <Setter Property="BackgroundColor" Value="{AppThemeBinding Light=#20000000, Dark=#20FFFFFF}" />
    <Setter Property="CornerRadius" Value="25" />
    <Setter Property="Padding" Value="8,12" />
    <Setter Property="HasShadow" Value="False" />
    <Setter Property="BorderColor" Value="Transparent" />
    <Setter Property="WidthRequest" Value="50" />
</Style>

<!-- Volume Button (inside pill) -->
<Style x:Key="GlassVolumeButton" TargetType="Button">
    <Setter Property="BackgroundColor" Value="Transparent" />
    <Setter Property="TextColor" Value="{AppThemeBinding Light={StaticResource TextPrimaryLight}, Dark={StaticResource TextPrimaryDark}}" />
    <Setter Property="FontSize" Value="18" />
    <Setter Property="HeightRequest" Value="44" />
    <Setter Property="WidthRequest" Value="44" />
    <Setter Property="CornerRadius" Value="22" />
    <Setter Property="Padding" Value="0" />
</Style>

<!-- Playback Button -->
<Style x:Key="GlassPlaybackButton" TargetType="Button" BasedOn="{StaticResource GlassButton}">
    <Setter Property="HeightRequest" Value="48" />
    <Setter Property="WidthRequest" Value="48" />
    <Setter Property="CornerRadius" Value="24" />
    <Setter Property="FontSize" Value="22" />
</Style>

<!-- Action Row Button -->
<Style x:Key="GlassActionButton" TargetType="Button" BasedOn="{StaticResource GlassButton}">
    <Setter Property="HeightRequest" Value="48" />
    <Setter Property="WidthRequest" Value="48" />
    <Setter Property="CornerRadius" Value="24" />
    <Setter Property="FontSize" Value="18" />
</Style>

<!-- Utility Button (full width pill) -->
<Style x:Key="GlassUtilityButton" TargetType="Button">
    <Setter Property="BackgroundColor" Value="{AppThemeBinding Light=#15000000, Dark=#15FFFFFF}" />
    <Setter Property="TextColor" Value="{AppThemeBinding Light={StaticResource TextPrimaryLight}, Dark={StaticResource TextPrimaryDark}}" />
    <Setter Property="FontSize" Value="14" />
    <Setter Property="CornerRadius" Value="20" />
    <Setter Property="HeightRequest" Value="40" />
    <Setter Property="Padding" Value="20,0" />
</Style>

<!-- D-Pad Glass Direction Button -->
<Style x:Key="GlassDPadButton" TargetType="Button" BasedOn="{StaticResource GlassButton}">
    <Setter Property="HeightRequest" Value="56" />
    <Setter Property="WidthRequest" Value="56" />
    <Setter Property="FontSize" Value="24" />
</Style>

<!-- D-Pad Glass Center Button -->
<Style x:Key="GlassDPadCenterButton" TargetType="Button">
    <Setter Property="BackgroundColor" Value="{StaticResource Primary}" />
    <Setter Property="TextColor" Value="{StaticResource White}" />
    <Setter Property="FontSize" Value="16" />
    <Setter Property="FontAttributes" Value="Bold" />
    <Setter Property="CornerRadius" Value="35" />
    <Setter Property="HeightRequest" Value="70" />
    <Setter Property="WidthRequest" Value="70" />
</Style>
```

**Step 2: Verify build**

Run: `dotnet build src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-maccatalyst --no-restore`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add src/RoMoteNoAds/Resources/Styles/Styles.xaml
git commit -m "feat: add glass button styles for modern remote"
```

---

### Task 1.3: Redesign RemotePage.xaml with Glass Layout

**Files:**
- Modify: `src/RoMoteNoAds/Views/RemotePage.xaml`

**Step 1: Replace entire RemotePage.xaml content**

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:RoMoteNoAds.ViewModels"
             xmlns:toolkit="http://schemas.microsoft.com/dotnet/2022/maui/toolkit"
             x:Class="RoMoteNoAds.Views.RemotePage"
             x:DataType="vm:RemoteViewModel"
             Title="Remote">

    <Grid RowDefinitions="Auto,*" Padding="16">
        <!-- Error Banner -->
        <Border Grid.Row="0"
                BackgroundColor="{StaticResource Error}"
                Padding="12,8"
                StrokeShape="RoundRectangle 8"
                IsVisible="{Binding ErrorMessage, Converter={toolkit:IsStringNotNullOrEmptyConverter}}">
            <Label Text="{Binding ErrorMessage}"
                   TextColor="White"
                   FontSize="12"
                   HorizontalOptions="Center" />
        </Border>

        <!-- Main Remote Layout -->
        <Grid Grid.Row="1" Padding="0,8,0,0">
            <!-- Glass Remote Card (centered) -->
            <Frame Style="{StaticResource GlassCard}"
                   HorizontalOptions="Center"
                   VerticalOptions="Start"
                   MaximumWidthRequest="340">
                <VerticalStackLayout Spacing="20">

                    <!-- Top Bar: Power + Device Name -->
                    <Grid ColumnDefinitions="Auto,*">
                        <!-- Power Button -->
                        <Button Grid.Column="0"
                                Text="⏻"
                                Command="{Binding PowerToggleCommand}"
                                Style="{StaticResource PowerButton}"
                                IsVisible="{Binding SupportsPower}" />

                        <!-- Device Name -->
                        <Label Grid.Column="1"
                               Text="{Binding CurrentDevice.DisplayName, FallbackValue='No Device'}"
                               Style="{StaticResource SubtitleLabel}"
                               VerticalOptions="Center"
                               HorizontalOptions="End"
                               MaxLines="1"
                               LineBreakMode="TailTruncation" />
                    </Grid>

                    <!-- D-Pad with Volume Strip -->
                    <Grid ColumnDefinitions="*,Auto" ColumnSpacing="16">
                        <!-- D-Pad -->
                        <Grid Grid.Column="0"
                              RowDefinitions="56,70,56"
                              ColumnDefinitions="56,70,56"
                              RowSpacing="8"
                              ColumnSpacing="8"
                              HorizontalOptions="Center">

                            <!-- Up -->
                            <Button Grid.Row="0" Grid.Column="1"
                                    Text="▲"
                                    Command="{Binding PressUpCommand}"
                                    Style="{StaticResource GlassDPadButton}" />

                            <!-- Left -->
                            <Button Grid.Row="1" Grid.Column="0"
                                    Text="◀"
                                    Command="{Binding PressLeftCommand}"
                                    Style="{StaticResource GlassDPadButton}" />

                            <!-- OK/Select -->
                            <Button Grid.Row="1" Grid.Column="1"
                                    Text="OK"
                                    Command="{Binding PressSelectCommand}"
                                    Style="{StaticResource GlassDPadCenterButton}" />

                            <!-- Right -->
                            <Button Grid.Row="1" Grid.Column="2"
                                    Text="▶"
                                    Command="{Binding PressRightCommand}"
                                    Style="{StaticResource GlassDPadButton}" />

                            <!-- Down -->
                            <Button Grid.Row="2" Grid.Column="1"
                                    Text="▼"
                                    Command="{Binding PressDownCommand}"
                                    Style="{StaticResource GlassDPadButton}" />
                        </Grid>

                        <!-- Volume Strip (right edge) -->
                        <Frame Grid.Column="1"
                               Style="{StaticResource VolumePillFrame}"
                               IsVisible="{Binding SupportsVolume}"
                               VerticalOptions="Center">
                            <VerticalStackLayout Spacing="4">
                                <Button Text="+"
                                        Command="{Binding VolumeUpCommand}"
                                        Style="{StaticResource GlassVolumeButton}" />
                                <Button Text="🔇"
                                        Command="{Binding VolumeMuteCommand}"
                                        Style="{StaticResource GlassVolumeButton}"
                                        FontSize="14" />
                                <Button Text="−"
                                        Command="{Binding VolumeDownCommand}"
                                        Style="{StaticResource GlassVolumeButton}" />
                            </VerticalStackLayout>
                        </Frame>
                    </Grid>

                    <!-- Playback Controls -->
                    <HorizontalStackLayout Spacing="20" HorizontalOptions="Center">
                        <Button Text="⏪"
                                Command="{Binding PressRewindCommand}"
                                Style="{StaticResource GlassPlaybackButton}" />
                        <Button Text="▶❚❚"
                                Command="{Binding PressPlayCommand}"
                                Style="{StaticResource GlassPlaybackButton}"
                                BackgroundColor="{StaticResource Primary}"
                                TextColor="White"
                                WidthRequest="64"
                                FontSize="14" />
                        <Button Text="⏩"
                                Command="{Binding PressFastForwardCommand}"
                                Style="{StaticResource GlassPlaybackButton}" />
                    </HorizontalStackLayout>

                    <!-- Action Row: Home, Back, Star, Search -->
                    <HorizontalStackLayout Spacing="16" HorizontalOptions="Center">
                        <Button Text="🏠"
                                Command="{Binding PressHomeCommand}"
                                Style="{StaticResource GlassActionButton}" />
                        <Button Text="←"
                                Command="{Binding PressBackCommand}"
                                Style="{StaticResource GlassActionButton}"
                                FontSize="22" />
                        <Button Text="★"
                                Command="{Binding PressInfoCommand}"
                                Style="{StaticResource GlassActionButton}" />
                        <Button Text="🔍"
                                Command="{Binding PressSearchCommand}"
                                Style="{StaticResource GlassActionButton}" />
                    </HorizontalStackLayout>

                    <!-- Utility Row -->
                    <Grid ColumnDefinitions="*,*" ColumnSpacing="12">
                        <Button Grid.Column="0"
                                Text="↺ Replay"
                                Command="{Binding PressReplayCommand}"
                                Style="{StaticResource GlassUtilityButton}" />
                        <Button Grid.Column="1"
                                Text="⌨ Keyboard"
                                Command="{Binding ShowKeyboardCommand}"
                                Style="{StaticResource GlassUtilityButton}" />
                    </Grid>

                </VerticalStackLayout>
            </Frame>
        </Grid>

        <!-- Keyboard Overlay -->
        <Grid Grid.RowSpan="2"
              IsVisible="{Binding IsKeyboardVisible}"
              BackgroundColor="{AppThemeBinding Light=#80FFFFFF, Dark=#80000000}">
            <Frame Style="{StaticResource GlassCard}"
                   VerticalOptions="Center"
                   HorizontalOptions="Center"
                   WidthRequest="320"
                   Padding="20">
                <VerticalStackLayout Spacing="16">
                    <Label Text="Enter Text"
                           Style="{StaticResource TitleLabel}"
                           HorizontalOptions="Center" />

                    <Entry Placeholder="Type here..."
                           Text="{Binding KeyboardText}"
                           ReturnCommand="{Binding SendKeyboardTextCommand}" />

                    <Grid ColumnDefinitions="*,*,*" ColumnSpacing="8">
                        <Button Grid.Column="0"
                                Text="⌫"
                                Command="{Binding SendBackspaceCommand}"
                                Style="{StaticResource GlassUtilityButton}" />
                        <Button Grid.Column="1"
                                Text="Send"
                                Command="{Binding SendKeyboardTextCommand}"
                                Style="{StaticResource PrimaryButton}"
                                HeightRequest="40" />
                        <Button Grid.Column="2"
                                Text="Cancel"
                                Command="{Binding HideKeyboardCommand}"
                                Style="{StaticResource GlassUtilityButton}" />
                    </Grid>
                </VerticalStackLayout>
            </Frame>
        </Grid>
    </Grid>
</ContentPage>
```

**Step 2: Verify build**

Run: `dotnet build src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-maccatalyst --no-restore`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add src/RoMoteNoAds/Views/RemotePage.xaml
git commit -m "feat: redesign remote with glass aesthetic and new layout"
```

---

## Phase 2: Shortcut System - Models and Service

### Task 2.1: Create Shortcut Models

**Files:**
- Create: `src/RoMoteNoAds/Models/ShortcutType.cs`
- Create: `src/RoMoteNoAds/Models/Shortcut.cs`

**Step 1: Create ShortcutType enum**

Create file `src/RoMoteNoAds/Models/ShortcutType.cs`:

```csharp
namespace RoMoteNoAds.Models;

/// <summary>
/// Types of shortcuts that can be created.
/// </summary>
public enum ShortcutType
{
    /// <summary>
    /// Launches a channel, optionally with a deep link.
    /// </summary>
    Channel,

    /// <summary>
    /// Executes a recorded sequence of key presses.
    /// </summary>
    KeySequence
}
```

**Step 2: Create Shortcut model**

Create file `src/RoMoteNoAds/Models/Shortcut.cs`:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace RoMoteNoAds.Models;

/// <summary>
/// Represents a user-created shortcut for quick actions.
/// </summary>
public partial class Shortcut : ObservableObject
{
    /// <summary>
    /// Unique identifier for the shortcut.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User-defined name for the shortcut.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL to the icon (for channel shortcuts).
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// Type of shortcut (Channel or KeySequence).
    /// </summary>
    public ShortcutType Type { get; set; }

    /// <summary>
    /// Channel ID (for Channel type shortcuts).
    /// </summary>
    public string? ChannelId { get; set; }

    /// <summary>
    /// Deep link URL (for Channel type shortcuts).
    /// </summary>
    public string? DeepLink { get; set; }

    /// <summary>
    /// Recorded key sequence (for KeySequence type shortcuts).
    /// </summary>
    public List<string>? KeySequence { get; set; }

    /// <summary>
    /// Sort order for display.
    /// </summary>
    public int SortOrder { get; set; }
}
```

**Step 3: Verify build**

Run: `dotnet build src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-maccatalyst --no-restore`
Expected: Build succeeded

**Step 4: Commit**

```bash
git add src/RoMoteNoAds/Models/ShortcutType.cs src/RoMoteNoAds/Models/Shortcut.cs
git commit -m "feat: add Shortcut model and ShortcutType enum"
```

---

### Task 2.2: Create Shortcut Service Interface

**Files:**
- Create: `src/RoMoteNoAds/Services/IShortcutService.cs`

**Step 1: Create interface**

Create file `src/RoMoteNoAds/Services/IShortcutService.cs`:

```csharp
using RoMoteNoAds.Models;

namespace RoMoteNoAds.Services;

/// <summary>
/// Service for managing and executing custom shortcuts.
/// </summary>
public interface IShortcutService
{
    /// <summary>
    /// Gets all saved shortcuts.
    /// </summary>
    Task<IEnumerable<Shortcut>> GetShortcutsAsync();

    /// <summary>
    /// Saves a shortcut (creates or updates).
    /// </summary>
    Task SaveShortcutAsync(Shortcut shortcut);

    /// <summary>
    /// Removes a shortcut.
    /// </summary>
    Task RemoveShortcutAsync(Shortcut shortcut);

    /// <summary>
    /// Executes a shortcut (launches channel or plays key sequence).
    /// </summary>
    Task<bool> ExecuteShortcutAsync(Shortcut shortcut);

    /// <summary>
    /// Reorders shortcuts.
    /// </summary>
    Task ReorderShortcutsAsync(IEnumerable<Shortcut> shortcuts);
}
```

**Step 2: Verify build**

Run: `dotnet build src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-maccatalyst --no-restore`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add src/RoMoteNoAds/Services/IShortcutService.cs
git commit -m "feat: add IShortcutService interface"
```

---

### Task 2.3: Implement Shortcut Service

**Files:**
- Create: `src/RoMoteNoAds/Services/ShortcutService.cs`

**Step 1: Create implementation**

Create file `src/RoMoteNoAds/Services/ShortcutService.cs`:

```csharp
using System.Text.Json;
using RoMoteNoAds.Models;

namespace RoMoteNoAds.Services;

/// <summary>
/// Service for managing and executing custom shortcuts.
/// </summary>
public class ShortcutService : IShortcutService
{
    private const string ShortcutsKey = "saved_shortcuts";
    private const int KeySequenceDelayMs = 100;

    private readonly IRokuControlService _controlService;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ShortcutService(IRokuControlService controlService)
    {
        _controlService = controlService;
    }

    public async Task<IEnumerable<Shortcut>> GetShortcutsAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                var json = Preferences.Get(ShortcutsKey, string.Empty);
                if (string.IsNullOrEmpty(json))
                    return Enumerable.Empty<Shortcut>();

                var shortcuts = JsonSerializer.Deserialize<List<Shortcut>>(json, _jsonOptions);
                return shortcuts?.OrderBy(s => s.SortOrder) ?? Enumerable.Empty<Shortcut>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading shortcuts: {ex.Message}");
                return Enumerable.Empty<Shortcut>();
            }
        });
    }

    public async Task SaveShortcutAsync(Shortcut shortcut)
    {
        await Task.Run(() =>
        {
            try
            {
                var shortcuts = GetShortcutsSync();

                var existing = shortcuts.FirstOrDefault(s => s.Id == shortcut.Id);
                if (existing != null)
                {
                    shortcuts.Remove(existing);
                }

                if (shortcut.SortOrder == 0)
                {
                    shortcut.SortOrder = shortcuts.Count > 0
                        ? shortcuts.Max(s => s.SortOrder) + 1
                        : 1;
                }

                shortcuts.Add(shortcut);
                SaveShortcutsSync(shortcuts);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving shortcut: {ex.Message}");
            }
        });
    }

    public async Task RemoveShortcutAsync(Shortcut shortcut)
    {
        await Task.Run(() =>
        {
            try
            {
                var shortcuts = GetShortcutsSync();
                var toRemove = shortcuts.FirstOrDefault(s => s.Id == shortcut.Id);

                if (toRemove != null)
                {
                    shortcuts.Remove(toRemove);
                    SaveShortcutsSync(shortcuts);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing shortcut: {ex.Message}");
            }
        });
    }

    public async Task<bool> ExecuteShortcutAsync(Shortcut shortcut)
    {
        try
        {
            return shortcut.Type switch
            {
                ShortcutType.Channel => await ExecuteChannelShortcutAsync(shortcut),
                ShortcutType.KeySequence => await ExecuteKeySequenceAsync(shortcut),
                _ => false
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error executing shortcut: {ex.Message}");
            return false;
        }
    }

    public async Task ReorderShortcutsAsync(IEnumerable<Shortcut> shortcuts)
    {
        await Task.Run(() =>
        {
            try
            {
                var list = shortcuts.ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].SortOrder = i + 1;
                }
                SaveShortcutsSync(list);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reordering shortcuts: {ex.Message}");
            }
        });
    }

    private async Task<bool> ExecuteChannelShortcutAsync(Shortcut shortcut)
    {
        if (string.IsNullOrEmpty(shortcut.ChannelId))
            return false;

        return await _controlService.LaunchChannelAsync(shortcut.ChannelId, shortcut.DeepLink);
    }

    private async Task<bool> ExecuteKeySequenceAsync(Shortcut shortcut)
    {
        if (shortcut.KeySequence == null || shortcut.KeySequence.Count == 0)
            return false;

        foreach (var key in shortcut.KeySequence)
        {
            var success = await _controlService.SendKeyPressAsync(key);
            if (!success)
                return false;

            await Task.Delay(KeySequenceDelayMs);
        }

        return true;
    }

    private List<Shortcut> GetShortcutsSync()
    {
        try
        {
            var json = Preferences.Get(ShortcutsKey, string.Empty);
            if (string.IsNullOrEmpty(json))
                return new List<Shortcut>();

            return JsonSerializer.Deserialize<List<Shortcut>>(json, _jsonOptions)
                   ?? new List<Shortcut>();
        }
        catch
        {
            return new List<Shortcut>();
        }
    }

    private void SaveShortcutsSync(List<Shortcut> shortcuts)
    {
        var json = JsonSerializer.Serialize(shortcuts, _jsonOptions);
        Preferences.Set(ShortcutsKey, json);
    }
}
```

**Step 2: Verify build**

Run: `dotnet build src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-maccatalyst --no-restore`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add src/RoMoteNoAds/Services/ShortcutService.cs
git commit -m "feat: implement ShortcutService with JSON persistence"
```

---

### Task 2.4: Register Shortcut Service

**Files:**
- Modify: `src/RoMoteNoAds/MauiProgram.cs`

**Step 1: Add service registration**

After line 27 (`builder.Services.AddSingleton<IRokuControlService, RokuControlService>();`), add:

```csharp
builder.Services.AddSingleton<IShortcutService, ShortcutService>();
```

**Step 2: Verify build**

Run: `dotnet build src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-maccatalyst --no-restore`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add src/RoMoteNoAds/MauiProgram.cs
git commit -m "feat: register ShortcutService in DI container"
```

---

## Phase 3: Shortcuts UI - ViewModel and Views

### Task 3.1: Create ShortcutsViewModel

**Files:**
- Create: `src/RoMoteNoAds/ViewModels/ShortcutsViewModel.cs`

**Step 1: Create ViewModel**

Create file `src/RoMoteNoAds/ViewModels/ShortcutsViewModel.cs`:

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoMoteNoAds.Models;
using RoMoteNoAds.Services;

namespace RoMoteNoAds.ViewModels;

/// <summary>
/// ViewModel for managing shortcuts.
/// </summary>
public partial class ShortcutsViewModel : BaseViewModel
{
    private readonly IShortcutService _shortcutService;
    private readonly IRokuControlService _controlService;

    [ObservableProperty]
    private ObservableCollection<Shortcut> _shortcuts = new();

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isRecording;

    [ObservableProperty]
    private List<string> _recordedKeys = new();

    public ShortcutsViewModel(
        IShortcutService shortcutService,
        IRokuControlService controlService)
    {
        _shortcutService = shortcutService;
        _controlService = controlService;
        Title = "Shortcuts";
    }

    public async Task InitializeAsync()
    {
        await LoadShortcutsAsync();
    }

    [RelayCommand]
    private async Task LoadShortcutsAsync()
    {
        try
        {
            IsBusy = true;
            var shortcuts = await _shortcutService.GetShortcutsAsync();
            Shortcuts = new ObservableCollection<Shortcut>(shortcuts);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load shortcuts: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExecuteShortcutAsync(Shortcut? shortcut)
    {
        if (shortcut == null || IsEditing)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var success = await _shortcutService.ExecuteShortcutAsync(shortcut);
            if (!success)
            {
                SetError("Failed to execute shortcut");
            }
        }
        catch (Exception ex)
        {
            SetError($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteShortcutAsync(Shortcut? shortcut)
    {
        if (shortcut == null)
            return;

        try
        {
            await _shortcutService.RemoveShortcutAsync(shortcut);
            Shortcuts.Remove(shortcut);
        }
        catch (Exception ex)
        {
            SetError($"Failed to delete: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ToggleEditing()
    {
        IsEditing = !IsEditing;
    }

    [RelayCommand]
    private void StartRecording()
    {
        RecordedKeys = new List<string>();
        IsRecording = true;
    }

    [RelayCommand]
    private void StopRecording()
    {
        IsRecording = false;
    }

    public void RecordKey(string key)
    {
        if (IsRecording)
        {
            RecordedKeys.Add(key);
        }
    }
}
```

**Step 2: Verify build**

Run: `dotnet build src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-maccatalyst --no-restore`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add src/RoMoteNoAds/ViewModels/ShortcutsViewModel.cs
git commit -m "feat: add ShortcutsViewModel for managing shortcuts"
```

---

### Task 3.2: Create ShortcutsPage for Mac

**Files:**
- Create: `src/RoMoteNoAds/Views/ShortcutsPage.xaml`
- Create: `src/RoMoteNoAds/Views/ShortcutsPage.xaml.cs`

**Step 1: Create XAML file**

Create file `src/RoMoteNoAds/Views/ShortcutsPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:RoMoteNoAds.ViewModels"
             xmlns:models="clr-namespace:RoMoteNoAds.Models"
             xmlns:toolkit="http://schemas.microsoft.com/dotnet/2022/maui/toolkit"
             x:Class="RoMoteNoAds.Views.ShortcutsPage"
             x:DataType="vm:ShortcutsViewModel"
             Title="Shortcuts">

    <Grid RowDefinitions="Auto,*,Auto" Padding="16">
        <!-- Header -->
        <HorizontalStackLayout Grid.Row="0" Spacing="12">
            <Label Text="Shortcuts"
                   Style="{StaticResource TitleLabel}"
                   VerticalOptions="Center" />
            <Button Text="{Binding IsEditing, Converter={toolkit:BoolToObjectConverter TrueObject='Done', FalseObject='Edit'}}"
                    Command="{Binding ToggleEditingCommand}"
                    Style="{StaticResource GlassUtilityButton}"
                    WidthRequest="80" />
        </HorizontalStackLayout>

        <!-- Shortcuts Grid -->
        <CollectionView Grid.Row="1"
                       ItemsSource="{Binding Shortcuts}"
                       Margin="0,16,0,0">
            <CollectionView.EmptyView>
                <VerticalStackLayout VerticalOptions="Center"
                                    HorizontalOptions="Center"
                                    Spacing="16">
                    <Label Text="No Shortcuts"
                           Style="{StaticResource TitleLabel}"
                           HorizontalOptions="Center" />
                    <Label Text="Add shortcuts for quick access to channels and actions"
                           Style="{StaticResource SubtitleLabel}"
                           HorizontalOptions="Center"
                           HorizontalTextAlignment="Center" />
                </VerticalStackLayout>
            </CollectionView.EmptyView>

            <CollectionView.ItemsLayout>
                <GridItemsLayout Orientation="Vertical"
                                Span="3"
                                HorizontalItemSpacing="12"
                                VerticalItemSpacing="12" />
            </CollectionView.ItemsLayout>

            <CollectionView.ItemTemplate>
                <DataTemplate x:DataType="models:Shortcut">
                    <Frame Style="{StaticResource GlassCard}"
                           Padding="12"
                           HeightRequest="100">
                        <Frame.GestureRecognizers>
                            <TapGestureRecognizer
                                Command="{Binding Source={RelativeSource AncestorType={x:Type vm:ShortcutsViewModel}}, Path=ExecuteShortcutCommand}"
                                CommandParameter="{Binding .}" />
                        </Frame.GestureRecognizers>

                        <Grid RowDefinitions="*,Auto">
                            <!-- Icon or Type Indicator -->
                            <Label Grid.Row="0"
                                   Text="{Binding Type, Converter={toolkit:BoolToObjectConverter TrueObject='🎬', FalseObject='⌨'}}"
                                   FontSize="32"
                                   HorizontalOptions="Center"
                                   VerticalOptions="Center" />

                            <!-- Name -->
                            <Label Grid.Row="1"
                                   Text="{Binding Name}"
                                   Style="{StaticResource BodyLabel}"
                                   HorizontalOptions="Center"
                                   MaxLines="1"
                                   LineBreakMode="TailTruncation" />

                            <!-- Delete Button (visible in edit mode) -->
                            <Button Grid.Row="0"
                                    Text="✕"
                                    Command="{Binding Source={RelativeSource AncestorType={x:Type vm:ShortcutsViewModel}}, Path=DeleteShortcutCommand}"
                                    CommandParameter="{Binding .}"
                                    IsVisible="{Binding Source={RelativeSource AncestorType={x:Type vm:ShortcutsViewModel}}, Path=IsEditing}"
                                    BackgroundColor="{StaticResource Error}"
                                    TextColor="White"
                                    CornerRadius="12"
                                    HeightRequest="24"
                                    WidthRequest="24"
                                    Padding="0"
                                    FontSize="12"
                                    HorizontalOptions="End"
                                    VerticalOptions="Start" />
                        </Grid>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>

        <!-- Add Button -->
        <Button Grid.Row="2"
                Text="+ Add Shortcut"
                Style="{StaticResource PrimaryButton}"
                HorizontalOptions="Center"
                WidthRequest="200"
                Margin="0,16,0,0" />
    </Grid>
</ContentPage>
```

**Step 2: Create code-behind file**

Create file `src/RoMoteNoAds/Views/ShortcutsPage.xaml.cs`:

```csharp
using RoMoteNoAds.ViewModels;

namespace RoMoteNoAds.Views;

public partial class ShortcutsPage : ContentPage
{
    private readonly ShortcutsViewModel _viewModel;

    public ShortcutsPage(ShortcutsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
```

**Step 3: Verify build**

Run: `dotnet build src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-maccatalyst --no-restore`
Expected: Build succeeded

**Step 4: Commit**

```bash
git add src/RoMoteNoAds/Views/ShortcutsPage.xaml src/RoMoteNoAds/Views/ShortcutsPage.xaml.cs
git commit -m "feat: add ShortcutsPage for Mac tab view"
```

---

### Task 3.3: Register ShortcutsViewModel and Page

**Files:**
- Modify: `src/RoMoteNoAds/MauiProgram.cs`

**Step 1: Register ViewModel**

After line 32 (`builder.Services.AddSingleton<ChannelsViewModel>();`), add:

```csharp
builder.Services.AddSingleton<ShortcutsViewModel>();
```

**Step 2: Register View**

After line 37 (`builder.Services.AddSingleton<ChannelsPage>();`), add:

```csharp
builder.Services.AddSingleton<ShortcutsPage>();
```

**Step 3: Verify build**

Run: `dotnet build src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-maccatalyst --no-restore`
Expected: Build succeeded

**Step 4: Commit**

```bash
git add src/RoMoteNoAds/MauiProgram.cs
git commit -m "feat: register ShortcutsViewModel and ShortcutsPage"
```

---

### Task 3.4: Add Shortcuts Tab (Mac Only)

**Files:**
- Modify: `src/RoMoteNoAds/AppShell.xaml`

**Step 1: Add Shortcuts tab with OnPlatform**

Replace the entire `<TabBar>` section with:

```xml
<TabBar>
    <ShellContent
        Title="Remote"
        Icon="{OnPlatform iOS='remote_icon.png', MacCatalyst='remote_icon.png'}"
        ContentTemplate="{DataTemplate views:RemotePage}" />

    <ShellContent
        Title="Channels"
        Icon="{OnPlatform iOS='channels_icon.png', MacCatalyst='channels_icon.png'}"
        ContentTemplate="{DataTemplate views:ChannelsPage}" />

    <ShellContent
        Title="Shortcuts"
        Icon="{OnPlatform iOS='shortcuts_icon.png', MacCatalyst='shortcuts_icon.png'}"
        ContentTemplate="{DataTemplate views:ShortcutsPage}"
        IsVisible="{OnPlatform MacCatalyst=True, Default=False}" />

    <ShellContent
        Title="Devices"
        Icon="{OnPlatform iOS='devices_icon.png', MacCatalyst='devices_icon.png'}"
        ContentTemplate="{DataTemplate views:DeviceSelectionPage}" />
</TabBar>
```

**Step 2: Verify build**

Run: `dotnet build src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-maccatalyst --no-restore`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add src/RoMoteNoAds/AppShell.xaml
git commit -m "feat: add Shortcuts tab for MacCatalyst"
```

---

## Phase 4: Final Integration

### Task 4.1: Add Shortcut Icon Placeholder

**Files:**
- Create: `src/RoMoteNoAds/Resources/Images/shortcuts_icon.png` (placeholder or copy existing)

**Step 1: Copy an existing icon as placeholder**

Run:
```bash
cp src/RoMoteNoAds/Resources/Images/channels_icon.png src/RoMoteNoAds/Resources/Images/shortcuts_icon.png
```

Note: This creates a placeholder. Replace with proper icon later.

**Step 2: Verify build**

Run: `dotnet build src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-maccatalyst --no-restore`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add src/RoMoteNoAds/Resources/Images/shortcuts_icon.png
git commit -m "chore: add placeholder shortcuts icon"
```

---

### Task 4.2: Final Build and Test

**Step 1: Clean and full build**

Run: `dotnet build src/RoMoteNoAds/RoMoteNoAds.csproj -f net10.0-maccatalyst`
Expected: Build succeeded

**Step 2: Commit all remaining changes**

```bash
git status
# If any uncommitted changes:
git add -A
git commit -m "chore: final cleanup for modern remote redesign"
```

---

## Summary of Changes

**New Files Created:**
- `src/RoMoteNoAds/Models/ShortcutType.cs`
- `src/RoMoteNoAds/Models/Shortcut.cs`
- `src/RoMoteNoAds/Services/IShortcutService.cs`
- `src/RoMoteNoAds/Services/ShortcutService.cs`
- `src/RoMoteNoAds/ViewModels/ShortcutsViewModel.cs`
- `src/RoMoteNoAds/Views/ShortcutsPage.xaml`
- `src/RoMoteNoAds/Views/ShortcutsPage.xaml.cs`
- `src/RoMoteNoAds/Resources/Images/shortcuts_icon.png`

**Modified Files:**
- `src/RoMoteNoAds/Resources/Styles/Colors.xaml` - Glass colors
- `src/RoMoteNoAds/Resources/Styles/Styles.xaml` - Glass button styles
- `src/RoMoteNoAds/Views/RemotePage.xaml` - New glass layout
- `src/RoMoteNoAds/MauiProgram.cs` - Service registrations
- `src/RoMoteNoAds/AppShell.xaml` - Shortcuts tab

**Not Yet Implemented (Future Tasks):**
- ShortcutEditorPage for creating/editing shortcuts
- Mobile swipe-up panel
- Channel icon loading in shortcuts
- Shortcut reordering UI
