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
