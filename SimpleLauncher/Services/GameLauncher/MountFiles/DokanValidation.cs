using System.Runtime.InteropServices;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

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