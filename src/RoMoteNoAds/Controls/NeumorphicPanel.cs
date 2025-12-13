using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace RoMoteNoAds.Controls;

/// <summary>
/// A neomorphic panel with inset (recessed) shadow effect.
/// Used for grouping controls like media buttons.
/// </summary>
[ContentProperty(nameof(Content))]
public class NeumorphicPanel : ContentView
{
    private readonly SKCanvasView _canvasView;

    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
        nameof(CornerRadius), typeof(float), typeof(NeumorphicPanel), 24f,
        propertyChanged: (b, _, _) => ((NeumorphicPanel)b)._canvasView.InvalidateSurface());

    public static readonly BindableProperty PaddingInsetProperty = BindableProperty.Create(
        nameof(PaddingInset), typeof(Thickness), typeof(NeumorphicPanel), new Thickness(20),
        propertyChanged: OnPaddingInsetChanged);

    public float CornerRadius
    {
        get => (float)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Thickness PaddingInset
    {
        get => (Thickness)GetValue(PaddingInsetProperty);
        set => SetValue(PaddingInsetProperty, value);
    }

    public NeumorphicPanel()
    {
        _canvasView = new SKCanvasView();
        _canvasView.PaintSurface += OnPaintSurface;

        // Use a Grid to layer the canvas behind the content
        var grid = new Grid();
        grid.Children.Add(_canvasView);

        // ContentPresenter will be added when Content is set
        base.Content = grid;
        Padding = PaddingInset;
    }

    private static void OnPaddingInsetChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var panel = (NeumorphicPanel)bindable;
        panel.Padding = (Thickness)newValue;
    }

    // Shadow the base Content property to add to our grid
    public new View? Content
    {
        get => _contentView;
        set
        {
            if (_contentView != null && base.Content is Grid grid)
            {
                grid.Children.Remove(_contentView);
            }
            _contentView = value;
            if (_contentView != null && base.Content is Grid g)
            {
                g.Children.Add(_contentView);
            }
        }
    }
    private View? _contentView;

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear(SKColors.Transparent);

        var scale = (float)DeviceDisplay.MainDisplayInfo.Density;
        var padding = 8f * scale;
        var cornerRadius = CornerRadius * scale;

        var bounds = new SKRect(padding, padding,
                                 info.Width - padding, info.Height - padding);

        // Draw inset effect
        DrawInsetPanel(canvas, bounds, cornerRadius, scale);
    }

    private void DrawInsetPanel(SKCanvas canvas, SKRect bounds, float cornerRadius, float scale)
    {
        var offset = NeomorphicColors.ShadowOffsetIn * scale;
        var blur = NeomorphicColors.ShadowBlurIn * scale;

        // Draw base shape
        using var basePaint = new SKPaint
        {
            Color = NeomorphicColors.Background,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRoundRect(bounds, cornerRadius, cornerRadius, basePaint);

        // Clip to shape for inset shadows
        canvas.Save();
        using var clipPath = new SKPath();
        clipPath.AddRoundRect(bounds, cornerRadius, cornerRadius);
        canvas.ClipPath(clipPath);

        // Dark inner shadow (top-left)
        using var darkPaint = new SKPaint
        {
            Color = NeomorphicColors.ShadowDark.WithAlpha(150),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = blur,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 2)
        };
        var darkRect = new SKRect(bounds.Left + offset, bounds.Top + offset,
                                   bounds.Right - offset, bounds.Bottom - offset);
        canvas.DrawRoundRect(darkRect, cornerRadius - offset, cornerRadius - offset, darkPaint);

        // Light inner highlight (bottom-right)
        using var lightPaint = new SKPaint
        {
            Color = NeomorphicColors.ShadowLight.WithAlpha(100),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = blur / 2,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 3)
        };
        var lightRect = new SKRect(bounds.Left + offset * 2, bounds.Top + offset * 2,
                                    bounds.Right, bounds.Bottom);
        canvas.DrawRoundRect(lightRect, cornerRadius, cornerRadius, lightPaint);

        canvas.Restore();
    }
}
