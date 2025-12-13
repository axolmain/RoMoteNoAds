using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using RoMote.Roku.Models;
using System.Windows.Input;

namespace RoMoteNoAds.Controls;

public class NeumorphicDevicePicker : SKCanvasView
{
    private bool _isExpanded;
    private float _animationProgress; // 0.0f (Closed) to 1.0f (Open)

    // Layout Constants
    private const float HEADER_HEIGHT = 60f;
    private const float ITEM_HEIGHT = 56f;

    #region Color & Style Properties

    // Default to Cupertino/Neo Light Palette
    public static readonly BindableProperty BaseColorProperty = BindableProperty.Create(
        nameof(BaseColor), typeof(Color), typeof(NeumorphicDevicePicker), Color.FromArgb("#E0E5EC"),
        propertyChanged: (b, _, _) => ((NeumorphicDevicePicker)b).InvalidateSurface());

    public static readonly BindableProperty AccentColorProperty = BindableProperty.Create(
        nameof(AccentColor), typeof(Color), typeof(NeumorphicDevicePicker), Color.FromArgb("#007AFF"),
        propertyChanged: (b, _, _) => ((NeumorphicDevicePicker)b).InvalidateSurface());

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor), typeof(Color), typeof(NeumorphicDevicePicker), Color.FromArgb("#4B5563"),
        propertyChanged: (b, _, _) => ((NeumorphicDevicePicker)b).InvalidateSurface());

    public Color BaseColor
    {
        get => (Color)GetValue(BaseColorProperty);
        set => SetValue(BaseColorProperty, value);
    }

    public Color AccentColor
    {
        get => (Color)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    #endregion

    #region Data Bindings

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
        nameof(SelectDeviceCommand), typeof(ICommand), typeof(NeumorphicDevicePicker), null);

    public static readonly BindableProperty ScanCommandProperty = BindableProperty.Create(
        nameof(ScanCommand), typeof(ICommand), typeof(NeumorphicDevicePicker), null);

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

    public ICommand? SelectDeviceCommand
    {
        get => (ICommand?)GetValue(SelectDeviceCommandProperty);
        set => SetValue(SelectDeviceCommandProperty, value);
    }

    public ICommand? ScanCommand
    {
        get => (ICommand?)GetValue(ScanCommandProperty);
        set => SetValue(ScanCommandProperty, value);
    }

    #endregion

    public event EventHandler<RokuDevice>? DeviceSelected;
    public event EventHandler? ScanRequested;

    public NeumorphicDevicePicker()
    {
        EnableTouchEvents = true;
        Touch += OnTouch;
        HeightRequest = HEADER_HEIGHT; 
        BackgroundColor = Colors.Transparent;
    }

    public void ToggleExpanded()
    {
        _isExpanded = !_isExpanded;
        AnimateState();
    }

    public void Collapse()
    {
        if (_isExpanded)
        {
            _isExpanded = false;
            AnimateState();
        }
    }

    private void AnimateState()
    {
        this.AbortAnimation("ExpandAnim");

        // Use HeightRequest to control the view size.
        // In an Overlay Grid, increasing HeightRequest allows it to draw over lower layers.
        var baseHeight = HEADER_HEIGHT;
        var deviceCount = Devices?.Count ?? 0;
        var extraItems = 1; // Scan button
        
        // Calculate max needed height
        var contentHeight = baseHeight + ((deviceCount + extraItems) * ITEM_HEIGHT) + 16;
        var targetHeight = _isExpanded ? contentHeight : baseHeight;

        var anim = new Animation(v =>
        {
            HeightRequest = v;
            
            // Calculate progress (0 to 1) for arrow rotation and shadow transitions
            var progress = (v - baseHeight) / (contentHeight - baseHeight);
            _animationProgress = (float)Math.Clamp(progress, 0, 1);
            if (_isExpanded && Math.Abs(v - targetHeight) < 0.1) _animationProgress = 1;
            if (!_isExpanded && Math.Abs(v - targetHeight) < 0.1) _animationProgress = 0;

            InvalidateSurface();
        }, HeightRequest, targetHeight, Easing.CubicOut); // Springy feel

        anim.Commit(this, "ExpandAnim", 16, 300);
    }

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        var scale = (float)DeviceDisplay.MainDisplayInfo.Density;
        var headerHeightPixels = HEADER_HEIGHT * scale;
        
        if (e.ActionType == SKTouchAction.Released)
        {
            if (e.Location.Y <= headerHeightPixels)
            {
                ToggleExpanded();
            }
            else if (_isExpanded)
            {
                var itemHeightPixels = ITEM_HEIGHT * scale;
                var dropdownRelativeY = e.Location.Y - headerHeightPixels - (8f * scale);
                
                if (dropdownRelativeY >= 0)
                {
                    var tappedIndex = (int)(dropdownRelativeY / itemHeightPixels);
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
                        ScanRequested?.Invoke(this, EventArgs.Empty);
                        ScanCommand?.Execute(null);
                    }
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
        
        // Convert Colors
        var skBase = BaseColor.ToSKColor();
        var skAccent = AccentColor.ToSKColor();
        var skText = TextColor.ToSKColor();
        var skTextSec = skText.WithAlpha(150);
        
        // Shadow Colors
        var shadowLight = SKColors.White.WithAlpha(200);
        var shadowDark = new SKColor(163, 177, 198, 150);

        // Dimensions
        var headerHeight = HEADER_HEIGHT * scale;
        var padding = 12f * scale;
        var width = info.Width;
        
        // 1. Draw Dropdown Body
        // We only draw this if animation > 0
        if (_animationProgress > 0.01f) 
        {
            var dropdownTop = headerHeight - (10f * scale);
            var totalDropdownHeight = info.Height - dropdownTop - padding;
            
            // Clip to reveal gradually if we wanted, but HeightRequest handles clipping usually.
            // We just draw the box.
            
            var dropdownRect = new SKRect(padding, dropdownTop, width - padding, info.Height - padding);
            
            // Main Dropdown Background
            using var bgPaint = new SKPaint { Color = skBase, IsAntialias = true, Style = SKPaintStyle.Fill };
            canvas.DrawRoundRect(dropdownRect, 16f * scale, 16f * scale, bgPaint);
            
            // Soft Shadow for Dropdown (floating effect)
            if (_isExpanded)
            {
                // We draw a subtle border/shadow to separate it from content below
                using var borderPaint = new SKPaint 
                { 
                    Color = shadowDark.WithAlpha(50), 
                    IsAntialias = true, 
                    Style = SKPaintStyle.Stroke, 
                    StrokeWidth = 1f * scale 
                };
                canvas.DrawRoundRect(dropdownRect, 16f * scale, 16f * scale, borderPaint);
            }

            // Draw Items
            var startY = headerHeight + (8f * scale);
            var itemHeightPixels = ITEM_HEIGHT * scale;
            var devices = Devices ?? new List<RokuDevice>();
            
            canvas.Save();
            canvas.ClipRoundRect(new SKRoundRect(dropdownRect, 16f * scale));

            for (int i = 0; i < devices.Count; i++)
            {
                DrawDeviceItem(canvas, devices[i], padding, startY + (i * itemHeightPixels), 
                             width - (padding * 2), itemHeightPixels, 
                             devices[i] == CurrentDevice, scale, skText, skAccent);
            }
            
            DrawScanButton(canvas, padding, startY + (devices.Count * itemHeightPixels), 
                         width - (padding * 2), itemHeightPixels, scale, skAccent);
            
            canvas.Restore();
        }

        // 2. Draw Header
        var headerRect = new SKRect(padding, padding, width - padding, headerHeight - padding);
        var cornerRadius = 16f * scale;
        
        // Visual State: Pressed when expanded
        bool isPressed = _isExpanded || _animationProgress > 0.5f;

        if (isPressed)
        {
            // --- INSET STATE ---
            using var bgPaint = new SKPaint { Color = skBase, IsAntialias = true };
            canvas.DrawRoundRect(headerRect, cornerRadius, cornerRadius, bgPaint);

            // Inner Shadow
            canvas.Save();
            canvas.ClipRoundRect(new SKRoundRect(headerRect, cornerRadius), SKClipOperation.Intersect);
            
            using var innerShadowDark = new SKPaint 
            { 
                Color = shadowDark.WithAlpha(100), 
                IsAntialias = true, 
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6f * scale) 
            };
            canvas.DrawRect(new SKRect(headerRect.Left, headerRect.Top, headerRect.Right, headerRect.Top + 10 * scale), innerShadowDark);
            canvas.DrawRect(new SKRect(headerRect.Left, headerRect.Top, headerRect.Left + 10 * scale, headerRect.Bottom), innerShadowDark);
            canvas.Restore();
        }
        else
        {
            // --- OUTSET STATE ---
            var offset = 5f * scale;
            var blur = 8f * scale;

            using var shadowDarkPaint = new SKPaint
            {
                Color = shadowDark,
                IsAntialias = true,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur)
            };
            canvas.DrawRoundRect(new SKRect(headerRect.Left + offset, headerRect.Top + offset, headerRect.Right + offset, headerRect.Bottom + offset), cornerRadius, cornerRadius, shadowDarkPaint);

            using var shadowLightPaint = new SKPaint
            {
                Color = shadowLight,
                IsAntialias = true,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur)
            };
            canvas.DrawRoundRect(new SKRect(headerRect.Left - offset, headerRect.Top - offset, headerRect.Right - offset, headerRect.Bottom - offset), cornerRadius, cornerRadius, shadowLightPaint);

            using var bgPaint = new SKPaint { Color = skBase, IsAntialias = true };
            canvas.DrawRoundRect(headerRect, cornerRadius, cornerRadius, bgPaint);
        }

        // 3. Header Content
        var dotRadius = 4f * scale;
        var dotX = headerRect.Left + 20f * scale;
        var dotY = headerRect.MidY;
        using var dotPaint = new SKPaint 
        { 
            Color = IsConnected ? SKColors.LimeGreen : skTextSec, 
            IsAntialias = true, 
            Style = SKPaintStyle.Fill 
        };
        canvas.DrawCircle(dotX, dotY, dotRadius, dotPaint);

        var deviceName = CurrentDevice?.DisplayName ?? "No Device";
        using var textFont = new SKFont(SKTypeface.FromFamilyName("SF Pro Display", SKFontStyle.Bold), 16f * scale);
        using var textPaint = new SKPaint { Color = skText, IsAntialias = true };
        
        var textBounds = new SKRect();
        textPaint.MeasureText(deviceName, ref textBounds);
        canvas.DrawText(deviceName, dotX + 16f * scale, headerRect.MidY - textBounds.MidY, textFont, textPaint);

        // Animated Arrow
        var arrowX = headerRect.Right - 24f * scale;
        var arrowY = headerRect.MidY;
        
        canvas.Save();
        canvas.Translate(arrowX, arrowY);
        // Rotate 180 degrees based on animation progress
        canvas.RotateDegrees(180 * _animationProgress); 
        
        using var arrowPath = new SKPath();
        var arrowSize = 5f * scale;
        // Draw V shape centered at 0,0
        arrowPath.MoveTo(-arrowSize, -arrowSize/2);
        arrowPath.LineTo(0, arrowSize/2);
        arrowPath.LineTo(arrowSize, -arrowSize/2);

        using var arrowPaint = new SKPaint 
        { 
            Color = skTextSec, 
            IsAntialias = true, 
            Style = SKPaintStyle.Stroke, 
            StrokeWidth = 2f * scale,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };
        canvas.DrawPath(arrowPath, arrowPaint);
        
        canvas.Restore();
    }

    private void DrawDeviceItem(SKCanvas canvas, RokuDevice device, float x, float y, float width, float height, 
                              bool isSelected, float scale, SKColor textColor, SKColor accentColor)
    {
        var centerY = y + (height / 2);
        
        if (isSelected)
        {
            var barRect = new SKRect(x + 4 * scale, y + 4 * scale, x + 4 * scale + 4 * scale, y + height - 4 * scale);
            using var barPaint = new SKPaint { Color = accentColor, IsAntialias = true, Style = SKPaintStyle.Fill };
            canvas.DrawRoundRect(barRect, 2 * scale, 2 * scale, barPaint);
        }

        using var nameFont = new SKFont(SKTypeface.FromFamilyName("SF Pro Text", SKFontStyle.Normal), 15f * scale);
        using var namePaint = new SKPaint { Color = isSelected ? accentColor : textColor, IsAntialias = true };
        
        var textBounds = new SKRect();
        namePaint.MeasureText(device.DisplayName, ref textBounds);
        canvas.DrawText(device.DisplayName, x + 20f * scale, centerY - textBounds.MidY - 6 * scale, nameFont, namePaint);
        
        using var ipFont = new SKFont(SKTypeface.FromFamilyName("SF Pro Text", SKFontStyle.Normal), 12f * scale);
        using var ipPaint = new SKPaint { Color = textColor.WithAlpha(128), IsAntialias = true };
        canvas.DrawText(device.IpAddress, x + 20f * scale, centerY + 10f * scale, ipFont, ipPaint);
    }

    private void DrawScanButton(SKCanvas canvas, float x, float y, float width, float height, float scale, SKColor accentColor)
    {
        var centerY = y + (height / 2);
        using var font = new SKFont(SKTypeface.FromFamilyName("SF Pro Text", SKFontStyle.Bold), 14f * scale);
        using var paint = new SKPaint { Color = accentColor, IsAntialias = true };
        
        var text = "Scan for devices...";
        var bounds = new SKRect();
        paint.MeasureText(text, ref bounds);
        canvas.DrawText(text, x + (width / 2) - (bounds.Width / 2), centerY - bounds.MidY, font, paint);
    }
}