using Microsoft.EntityFrameworkCore;
using Sona.Application.Interfaces;
using Sona.Domain;
using Sona.Infrastructure.Data;

namespace Sona.Infrastructure.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly SonaDbContext _dbContext;

    public SettingsService(SonaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }

    public async Task<T> GetValueAsync<T>(string key, T defaultValue, CancellationToken cancellationToken = default)
    {
        var setting = await _dbContext.AppSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == key, cancellationToken);

        if (setting is null)
        {
            return defaultValue;
        }

        var value = setting.Value;
        return value is null ? defaultValue : (T)Convert.ChangeType(value, typeof(T));
    }

    public async Task SetValueAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        var setting = await _dbContext.AppSettings
            .FirstOrDefaultAsync(x => x.Key == key, cancellationToken);

        var serialized = value?.ToString() ?? string.Empty;

        if (setting is null)
        {
            _dbContext.AppSettings.Add(new AppSetting { Key = key, Value = serialized });
        }
        else
        {
            setting.Value = serialized;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> GetLibraryRootAsync(CancellationToken cancellationToken = default)
    {
        var configuredFolder = await GetValueAsync("libraryFolder", string.Empty, cancellationToken);
        return string.IsNullOrWhiteSpace(configuredFolder)
            ? Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)
            : configuredFolder;
    }

    public async Task<bool> GetAutoScanAsync(CancellationToken cancellationToken = default)
    {
        return await GetValueAsync("autoScan", true, cancellationToken);
    }
}
