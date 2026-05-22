using System.Runtime.InteropServices;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

public static class DokanValidation
{
    [DllImport("dokan2.dll", ExactSpelling = true)]
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
    }
}