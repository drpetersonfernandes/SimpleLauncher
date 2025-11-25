using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleLauncher.AdminAPI.Data;
using SimpleLauncher.AdminAPI.Models;

namespace SimpleLauncher.AdminAPI.Pages.Admin.Systems;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public SystemConfiguration? SystemConfiguration { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        SystemConfiguration = await _context.SystemConfigurations
            .Include(s => s.Emulator) // Include emulator to display its details before deletion
            .FirstOrDefaultAsync(m => m.Id == id);

        if (SystemConfiguration == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var systemConfiguration = await _context.SystemConfigurations
            .Include(s => s.Emulator) // Ensure emulator is loaded for deletion
            .FirstOrDefaultAsync(m => m.Id == id);

        if (systemConfiguration == null)
        {
            return NotFound();
        }

        SystemConfiguration = systemConfiguration;

        // If cascade delete is configured in ApplicationDbContext (which it is for this project),
        // deleting the SystemConfiguration will automatically delete the associated EmulatorConfiguration.
        // Explicitly removing it here is a safeguard but might not be strictly necessary.
        if (SystemConfiguration.Emulator != null)
        {
            _context.EmulatorConfigurations.Remove(SystemConfiguration.Emulator);
        }

        _context.SystemConfigurations.Remove(SystemConfiguration);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}