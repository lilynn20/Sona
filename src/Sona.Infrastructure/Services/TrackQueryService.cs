using Microsoft.EntityFrameworkCore;
using Sona.Application.Interfaces;
using Sona.Infrastructure.Data;

namespace Sona.Infrastructure.Services;

public sealed class TrackQueryService : ITrackQueryService
{
    private readonly SonaDbContext _dbContext;

    public TrackQueryService(SonaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TrackListItem>> GetTracksAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tracks
            .AsNoTracking()
            .Include(x => x.Artist)
            .Include(x => x.Album)
            .OrderBy(x => x.Title)
            .Select(x => new TrackListItem(
                x.Id,
                x.Title,
                x.Artist != null ? x.Artist.Name : "Unknown Artist",
                x.Album != null ? x.Album.Title : "Unknown Album",
                x.FilePath))
            .ToListAsync(cancellationToken);
    }
}
