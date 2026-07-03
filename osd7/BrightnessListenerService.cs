using System;
using System.Management;

namespace osd7
{
    /// <summary>
    /// Event-driven brightness listener service using WMI monitor brightness events.
    /// </summary>
    public class BrightnessListenerService : IDisposable
    {
        private const string WmiScope = @"\\.\root\WMI";

        private ManagementEventWatcher _brightnessWatcher;
        private ManagementObject _brightnessObject;

        /// <summary>
        /// Fired when monitor brightness changes (0 to 100).
        /// </summary>
        public event EventHandler<BrightnessChangedEventArgs> BrightnessChanged;

        /// <summary>
        /// Initializes the brightness listener against the first available WMI monitor brightness source.
        /// </summary>
        public void Initialize()
        {
            try
            {
                _brightnessObject = GetBrightnessObject();
                if (_brightnessObject == null)
                {
                    return;
                }

                var scope = new ManagementScope(WmiScope);
                var query = new WqlEventQuery("SELECT * FROM WmiMonitorBrightnessEvent");
                _brightnessWatcher = new ManagementEventWatcher(scope, query);
                _brightnessWatcher.EventArrived += BrightnessWatcher_EventArrived;
                _brightnessWatcher.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BrightnessListenerService.Initialize failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current brightness percentage (0 to 100).
        /// </summary>
        public int GetCurrentBrightness()
        {
            try
            {
                if (_brightnessObject == null)
                {
                    return 0;
                }

                return Convert.ToInt32(_brightnessObject["CurrentBrightness"]);
            }
            catch
            {
                return 0;
            }
        }

        private ManagementObject GetBrightnessObject()
        {
            try
            {
                var scope = new ManagementScope(WmiScope);
                scope.Connect();

                var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM WmiMonitorBrightness"));
                foreach (ManagementObject brightnessObject in searcher.Get())
                {
                    return brightnessObject;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BrightnessListenerService.GetBrightnessObject failed: {ex.Message}");
            }

            return null;
        }

        private void BrightnessWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            try
            {
                if (e.NewEvent == null)
                {
                    return;
                }

                int brightness = Convert.ToInt32(e.NewEvent["Brightness"]);
                BrightnessChanged?.Invoke(this, new BrightnessChangedEventArgs { Brightness = brightness });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BrightnessListenerService.EventArrived failed: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_brightnessWatcher != null)
            {
                _brightnessWatcher.EventArrived -= BrightnessWatcher_EventArrived;
                _brightnessWatcher.Stop();
                _brightnessWatcher.Dispose();
                _brightnessWatcher = null;
            }

            _brightnessObject?.Dispose();
            _brightnessObject = null;
        }
    }

    /// <summary>
    /// Event arguments for brightness changes.
    /// </summary>
    public class BrightnessChangedEventArgs : EventArgs
    {
        public int Brightness { get; set; }
    }
}