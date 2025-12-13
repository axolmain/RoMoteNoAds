namespace RoMoteNoAds.Services;

/// <summary>
/// Service to capture hardware volume button presses on iOS devices.
/// When volume buttons are pressed, this service fires events that can be used
/// to control Roku device volume instead of (or in addition to) system volume.
/// </summary>
public interface IVolumeButtonService
{
    /// <summary>
    /// Fired when the hardware volume up button is pressed.
    /// </summary>
    event EventHandler? VolumeUpPressed;

    /// <summary>
    /// Fired when the hardware volume down button is pressed.
    /// </summary>
    event EventHandler? VolumeDownPressed;

    /// <summary>
    /// Gets whether the service is currently listening for volume button presses.
    /// </summary>
    bool IsListening { get; }

    /// <summary>
    /// Start listening for hardware volume button presses.
    /// Should be called when the remote view becomes active.
    /// </summary>
    void StartListening();

    /// <summary>
    /// Stop listening for hardware volume button presses.
    /// Should be called when the remote view becomes inactive.
    /// </summary>
    void StopListening();
}

/// <summary>
/// Default no-op implementation for platforms that don't support volume button capture.
/// </summary>
public class NullVolumeButtonService : IVolumeButtonService
{
#pragma warning disable CS0067 // Event is never used (expected for no-op implementation)
    public event EventHandler? VolumeUpPressed;
    public event EventHandler? VolumeDownPressed;
#pragma warning restore CS0067

    public bool IsListening => false;

    public void StartListening() { }
    public void StopListening() { }
}
