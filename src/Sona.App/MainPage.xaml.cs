namespace Sona.App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnScanClicked(object sender, EventArgs e)
    {
        LibraryStatusLabel.Text = "Scan queued. Library import will start in Phase 2.";
        SemanticScreenReader.Announce(LibraryStatusLabel.Text);
    }
}

