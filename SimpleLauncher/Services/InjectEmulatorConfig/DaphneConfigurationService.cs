using System.Globalization;
using System.Text;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class DaphneConfigurationService
{
    /// <summary>
    /// Builds a string of command-line arguments for Daphne based on saved settings.
    /// </summary>
    /// <param name="settings">The application settings manager.</param>
    /// <returns>A string containing command-line arguments.</returns>
    public static string BuildArguments(SettingsManager.SettingsManager settings)
    {
        var sb = new StringBuilder();

        if (settings.DaphneFullscreen)
        {
            sb.Append(" -fullscreen");
        }

        if (settings.DaphneResX > 0)
        {
            sb.Append(CultureInfo.InvariantCulture, $" -x {settings.DaphneResX}");
        }

        if (settings.DaphneResY > 0)
        {
            sb.Append(CultureInfo.InvariantCulture, $" -y {settings.DaphneResY}");
        }

        if (settings.DaphneDisableCrosshairs)
        {
            sb.Append(" -nocrosshairs");
        }

        // Note: -nolinear_scale disables bilinear filtering.
        if (!settings.DaphneBilinear)
        {
            sb.Append(" -nolinear_scale");
        }

        // Note: -nosound disables sound.
        if (!settings.DaphneEnableSound)
        {
            sb.Append(" -nosound");
        }

        sb.Append(settings.DaphneUseOverlays ? " -use_overlays 1" : " -use_overlays 0");

        return sb.ToString().Trim();
    }
}