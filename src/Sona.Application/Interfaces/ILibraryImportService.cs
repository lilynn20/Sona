namespace Sona.Application.Interfaces;

public sealed record LibraryImportResult(int FilesImported, List<string> Errors);

public interface ILibraryImportService
{
    Task<LibraryImportResult> ImportAsync(string rootPath, CancellationToken cancellationToken = default);
}
