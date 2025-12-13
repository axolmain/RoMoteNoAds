using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using RoMote.Roku.Models;

namespace RoMoteNoAds.Controls;

/// <summary>
/// A neomorphic device picker that shows current device with dropdown.
/// </summary>
public class NeumorphicDevicePicker : SKCanvasView
{
    private bool _isExpanded;

    #region Bindable Properties

    public static readonly BindableProperty CurrentDeviceProperty = BindableProperty.Create(
        nameof(CurrentDevice), typeof(RokuDevice), typeof(NeumorphicDevicePicker), null,
        propertyChanged: (b, _, _) => ((NeumorphicDevicePicker)b).InvalidateSurface());

    public static readonly BindableProperty DevicesProperty = BindableProperty.Create(
        nameof(Devices), typeof(IList<RokuDevice>), typeof(NeumorphicDevicePicker), null,
        propertyChanged: (b, _, _) => ((NeumorphicDevicePicker)b).InvalidateSurface());

    public static readonly BindableProperty IsConnectedProperty = BindableProperty.Create(
        nameof(IsConnected), typeof(bool), typeof(NeumorphicDevicePicker), false,
        propertyChanged: (b, _, _) => ((NeumorphicDevicePicker)b).InvalidateSurface());

    public static readonly BindableProperty SelectDeviceCommandProperty = BindableProperty.Create(
        nameof(SelectDeviceCommand), typeof(System.Windows.Input.ICommand), typeof(NeumorphicDevicePicker), null);

    public static readonly BindableProperty ScanCommandProperty = BindableProperty.Create(
        nameof(ScanCommand), typeof(System.Windows.Input.ICommand), typeof(NeumorphicDevicePicker), null);

    public RokuDevice? CurrentDevice
    {
        get => (RokuDevice?)GetValue(CurrentDeviceProperty);
        set => SetValue(CurrentDeviceProperty, value);
    }

    public IList<RokuDevice>? Devices
    {
        get => (IList<RokuDevice>?)GetValue(DevicesProperty);
        set => SetValue(DevicesProperty, value);
    }

    public bool IsConnected
    {
        get => (bool)GetValue(IsConnectedProperty);
        set => SetValue(IsConnectedProperty, value);
    }

    public System.Windows.Input.ICommand? SelectDeviceCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(SelectDeviceCommandProperty);
        set => SetValue(SelectDeviceCommandProperty, value);
    }

    public System.Windows.Input.ICommand? ScanCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(ScanCommandProperty);
        set => SetValue(ScanCommandProperty, value);
    }

    #endregion

    public event EventHandler<RokuDevice>? DeviceSelected;
    public event EventHandler? ScanRequested;

    public NeumorphicDevicePicker()
    {
        EnableTouchEvents = true;
        Touch += OnTouch;
        HeightRequest = 44;
    }

    public void ToggleExpanded()
    {
        _isExpanded = !_isExpanded;
        UpdateHeight();
        InvalidateSurface();
    }

    public void Collapse()
    {
        if (_isExpanded)
        {
            _isExpanded = false;
            UpdateHeight();
            InvalidateSurface();
        }
    }

    private void UpdateHeight()
    {
        var scale = DeviceDisplay.MainDisplayInfo.Density;
        var baseHeight = 44 * scale;
        var itemHeight = 56 * scale;
        var deviceCount = Devices?.Count ?? 0;
        var extraItems = 1; // Scan button

        HeightRequest = _isExpanded
            ? baseHeight + ((deviceCount + extraItems) * itemHeight) + (16 * scale)
            : baseHeight;
    }

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        var scale = (float)DeviceDisplay.MainDisplayInfo.Density;
        var headerHeight = 44f * scale;

        if (e.ActionType == SKTouchAction.Released)
        {
            if (e.Location.Y <= headerHeight)
            {
                // Tapped header - toggle dropdown
                ToggleExpanded();
            }
            else if (_isExpanded)
            {
                // Tapped in dropdown area
                var itemHeight = 56f * scale;
                var dropdownY = e.Location.Y - headerHeight - (8f * scale);
                var tappedIndex = (int)(dropdownY / itemHeight);
                var deviceCount = Devices?.Count ?? 0;

                if (tappedIndex >= 0 && tappedIndex < deviceCount)
                {
                    var device = Devices![tappedIndex];
                    DeviceSelected?.Invoke(this, device);
                    SelectDeviceCommand?.Execute(device);
                    Collapse();
                }
                else if (tappedIndex == deviceCount)
                {
                    // Scan button
                    ScanRequested?.Invoke(this, EventArgs.Empty);
                    ScanCommand?.Execute(null);
                }
            }
            e.Handled = true;
        }
        else if (e.ActionType == SKTouchAction.Pressed)
        {
            e.Handled = true;
        }
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear(SKColors.Transparent);

        var scale = (float)DeviceDisplay.MainDisplayInfo.Density;
        var headerHeight = 44f * scale;

        // Draw header (always visible)
        DrawHeader(canvas, info.Width, headerHeight, scale);

        // Draw dropdown if expanded
        if (_isExpanded)
        {
            DrawDropdown(canvas, info.Width, headerHeight, info.Height, scale);
        }
    }

    private void DrawHeader(SKCanvas canvas, float width, float height, float scale)
    {
        var padding = 8f * scale;
        var bounds = new SKRect(padding, padding, width - padding, height - padding);
        var cornerRadius = 12f * scale;

        // Outset shadow
        var offset = 4f * scale;
        var blur = 8f * scale;

        using var darkPaint = new SKPaint
        {
            Color = NeomorphicColors.ShadowDark,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 2)
        };
        canvas.DrawRoundRect(bounds.Left + offset, bounds.Top + offset,
                              bounds.Width, bounds.Height,
                              cornerRadius, cornerRadius, darkPaint);

        using var lightPaint = new SKPaint
        {
            Color = NeomorphicColors.ShadowLight,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 2)
        };
        canvas.DrawRoundRect(bounds.Left - offset, bounds.Top - offset,
                              bounds.Width, bounds.Height,
                              cornerRadius, cornerRadius, lightPaint);

        // Background
        using var bgPaint = new SKPaint
        {
            Color = NeomorphicColors.Background,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRoundRect(bounds, cornerRadius, cornerRadius, bgPaint);

        // Status dot
        var dotRadius = 5f * scale;
        var dotX = bounds.Left + 16f * scale;
        var dotY = bounds.MidY;

        using var dotPaint = new SKPaint
        {
            Color = IsConnected ? NeomorphicColors.Connected : NeomorphicColors.TextSecondary,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawCircle(dotX, dotY, dotRadius, dotPaint);

        // Device name - using modern SKFont API
        var deviceName = CurrentDevice?.DisplayName ?? "No Device";
        using var textTypeface = SKTypeface.FromFamilyName("SF Pro", SKFontStyle.Bold);
        using var textFont = new SKFont(textTypeface, 14f * scale);
        using var textPaint = new SKPaint
        {
            Color = NeomorphicColors.TextPrimary,
            IsAntialias = true
        };
        canvas.DrawText(deviceName, dotX + 16f * scale, bounds.MidY + 5f * scale, textFont, textPaint);

        // Dropdown arrow - using modern SKFont API
        var arrowText = _isExpanded ? "▲" : "▼";
        using var arrowTypeface = SKTypeface.FromFamilyName("SF Pro", SKFontStyle.Normal);
        using var arrowFont = new SKFont(arrowTypeface, 10f * scale);
        using var arrowPaint = new SKPaint
        {
            Color = NeomorphicColors.TextSecondary,
            IsAntialias = true
        };
        canvas.DrawText(arrowText, bounds.Right - 12f * scale, bounds.MidY + 4f * scale, SKTextAlign.Right, arrowFont, arrowPaint);
    }

    private void DrawDropdown(SKCanvas canvas, float width, float headerHeight, float totalHeight, float scale)
    {
        var padding = 8f * scale;
        var dropdownTop = headerHeight + padding;
        var bounds = new SKRect(padding, dropdownTop, width - padding, totalHeight - padding);
        var cornerRadius = 12f * scale;

        // Inset dropdown background
        using var bgPaint = new SKPaint
        {
            Color = NeomorphicColors.Background,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRoundRect(bounds, cornerRadius, cornerRadius, bgPaint);

        canvas.Save();
        using var clipPath = new SKPath();
        clipPath.AddRoundRect(bounds, cornerRadius, cornerRadius);
        canvas.ClipPath(clipPath);

        using var shadowPaint = new SKPaint
        {
            Color = NeomorphicColors.ShadowDark.WithAlpha(100),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 8f * scale,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4f * scale)
        };
        canvas.DrawRoundRect(bounds, cornerRadius, cornerRadius, shadowPaint);

        canvas.Restore();

        // Draw device items
        var itemHeight = 56f * scale;
        var itemY = dropdownTop + 8f * scale;
        var devices = Devices ?? new List<RokuDevice>();

        foreach (var device in devices)
        {
            var isSelected = device == CurrentDevice;
            DrawDeviceItem(canvas, device, padding + 8f * scale, itemY,
                           width - (padding * 2) - 16f * scale, itemHeight - 8f * scale,
                           isSelected, scale);
            itemY += itemHeight;
        }

        // Draw scan button
        DrawScanButton(canvas, padding + 8f * scale, itemY,
                       width - (padding * 2) - 16f * scale, itemHeight - 8f * scale, scale);
    }

    private void DrawDeviceItem(SKCanvas canvas, RokuDevice device, float x, float y,
                                 float width, float height, bool isSelected, float scale)
    {
        var bounds = new SKRect(x, y, x + width, y + height);

        if (isSelected)
        {
            using var selectPaint = new SKPaint
            {
                Color = NeomorphicColors.Accent.WithAlpha(30),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRoundRect(bounds, 8f * scale, 8f * scale, selectPaint);
        }

        // Checkmark for selected - using modern SKFont API
        if (isSelected)
        {
            using var checkTypeface = SKTypeface.FromFamilyName("SF Pro", SKFontStyle.Bold);
            using var checkFont = new SKFont(checkTypeface, 14f * scale);
            using var checkPaint = new SKPaint
            {
                Color = NeomorphicColors.Accent,
                IsAntialias = true
            };
            canvas.DrawText("✓", x + 12f * scale, y + height / 2 + 5f * scale, checkFont, checkPaint);
        }

        // Device name - using modern SKFont API
        using var nameTypeface = SKTypeface.FromFamilyName("SF Pro", isSelected ? SKFontStyle.Bold : SKFontStyle.Normal);
        using var nameFont = new SKFont(nameTypeface, 14f * scale);
        using var namePaint = new SKPaint
        {
            Color = NeomorphicColors.TextPrimary,
            IsAntialias = true
        };
        canvas.DrawText(device.DisplayName, x + 36f * scale, y + height / 2 - 2f * scale, nameFont, namePaint);

        // IP address - using modern SKFont API
        using var ipTypeface = SKTypeface.FromFamilyName("SF Pro", SKFontStyle.Normal);
        using var ipFont = new SKFont(ipTypeface, 11f * scale);
        using var ipPaint = new SKPaint
        {
            Color = NeomorphicColors.TextSecondary,
            IsAntialias = true
        };
        canvas.DrawText(device.IpAddress, x + 36f * scale, y + height / 2 + 14f * scale, ipFont, ipPaint);
    }

    private void DrawScanButton(SKCanvas canvas, float x, float y, float width, float height, float scale)
    {
        // Scan button text - using modern SKFont API
        using var textTypeface = SKTypeface.FromFamilyName("SF Pro", SKFontStyle.Bold);
        using var textFont = new SKFont(textTypeface, 14f * scale);
        using var textPaint = new SKPaint
        {
            Color = NeomorphicColors.Accent,
            IsAntialias = true
        };
        canvas.DrawText("⟳ Scan for devices...", x + 12f * scale, y + height / 2 + 5f * scale, textFont, textPaint);
    }
}
