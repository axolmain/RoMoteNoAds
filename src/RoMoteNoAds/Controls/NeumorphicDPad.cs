using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace RoMoteNoAds.Controls;

/// <summary>
/// A circular neomorphic D-Pad with directional buttons and center OK button.
/// Matches the prototype design with concentric circles.
/// </summary>
public class NeumorphicDPad : SKCanvasView
{
    private DPadRegion _pressedRegion = DPadRegion.None;
    private DateTime _lastTapTime = DateTime.MinValue;

    private enum DPadRegion { None, Up, Down, Left, Right, Center }

    #region Bindable Properties

    public static readonly BindableProperty UpCommandProperty = BindableProperty.Create(
        nameof(UpCommand), typeof(System.Windows.Input.ICommand), typeof(NeumorphicDPad), null);

    public static readonly BindableProperty DownCommandProperty = BindableProperty.Create(
        nameof(DownCommand), typeof(System.Windows.Input.ICommand), typeof(NeumorphicDPad), null);

    public static readonly BindableProperty LeftCommandProperty = BindableProperty.Create(
        nameof(LeftCommand), typeof(System.Windows.Input.ICommand), typeof(NeumorphicDPad), null);

    public static readonly BindableProperty RightCommandProperty = BindableProperty.Create(
        nameof(RightCommand), typeof(System.Windows.Input.ICommand), typeof(NeumorphicDPad), null);

    public static readonly BindableProperty SelectCommandProperty = BindableProperty.Create(
        nameof(SelectCommand), typeof(System.Windows.Input.ICommand), typeof(NeumorphicDPad), null);

    public System.Windows.Input.ICommand? UpCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(UpCommandProperty);
        set => SetValue(UpCommandProperty, value);
    }

    public System.Windows.Input.ICommand? DownCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(DownCommandProperty);
        set => SetValue(DownCommandProperty, value);
    }

    public System.Windows.Input.ICommand? LeftCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(LeftCommandProperty);
        set => SetValue(LeftCommandProperty, value);
    }

    public System.Windows.Input.ICommand? RightCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(RightCommandProperty);
        set => SetValue(RightCommandProperty, value);
    }

    public System.Windows.Input.ICommand? SelectCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(SelectCommandProperty);
        set => SetValue(SelectCommandProperty, value);
    }

    #endregion

    public event EventHandler? UpPressed;
    public event EventHandler? DownPressed;
    public event EventHandler? LeftPressed;
    public event EventHandler? RightPressed;
    public event EventHandler? SelectPressed;

    public NeumorphicDPad()
    {
        EnableTouchEvents = true;
        Touch += OnTouch;
    }

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        var info = CanvasSize;
        var scale = (float)DeviceDisplay.MainDisplayInfo.Density;
        var center = new SKPoint(info.Width / 2f, info.Height / 2f);
        var touchPoint = e.Location;

        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                _pressedRegion = GetRegion(touchPoint, center, info.Width / 2f, scale);
                InvalidateSurface();
                e.Handled = true;
                break;

            case SKTouchAction.Released:
                if (_pressedRegion != DPadRegion.None)
                {
                    var releasedRegion = GetRegion(touchPoint, center, info.Width / 2f, scale);
                    if (releasedRegion == _pressedRegion &&
                        (DateTime.Now - _lastTapTime).TotalMilliseconds > 100)
                    {
                        _lastTapTime = DateTime.Now;
                        ExecuteCommand(_pressedRegion);
                    }
                    _pressedRegion = DPadRegion.None;
                    InvalidateSurface();
                }
                e.Handled = true;
                break;

            case SKTouchAction.Cancelled:
            case SKTouchAction.Exited:
                _pressedRegion = DPadRegion.None;
                InvalidateSurface();
                e.Handled = true;
                break;
        }
    }

    private DPadRegion GetRegion(SKPoint point, SKPoint center, float radius, float scale)
    {
        var dx = point.X - center.X;
        var dy = point.Y - center.Y;
        var distance = MathF.Sqrt(dx * dx + dy * dy);

        // Center button radius (about 30% of total)
        var centerRadius = radius * 0.30f;
        if (distance <= centerRadius)
            return DPadRegion.Center;

        // Outside the outer circle
        if (distance > radius - 10 * scale)
            return DPadRegion.None;

        // Determine direction based on angle
        var angle = MathF.Atan2(dy, dx) * 180f / MathF.PI;

        // Up: -135 to -45 degrees
        if (angle >= -135 && angle < -45)
            return DPadRegion.Up;
        // Down: 45 to 135 degrees
        if (angle >= 45 && angle < 135)
            return DPadRegion.Down;
        // Right: -45 to 45 degrees
        if (angle >= -45 && angle < 45)
            return DPadRegion.Right;
        // Left: 135 to 180 or -180 to -135
        return DPadRegion.Left;
    }

    private void ExecuteCommand(DPadRegion region)
    {
        switch (region)
        {
            case DPadRegion.Up:
                UpPressed?.Invoke(this, EventArgs.Empty);
                if (UpCommand?.CanExecute(null) == true) UpCommand.Execute(null);
                break;
            case DPadRegion.Down:
                DownPressed?.Invoke(this, EventArgs.Empty);
                if (DownCommand?.CanExecute(null) == true) DownCommand.Execute(null);
                break;
            case DPadRegion.Left:
                LeftPressed?.Invoke(this, EventArgs.Empty);
                if (LeftCommand?.CanExecute(null) == true) LeftCommand.Execute(null);
                break;
            case DPadRegion.Right:
                RightPressed?.Invoke(this, EventArgs.Empty);
                if (RightCommand?.CanExecute(null) == true) RightCommand.Execute(null);
                break;
            case DPadRegion.Center:
                SelectPressed?.Invoke(this, EventArgs.Empty);
                if (SelectCommand?.CanExecute(null) == true) SelectCommand.Execute(null);
                break;
        }
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear(SKColors.Transparent);

        var scale = (float)DeviceDisplay.MainDisplayInfo.Density;
        var size = Math.Min(info.Width, info.Height);
        var center = new SKPoint(info.Width / 2f, info.Height / 2f);
        var outerRadius = (size / 2f) - (15f * scale);
        var centerRadius = outerRadius * 0.30f;

        // Draw outer ring with shadow
        DrawOuterRing(canvas, center, outerRadius, scale);

        // Draw directional indicators
        DrawDirectionalArrows(canvas, center, outerRadius, centerRadius, scale);

        // Draw center OK button
        DrawCenterButton(canvas, center, centerRadius, scale);
    }

    private void DrawOuterRing(SKCanvas canvas, SKPoint center, float radius, float scale)
    {
        var offset = NeomorphicColors.ShadowOffsetOut * scale;
        var blur = NeomorphicColors.ShadowBlurOut * scale;

        // Dark shadow
        using var darkPaint = new SKPaint
        {
            Color = NeomorphicColors.ShadowDark,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 2)
        };
        canvas.DrawCircle(center.X + offset, center.Y + offset, radius, darkPaint);

        // Light shadow
        using var lightPaint = new SKPaint
        {
            Color = NeomorphicColors.ShadowLight,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 2)
        };
        canvas.DrawCircle(center.X - offset, center.Y - offset, radius, lightPaint);

        // Main circle
        using var fillPaint = new SKPaint
        {
            Color = NeomorphicColors.Background,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawCircle(center, radius, fillPaint);
    }

    private void DrawDirectionalArrows(SKCanvas canvas, SKPoint center, float outerRadius, float centerRadius, float scale)
    {
        var arrowDistance = (outerRadius + centerRadius) / 2f;
        var arrowSize = 18f * scale;

        var positions = new[]
        {
            (DPadRegion.Up, new SKPoint(center.X, center.Y - arrowDistance), "^"),
            (DPadRegion.Down, new SKPoint(center.X, center.Y + arrowDistance), "v"),
            (DPadRegion.Left, new SKPoint(center.X - arrowDistance, center.Y), "<"),
            (DPadRegion.Right, new SKPoint(center.X + arrowDistance, center.Y), ">"),
        };

        foreach (var (region, pos, symbol) in positions)
        {
            var isPressed = _pressedRegion == region;
            var color = isPressed ? NeomorphicColors.Accent : NeomorphicColors.TextSecondary;

            using var typeface = SKTypeface.FromFamilyName("SF Pro", SKFontStyle.Bold);
            using var font = new SKFont(typeface, arrowSize);
            using var paint = new SKPaint
            {
                Color = color,
                IsAntialias = true
            };

            var textBounds = new SKRect();
            font.MeasureText(symbol, out textBounds);
            canvas.DrawText(symbol, pos.X, pos.Y - textBounds.MidY, SKTextAlign.Center, font, paint);
        }
    }

    private void DrawCenterButton(SKCanvas canvas, SKPoint center, float radius, float scale)
    {
        var isPressed = _pressedRegion == DPadRegion.Center;
        var offset = NeomorphicColors.ShadowOffsetOut * scale * 0.5f;
        var blur = NeomorphicColors.ShadowBlurOut * scale * 0.5f;

        if (isPressed)
        {
            // Inset shadow for pressed state
            using var basePaint = new SKPaint
            {
                Color = NeomorphicColors.Background,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawCircle(center, radius, basePaint);

            canvas.Save();
            using var clipPath = new SKPath();
            clipPath.AddCircle(center.X, center.Y, radius);
            canvas.ClipPath(clipPath);

            using var insetPaint = new SKPaint
            {
                Color = NeomorphicColors.ShadowDark.WithAlpha(150),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = blur,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 2)
            };
            canvas.DrawCircle(center.X + offset, center.Y + offset, radius - offset, insetPaint);
            canvas.Restore();
        }
        else
        {
            // Outset shadow
            using var darkPaint = new SKPaint
            {
                Color = NeomorphicColors.ShadowDark,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 2)
            };
            canvas.DrawCircle(center.X + offset, center.Y + offset, radius, darkPaint);

            using var lightPaint = new SKPaint
            {
                Color = NeomorphicColors.ShadowLight,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 2)
            };
            canvas.DrawCircle(center.X - offset, center.Y - offset, radius, lightPaint);

            using var fillPaint = new SKPaint
            {
                Color = NeomorphicColors.Background,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawCircle(center, radius, fillPaint);
        }

        // OK text
        using var typeface = SKTypeface.FromFamilyName("SF Pro", SKFontStyle.Bold);
        using var font = new SKFont(typeface, 16f * scale);
        using var textPaint = new SKPaint
        {
            Color = isPressed ? NeomorphicColors.Accent : NeomorphicColors.TextPrimary,
            IsAntialias = true
        };

        var textBounds = new SKRect();
        font.MeasureText("OK", out textBounds);
        canvas.DrawText("OK", center.X, center.Y - textBounds.MidY, SKTextAlign.Center, font, textPaint);
    }
}
