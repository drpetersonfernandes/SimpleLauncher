using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleLauncher.AdminAPI.Data;
using SimpleLauncher.AdminAPI.Models;

namespace SimpleLauncher.AdminAPI.Pages.Admin.Systems;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public SystemConfiguration? SystemConfiguration { get; set; }

    // Navigation properties
    public int? PreviousId { get; set; }
    public int? NextId { get; set; }
    public string? PreviousName { get; set; }
    public string? NextName { get; set; }

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

        // Load navigation
        await LoadNavigationAsync(id.Value);

        return Page();
    }

    private async Task LoadNavigationAsync(int currentId)
    {
        var systems = await _context.SystemConfigurations
            .OrderBy(s => s.SystemName)
            .ThenBy(s => s.Id) // Secondary sort for stability
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
}