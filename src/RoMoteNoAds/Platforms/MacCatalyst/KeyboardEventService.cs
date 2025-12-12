using Foundation;
using UIKit;
using ObjCRuntime;

namespace RoMoteNoAds.Platforms.MacCatalyst;

/// <summary>
/// Service for handling keyboard events on Mac Catalyst.
/// Uses pressesBegan/pressesEnded for hardware key detection.
/// </summary>
public static class KeyboardEventService
{
    public static event EventHandler<KeyboardEventArgs>? KeyPressed;

    public static void RaiseKeyPressed(UIKeyboardHidUsage keyCode, string? characters, UIKeyModifierFlags modifiers)
    {
        KeyPressed?.Invoke(null, new KeyboardEventArgs(keyCode, characters, modifiers));
    }
}

public class KeyboardEventArgs : EventArgs
{
    public UIKeyboardHidUsage KeyCode { get; }
    public string? Characters { get; }
    public UIKeyModifierFlags Modifiers { get; }

    public KeyboardEventArgs(UIKeyboardHidUsage keyCode, string? characters, UIKeyModifierFlags modifiers)
    {
        KeyCode = keyCode;
        Characters = characters;
        Modifiers = modifiers;
    }
}
