using Microsoft.Maui.Controls;
using Sona.Application.Interfaces;

namespace Sona.App;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object? sender, EventArgs e)
    {
        await LoadCurrentSettingsAsync();
    }

    private async Task LoadCurrentSettingsAsync()
    {
        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
        var settings = services?.GetService<ISettingsService>();

        if (settings is null)
        {
            return;
        }

        await settings.EnsureInitializedAsync();

        var configuredFolder = await settings.GetValueAsync("libraryFolder", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
        var autoScan = await settings.GetValueAsync("autoScan", true);

        LibraryFolderEntry.Text = string.IsNullOrWhiteSpace(configuredFolder)
            ? string.Empty
            : configuredFolder;

        AutoScanCheckBox.IsChecked = autoScan;
        AutoScanCheckBox.CheckedChanged += async (_, __) =>
        {
            await settings.SetValueAsync("autoScan", AutoScanCheckBox.IsChecked == true);
        };
    }

    private async void OnSaveFolderClicked(object? sender, EventArgs e)
    {
        try
        {
            var path = LibraryFolderEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                await DisplayAlert("Settings", "Please enter a valid folder path that exists on this machine.", "OK");
                return;
            }

            var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
            var settings = services?.GetService<ISettingsService>();

            if (settings is null)
            {
                await DisplayAlert("Settings", "The settings service is not available.", "OK");
                return;
            }

            await settings.SetValueAsync("libraryFolder", path);
            await DisplayAlert("Settings", $"Library folder saved: {path}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Settings", $"Couldn't save the folder: {ex.Message}", "OK");
        }
    }
}
