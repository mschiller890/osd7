# osd7

A simple on-screen display (OSD) for volume and brightness that's missing on Windows 7, rendered with the Windows Aero glass effect.

## Why?

I installed Windows 7 alongside 11 and CachyOS on my ThinkPad X260 and missed the Windows OSD for audio and brightness levels.
What else is there to say?

## Features

- **Volume OSD** — reacts to system volume and mute changes via the CoreAudio API (NAudio).
- **Brightness OSD** — reacts to monitor brightness changes via WMI `WmiMonitorBrightnessEvent`.
- **Aero glass** — uses `dwmapi.dll` (`DwmExtendFrameIntoClientArea` / `DwmEnableBlurBehindWindow`) for the native blur-behind look.
- **Event-driven** — no polling. Audio and brightness listeners subscribe to OS events and only wake the OSD when something changes.
- **Auto-hide** — the OSD fades out automatically after a short delay; repeated changes reset the timer.
- **Single instance** — one OSD window is reused and updated for every notification.

## How it works

| Component | Responsibility |
| --- | --- |
| `AudioListenerService` | Subscribes to `MMDevice.AudioEndpointVolume.OnVolumeNotification` for volume/mute events. |
| `BrightnessListenerService` | Watches `WmiMonitorBrightnessEvent` through a `ManagementEventWatcher`. |
| `OSDController` | Owns the OSD window, updates its state, and runs the auto-hide `DispatcherTimer`. |
| `VolumeOSDWindow` | The transparent, topmost, taskbar-hidden window that renders the OSD. |
| `DwmInterop` | P/Invoke wrappers around `dwmapi.dll` and `shell32.dll` for glass composition and stock icons. |

## Requirements

- Windows 7+ with Desktop Window Manager (DWM) composition enabled for the glass effect.
- .NET Framework 4.7.2.
- [NAudio](https://github.com/naudio/NAudio) 2.3.0 (restored via NuGet / `packages.config`).

## Building

Open `osd7.sln` in Visual Studio and build, or from a Developer Command Prompt:

```bash
msbuild osd7.sln /p:Configuration=Release
```

The output executable is written to `osd7\bin\Release\osd7.exe`.

