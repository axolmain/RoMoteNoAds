using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace RoMoteNoAds.Controls;

/// <summary>
/// A neomorphic volume slider with inset track and raised thumb.
/// </summary>
public class NeumorphicSlider : SKCanvasView
{
    private bool _isDragging;
    private float _dragStartValue;

    #region Bindable Properties

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(double), typeof(NeumorphicSlider), 0.5,
        BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((NeumorphicSlider)b).InvalidateSurface());

    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum), typeof(double), typeof(NeumorphicSlider), 0.0);

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum), typeof(double), typeof(NeumorphicSlider), 1.0);

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, Math.Clamp(value, Minimum, Maximum));
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    #endregion

    public event EventHandler<double>? ValueChanged;

    public NeumorphicSlider()
    {
        EnableTouchEvents = true;
        Touch += OnTouch;
        HeightRequest = 40;
    }

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        var info = CanvasSize;
        var scale = (float)DeviceDisplay.MainDisplayInfo.Density;
        var trackPadding = 20f * scale;
        var trackWidth = info.Width - (trackPadding * 2);

        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                _isDragging = true;
                _dragStartValue = (float)Value;
                UpdateValueFromTouch(e.Location.X, trackPadding, trackWidth);
                e.Handled = true;
                break;

            case SKTouchAction.Moved:
                if (_isDragging)
                {
                    UpdateValueFromTouch(e.Location.X, trackPadding, trackWidth);
                    e.Handled = true;
                }
                break;

            case SKTouchAction.Released:
            case SKTouchAction.Cancelled:
                _isDragging = false;
                InvalidateSurface();
                e.Handled = true;
                break;
        }
    }

    private void UpdateValueFromTouch(float x, float trackStart, float trackWidth)
    {
        var normalized = (x - trackStart) / trackWidth;
        normalized = Math.Clamp(normalized, 0f, 1f);
        var newValue = Minimum + (normalized * (Maximum - Minimum));

        if (Math.Abs(newValue - Value) > 0.001)
        {
            Value = newValue;
            ValueChanged?.Invoke(this, Value);
        }
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear(SKColors.Transparent);

        var scale = (float)DeviceDisplay.MainDisplayInfo.Density;
        var trackHeight = 10f * scale;
        var trackPadding = 20f * scale;
        var thumbRadius = 12f * scale;

        var trackY = (info.Height - trackHeight) / 2f;
        var trackLeft = trackPadding;
        var trackRight = info.Width - trackPadding;
        var trackWidth = trackRight - trackLeft;

        // Draw track (inset)
        DrawTrack(canvas, trackLeft, trackY, trackWidth, trackHeight, scale);

        // Draw fill
        var fillPercentage = (float)((Value - Minimum) / (Maximum - Minimum));
        DrawFill(canvas, trackLeft, trackY, trackWidth * fillPercentage, trackHeight, scale);

        // Draw thumb
        var thumbX = trackLeft + (trackWidth * fillPercentage);
        var thumbY = info.Height / 2f;
        DrawThumb(canvas, thumbX, thumbY, thumbRadius, scale);
    }

    private void DrawTrack(SKCanvas canvas, float x, float y, float width, float height, float scale)
    {
        var cornerRadius = height / 2f;
        var bounds = new SKRect(x, y, x + width, y + height);

        // Inset shadow effect
        using var basePaint = new SKPaint
        {
            Color = NeomorphicColors.Background,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRoundRect(bounds, cornerRadius, cornerRadius, basePaint);

        canvas.Save();
        using var clipPath = new SKPath();
        clipPath.AddRoundRect(bounds, cornerRadius, cornerRadius);
        canvas.ClipPath(clipPath);

        using var shadowPaint = new SKPaint
        {
            Color = NeomorphicColors.ShadowDark.WithAlpha(150),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 4f * scale,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3f * scale)
        };
        canvas.DrawRoundRect(bounds, cornerRadius, cornerRadius, shadowPaint);

        canvas.Restore();
    }

    private void DrawFill(SKCanvas canvas, float x, float y, float width, float height, float scale)
    {
        if (width <= 0) return;

        var cornerRadius = height / 2f;
        var bounds = new SKRect(x, y, x + width, y + height);

        using var fillPaint = new SKPaint
        {
            Color = NeomorphicColors.Accent,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRoundRect(bounds, cornerRadius, cornerRadius, fillPaint);
    }

    private void DrawThumb(SKCanvas canvas, float x, float y, float radius, float scale)
    {
        var offset = 3f * scale;
        var blur = 6f * scale;

        // Dark shadow
        using var darkPaint = new SKPaint
        {
            Color = NeomorphicColors.ShadowDark,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 2)
        };
        canvas.DrawCircle(x + offset, y + offset, radius, darkPaint);

        // Light shadow
        using var lightPaint = new SKPaint
        {
            Color = NeomorphicColors.ShadowLight,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 2)
        };
        canvas.DrawCircle(x - offset, y - offset, radius, lightPaint);

        // Thumb
        using var thumbPaint = new SKPaint
        {
            Color = NeomorphicColors.Background,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawCircle(x, y, radius, thumbPaint);
    }
}
