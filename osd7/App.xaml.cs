using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace osd7
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private AudioListenerService _audioListener;
        private BrightnessListenerService _brightnessListener;
        private OSDController _osdController;
        private VolumeOSDWindow _osdWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                _osdWindow = new VolumeOSDWindow();
                MainWindow = _osdWindow;

                InitializeOSD();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize OSD: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        /// <summary>
        /// Initializes the OSD window, controller, and input listener services.
        /// </summary>
        private void InitializeOSD()
        {
            _osdController = new OSDController(_osdWindow);

            _audioListener = new AudioListenerService();
            _audioListener.VolumeChanged += AudioListener_VolumeChanged;
            _audioListener.MuteChanged += AudioListener_MuteChanged;
            _audioListener.Initialize();

            _brightnessListener = new BrightnessListenerService();
            _brightnessListener.BrightnessChanged += BrightnessListener_BrightnessChanged;
            _brightnessListener.Initialize();

            System.Diagnostics.Debug.WriteLine("OSD system initialized successfully.");
        }

        /// <summary>
        /// Handles volume change events from the audio listener.
        /// Triggered when system volume or mute state changes.
        /// </summary>
        private void AudioListener_VolumeChanged(object sender, VolumeChangedEventArgs e)
        {
            _osdController.ShowOSD(e.Volume, e.IsMuted);
        }

        /// <summary>
        /// Handles mute state change events from the audio listener.
        /// </summary>
        private void AudioListener_MuteChanged(object sender, MuteChangedEventArgs e)
        {
        }

        /// <summary>
        /// Handles brightness change events from the brightness listener.
        /// </summary>
        private void BrightnessListener_BrightnessChanged(object sender, BrightnessChangedEventArgs e)
        {
            _osdController.ShowBrightnessOSD(e.Brightness / 100f);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _osdController?.Dispose();
            _audioListener?.Dispose();
            _brightnessListener?.Dispose();
            _osdWindow?.Close();

            base.OnExit(e);
        }
    }
}
