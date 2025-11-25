using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleLauncher.AdminAPI.Data;
using SimpleLauncher.AdminAPI.Models;

namespace SimpleLauncher.AdminAPI.Pages.Admin.Systems;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    // Navigation properties
    public int? PreviousId { get; set; }
    public int? NextId { get; set; }
    public string? PreviousName { get; set; }
    public string? NextName { get; set; }

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public SystemConfiguration? SystemConfiguration { get; set; }

    [BindProperty]
    public EmulatorConfiguration EmulatorConfiguration { get; set; } = null!;

    // Helper properties for comma-separated strings
    [BindProperty]
    public string? FileFormatsToSearchString { get; set; }

    [BindProperty]
    public string? FileFormatsToLaunchString { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        SystemConfiguration = await _context.SystemConfigurations
            .Include(s => s.Emulator)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (SystemConfiguration == null)
        {
            return NotFound();
        }

        // Populate helper string properties from the list properties
        FileFormatsToSearchString = string.Join(",", SystemConfiguration.FileFormatsToSearch);
        FileFormatsToLaunchString = string.Join(",", SystemConfiguration.FileFormatsToLaunch);

        // If an emulator exists, bind its properties to EmulatorConfiguration
        EmulatorConfiguration = SystemConfiguration.Emulator ??
                                // If no emulator exists, initialize a new one for the form with a default "None" name
                                // This won't be saved unless the user fills in details.
                                new EmulatorConfiguration { EmulatorName = "None" };

        // Load navigation
        await LoadNavigationAsync(id.Value);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Custom validation for EmulatorConfiguration:
        // If EmulatorName is "None" but other emulator details are provided, it's an invalid state.
        // Or if EmulatorName is not "None" but is empty.
        if (EmulatorConfiguration.EmulatorName == "None" && !string.IsNullOrEmpty(EmulatorConfiguration.EmulatorLocation))
        {
            ModelState.AddModelError("EmulatorConfiguration.EmulatorName", "Emulator Name is required if other emulator details are provided.");
        }
        else if (EmulatorConfiguration.EmulatorName != "None" && string.IsNullOrWhiteSpace(EmulatorConfiguration.EmulatorName))
        {
            ModelState.AddModelError("EmulatorConfiguration.EmulatorName", "The Emulator Name field is required.");
        }

        // Remove navigation properties from model state validation
        // as they are not directly bound by input fields and can cause validation issues.
        ModelState.Remove("SystemConfiguration.Emulator");
        ModelState.Remove("EmulatorConfiguration.SystemConfiguration");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var systemToUpdate = await _context.SystemConfigurations
            .Include(s => s.Emulator) // Ensure emulator is loaded for update
            .FirstOrDefaultAsync(s => SystemConfiguration != null && s.Id == SystemConfiguration.Id);

        if (systemToUpdate == null)
        {
            return NotFound();
        }

        // Update SystemConfiguration properties from the bind property
        systemToUpdate.SystemName = SystemConfiguration!.SystemName;
        systemToUpdate.Architecture = SystemConfiguration!.Architecture;
        systemToUpdate.SystemFolder = SystemConfiguration!.SystemFolder;
        systemToUpdate.SystemImageFolder = SystemConfiguration!.SystemImageFolder;
        systemToUpdate.SystemIsMame = SystemConfiguration!.SystemIsMame;
        systemToUpdate.ExtractFileBeforeLaunch = SystemConfiguration!.ExtractFileBeforeLaunch;
        systemToUpdate.FileFormatsToSearchDb = FileFormatsToSearchString;
        systemToUpdate.FileFormatsToLaunchDb = FileFormatsToLaunchString;

        // Handle EmulatorConfiguration: update, create, or remove
        if (EmulatorConfiguration.EmulatorName == "None" && string.IsNullOrEmpty(EmulatorConfiguration.EmulatorLocation))
        {
            // User intends to remove the emulator or keep it empty
            if (systemToUpdate.Emulator != null)
            {
                _context.EmulatorConfigurations.Remove(systemToUpdate.Emulator);
                systemToUpdate.Emulator = null; // Detach from system
            }
        }
        else
        {
            // User provided emulator details
            if (systemToUpdate.Emulator == null)
            {
                // Create a new emulator if one didn't exist
                systemToUpdate.Emulator = new EmulatorConfiguration();
                _context.EmulatorConfigurations.Add(systemToUpdate.Emulator);
            }

            // Update existing or newly created emulator properties
            systemToUpdate.Emulator.EmulatorName = EmulatorConfiguration.EmulatorName;
            systemToUpdate.Emulator.EmulatorLocation = EmulatorConfiguration.EmulatorLocation;
            systemToUpdate.Emulator.EmulatorParameters = EmulatorConfiguration.EmulatorParameters;
            systemToUpdate.Emulator.EmulatorDownloadPage = EmulatorConfiguration.EmulatorDownloadPage;
            systemToUpdate.Emulator.EmulatorLatestVersion = EmulatorConfiguration.EmulatorLatestVersion;
            systemToUpdate.Emulator.EmulatorDownloadLink = EmulatorConfiguration.EmulatorDownloadLink;
            systemToUpdate.Emulator.EmulatorDownloadExtractPath = EmulatorConfiguration.EmulatorDownloadExtractPath;
            systemToUpdate.Emulator.CoreLocation = EmulatorConfiguration.CoreLocation;
            systemToUpdate.Emulator.CoreLatestVersion = EmulatorConfiguration.CoreLatestVersion;
            systemToUpdate.Emulator.CoreDownloadLink = EmulatorConfiguration.CoreDownloadLink;
            systemToUpdate.Emulator.CoreDownloadExtractPath = EmulatorConfiguration.CoreDownloadExtractPath;
            systemToUpdate.Emulator.ImagePackDownloadLink = EmulatorConfiguration.ImagePackDownloadLink;
            systemToUpdate.Emulator.ImagePackDownloadLink2 = EmulatorConfiguration.ImagePackDownloadLink2;
            systemToUpdate.Emulator.ImagePackDownloadLink3 = EmulatorConfiguration.ImagePackDownloadLink3;
            systemToUpdate.Emulator.ImagePackDownloadLink4 = EmulatorConfiguration.ImagePackDownloadLink4;
            systemToUpdate.Emulator.ImagePackDownloadLink5 = EmulatorConfiguration.ImagePackDownloadLink5;
            systemToUpdate.Emulator.ImagePackDownloadExtractPath = EmulatorConfiguration.ImagePackDownloadExtractPath;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SystemConfigurationExists(SystemConfiguration!.Id))
            {
                return NotFound();
            }
            else
            {
                throw; // Re-throw if it's a different concurrency issue
            }
        }

        return RedirectToPage("./Index");
    }

    private async Task LoadNavigationAsync(int currentId)
    {
        var systems = await _context.SystemConfigurations
            .OrderBy(s => s.SystemName)
            .ThenBy(s => s.Id)
            .Select(s => new { s.Id, s.SystemName })
            .ToListAsync();

        var currentIndex = systems.FindIndex(s => s.Id == currentId);

        if (currentIndex > 0)
        {
            PreviousId = systems[currentIndex - 1].Id;
            PreviousName = systems[currentIndex - 1].SystemName;
        }

        if (currentIndex >= 0 && currentIndex < systems.Count - 1)
        {
            NextId = systems[currentIndex + 1].Id;
            NextName = systems[currentIndex + 1].SystemName;
        }
    }

    private bool SystemConfigurationExists(int id)
    {
        return _context.SystemConfigurations.Any(e => e.Id == id);
    }
}