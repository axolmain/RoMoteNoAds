using System.Globalization;

namespace RoMoteNoAds.Converters;

/// <summary>
/// Converts a boolean value to a color (used for active/selected states).
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive && isActive)
        {
            // Return primary color for active state
            if (Application.Current?.Resources.TryGetValue("Primary", out var primaryColor) == true)
            {
                return primaryColor;
            }
            return Colors.Purple;
        }

        // Return transparent for inactive state
        return Colors.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
