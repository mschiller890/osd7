using System;
using NAudio.CoreAudioApi;

namespace osd7
{
    /// <summary>
    /// Event-driven audio listener service using NAudio CoreAudio API.
    /// Monitors system volume changes via MMDevice.AudioEndpointVolume.OnVolumeNotification.
    /// No polling - all events are triggered by OS audio subsystem.
    /// </summary>
    public class AudioListenerService : IDisposable
    {
        private MMDeviceEnumerator _deviceEnumerator;
        private MMDevice _device;

        /// <summary>
        /// Fired when system volume level changes (0.0 to 1.0).
        /// </summary>
        public event EventHandler<VolumeChangedEventArgs> VolumeChanged;

        /// <summary>
        /// Fired when mute state changes.
        /// </summary>
        public event EventHandler<MuteChangedEventArgs> MuteChanged;

        public AudioListenerService()
        {
            _deviceEnumerator = new MMDeviceEnumerator();
        }

        /// <summary>
        /// Initializes the audio listener on the default playback device.
        /// Must be called before the service can detect volume changes.
        /// </summary>
        public void Initialize()
        {
            try
            {
                _device = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

                if (_device != null)
                {
                    _device.AudioEndpointVolume.OnVolumeNotification += OnVolumeNotification;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AudioListenerService.Initialize failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current system volume level (0.0 to 1.0).
        /// </summary>
        public float GetCurrentVolume()
        {
            if (_device?.AudioEndpointVolume != null)
            {
                return _device.AudioEndpointVolume.MasterVolumeLevelScalar;
            }
            return 0f;
        }

        /// <summary>
        /// Gets the current mute state.
        /// </summary>
        public bool GetIsMuted()
        {
            if (_device?.AudioEndpointVolume != null)
            {
                return _device.AudioEndpointVolume.Mute;
            }
            return false;
        }

        private void OnVolumeNotification(AudioVolumeNotificationData data)
        {
            // Raised on a thread pool thread; UI code must marshal to the UI thread.
            if (_device?.AudioEndpointVolume != null)
            {
                float currentVolume = _device.AudioEndpointVolume.MasterVolumeLevelScalar;
                bool isMuted = _device.AudioEndpointVolume.Mute;

                VolumeChanged?.Invoke(this, new VolumeChangedEventArgs { Volume = currentVolume, IsMuted = isMuted });

                if (data.Muted != isMuted)
                {
                    MuteChanged?.Invoke(this, new MuteChangedEventArgs { IsMuted = isMuted });
                }
            }
        }

        public void Dispose()
        {
            if (_device?.AudioEndpointVolume != null)
            {
                _device.AudioEndpointVolume.OnVolumeNotification -= OnVolumeNotification;
            }

            _device?.Dispose();
            _deviceEnumerator?.Dispose();
        }
    }

    /// <summary>
    /// Event arguments for volume changes.
    /// </summary>
    public class VolumeChangedEventArgs : EventArgs
    {
        public float Volume { get; set; }
        public bool IsMuted { get; set; }
    }

    /// <summary>
    /// Event arguments for mute state changes.
    /// </summary>
    public class MuteChangedEventArgs : EventArgs
    {
        public bool IsMuted { get; set; }
    }
}
