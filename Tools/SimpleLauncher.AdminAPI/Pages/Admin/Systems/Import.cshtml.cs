using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; // Add this using directive
using SimpleLauncher.AdminAPI.Data;
using SimpleLauncher.AdminAPI.Models;

namespace SimpleLauncher.AdminAPI.Pages.Admin.Systems;

public class ImportModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ImportModel> _logger;

    public ImportModel(ApplicationDbContext context, ILogger<ImportModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    [BindProperty]
    [Required]
    public string Architecture { get; set; } = "x64";

    [BindProperty]
    [Required]
    [Display(Name = "XML File")]
    public IFormFile XmlFile { get; set; } = null!;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (XmlFile.Length == 0)
        {
            ModelState.AddModelError("XmlFile", "The file is empty.");
            return Page();
        }

        try
        {
            await using var stream = XmlFile.OpenReadStream();
            var doc = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);

            if (doc.Root == null || doc.Root.Name != "EasyMode")
            {
                ModelState.AddModelError("XmlFile", "Invalid XML format. Root element must be <EasyMode>.");
                return Page();
            }

            var importedSystemConfigs = new List<SystemConfiguration>();

            foreach (var element in doc.Root.Elements("EasyModeSystemConfig"))
            {
                var systemName = GetValue(element, "SystemName");
                if (string.IsNullOrWhiteSpace(systemName)) continue;

                // Parse File Formats Lists
                var searchFormats = element.Element("FileFormatsToSearch")?
                    .Elements("FormatToSearch")
                    .Select(x => x.Value.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList() ?? new List<string>();

                var launchFormats = element.Element("FileFormatsToLaunch")?
                    .Elements("FormatToLaunch")
                    .Select(x => x.Value.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList() ?? new List<string>();

                var newSysConfig = new SystemConfiguration
                {
                    SystemName = systemName,
                    Architecture = Architecture,
                    SystemFolder = GetValue(element, "SystemFolder"),
                    SystemImageFolder = GetValue(element, "SystemImageFolder"),
                    SystemIsMame = GetBoolValue(element, "SystemIsMAME"),
                    ExtractFileBeforeLaunch = GetBoolValue(element, "ExtractFileBeforeLaunch"),
                    FileFormatsToSearchDb = string.Join(",", searchFormats),
                    FileFormatsToLaunchDb = string.Join(",", launchFormats)
                };

                // Parse Emulator (Assuming 1:1 relationship based on DB model, taking the first one found)
                var emulatorElement = element.Element("Emulators")?.Element("Emulator");
                if (emulatorElement != null)
                {
                    newSysConfig.Emulator = new EmulatorConfiguration
                    {
                        EmulatorName = GetValue(emulatorElement, "EmulatorName") ?? "Unknown",
                        EmulatorLocation = GetValue(emulatorElement, "EmulatorLocation"),
                        EmulatorParameters = GetValue(emulatorElement, "EmulatorParameters"),
                        EmulatorDownloadPage = GetValue(emulatorElement, "EmulatorDownloadPage"),
                        EmulatorLatestVersion = GetValue(emulatorElement, "EmulatorLatestVersion"),
                        EmulatorDownloadLink = GetValue(emulatorElement, "EmulatorDownloadLink"),
                        EmulatorDownloadExtractPath = GetValue(emulatorElement, "EmulatorDownloadExtractPath"),

                        CoreLocation = GetValue(emulatorElement, "CoreLocation"),
                        CoreLatestVersion = GetValue(emulatorElement, "CoreLatestVersion"),
                        CoreDownloadLink = GetValue(emulatorElement, "CoreDownloadLink"),
                        CoreDownloadExtractPath = GetValue(emulatorElement, "CoreDownloadExtractPath"),

                        ImagePackDownloadLink = GetValue(emulatorElement, "ImagePackDownloadLink"),
                        ImagePackDownloadLink2 = GetValue(emulatorElement, "ImagePackDownloadLink2"),
                        ImagePackDownloadLink3 = GetValue(emulatorElement, "ImagePackDownloadLink3"),
                        ImagePackDownloadLink4 = GetValue(emulatorElement, "ImagePackDownloadLink4"),
                        ImagePackDownloadLink5 = GetValue(emulatorElement, "ImagePackDownloadLink5"),
                        ImagePackDownloadExtractPath = GetValue(emulatorElement, "ImagePackDownloadExtractPath")
                    };
                }
                else
                {
                    // Create a dummy emulator config if none exists to satisfy the required relationship
                    newSysConfig.Emulator = new EmulatorConfiguration { EmulatorName = "None" };
                }

                importedSystemConfigs.Add(newSysConfig);
            }

            var systemsAdded = 0;
            var systemsUpdated = 0;

            foreach (var newSysConfig in importedSystemConfigs)
            {
                // Check if a system with the same name and architecture already exists
                var existingSysConfig = await _context.SystemConfigurations
                    .Include(s => s.Emulator) // Include the related emulator configuration
                    .SingleOrDefaultAsync(s => s.SystemName == newSysConfig.SystemName && s.Architecture == newSysConfig.Architecture);

                if (existingSysConfig != null)
                {
                    // Update existing system configuration
                    existingSysConfig.SystemFolder = newSysConfig.SystemFolder;
                    existingSysConfig.SystemImageFolder = newSysConfig.SystemImageFolder;
                    existingSysConfig.SystemIsMame = newSysConfig.SystemIsMame;
                    existingSysConfig.ExtractFileBeforeLaunch = newSysConfig.ExtractFileBeforeLaunch;
                    existingSysConfig.FileFormatsToSearchDb = newSysConfig.FileFormatsToSearchDb;
                    existingSysConfig.FileFormatsToLaunchDb = newSysConfig.FileFormatsToLaunchDb;
                    // Note: SystemName and Architecture are used for matching, so they are not typically updated here.

                    // Update or replace associated emulator configuration
                    if (newSysConfig.Emulator != null)
                    {
                        if (existingSysConfig.Emulator != null)
                        {
                            // Update existing emulator properties
                            existingSysConfig.Emulator.EmulatorName = newSysConfig.Emulator.EmulatorName;
                            existingSysConfig.Emulator.EmulatorLocation = newSysConfig.Emulator.EmulatorLocation;
                            existingSysConfig.Emulator.EmulatorParameters = newSysConfig.Emulator.EmulatorParameters;
                            existingSysConfig.Emulator.EmulatorDownloadPage = newSysConfig.Emulator.EmulatorDownloadPage;
                            existingSysConfig.Emulator.EmulatorLatestVersion = newSysConfig.Emulator.EmulatorLatestVersion;
                            existingSysConfig.Emulator.EmulatorDownloadLink = newSysConfig.Emulator.EmulatorDownloadLink;
                            existingSysConfig.Emulator.EmulatorDownloadExtractPath = newSysConfig.Emulator.EmulatorDownloadExtractPath;
                            existingSysConfig.Emulator.CoreLocation = newSysConfig.Emulator.CoreLocation;
                            existingSysConfig.Emulator.CoreLatestVersion = newSysConfig.Emulator.CoreLatestVersion;
                            existingSysConfig.Emulator.CoreDownloadLink = newSysConfig.Emulator.CoreDownloadLink;
                            existingSysConfig.Emulator.CoreDownloadExtractPath = newSysConfig.Emulator.CoreDownloadExtractPath;
                            existingSysConfig.Emulator.ImagePackDownloadLink = newSysConfig.Emulator.ImagePackDownloadLink;
                            existingSysConfig.Emulator.ImagePackDownloadLink2 = newSysConfig.Emulator.ImagePackDownloadLink2;
                            existingSysConfig.Emulator.ImagePackDownloadLink3 = newSysConfig.Emulator.ImagePackDownloadLink3;
                            existingSysConfig.Emulator.ImagePackDownloadLink4 = newSysConfig.Emulator.ImagePackDownloadLink4;
                            existingSysConfig.Emulator.ImagePackDownloadLink5 = newSysConfig.Emulator.ImagePackDownloadLink5;
                            existingSysConfig.Emulator.ImagePackDownloadExtractPath = newSysConfig.Emulator.ImagePackDownloadExtractPath;
                            _context.EmulatorConfigurations.Update(existingSysConfig.Emulator);
                        }
                        else
                        {
                            // Add new emulator configuration if it didn't exist before
                            newSysConfig.Emulator.SystemConfigurationId = existingSysConfig.Id;
                            _context.EmulatorConfigurations.Add(newSysConfig.Emulator);
                            existingSysConfig.Emulator = newSysConfig.Emulator; // Link it
                        }
                    }
                    else if (existingSysConfig.Emulator != null)
                    {
                        // If new config has no emulator, but existing one does, remove the existing one
                        _context.EmulatorConfigurations.Remove(existingSysConfig.Emulator);
                        existingSysConfig.Emulator = null;
                    }

                    _context.SystemConfigurations.Update(existingSysConfig);
                    systemsUpdated++;
                }
                else
                {
                    // Add new system configuration
                    _context.SystemConfigurations.Add(newSysConfig);
                    systemsAdded++;
                }
            }

            if (systemsAdded > 0 || systemsUpdated > 0)
            {
                await _context.SaveChangesAsync();
                Log.ImportedSystems(_logger, systemsAdded + systemsUpdated, Architecture);
                TempData["SuccessMessage"] = $"Successfully imported {systemsAdded} new systems and updated {systemsUpdated} existing systems for architecture {Architecture}.";
            }
            else
            {
                TempData["InfoMessage"] = $"No systems were added or updated for architecture {Architecture}.";
            }

            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            Log.ErrorImportingXml(_logger, ex);
            ModelState.AddModelError(string.Empty, $"An error occurred while importing: {ex.Message}");
            return Page();
        }
    }

    private static string? GetValue(XElement parent, string elementName)
    {
        var val = parent.Element(elementName)?.Value;
        return string.IsNullOrWhiteSpace(val) ? null : val.Trim();
    }

    private static bool GetBoolValue(XElement parent, string elementName)
    {
        var val = parent.Element(elementName)?.Value;
        if (bool.TryParse(val, out var result))
        {
            return result;
        }

        return false;
    }
}
