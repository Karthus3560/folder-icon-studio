namespace FolderIconStudio.Models;

public enum AppTheme
{
    System,
    Light,
    Dark
}

public sealed class AppSettings
{
    public string LibraryPath { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
        "Folder Icons");

    public AppTheme Theme { get; set; } = AppTheme.System;
    public bool ContextMenuEnabled { get; set; }
    public bool FirstRunCompleted { get; set; }
    public List<string> Favorites { get; set; } = [];
    public List<string> RecentlyUsed { get; set; } = [];
}

public sealed class IconItem
{
    public required string FilePath { get; init; }
    public required string DisplayName { get; init; }
    public bool IsStarter { get; init; }
    public bool IsFavorite { get; set; }
    public bool IsMissing { get; init; }
}

public sealed class FolderIconState
{
    public string? CurrentIconPath { get; init; }
    public bool HasCustomIcon { get; init; }
    public bool IconFileMissing { get; init; }
    public bool AppSetAttributes { get; init; }
}
