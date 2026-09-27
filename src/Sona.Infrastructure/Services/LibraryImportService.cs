using Microsoft.EntityFrameworkCore;
using Sona.Application.Interfaces;
using Sona.Domain;
using Sona.Infrastructure.Data;

namespace Sona.Infrastructure.Services;

public sealed class LibraryImportService : ILibraryImportService
{
    private const string SupportedAudioExtensions = ".mp3;.flac;.wav;.m4a;.aac;.ogg;.wma";
    private readonly SonaDbContext _dbContext;

    public LibraryImportService(SonaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LibraryImportResult> ImportAsync(string rootPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
        {
            return new LibraryImportResult(0, new List<string> { "Library path is not available." });
        }

        var errors = new List<string>();
        var filesImported = 0;

        foreach (var file in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var extension = Path.GetExtension(file);
            var supportedExtensions = SupportedAudioExtensions.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (!supportedExtensions.Any(x => string.Equals(x, extension, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            try
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var directoryName = Path.GetDirectoryName(file) is { } dir && !string.IsNullOrWhiteSpace(dir)
                    ? Path.GetFileName(dir)
                    : string.Empty;

                var artistName = GetArtistName(rootPath, file);
                var albumTitle = GetAlbumTitle(rootPath, file, directoryName);

                var artist = await _dbContext.Artists.FirstOrDefaultAsync(x => x.Name == artistName, cancellationToken)
                    ?? new Artist { Name = artistName };

                if (artist.Id == 0)
                {
                    _dbContext.Artists.Add(artist);
                }

                Album? album = null;
                if (!string.IsNullOrWhiteSpace(albumTitle))
                {
                    album = await _dbContext.Albums
                        .Include(x => x.Artist)
                        .FirstOrDefaultAsync(x => x.ArtistId == artist.Id && x.Title == albumTitle, cancellationToken);

                    if (album is null)
                    {
                        album = new Album { Title = albumTitle, Artist = artist };
                        _dbContext.Albums.Add(album);
                    }
                }

                var track = new Track
                {
                    Title = fileName,
                    FilePath = file,
                    Artist = artist,
                    Album = album,
                    Extension = extension,
                    DurationSeconds = 0,
                    TrackNumber = 0
                };

                var existingTrack = await _dbContext.Tracks
                    .FirstOrDefaultAsync(x => x.FilePath == file, cancellationToken);

                if (existingTrack is null)
                {
                    _dbContext.Tracks.Add(track);
                    filesImported++;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to import {file}: {ex.Message}");
            }
        }

        return new LibraryImportResult(filesImported, errors);
    }

    private static string GetArtistName(string rootPath, string filePath)
    {
        var relative = Path.GetRelativePath(rootPath, filePath);
        var segments = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        return segments.Length > 1 ? segments[0] : "Unknown Artist";
    }

    private static string GetAlbumTitle(string rootPath, string filePath, string directoryName)
    {
        var relative = Path.GetRelativePath(rootPath, filePath);
        var segments = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        if (segments.Length >= 2)
        {
            return segments[^2];
        }

        return string.IsNullOrWhiteSpace(directoryName) ? "Unknown Album" : directoryName;
    }
}
