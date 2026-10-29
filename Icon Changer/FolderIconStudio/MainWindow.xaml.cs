using System.Windows;
using System.Windows.Input;
using FolderIconStudio.Helpers;
using FolderIconStudio.Services;
using FolderIconStudio.ViewModels;
using FolderIconStudio.Views;

namespace FolderIconStudio;

public partial class MainWindow : Window
{
    private readonly SettingsService _settings;
    private readonly RegistryService _registry;
    private readonly IconLibraryService _library;
    public MainViewModel ViewModel { get; }

    public MainWindow(
        SettingsService settings,
        IconLibraryService library,
        FolderIconService folderIcons,
        RegistryService registry,
        string? targetFolder)
    {
        InitializeComponent();
        _settings = settings;
        _registry = registry;
        _library = library;

        ViewModel = new MainViewModel(settings, library, folderIcons, registry);
        DataContext = ViewModel;

        if (!string.IsNullOrEmpty(targetFolder) && Directory.Exists(targetFolder))
            ViewModel.TargetFolder = targetFolder;

        ThemeHelper.ApplyTheme(this, settings.Settings.Theme);
        settings.SettingsChanged += (_, _) =>
            ThemeHelper.ApplyTheme(this, settings.Settings.Theme);

        Activated += (_, _) => ViewModel.ReloadIcons();
    }

    public void SetTargetFolder(string folder)
    {
        if (Directory.Exists(folder))
            ViewModel.TargetFolder = folder;

        if (WindowState == WindowState.Minimized)
            WindowState = WindowState.Normal;
        Activate();
        Topmost = true;
        Topmost = false;
        Focus();
    }

    private void IconTile_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { Tag: IconViewModel vm })
            ViewModel.SelectIconCommand.Execute(vm);
    }

    private void Favorite_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (sender is FrameworkElement { Tag: IconViewModel vm })
            ViewModel.ToggleFavoriteCommand.Execute(vm);
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SettingsWindow(_settings, _registry, _library)
        {
            Owner = this
        };
        if (dlg.ShowDialog() == true)
            ThemeHelper.ApplyTheme(this, _settings.Settings.Theme);
    }

    private void Window_DragOver(object sender, System.Windows.DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? System.Windows.DragDropEffects.Copy
            : System.Windows.DragDropEffects.None;
        e.Handled = true;
    }

    private void Window_Drop(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
            ViewModel.ImportDroppedFiles(files);
    }
}
