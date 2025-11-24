using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleLauncher.AdminAPI.Data;
using SimpleLauncher.AdminAPI.Models;

namespace SimpleLauncher.AdminAPI.Pages.Admin.Systems;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<SystemConfiguration> SystemConfiguration { get; set; } = null!;

    public async Task OnGetAsync()
    {
        SystemConfiguration = await _context.SystemConfigurations
            .Include(s => s.Emulator)
            .OrderBy(s => s.SystemName)
            .ToListAsync();
    }
}