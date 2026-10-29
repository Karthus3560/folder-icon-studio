using Microsoft.Win32;

namespace FolderIconStudio.Services;

public sealed class RegistryService
{
    private const string ShellKeyPath = @"Software\Classes\Directory\shell\ChangeFolderIcon";
    private const string CommandKeyPath = ShellKeyPath + @"\command";

    public bool IsInstalled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(ShellKeyPath);
        return key != null;
    }

    public void Install(string exePath)
    {
        exePath = Path.GetFullPath(exePath);
        if (!File.Exists(exePath))
            throw new FileNotFoundException("Executable not found.", exePath);

        using var shellKey = Registry.CurrentUser.CreateSubKey(ShellKeyPath, true)
            ?? throw new InvalidOperationException("Failed to create registry key.");
        shellKey.SetValue("MUIVerb", "Change Icon…");
        shellKey.SetValue("Icon", $"{exePath},0");

        using var commandKey = Registry.CurrentUser.CreateSubKey(CommandKeyPath, true)
            ?? throw new InvalidOperationException("Failed to create command registry key.");
        commandKey.SetValue("", $"\"{exePath}\" \"%V\"");
    }

    public void Uninstall()
    {
        Registry.CurrentUser.DeleteSubKeyTree(CommandKeyPath, throwOnMissingSubKey: false);
        Registry.CurrentUser.DeleteSubKeyTree(ShellKeyPath, throwOnMissingSubKey: false);
    }
}
