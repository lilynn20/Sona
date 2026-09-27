namespace Sona.Application.Interfaces;

public sealed record TrackListItem(int Id, string Title, string ArtistName, string AlbumTitle, string FilePath);

public interface ITrackQueryService
{
    Task<IReadOnlyList<TrackListItem>> GetTracksAsync(string? searchText = null, CancellationToken cancellationToken = default);
}
