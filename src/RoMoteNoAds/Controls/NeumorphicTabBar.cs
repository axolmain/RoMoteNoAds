using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace RoMoteNoAds.Controls;

/// <summary>
/// A neomorphic bottom tab bar for phone navigation.
/// </summary>
public class NeumorphicTabBar : SKCanvasView
{
    private readonly List<TabItem> _tabs = new();
    private int _selectedIndex;

    public class TabItem
    {
        public required string Id { get; init; }
        public required string Label { get; init; }
        public required string Icon { get; init; }
    }

    #region Bindable Properties

    public static readonly BindableProperty SelectedTabProperty = BindableProperty.Create(
        nameof(SelectedTab), typeof(string), typeof(NeumorphicTabBar), null,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedTabChanged);

    public string? SelectedTab
    {
        get => (string?)GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }

    #endregion

    public event EventHandler<string>? TabSelected;

    public NeumorphicTabBar()
    {
        EnableTouchEvents = true;
        Touch += OnTouch;
        HeightRequest = 90;

        // Default tabs
        _tabs.Add(new TabItem { Id = "remote", Label = "Remote", Icon = "R" });
        _tabs.Add(new TabItem { Id = "channels", Label = "Channels", Icon = "C" });
        _tabs.Add(new TabItem { Id = "shortcuts", Label = "Shortcuts", Icon = "S" });
    }

    private static void OnSelectedTabChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var tabBar = (NeumorphicTabBar)bindable;
        var newTab = newValue as string;
        var index = tabBar._tabs.FindIndex(t => t.Id == newTab);
        if (index >= 0)
        {
            tabBar._selectedIndex = index;
            tabBar.InvalidateSurface();
        }
    }

    public void SetTabs(IEnumerable<TabItem> tabs)
    {
        _tabs.Clear();
        _tabs.AddRange(tabs);
        InvalidateSurface();
    }

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        if (e.ActionType == SKTouchAction.Released)
        {
            var info = CanvasSize;
            var tabWidth = info.Width / _tabs.Count;
            var tappedIndex = (int)(e.Location.X / tabWidth);

            if (tappedIndex >= 0 && tappedIndex < _tabs.Count && tappedIndex != _selectedIndex)
            {
                _selectedIndex = tappedIndex;
                SelectedTab = _tabs[tappedIndex].Id;
                TabSelected?.Invoke(this, _tabs[tappedIndex].Id);
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

        // Draw background with top shadow
        DrawBackground(canvas, info, scale);

        // Draw tabs
        var tabWidth = info.Width / (float)_tabs.Count;
        for (int i = 0; i < _tabs.Count; i++)
        {
            var isSelected = i == _selectedIndex;
            var tabRect = new SKRect(i * tabWidth, 0, (i + 1) * tabWidth, info.Height);
            DrawTab(canvas, _tabs[i], tabRect, isSelected, scale);
        }
    }

    private void DrawBackground(SKCanvas canvas, SKImageInfo info, float scale)
    {
        var cornerRadius = 24f * scale;
        var bounds = new SKRect(0, 0, info.Width, info.Height + cornerRadius);

        // Shadow
        var shadowOffset = 6f * scale;
        var shadowBlur = 12f * scale;

        using var shadowPaint = new SKPaint
        {
            Color = NeomorphicColors.ShadowDark.WithAlpha(100),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, shadowBlur / 2)
        };
        var shadowBounds = new SKRect(bounds.Left, bounds.Top - shadowOffset,
                                       bounds.Right, bounds.Bottom);
        canvas.DrawRoundRect(shadowBounds, cornerRadius, cornerRadius, shadowPaint);

        // Background
        using var bgPaint = new SKPaint
        {
            Color = NeomorphicColors.Background,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRoundRect(bounds, cornerRadius, cornerRadius, bgPaint);
    }

    private void DrawTab(SKCanvas canvas, TabItem tab, SKRect bounds, bool isSelected, float scale)
    {
        var centerX = bounds.MidX;
        var iconY = bounds.Top + 20f * scale;
        var labelY = bounds.Top + 60f * scale;

        // Icon background (selected = inset, unselected = flat)
        if (isSelected)
        {
            var iconBgSize = 40f * scale;
            var iconBgRect = new SKRect(
                centerX - iconBgSize / 2,
                iconY - iconBgSize / 2 + 8f * scale,
                centerX + iconBgSize / 2,
                iconY + iconBgSize / 2 + 8f * scale);

            using var insetPaint = new SKPaint
            {
                Color = NeomorphicColors.ShadowDark.WithAlpha(80),
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4f * scale)
            };
            canvas.DrawRoundRect(iconBgRect, 12f * scale, 12f * scale, insetPaint);
        }

        // Icon
        using var iconTypeface = SKTypeface.FromFamilyName("SF Pro", isSelected ? SKFontStyle.Bold : SKFontStyle.Normal);
        using var iconFont = new SKFont(iconTypeface, 24f * scale);
        using var iconPaint = new SKPaint
        {
            Color = isSelected ? NeomorphicColors.Accent : NeomorphicColors.TextSecondary,
            IsAntialias = true
        };
        canvas.DrawText(tab.Icon, centerX, iconY + 8f * scale, SKTextAlign.Center, iconFont, iconPaint);

        // Label
        using var labelTypeface = SKTypeface.FromFamilyName("SF Pro", SKFontStyle.Bold);
        using var labelFont = new SKFont(labelTypeface, 10f * scale);
        using var labelPaint = new SKPaint
        {
            Color = isSelected ? NeomorphicColors.Accent : NeomorphicColors.TextSecondary,
            IsAntialias = true
        };
        canvas.DrawText(tab.Label, centerX, labelY, SKTextAlign.Center, labelFont, labelPaint);
    }
}
