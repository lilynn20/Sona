using Microsoft.Maui.Controls;
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
        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
        var importer = services?.GetService<ILibraryImportService>();
        var settings = services?.GetService<ISettingsService>();

        if (importer is null || settings is null)
        {
            LibraryStatusLabel.Text = "The library import service is not available.";
            return;
        }

        var configuredFolder = await settings.GetValueAsync("libraryFolder", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
        var scanRoot = string.IsNullOrWhiteSpace(configuredFolder) ? Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) : configuredFolder;

        LibraryStatusLabel.Text = "Importing library...";

        try
        {
            var result = await importer.ImportAsync(scanRoot, CancellationToken.None);

            if (result.Errors.Count > 0)
            {
                LibraryStatusLabel.Text = result.Errors[0];
                return;
            }

            LibraryStatusLabel.Text = result.FilesImported > 0
                ? $"Imported {result.FilesImported} audio files into the library database."
                : "No supported audio files were found in the configured library path.";

            SemanticScreenReader.Announce(LibraryStatusLabel.Text);
        }
        catch (OperationCanceledException)
        {
            LibraryStatusLabel.Text = "Library import was cancelled.";
        }
        catch (Exception ex)
        {
            LibraryStatusLabel.Text = $"Import failed: {ex.Message}";
        }
    }
}

