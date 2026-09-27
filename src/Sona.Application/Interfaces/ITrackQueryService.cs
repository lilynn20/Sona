namespace Sona.Application.Interfaces;

public sealed record TrackListItem(int Id, string Title, string ArtistName, string AlbumTitle, string FilePath);

public interface ITrackQueryService
{
    Task<IReadOnlyList<TrackListItem>> GetTracksAsync(CancellationToken cancellationToken = default);
}
