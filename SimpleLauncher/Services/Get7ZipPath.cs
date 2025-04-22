using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SimpleLauncher.Services;

public static class Get7ZipPath
{
    public static string Get7ZipExecutablePath()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.X64:
                return Path.Combine(baseDirectory, "7z.exe");
            case Architecture.X86:
                return Path.Combine(baseDirectory, "7z_x86.exe");
            default:
                // Notify developer
                var ex = new PlatformNotSupportedException("Unsupported architecture for 7z extraction.");
                _ = LogErrors.LogErrorAsync(ex, "Unsupported architecture for 7z extraction.");

                // Notify user
                MessageBoxLibrary.UnsupportedArchitectureMessageBox();

                return null;
        }
    }
}