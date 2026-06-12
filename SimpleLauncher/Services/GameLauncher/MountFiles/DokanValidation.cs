using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

/// <summary>
/// Validates whether the Dokan user-mode filesystem driver is installed and available.
/// </summary>
[SupportedOSPlatform("windows")]
public static class DokanValidation
{
    [DllImport("dokan2.dll", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
    private static extern uint DokanVersion();

    /// <summary>
    /// Checks whether the Dokan library (dokan2.dll) can be loaded and reports a valid version.
    /// </summary>
    public static bool IsDokanInstalled()
    {
        try
        {
            return DokanVersion() > 0;
        }
        catch (DllNotFoundException)
        {
            return false;
        }
        catch (EntryPointNotFoundException)
        {
            return false;
        }
        catch (BadImageFormatException)
        {
            return false;
        }
    }
}
