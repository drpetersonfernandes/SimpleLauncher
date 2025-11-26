using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleLauncher.AdminAPI.Data;
using SimpleLauncher.AdminAPI.Models.DTOs;

namespace SimpleLauncher.AdminAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SystemsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SystemsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Systems/x64 or api/Systems/arm64
    [HttpGet("{architecture}")]
    [SuppressMessage("Performance", "CA1862:Use the \'StringComparison\' method overloads to perform case-insensitive string comparisons")]
    public async Task<ActionResult<IEnumerable<SystemConfigurationDto>>> GetSystemConfigurations(string architecture)
    {
        if (!architecture.Equals("x64", StringComparison.OrdinalIgnoreCase) && !architecture.Equals("arm64", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Invalid architecture specified. Use 'x64' or 'arm64'.");
        }

        var configs = await _context.SystemConfigurations
            .Include(static s => s.Emulator)
            .Where(s => s.Architecture.ToLower() == architecture.ToLower())
            .OrderBy(static s => s.SystemName)
            .ToListAsync();

        // Map to DTO to match the client's expected format
        var dtos = configs.Select(static s => new SystemConfigurationDto
        {
            SystemName = s.SystemName,
            SystemFolder = s.SystemFolder,
            SystemImageFolder = s.SystemImageFolder,
            SystemIsMame = s.SystemIsMame,
            FileFormatsToSearch = s.FileFormatsToSearch,
            ExtractFileBeforeLaunch = s.ExtractFileBeforeLaunch,
            FileFormatsToLaunch = s.FileFormatsToLaunch,
            Emulators = new EmulatorsConfigDto
            {
                Emulator = s.Emulator == null
                    ? null
                    : new EmulatorConfigDto
                    {
                        EmulatorName = s.Emulator.EmulatorName,
                        EmulatorLocation = s.Emulator.EmulatorLocation,
                        EmulatorParameters = s.Emulator.EmulatorParameters,
                        EmulatorDownloadPage = s.Emulator.EmulatorDownloadPage,
                        EmulatorLatestVersion = s.Emulator.EmulatorLatestVersion,
                        EmulatorDownloadLink = s.Emulator.EmulatorDownloadLink,
                        EmulatorDownloadExtractPath = s.Emulator.EmulatorDownloadExtractPath,
                        CoreLocation = s.Emulator.CoreLocation,
                        CoreLatestVersion = s.Emulator.CoreLatestVersion,
                        CoreDownloadLink = s.Emulator.CoreDownloadLink,
                        CoreDownloadExtractPath = s.Emulator.CoreDownloadExtractPath,
                        ImagePackDownloadLink = s.Emulator.ImagePackDownloadLink,
                        ImagePackDownloadLink2 = s.Emulator.ImagePackDownloadLink2,
                        ImagePackDownloadLink3 = s.Emulator.ImagePackDownloadLink3,
                        ImagePackDownloadLink4 = s.Emulator.ImagePackDownloadLink4,
                        ImagePackDownloadLink5 = s.Emulator.ImagePackDownloadLink5,
                        ImagePackDownloadExtractPath = s.Emulator.ImagePackDownloadExtractPath
                    }
            }
        }).ToList();

        return Ok(dtos);
    }
}