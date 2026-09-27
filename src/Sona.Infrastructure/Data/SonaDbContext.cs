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
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Track> Tracks => Set<Track>();

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

        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).IsRequired().HasMaxLength(512);
        });

        modelBuilder.Entity<Album>(entity =>
        {
            entity.HasIndex(x => new { x.ArtistId, x.Title }).IsUnique();
            entity.Property(x => x.Title).IsRequired().HasMaxLength(512);
            entity.HasOne(x => x.Artist)
                .WithMany(x => x.Albums)
                .HasForeignKey(x => x.ArtistId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasIndex(x => x.FilePath).IsUnique();
            entity.Property(x => x.Title).IsRequired().HasMaxLength(512);
            entity.Property(x => x.FilePath).IsRequired().HasMaxLength(2048);
            entity.Property(x => x.Extension).HasMaxLength(32);

            entity.HasOne(x => x.Artist)
                .WithMany(x => x.Tracks)
                .HasForeignKey(x => x.ArtistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Album)
                .WithMany(x => x.Tracks)
                .HasForeignKey(x => x.AlbumId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
