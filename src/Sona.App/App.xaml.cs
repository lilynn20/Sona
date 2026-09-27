using Microsoft.Maui.Controls;

namespace Sona.App;

public partial class App : Microsoft.Maui.Controls.Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}
