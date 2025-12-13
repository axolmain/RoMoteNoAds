using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace RoMoteNoAds.Controls;

/// <summary>
/// A neomorphic-styled button with soft shadows.
/// Supports circular and rounded rectangle shapes.
/// </summary>
public class NeumorphicButton : SKCanvasView
{
    private bool _isPressed;
    private DateTime _lastTapTime = DateTime.MinValue;

    #region Bindable Properties

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(NeumorphicButton), string.Empty,
        propertyChanged: (b, _, _) => ((NeumorphicButton)b).InvalidateSurface());

    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize), typeof(float), typeof(NeumorphicButton), 20f,
        propertyChanged: (b, _, _) => ((NeumorphicButton)b).InvalidateSurface());

    public static readonly BindableProperty IsCircularProperty = BindableProperty.Create(
        nameof(IsCircular), typeof(bool), typeof(NeumorphicButton), true,
        propertyChanged: (b, _, _) => ((NeumorphicButton)b).InvalidateSurface());

    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
        nameof(CornerRadius), typeof(float), typeof(NeumorphicButton), 12f,
        propertyChanged: (b, _, _) => ((NeumorphicButton)b).InvalidateSurface());

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor), typeof(Color), typeof(NeumorphicButton), null,
        propertyChanged: (b, _, _) => ((NeumorphicButton)b).InvalidateSurface());

    public static readonly BindableProperty IsPressedStyleProperty = BindableProperty.Create(
        nameof(IsPressedStyle), typeof(bool), typeof(NeumorphicButton), false,
        propertyChanged: (b, _, _) => ((NeumorphicButton)b).InvalidateSurface());

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(System.Windows.Input.ICommand), typeof(NeumorphicButton), null);

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(NeumorphicButton), null);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public float FontSize
    {
        get => (float)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public bool IsCircular
    {
        get => (bool)GetValue(IsCircularProperty);
        set => SetValue(IsCircularProperty, value);
    }

    public float CornerRadius
    {
        get => (float)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Color? TextColor
    {
        get => (Color?)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public bool IsPressedStyle
    {
        get => (bool)GetValue(IsPressedStyleProperty);
        set => SetValue(IsPressedStyleProperty, value);
    }

    public System.Windows.Input.ICommand? Command
    {
        get => (System.Windows.Input.ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    #endregion

    public event EventHandler? Clicked;

    public NeumorphicButton()
    {
        EnableTouchEvents = true;
        Touch += OnTouch;
    }

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                _isPressed = true;
                InvalidateSurface();
                e.Handled = true;
                break;

            case SKTouchAction.Released:
                if (_isPressed)
                {
                    _isPressed = false;
                    InvalidateSurface();

                    // Debounce rapid taps
                    if ((DateTime.Now - _lastTapTime).TotalMilliseconds > 100)
                    {
                        _lastTapTime = DateTime.Now;
                        Clicked?.Invoke(this, EventArgs.Empty);
                        if (Command?.CanExecute(CommandParameter) == true)
                        {
                            Command.Execute(CommandParameter);
                        }
                    }
                }
                e.Handled = true;
                break;

            case SKTouchAction.Cancelled:
            case SKTouchAction.Exited:
                _isPressed = false;
                InvalidateSurface();
                e.Handled = true;
                break;
        }
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear(SKColors.Transparent);

        var width = info.Width;
        var height = info.Height;
        var scale = (float)DeviceDisplay.MainDisplayInfo.Density;

        var padding = 10f * scale;
        var drawWidth = width - (padding * 2);
        var drawHeight = height - (padding * 2);

        var isPressed = _isPressed || IsPressedStyle;

        // Calculate shape bounds
        SKRect bounds;
        float cornerRadius;

        if (IsCircular)
        {
            var size = Math.Min(drawWidth, drawHeight);
            var left = (width - size) / 2f;
            var top = (height - size) / 2f;
            bounds = new SKRect(left, top, left + size, top + size);
            cornerRadius = size / 2f;
        }
        else
        {
            bounds = new SKRect(padding, padding, width - padding, height - padding);
            cornerRadius = CornerRadius * scale;
        }

        // Draw shadows
        if (isPressed)
        {
            DrawInsetShadows(canvas, bounds, cornerRadius, scale);
        }
        else
        {
            DrawOutsetShadows(canvas, bounds, cornerRadius, scale);
        }

        // Draw main shape
        using var fillPaint = new SKPaint
        {
            Color = NeomorphicColors.Background,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRoundRect(bounds, cornerRadius, cornerRadius, fillPaint);

        // Draw text
        if (!string.IsNullOrEmpty(Text))
        {
            DrawText(canvas, bounds, scale);
        }
    }

    private void DrawOutsetShadows(SKCanvas canvas, SKRect bounds, float cornerRadius, float scale)
    {
        var offset = NeomorphicColors.ShadowOffsetOut * scale;
        var blur = NeomorphicColors.ShadowBlurOut * scale;

        // Dark shadow (bottom-right)
        using var darkShadowPaint = new SKPaint
        {
            Color = NeomorphicColors.ShadowDark,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 2)
        };
        var darkBounds = new SKRect(bounds.Left + offset, bounds.Top + offset,
                                     bounds.Right + offset, bounds.Bottom + offset);
        canvas.DrawRoundRect(darkBounds, cornerRadius, cornerRadius, darkShadowPaint);

        // Light shadow (top-left)
        using var lightShadowPaint = new SKPaint
        {
            Color = NeomorphicColors.ShadowLight,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 2)
        };
        var lightBounds = new SKRect(bounds.Left - offset, bounds.Top - offset,
                                      bounds.Right - offset, bounds.Bottom - offset);
        canvas.DrawRoundRect(lightBounds, cornerRadius, cornerRadius, lightShadowPaint);
    }

    private void DrawInsetShadows(SKCanvas canvas, SKRect bounds, float cornerRadius, float scale)
    {
        var offset = NeomorphicColors.ShadowOffsetIn * scale;
        var blur = NeomorphicColors.ShadowBlurIn * scale;

        // Draw base shape first
        using var basePaint = new SKPaint
        {
            Color = NeomorphicColors.Background,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRoundRect(bounds, cornerRadius, cornerRadius, basePaint);

        // Save and clip to shape
        canvas.Save();
        using var clipPath = new SKPath();
        clipPath.AddRoundRect(bounds, cornerRadius, cornerRadius);
        canvas.ClipPath(clipPath);

        // Dark inset shadow (top-left inside)
        using var darkInsetPaint = new SKPaint
        {
            Color = NeomorphicColors.ShadowDark.WithAlpha(180),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 2)
        };
        var darkInsetBounds = new SKRect(bounds.Left - cornerRadius + offset,
                                          bounds.Top - cornerRadius + offset,
                                          bounds.Right - offset,
                                          bounds.Bottom - offset);
        canvas.DrawRoundRect(darkInsetBounds, cornerRadius, cornerRadius, darkInsetPaint);

        // Light inset shadow (bottom-right inside)
        using var lightInsetPaint = new SKPaint
        {
            Color = NeomorphicColors.ShadowLight.WithAlpha(180),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 2)
        };
        var lightInsetBounds = new SKRect(bounds.Left + offset,
                                           bounds.Top + offset,
                                           bounds.Right + cornerRadius - offset,
                                           bounds.Bottom + cornerRadius - offset);
        canvas.DrawRoundRect(lightInsetBounds, cornerRadius, cornerRadius, lightInsetPaint);

        canvas.Restore();
    }

    private void DrawText(SKCanvas canvas, SKRect bounds, float scale)
    {
        var textColor = TextColor != null
            ? new SKColor(
                (byte)(TextColor.Red * 255),
                (byte)(TextColor.Green * 255),
                (byte)(TextColor.Blue * 255),
                (byte)(TextColor.Alpha * 255))
            : NeomorphicColors.TextPrimary;

        using var typeface = SKTypeface.FromFamilyName("SF Pro", SKFontStyle.Bold);
        using var font = new SKFont(typeface, FontSize * scale);
        using var textPaint = new SKPaint
        {
            Color = textColor,
            IsAntialias = true
        };

        var textBounds = new SKRect();
        font.MeasureText(Text, out textBounds);

        var x = bounds.MidX;
        var y = bounds.MidY - textBounds.MidY;

        canvas.DrawText(Text, x, y, SKTextAlign.Center, font, textPaint);
    }
}
