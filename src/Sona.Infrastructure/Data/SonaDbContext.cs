using Microsoft.EntityFrameworkCore;
using Sona.Domain;

namespace Sona.Infrastructure.Data;

public sealed class SonaDbContext : DbContext
{
    public SonaDbContext(DbContextOptions<SonaDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<LibraryFolder> LibraryFolders => Set<LibraryFolder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.HasIndex(x => x.Key).IsUnique();
            entity.Property(x => x.Key).IsRequired().HasMaxLength(256);
            entity.Property(x => x.Value).IsRequired();
        });

        modelBuilder.Entity<LibraryFolder>(entity =>
        {
            entity.Property(x => x.Path).IsRequired().HasMaxLength(2048);
        });
    }
}
