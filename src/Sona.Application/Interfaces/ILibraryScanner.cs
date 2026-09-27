namespace Sona.Application.Interfaces;

public sealed record LibraryScanResult(int FilesFound, int FoldersScanned, List<string> Errors);

public interface ILibraryScanner
{
    Task<LibraryScanResult> ScanAsync(string rootPath, CancellationToken cancellationToken = default);
}
