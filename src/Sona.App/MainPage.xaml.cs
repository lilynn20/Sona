using Sona.Application.Interfaces;

namespace Sona.App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnScanClicked(object sender, EventArgs e)
    {
        var services = Application.Current?.Handler?.MauiContext?.Services;
        var scanner = services?.GetService<ILibraryScanner>();
        var settings = services?.GetService<ISettingsService>();

        if (scanner is null || settings is null)
        {
            LibraryStatusLabel.Text = "The library scanner is not available.";
            return;
        }

        var configuredFolder = await settings.GetValueAsync("libraryFolder", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
        var scanRoot = string.IsNullOrWhiteSpace(configuredFolder) ? Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) : configuredFolder;

        LibraryStatusLabel.Text = "Scanning library...";

        try
        {
            var result = await scanner.ScanAsync(scanRoot, CancellationToken.None);

            if (result.Errors.Count > 0)
            {
                LibraryStatusLabel.Text = result.Errors[0];
                return;
            }

            LibraryStatusLabel.Text = result.FilesFound > 0
                ? $"Scanned {result.FilesFound} audio files across {result.FoldersScanned} folders."
                : "No supported audio files were found in the configured library path.";

            SemanticScreenReader.Announce(LibraryStatusLabel.Text);
        }
        catch (OperationCanceledException)
        {
            LibraryStatusLabel.Text = "Library scan was cancelled.";
        }
        catch (Exception ex)
        {
            LibraryStatusLabel.Text = $"Scan failed: {ex.Message}";
        }
    }
}

