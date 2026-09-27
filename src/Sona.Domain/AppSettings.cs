namespace Sona.Domain;

public sealed class AppSetting
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public sealed class LibraryFolder
{
    public int Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}

public sealed class Artist
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Album> Albums { get; set; } = new List<Album>();
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}

public sealed class Album
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}

public sealed class Track
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;
    public int? AlbumId { get; set; }
    public Album? Album { get; set; }
    public int DurationSeconds { get; set; }
    public int TrackNumber { get; set; }
    public string Extension { get; set; } = string.Empty;
}
