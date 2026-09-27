using Microsoft.Maui.Controls;
using Sona.Application.Interfaces;

namespace Sona.App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object? sender, EventArgs e)
    {
        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
        var settings = services?.GetService<ISettingsService>();

        if (settings is null)
        {
            return;
        }

        await settings.EnsureInitializedAsync();

        if (await settings.GetAutoScanAsync())
        {
            await ScanLibraryAsync();
        }

        await RefreshTracksAsync();
    }

    private async void OnScanClicked(object sender, EventArgs e)
    {
        await ScanLibraryAsync();
    }

    private async Task ScanLibraryAsync()
    {
        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
        var importer = services?.GetService<ILibraryImportService>();
        var settings = services?.GetService<ISettingsService>();

        if (importer is null || settings is null)
        {
            LibraryStatusLabel.Text = "The library import service is not available.";
            return;
        }

        await settings.EnsureInitializedAsync();
        var scanRoot = await settings.GetLibraryRootAsync();

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

            await RefreshTracksAsync();
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

    private async Task RefreshTracksAsync()
    {
        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
        var trackQuery = services?.GetService<ITrackQueryService>();

        if (trackQuery is null)
        {
            return;
        }

        var tracks = await trackQuery.GetTracksAsync();
        TrackListView.ItemsSource = tracks;
        TrackCountLabel.Text = tracks.Count.ToString();

        var uniqueArtists = tracks
            .Select(x => x.ArtistName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        ArtistCountLabel.Text = uniqueArtists.ToString();
    }
}

