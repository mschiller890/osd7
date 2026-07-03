using System;
using System.Runtime.InteropServices;

namespace osd7
{
    /// <summary>
    /// Uses dwmapi.dll for native glass composition and blur-behind window effects.
    /// </summary>
    internal static class DwmInterop
    {
        [DllImport("dwmapi.dll", PreserveSig = false)]
        internal static extern void DwmExtendFrameIntoClientArea(
            IntPtr hwnd,
            ref Margins pMarInset);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        internal static extern void DwmEnableBlurBehindWindow(
            IntPtr hwnd,
            ref DwmBlurBehind pBlurBehind);

        [DllImport("dwmapi.dll")]
        internal static extern int DwmIsCompositionEnabled(out bool pfEnabled);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        internal static extern int SHGetStockIconInfo(
            SHSTOCKICONID siid,
            uint uFlags,
            ref SHSTOCKICONINFO psii);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool DestroyIcon(IntPtr hIcon);

        internal const uint SHGSI_ICON = 0x000000100;
        internal const uint SHGSI_SMALLICON = 0x000000001;

        [StructLayout(LayoutKind.Sequential)]
        internal struct Margins
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;

            /// <summary>
            /// Extends glass into entire client area (all edges).
            /// </summary>
            public static readonly Margins ExtendToAllEdges = new Margins
            {
                cxLeftWidth = -1,
                cxRightWidth = -1,
                cyTopHeight = -1,
                cyBottomHeight = -1
            };
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DwmBlurBehind
        {
            public DwmBlurBehindFlags dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;

            private const uint DWM_BB_ENABLE = 0x00000001;
            private const uint DWM_BB_BLURREGION = 0x00000002;
            private const uint DWM_BB_TRANSITIONONMAXIMIZED = 0x00000004;

            /// <summary>
            /// Creates a blur-behind configuration with blur enabled.
            /// </summary>
            public static DwmBlurBehind CreateEnabled()
            {
                return new DwmBlurBehind
                {
                    dwFlags = DwmBlurBehindFlags.EnableBlur,
                    fEnable = true,
                    hRgnBlur = IntPtr.Zero,
                    fTransitionOnMaximized = false
                };
            }
        }

        [Flags]
        internal enum DwmBlurBehindFlags : uint
        {
            EnableBlur = 0x00000001,
            BlurRegion = 0x00000002,
            TransitionOnMaximized = 0x00000004
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct SHSTOCKICONINFO
        {
            public uint cbSize;
            public IntPtr hIcon;
            public int iSysImageIndex;
            public int iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPath;
        }

        internal enum SHSTOCKICONID
        {
            SIID_DEVICEAUDIOPLAYER = 102
        }

        /// <summary>
        /// Checks if DWM composition is currently enabled on the system.
        /// Windows 7 with Aero theme should have this enabled.
        /// </summary>
        internal static bool IsCompositionEnabled
        {
            get
            {
                try
                {
                    bool isEnabled;
                    int result = DwmIsCompositionEnabled(out isEnabled);
                    return result == 0 && isEnabled;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
