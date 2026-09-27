using Sona.Infrastructure.Services;
using Xunit;

namespace Sona.Tests;

public sealed class LibraryScannerTests
{
    [Fact]
    public async Task ScanAsync_WhenDirectoryExists_ReturnsFilesFoundAndFoldersScanned()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"sona-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        Directory.CreateDirectory(Path.Combine(tempRoot, "Album One"));
        Directory.CreateDirectory(Path.Combine(tempRoot, "Album Two"));

        var trackOne = Path.Combine(tempRoot, "Album One", "track1.mp3");
        var trackTwo = Path.Combine(tempRoot, "Album Two", "track2.flac");
        var cover = Path.Combine(tempRoot, "notes.txt");

        await File.WriteAllTextAsync(trackOne, "audio");
        await File.WriteAllTextAsync(trackTwo, "audio");
        await File.WriteAllTextAsync(cover, "ignore me");

        try
        {
            var scanner = new LibraryScannerService();
            var result = await scanner.ScanAsync(tempRoot, CancellationToken.None);

            Assert.Equal(2, result.FilesFound);
            Assert.Equal(2, result.FoldersScanned);
            Assert.Empty(result.Errors);
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }
}
