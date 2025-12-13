using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace RoMoteNoAds.Controls;

/// <summary>
/// A neomorphic sidebar for tablet navigation.
/// </summary>
public class NeumorphicSidebar : SKCanvasView
{
    private readonly List<SidebarItem> _items = new();
    private int _selectedIndex;

    public class SidebarItem
    {
        public required string Id { get; init; }
        public required string Label { get; init; }
        public required string Icon { get; init; }
    }

    #region Bindable Properties

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem), typeof(string), typeof(NeumorphicSidebar), null,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedItemChanged);

    public string? SelectedItem
    {
        get => (string?)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    #endregion

    public event EventHandler<string>? ItemSelected;

    public NeumorphicSidebar()
    {
        EnableTouchEvents = true;
        Touch += OnTouch;
        WidthRequest = 100;

        // Default items
        _items.Add(new SidebarItem { Id = "remote", Label = "Remote", Icon = "R" });
        _items.Add(new SidebarItem { Id = "channels", Label = "Channels", Icon = "C" });
        _items.Add(new SidebarItem { Id = "shortcuts", Label = "Shortcuts", Icon = "S" });
    }

    private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var sidebar = (NeumorphicSidebar)bindable;
        var newItem = newValue as string;
        var index = sidebar._items.FindIndex(i => i.Id == newItem);
        if (index >= 0)
        {
            sidebar._selectedIndex = index;
            sidebar.InvalidateSurface();
        }
    }

    public void SetItems(IEnumerable<SidebarItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
        InvalidateSurface();
    }

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        if (e.ActionType == SKTouchAction.Released)
        {
            var scale = (float)DeviceDisplay.MainDisplayInfo.Density;
            var itemHeight = 80f * scale;
            var topPadding = 32f * scale;

            var tappedIndex = (int)((e.Location.Y - topPadding) / itemHeight);

            if (tappedIndex >= 0 && tappedIndex < _items.Count && tappedIndex != _selectedIndex)
            {
                _selectedIndex = tappedIndex;
                SelectedItem = _items[tappedIndex].Id;
                ItemSelected?.Invoke(this, _items[tappedIndex].Id);
                InvalidateSurface();
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

        // Draw background
        using var bgPaint = new SKPaint
        {
            Color = NeomorphicColors.Background,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRect(0, 0, info.Width, info.Height, bgPaint);

        // Draw right border shadow
        using var borderPaint = new SKPaint
        {
            Color = NeomorphicColors.ShadowDark.WithAlpha(50),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f * scale
        };
        canvas.DrawLine(info.Width - 1, 0, info.Width - 1, info.Height, borderPaint);

        // Draw items
        var itemHeight = 80f * scale;
        var topPadding = 32f * scale;

        for (int i = 0; i < _items.Count; i++)
        {
            var isSelected = i == _selectedIndex;
            var itemRect = new SKRect(
                0,
                topPadding + (i * itemHeight),
                info.Width,
                topPadding + ((i + 1) * itemHeight));
            DrawItem(canvas, _items[i], itemRect, isSelected, scale);
        }
    }

    private void DrawItem(SKCanvas canvas, SidebarItem item, SKRect bounds, bool isSelected, float scale)
    {
        var centerX = bounds.MidX;
        var iconY = bounds.MidY - 10f * scale;
        var labelY = bounds.MidY + 24f * scale;

        // Button background
        var buttonSize = 56f * scale;
        var buttonRect = new SKRect(
            centerX - buttonSize / 2,
            iconY - buttonSize / 2 + 4f * scale,
            centerX + buttonSize / 2,
            iconY + buttonSize / 2 + 4f * scale);

        if (isSelected)
        {
            // Inset effect
            using var insetPaint = new SKPaint
            {
                Color = NeomorphicColors.ShadowDark.WithAlpha(100),
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6f * scale)
            };
            canvas.DrawRoundRect(buttonRect, 16f * scale, 16f * scale, insetPaint);
        }
        else
        {
            // Outset effect
            var offset = 4f * scale;
            var blur = 8f * scale;

            using var darkPaint = new SKPaint
            {
                Color = NeomorphicColors.ShadowDark.WithAlpha(80),
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 2)
            };
            var darkRect = new SKRect(buttonRect.Left + offset, buttonRect.Top + offset,
                                       buttonRect.Right + offset, buttonRect.Bottom + offset);
            canvas.DrawRoundRect(darkRect, 16f * scale, 16f * scale, darkPaint);

            using var lightPaint = new SKPaint
            {
                Color = NeomorphicColors.ShadowLight.WithAlpha(150),
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur / 2)
            };
            var lightRect = new SKRect(buttonRect.Left - offset, buttonRect.Top - offset,
                                        buttonRect.Right - offset, buttonRect.Bottom - offset);
            canvas.DrawRoundRect(lightRect, 16f * scale, 16f * scale, lightPaint);

            using var bgPaint = new SKPaint
            {
                Color = NeomorphicColors.Background,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRoundRect(buttonRect, 16f * scale, 16f * scale, bgPaint);
        }

        // Icon - using modern SKFont API
        var iconColor = isSelected ? NeomorphicColors.Accent : NeomorphicColors.TextSecondary;
        using var iconTypeface = SKTypeface.FromFamilyName("SF Pro", isSelected ? SKFontStyle.Bold : SKFontStyle.Normal);
        using var iconFont = new SKFont(iconTypeface, 24f * scale);
        using var iconPaint = new SKPaint
        {
            Color = iconColor,
            IsAntialias = true
        };
        canvas.DrawText(item.Icon, centerX, iconY + 8f * scale, SKTextAlign.Center, iconFont, iconPaint);

        // Label - using modern SKFont API
        var labelColor = isSelected ? NeomorphicColors.Accent : NeomorphicColors.TextSecondary;
        using var labelTypeface = SKTypeface.FromFamilyName("SF Pro", SKFontStyle.Bold);
        using var labelFont = new SKFont(labelTypeface, 9f * scale);
        using var labelPaint = new SKPaint
        {
            Color = labelColor,
            IsAntialias = true
        };
        canvas.DrawText(item.Label, centerX, labelY, SKTextAlign.Center, labelFont, labelPaint);
    }
}
