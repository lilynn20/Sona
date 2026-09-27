using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sona.Application.Interfaces;
using Sona.Infrastructure.Data;
using Sona.Infrastructure.Services;

namespace Sona.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "sona.db");

		builder.Services.AddDbContext<SonaDbContext>(options =>
		{
			options.UseSqlite($"Data Source={dbPath}");
		});
		builder.Services.AddScoped<ISettingsService, SettingsService>();
		builder.Services.AddScoped<ILibraryScanner, LibraryScannerService>();
		builder.Services.AddScoped<ILibraryImportService, LibraryImportService>();
		builder.Services.AddScoped<ITrackQueryService, TrackQueryService>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
