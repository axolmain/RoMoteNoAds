using AVFoundation;
using Foundation;
using RoMoteNoAds.Services;

namespace RoMoteNoAds.Platforms.iOS;

/// <summary>
/// iOS implementation of IVolumeButtonService that captures hardware volume button presses
/// by observing changes to the system output volume via AVAudioSession.
/// </summary>
public class VolumeButtonService : IVolumeButtonService, IDisposable
{
    private float _lastVolume;
    private IDisposable? _volumeObserver;
    private bool _isListening;
    private bool _disposed;

    public event EventHandler? VolumeUpPressed;
    public event EventHandler? VolumeDownPressed;

    public bool IsListening => _isListening;

    public void StartListening()
    {
        if (_isListening || _disposed)
            return;

        try
        {
            var session = AVAudioSession.SharedInstance();

            // Set up the audio session to allow volume observation
            // Use Ambient category so we don't interrupt other audio
            session.SetCategory(AVAudioSessionCategory.Ambient);
            session.SetActive(true);

            // Store initial volume
            _lastVolume = session.OutputVolume;

            // Observe volume changes using KVO
            _volumeObserver = session.AddObserver(
                "outputVolume",
                NSKeyValueObservingOptions.New,
                OnVolumeChanged);

            _isListening = true;
            System.Diagnostics.Debug.WriteLine($"[VolumeButtonService] Started listening, initial volume: {_lastVolume}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VolumeButtonService] Failed to start: {ex.Message}");
        }
    }

    public void StopListening()
    {
        if (!_isListening)
            return;

        try
        {
            _volumeObserver?.Dispose();
            _volumeObserver = null;

            _isListening = false;
            System.Diagnostics.Debug.WriteLine("[VolumeButtonService] Stopped listening");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VolumeButtonService] Failed to stop: {ex.Message}");
        }
    }

    private void OnVolumeChanged(NSObservedChange change)
    {
        try
        {
            var newVolume = AVAudioSession.SharedInstance().OutputVolume;

            // Determine direction based on delta
            if (newVolume > _lastVolume)
            {
                System.Diagnostics.Debug.WriteLine($"[VolumeButtonService] Volume UP: {_lastVolume} -> {newVolume}");
                MainThread.BeginInvokeOnMainThread(() => VolumeUpPressed?.Invoke(this, EventArgs.Empty));
            }
            else if (newVolume < _lastVolume)
            {
                System.Diagnostics.Debug.WriteLine($"[VolumeButtonService] Volume DOWN: {_lastVolume} -> {newVolume}");
                MainThread.BeginInvokeOnMainThread(() => VolumeDownPressed?.Invoke(this, EventArgs.Empty));
            }

            _lastVolume = newVolume;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VolumeButtonService] Error in volume change handler: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        StopListening();
    }
}
