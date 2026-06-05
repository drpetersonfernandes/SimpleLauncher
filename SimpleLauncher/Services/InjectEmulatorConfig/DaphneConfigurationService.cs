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
        ArgumentNullException.ThrowIfNull(settings);

        var sb = new StringBuilder();

        if (settings.Daphne.Fullscreen)
        {
            sb.Append(" -fullscreen");
        }

        // Validate against XAML-defined ranges (640-7680 for X, 480-4320 for Y)
        if (settings.Daphne.ResX is >= 640 and <= 7680)
        {
            sb.Append(CultureInfo.InvariantCulture, $" -x {settings.Daphne.ResX}");
        }

        if (settings.Daphne.ResY is >= 480 and <= 4320)
        {
            sb.Append(CultureInfo.InvariantCulture, $" -y {settings.Daphne.ResY}");
        }

        if (settings.Daphne.DisableCrosshairs)
        {
            sb.Append(" -nocrosshairs");
        }

        // Note: -nolinear_scale disables bilinear filtering.
        if (!settings.Daphne.Bilinear)
        {
            sb.Append(" -nolinear_scale");
        }

        // Note: -nosound disables sound.
        if (!settings.Daphne.EnableSound)
        {
            sb.Append(" -nosound");
        }

        sb.Append(settings.Daphne.UseOverlays ? " -use_overlays 1" : " -use_overlays 0");

        return sb.ToString().Trim();
    }
}