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

        return Page();
    }
}