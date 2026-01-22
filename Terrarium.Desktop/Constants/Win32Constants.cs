namespace Terrarium.Desktop.Constants;

/// <summary>
/// Win32 API constants for window management and hit testing.
/// These are Windows platform-specific values defined by Microsoft.
/// </summary>
/// <remarks>
/// For more information, see:
/// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-nchittest.
/// </remarks>
public static class Win32Constants
{
    /// <summary>
    /// WM_NCHITTEST message identifier.
    /// Sent to a window to determine which part of the window corresponds to a particular screen coordinate.
    /// Value 0x0084 is defined by the Windows API.
    /// </summary>
    public const int WMNCHITTEST = 0x0084;

    /// <summary>
    /// HTTRANSPARENT hit test result code.
    /// Indicates the window is transparent to mouse events (click-through).
    /// Value -1 is defined by the Windows API.
    /// Used to make UI elements ignore mouse clicks and pass them to windows beneath.
    /// </summary>
    public const int HTTRANSPARENT = -1;
}
