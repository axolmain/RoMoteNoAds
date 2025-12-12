namespace RoMote.Roku.Models;

/// <summary>
/// Roku ECP key codes for remote control commands.
/// </summary>
public static class RokuKey
{
    // Navigation
    public const string Home = "Home";
    public const string Back = "Back";
    public const string Select = "Select";
    public const string Up = "Up";
    public const string Down = "Down";
    public const string Left = "Left";
    public const string Right = "Right";

    // Playback
    public const string Play = "Play";
    public const string Pause = "Play"; // Same as Play (toggle)
    public const string Rev = "Rev";
    public const string Fwd = "Fwd";
    public const string InstantReplay = "InstantReplay";

    // Info/Search
    public const string Info = "Info";
    public const string Search = "Search";

    // TV Controls (check device capabilities first)
    public const string VolumeUp = "VolumeUp";
    public const string VolumeDown = "VolumeDown";
    public const string VolumeMute = "VolumeMute";
    public const string Power = "Power";
    public const string PowerOff = "PowerOff";
    public const string PowerOn = "PowerOn";

    // TV Channel Controls
    public const string ChannelUp = "ChannelUp";
    public const string ChannelDown = "ChannelDown";

    // TV Input Controls
    public const string InputTuner = "InputTuner";
    public const string InputHDMI1 = "InputHDMI1";
    public const string InputHDMI2 = "InputHDMI2";
    public const string InputHDMI3 = "InputHDMI3";
    public const string InputHDMI4 = "InputHDMI4";
    public const string InputAV1 = "InputAV1";

    // Keyboard
    public const string Backspace = "Backspace";
    public const string Enter = "Enter";

    // Find Remote
    public const string FindRemote = "FindRemote";

    /// <summary>
    /// Generates a literal character key code for keyboard input.
    /// Characters are URL-encoded as needed.
    /// </summary>
    /// <param name="character">The character to send.</param>
    /// <returns>The key code in Lit_{character} format.</returns>
    public static string Literal(char character)
    {
        var encoded = Uri.EscapeDataString(character.ToString());
        return $"Lit_{encoded}";
    }
}
