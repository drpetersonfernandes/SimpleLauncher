using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleLauncher.AdminAPI.Data;
using SimpleLauncher.AdminAPI.Models;

namespace SimpleLauncher.AdminAPI.Pages.Admin.Systems;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public SystemConfiguration SystemConfiguration { get; set; } = new();

    [BindProperty]
    public EmulatorConfiguration EmulatorConfiguration { get; set; } = new();

    // Helper properties for comma-separated strings
    [BindProperty]
    public string? FileFormatsToSearchString { get; set; }

    [BindProperty]
    public string? FileFormatsToLaunchString { get; set; }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Assign the list from the helper string property
        SystemConfiguration.FileFormatsToSearch = FileFormatsToSearchString?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
        SystemConfiguration.FileFormatsToLaunch = FileFormatsToLaunchString?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();

        // Associate the emulator with the system
        SystemConfiguration.Emulator = EmulatorConfiguration;

        _context.SystemConfigurations.Add(SystemConfiguration);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}