using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace prcEditor.Services;

public static class ExternalFileLauncher
{
    public static void Open(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", path);
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", path);
            return;
        }

        throw new PlatformNotSupportedException("Opening files with the default editor is not supported on this OS.");
    }
}