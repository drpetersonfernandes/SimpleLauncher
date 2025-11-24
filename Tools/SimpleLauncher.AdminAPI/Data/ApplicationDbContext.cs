using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleLauncher.AdminAPI.Models;

namespace SimpleLauncher.AdminAPI.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
    public DbSet<EmulatorConfiguration> EmulatorConfigurations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure the one-to-one relationship between SystemConfiguration and EmulatorConfiguration
        builder.Entity<SystemConfiguration>()
            .HasOne(s => s.Emulator)
            .WithOne(e => e.SystemConfiguration)
            .HasForeignKey<EmulatorConfiguration>(e => e.SystemConfigurationId);
    }
}