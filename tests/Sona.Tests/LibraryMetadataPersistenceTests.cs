using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sona.Domain;
using Sona.Infrastructure.Data;
using Xunit;

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
}
