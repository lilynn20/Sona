using Sona.Application.Interfaces;

namespace Sona.Infrastructure.Services;

public sealed class LibraryScannerService : ILibraryScanner
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3",
        ".flac",
        ".wav",
        ".m4a",
        ".aac",
        ".ogg",
        ".wma"
    };

    public Task<LibraryScanResult> ScanAsync(string rootPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            return Task.FromResult(new LibraryScanResult(0, 0, new List<string> { "Library path is empty." }));
        }

        if (!Directory.Exists(rootPath))
        {
            return Task.FromResult(new LibraryScanResult(0, 0, new List<string> { $"Folder does not exist: {rootPath}" }));
        }

        var files = new List<string>();
        var foldersScanned = 0;

        foreach (var directory in Directory.EnumerateDirectories(rootPath, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            foldersScanned++;
        }

        foreach (var file in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (SupportedExtensions.Contains(Path.GetExtension(file)))
            {
                files.Add(file);
            }
        }

        return Task.FromResult(new LibraryScanResult(files.Count, foldersScanned, new List<string>()));
    }
}
