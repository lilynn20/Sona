using System.Diagnostics;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sona.Application.Interfaces;
using Sona.Domain;
using Sona.Infrastructure.Data;
using Sona.Infrastructure.Services;
using TagLib;
using Xunit;
using TagFile = TagLib.File;

namespace Sona.Tests;

public sealed class LibraryMetadataPersistenceTests
{
    [Fact]
    public async Task SaveTrack_WithArtistAndAlbum_PersistsRelationship()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<SonaDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var context = new SonaDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var artist = new Artist { Name = "The Beatles" };
            var album = new Album { Title = "Abbey Road", Artist = artist };
            var track = new Track
            {
                Title = "Come Together",
                FilePath = "C:/Music/Abbey Road/Come Together.mp3",
                Artist = artist,
                Album = album
            };

            context.Tracks.Add(track);
            await context.SaveChangesAsync();
        }

        await using (var context = new SonaDbContext(options))
        {
            var track = await context.Tracks
                .Include(x => x.Artist)
                .Include(x => x.Album)
                .SingleAsync();

            Assert.NotNull(track.Artist);
            Assert.NotNull(track.Album);
            Assert.Equal("Come Together", track.Title);
            Assert.Equal("The Beatles", track.Artist.Name);
            Assert.Equal("Abbey Road", track.Album.Title);
        }
    }

    [Fact]
    public async Task ImportAsync_WhenFilesExist_PersistsTracksWithArtistsAndAlbums()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"sona-import-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        Directory.CreateDirectory(Path.Combine(tempRoot, "The Beatles"));
        Directory.CreateDirectory(Path.Combine(tempRoot, "The Beatles", "Abbey Road"));

        var filePath = Path.Combine(tempRoot, "The Beatles", "Abbey Road", "Come Together.mp3");
        await System.IO.File.WriteAllTextAsync(filePath, "audio");

        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<SonaDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var context = new SonaDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
            var service = new LibraryImportService(context);

            var result = await service.ImportAsync(tempRoot, CancellationToken.None);

            Assert.Equal(1, result.FilesImported);
            Assert.Empty(result.Errors);

            var track = await context.Tracks
                .Include(x => x.Artist)
                .Include(x => x.Album)
                .SingleAsync();

            Assert.NotNull(track.Artist);
            Assert.NotNull(track.Album);
            Assert.Equal("Come Together", track.Title);
            Assert.Equal("The Beatles", track.Artist.Name);
            Assert.Equal("Abbey Road", track.Album.Title);
        }

        Directory.Delete(tempRoot, recursive: true);
    }

    [Fact]
    public async Task ImportAsync_WhenAudioTagsExist_UsesMetadataInsteadOfFolderNames()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"sona-tag-import-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        Directory.CreateDirectory(Path.Combine(tempRoot, "Folder Artist"));
        Directory.CreateDirectory(Path.Combine(tempRoot, "Folder Artist", "Folder Album"));

        var filePath = Path.Combine(tempRoot, "Folder Artist", "Folder Album", "Ignored-Name.mp3");
        await CreateValidMp3Async(filePath);

        using (var tagFile = TagFile.Create(filePath))
        {
            tagFile.Tag.Title = "Actual Song Title";
            tagFile.Tag.Album = "Actual Album";
            tagFile.Tag.Performers = new[] { "Actual Artist" };
            tagFile.Save();
        }

        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<SonaDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var context = new SonaDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
            var service = new LibraryImportService(context);

            var result = await service.ImportAsync(tempRoot, CancellationToken.None);

            Assert.Equal(1, result.FilesImported);
            Assert.Empty(result.Errors);

            var track = await context.Tracks
                .Include(x => x.Artist)
                .Include(x => x.Album)
                .SingleAsync();

            Assert.NotNull(track.Artist);
            Assert.NotNull(track.Album);
            Assert.Equal("Actual Song Title", track.Title);
            Assert.Equal("Actual Artist", track.Artist.Name);
            Assert.Equal("Actual Album", track.Album.Title);
        }

        Directory.Delete(tempRoot, recursive: true);
    }

    [Fact]
    public async Task SettingsService_WhenValueSaved_RetrievesConfiguredLibraryFolder()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<SonaDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var context = new SonaDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
            var service = new SettingsService(context);

            var expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Sona Library");
            await service.SetValueAsync("libraryFolder", expected);

            var actual = await service.GetValueAsync("libraryFolder", string.Empty);
            Assert.Equal(expected, actual);
        }
    }

    private static async Task CreateValidMp3Async(string filePath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-y -f lavfi -i sine=frequency=1000:duration=1 -q:a 9 \"{filePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            throw new InvalidOperationException("ffmpeg is not available on PATH.");
        }

        await process.WaitForExitAsync();
        var error = await process.StandardError.ReadToEndAsync();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"ffmpeg failed to create a valid sample: {error}");
        }
    }
}
