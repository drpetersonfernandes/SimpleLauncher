using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Updater.Services;

/// <summary>
/// Service for detecting whether the Dokan library is installed,
/// and for downloading and installing it if missing.
/// </summary>
public class DokanService
{
    private const string DokanX64Url = "https://github.com/dokan-dev/dokany/releases/download/v2.3.1.1000/Dokan_x64.msi";
    private const string DokanArm64Url = "https://github.com/dokan-dev/dokany/releases/download/v2.3.1.1000/Dokan_ARM64.msi";

    private readonly HttpClient _httpClient;

    /// <summary>
    /// Event raised when a log message needs to be displayed.
    /// </summary>
    public event Action<string>? LogMessage;

    /// <summary>
    /// Event raised when download progress changes.
    /// </summary>
    public event Action<DownloadProgressInfo>? ProgressChanged;

    /// <summary>
    /// Initializes a new instance of the DokanService class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for downloads.</param>
    public DokanService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Checks whether the Dokan library is installed on this system.
    /// </summary>
    /// <returns>True if Dokan is detected, false otherwise.</returns>
    public bool IsDokanInstalled()
    {
        LogMessage?.Invoke("Checking if Dokan is installed...");

        // Check 1: Look for Dokan in installed programs (registry)
        if (IsDokanInRegistry())
        {
            LogMessage?.Invoke("Dokan found in installed programs.");
            return true;
        }

        // Check 2: Look for Dokan DLL in System32
        if (IsDokanDllPresent())
        {
            LogMessage?.Invoke("Dokan DLL found in System32.");
            return true;
        }

        LogMessage?.Invoke("Dokan is not installed.");
        return false;
    }

    /// <summary>
    /// Gets the Dokan MSI download URL for the current processor architecture.
    /// </summary>
    /// <returns>The download URL for the appropriate Dokan MSI installer.</returns>
    public static string GetDokanDownloadUrl()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.Arm64 => DokanArm64Url,
            _ => DokanX64Url
        };
    }

    /// <summary>
    /// Downloads the Dokan MSI installer to the application directory and launches it.
    /// </summary>
    /// <param name="appDirectory">The directory to save the MSI file to.</param>
    public async Task DownloadAndInstallDokanAsync(string appDirectory)
    {
        var downloadUrl = GetDokanDownloadUrl();
        var fileName = Path.GetFileName(new Uri(downloadUrl).LocalPath);
        var msiPath = Path.Combine(appDirectory, fileName);

        LogMessage?.Invoke($"Downloading Dokan installer from: {downloadUrl}");

        try
        {
            // Download the MSI file
            var downloadService = new DownloadService(_httpClient);
            downloadService.LogMessage += msg => LogMessage?.Invoke(msg);
            downloadService.ProgressChanged += info => ProgressChanged?.Invoke(info);

            using var memoryStream = await downloadService.DownloadToMemoryAsync(downloadUrl);

            // Save to disk
            LogMessage?.Invoke($"Saving installer to: {msiPath}");
            await using var fileStream = File.Create(msiPath);
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(fileStream);
            await fileStream.FlushAsync();

            LogMessage?.Invoke("Download complete. Launching Dokan installer...");

            // Launch the MSI installer (shows UI, handles its own elevation if needed)
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = msiPath,
                UseShellExecute = true
            });

            if (process != null)
            {
                process.Dispose();
                LogMessage?.Invoke("Dokan installer launched. Please follow the installation wizard.");
            }
            else
            {
                LogMessage?.Invoke("Failed to launch the Dokan installer.");
            }
        }
        catch (Exception ex)
        {
            await BugReportService.ReportBugAsync(ex, "Error downloading or installing Dokan");
            LogMessage?.Invoke($"Error during Dokan installation: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Checks the Windows registry for any Dokan installation entry.
    /// </summary>
    private static bool IsDokanInRegistry()
    {
        // Check both native and WOW64 uninstall registry locations
        return CheckUninstallRegistry(RegistryView.Registry64) ||
               CheckUninstallRegistry(RegistryView.Registry32);
    }

    /// <summary>
    /// Checks a specific registry view for Dokan uninstall entries.
    /// </summary>
    private static bool CheckUninstallRegistry(RegistryView registryView)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView);
            using var uninstallKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (uninstallKey == null) return false;

            foreach (var subKeyName in uninstallKey.GetSubKeyNames())
            {
                try
                {
                    using var subKey = uninstallKey.OpenSubKey(subKeyName);
                    var displayName = subKey?.GetValue("DisplayName") as string;
                    if (!string.IsNullOrEmpty(displayName) &&
                        displayName.Contains("Dokan", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                catch
                {
                    // Skip keys that can't be read
                }
            }
        }
        catch
        {
            // Registry access may fail; treat as not found
        }

        return false;
    }

    /// <summary>
    /// Checks whether the Dokan DLL exists in the System32 directory.
    /// </summary>
    private static bool IsDokanDllPresent()
    {
        try
        {
            var system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
            return File.Exists(Path.Combine(system32, "dokan2.dll")) ||
                   File.Exists(Path.Combine(system32, "dokan1.dll"));
        }
        catch
        {
            return false;
        }
    }
}
