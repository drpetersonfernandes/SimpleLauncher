using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace SimpleLauncher.Core.Services.GameLauncher.MountFiles;

[SupportedOSPlatform("windows")]
public static class DokanValidation
{
    [DllImport("dokan2.dll", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
    private static extern uint DokanVersion();

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