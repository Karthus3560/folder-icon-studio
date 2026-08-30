using System.IO;
using System.Reflection;
using System.Text.Json;
using FolderIconStudio.Models;

namespace FolderIconStudio.Services;

public sealed class SettingsService
{
    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FolderIconStudio");

    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AppSettings Settings { get; private set; } = new();

    public event EventHandler? SettingsChanged;

    public void Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                Settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
        }
        catch
        {
            Settings = new AppSettings();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(SettingsDir);
        var json = JsonSerializer.Serialize(Settings, JsonOptions);
        File.WriteAllText(SettingsPath, json);
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddRecent(string iconPath)
    {
        Settings.RecentlyUsed.Remove(iconPath);
        Settings.RecentlyUsed.Insert(0, iconPath);
        if (Settings.RecentlyUsed.Count > 20)
            Settings.RecentlyUsed = Settings.RecentlyUsed.Take(20).ToList();
        Save();
    }

    public void ToggleFavorite(string iconPath)
    {
        if (Settings.Favorites.Contains(iconPath))
            Settings.Favorites.Remove(iconPath);
        else
            Settings.Favorites.Add(iconPath);
        Save();
    }

    public bool IsFavorite(string iconPath) => Settings.Favorites.Contains(iconPath);
}
