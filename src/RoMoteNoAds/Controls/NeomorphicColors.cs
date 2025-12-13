using SkiaSharp;

namespace RoMoteNoAds.Controls;

/// <summary>
/// Provides neomorphic color values for SkiaSharp drawing.
/// Handles light/dark mode switching.
/// </summary>
public static class NeomorphicColors
{
    public static bool IsDarkMode => Application.Current?.RequestedTheme == AppTheme.Dark;

    // Background
    public static SKColor Background => IsDarkMode
        ? SKColor.Parse("#2D3748")
        : SKColor.Parse("#E0E5EC");

    // Shadows
    public static SKColor ShadowDark => IsDarkMode
        ? SKColor.Parse("#1A202C")
        : SKColor.Parse("#A3B1C6");

    public static SKColor ShadowLight => IsDarkMode
        ? SKColor.Parse("#4A5568")
        : SKColor.Parse("#FFFFFF");

    // Text
    public static SKColor TextPrimary => IsDarkMode
        ? SKColor.Parse("#E2E8F0")
        : SKColor.Parse("#6B7280");

    public static SKColor TextSecondary => IsDarkMode
        ? SKColor.Parse("#A0AEC0")
        : SKColor.Parse("#9CA3AF");

    // Accent
    public static SKColor Accent => IsDarkMode
        ? SKColor.Parse("#63B3ED")
        : SKColor.Parse("#3B82F6");

    // Status
    public static SKColor Connected => IsDarkMode
        ? SKColor.Parse("#34D399")
        : SKColor.Parse("#10B981");

    public static SKColor Power => IsDarkMode
        ? SKColor.Parse("#F87171")
        : SKColor.Parse("#EF4444");

    // Shadow offsets and blur
    public const float ShadowOffsetOut = 6f;
    public const float ShadowBlurOut = 12f;
    public const float ShadowOffsetIn = 4f;
    public const float ShadowBlurIn = 8f;
}
