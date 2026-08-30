using System.Windows;
using FolderIconStudio.Native;
using FolderIconStudio.Services;
using FolderIconStudio.Views;

namespace FolderIconStudio;

public partial class App : System.Windows.Application
{
    private SingleInstanceService? _singleInstance;
    private SettingsService? _settings;
    private IconLibraryService? _library;
    private FolderIconService? _folderIcons;
    private RegistryService? _registry;
    private MainWindow? _mainWindow;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();

        _settings = new SettingsService();
        _settings.Load();
        _folderIcons = new FolderIconService();
        _registry = new RegistryService();
        _library = new IconLibraryService();
        _library.Initialize(_settings.Settings.LibraryPath);

        if (ElevationHelper.TryHandleElevatedJob(args, _folderIcons))
            return;

        _singleInstance = new SingleInstanceService();
        if (!_singleInstance.TryAcquirePrimary())
        {
            if (_singleInstance.TrySendToPrimary(args))
            {
                Shutdown();
                return;
            }
        }

        var targetFolder = ParseTargetFolder(args);

        if (!_settings.Settings.FirstRunCompleted)
            RunFirstTimeSetup();

        _mainWindow = new MainWindow(_settings, _library, _folderIcons, _registry, targetFolder);
        _mainWindow.Show();
    }

    private void RunFirstTimeSetup()
    {
        var dlg = new FirstRunDialog();
        dlg.ShowDialog();

        if (dlg.RegisterContextMenu)
        {
            var exe = Environment.ProcessPath ??
                      System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrEmpty(exe))
            {
                try
                {
                    _registry!.Install(exe);
                    _settings!.Settings.ContextMenuEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not register context menu:\n{ex.Message}",
                        "Folder Icon Studio", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        _settings!.Settings.FirstRunCompleted = true;
        _settings.Save();
    }

    private static string? ParseTargetFolder(string[] args)
    {
        foreach (var arg in args)
        {
            if (arg.StartsWith("--", StringComparison.Ordinal))
                continue;

            var path = PathHelper.Normalize(arg);
            if (Directory.Exists(path))
                return path;
        }

        return null;
    }

    public void HandleSecondaryInstance(string[] args)
    {
        var folder = ParseTargetFolder(args);
        if (_mainWindow != null)
        {
            if (!string.IsNullOrEmpty(folder))
                _mainWindow.SetTargetFolder(folder);
            else
            {
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
            }
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _library?.Dispose();
        _singleInstance?.Dispose();
        base.OnExit(e);
    }
}
