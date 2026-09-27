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

    public async Task<IReadOnlyList<TrackListItem>> GetTracksAsync(string? searchText = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Tracks
            .AsNoTracking()
            .Include(x => x.Artist)
            .Include(x => x.Album)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var normalizedSearch = searchText.Trim();
            var lowerSearch = normalizedSearch.ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(lowerSearch) ||
                (x.Artist != null && x.Artist.Name.ToLower().Contains(lowerSearch)) ||
                (x.Album != null && x.Album.Title.ToLower().Contains(lowerSearch)));
        }

        return await query
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
