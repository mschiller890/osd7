using System;
using System.Windows;
using System.Windows.Threading;

namespace osd7
{
    /// <summary>
    /// Controller that manages the lifecycle, visibility, and auto-hide behavior of the volume OSD.
    /// Ensures only a single OSD instance exists and handles repeated level changes.
    /// </summary>
    public class OSDController
    {
        private readonly VolumeOSDWindow _osdWindow;
        private readonly DispatcherTimer _autoHideTimer;
        private readonly int _displayDurationMs = 1500;

        /// <summary>
        /// Initializes the OSD controller with a volume OSD window.
        /// </summary>
        /// <param name="osdWindow">The VolumeOSDWindow instance to manage.</param>
        public OSDController(VolumeOSDWindow osdWindow)
        {
            _osdWindow = osdWindow ?? throw new ArgumentNullException(nameof(osdWindow));

            _autoHideTimer = new DispatcherTimer();
            _autoHideTimer.Interval = TimeSpan.FromMilliseconds(_displayDurationMs);
            _autoHideTimer.Tick += AutoHideTimer_Tick;
        }

        /// <summary>
        /// Shows or updates the OSD with the current volume information.
        /// Resets the auto-hide timer on each call (repeated volume changes extend display time).
        /// </summary>
        /// <param name="volumeLevel">Volume level (0.0 to 1.0).</param>
        /// <param name="isMuted">Whether audio is muted.</param>
        public void ShowOSD(float volumeLevel, bool isMuted)
        {
            ShowOSD(volumeLevel, isMuted, OSDDisplayMode.Volume);
        }

        /// <summary>
        /// Shows or updates the OSD with the current brightness information.
        /// </summary>
        /// <param name="brightnessLevel">Brightness level (0.0 to 1.0).</param>
        public void ShowBrightnessOSD(float brightnessLevel)
        {
            ShowOSD(brightnessLevel, isMuted: false, OSDDisplayMode.Brightness);
        }

        private void ShowOSD(float level, bool isMuted, OSDDisplayMode mode)
        {
            try
            {
                ExecuteOnUiThread(() =>
                {
                    if (mode == OSDDisplayMode.Brightness)
                    {
                        _osdWindow.SetBrightness(level);
                    }
                    else
                    {
                        _osdWindow.SetVolume(level);
                        _osdWindow.SetMuted(isMuted);
                    }

                    if (!_osdWindow.IsVisible)
                    {
                        _osdWindow.ShowWithNativeAnimation();
                    }

                    _autoHideTimer.Stop();
                    _autoHideTimer.Start();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OSDController.ShowOSD failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Immediately hides the OSD with fade-out animation.
        /// Stops the auto-hide timer.
        /// </summary>
        public void HideOSD()
        {
            try
            {
                ExecuteOnUiThread(() =>
                {
                    if (_osdWindow.IsVisible)
                    {
                        _autoHideTimer.Stop();
                        _osdWindow.FadeOut();
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OSDController.HideOSD failed: {ex.Message}");
            }
        }

        private void ExecuteOnUiThread(Action action)
        {
            if (_osdWindow.Dispatcher.CheckAccess())
            {
                action();
                return;
            }

            _osdWindow.Dispatcher.Invoke(action);
        }

        private void AutoHideTimer_Tick(object sender, EventArgs e)
        {
            _autoHideTimer.Stop();
            HideOSD();
        }

        /// <summary>
        /// Releases resources associated with the OSD controller.
        /// </summary>
        public void Dispose()
        {
            if (_autoHideTimer != null)
            {
                _autoHideTimer.Stop();
                _autoHideTimer.Tick -= AutoHideTimer_Tick;
            }
        }
    }
}
