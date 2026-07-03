using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Media;

namespace osd7
{
    public enum OSDDisplayMode
    {
        Volume,
        Brightness
    }

    public partial class VolumeOSDWindow : Window
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private WindowInteropHelper _interopHelper;
        private bool _glassApplied = false;
        private OSDDisplayMode _mode = OSDDisplayMode.Volume;
        private bool _isMuted;
        private bool _isPrepared;

        public VolumeOSDWindow()
        {
            InitializeComponent();

            this.WindowStyle = WindowStyle.None;
            this.Background = System.Windows.Media.Brushes.Transparent;
            this.ShowInTaskbar = false;

            // ShowActivated must be true to allow native window focus assignment
            this.ShowActivated = true;
            this.Topmost = true;
            this.Visibility = Visibility.Hidden;

            this.Loaded += VolumeOSDWindow_Loaded;
        }

        private void VolumeOSDWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _interopHelper = new WindowInteropHelper(this);
            ApplyGlassEffect();
            PositionOnScreen();
            _isPrepared = true;
        }

        public void ShowWithNativeAnimation()
        {
            if (!IsVisible)
            {
                PositionOnScreen();
                if (!_isPrepared)
                {
                    this.Visibility = Visibility.Hidden;
                }

                this.Visibility = Visibility.Visible;
                Show();

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (_interopHelper == null)
                    {
                        _interopHelper = new WindowInteropHelper(this);
                    }

                    IntPtr hwnd = _interopHelper.Handle;
                    if (hwnd != IntPtr.Zero)
                    {
                        SetForegroundWindow(hwnd);
                    }

                    this.Activate();
                    this.Focus();
                }), DispatcherPriority.Render);
            }
            else
            {
                IntPtr hwnd = _interopHelper.Handle;
                if (hwnd != IntPtr.Zero)
                {
                    SetForegroundWindow(hwnd);
                }
                this.Activate();
                this.Focus();
            }
        }

        /// <summary>
        /// Extends the glass frame into the client area and enables blur-behind.
        /// </summary>
        private void ApplyGlassEffect()
        {
            if (!DwmInterop.IsCompositionEnabled)
            {
                System.Diagnostics.Debug.WriteLine("DWM composition is not enabled. Glass effect unavailable.");
                return;
            }

            try
            {
                DwmInterop.Margins margins = DwmInterop.Margins.ExtendToAllEdges;
                DwmInterop.DwmExtendFrameIntoClientArea(_interopHelper.Handle, ref margins);

                _glassApplied = true;
                System.Diagnostics.Debug.WriteLine("DWM glass effects applied successfully.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply glass effect: {ex.Message}");
                _glassApplied = false;
            }
        }

        private void PositionOnScreen()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            this.Left = (screenWidth - this.Width) / 2;
            this.Top = screenHeight - this.Height - 80;
        }

        /// <summary>
        /// Updates the OSD for volume changes.
        /// </summary>
        public void SetVolume(float volumeLevel)
        {
            UpdateLevel(OSDDisplayMode.Volume, volumeLevel, isMuted: false);
        }

        /// <summary>
        /// Updates the OSD for brightness changes.
        /// </summary>
        public void SetBrightness(float brightnessLevel)
        {
            UpdateLevel(OSDDisplayMode.Brightness, brightnessLevel, isMuted: false);
        }

        private void UpdateLevel(OSDDisplayMode mode, float level, bool isMuted)
        {
            _mode = mode;
            _isMuted = isMuted;

            level = Math.Max(0, Math.Min(1, level));

            int levelPercent = (int)(level * 100);
            LevelPercentageText.Text = $"{levelPercent}%";

            LevelBarFill.Width = 160 * level;

            UpdateLevelBarBrush();
        }

        /// <summary>
        /// Shows or hides the mute indicator text.
        /// </summary>
        public void SetMuted(bool isMuted)
        {
            _isMuted = isMuted;
            UpdateLevelBarBrush();
        }

        private void UpdateLevelBarBrush()
        {
            if (_mode == OSDDisplayMode.Volume && _isMuted)
            {
                LevelBarFill.Fill = new LinearGradientBrush(
                    Color.FromRgb(255, 170, 64),
                    Color.FromRgb(232, 82, 22),
                    90.0);
                return;
            }

            if (_mode == OSDDisplayMode.Brightness)
            {
                LevelBarFill.Fill = new LinearGradientBrush(
                    Color.FromRgb(255, 229, 128),
                    Color.FromRgb(255, 186, 64),
                    90.0);
                return;
            }

            LevelBarFill.Fill = new LinearGradientBrush(
                Color.FromRgb(255, 255, 255),
                Color.FromRgb(214, 214, 214),
                90.0);
        }

        public void FadeOut()
        {
            if (IsVisible)
            {
                this.Hide();
            }
        }
    }
}
