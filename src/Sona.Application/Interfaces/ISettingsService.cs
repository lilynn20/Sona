namespace Sona.Application.Interfaces;

public interface ISettingsService
{
    Task<T> GetValueAsync<T>(string key, T defaultValue, CancellationToken cancellationToken = default);
    Task SetValueAsync<T>(string key, T value, CancellationToken cancellationToken = default);
    Task EnsureInitializedAsync(CancellationToken cancellationToken = default);
}
