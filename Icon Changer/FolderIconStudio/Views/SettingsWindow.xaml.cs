using System.Windows;
using System.Windows.Controls;
using FolderIconStudio.Helpers;
using FolderIconStudio.Models;
using FolderIconStudio.Services;
using FolderIconStudio.ViewModels;
using Microsoft.Win32;

namespace FolderIconStudio.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsService _settings;
    private readonly RegistryService _registry;
    private readonly IconLibraryService _library;

    public SettingsWindow(SettingsService settings, RegistryService registry, IconLibraryService library)
    {
        InitializeComponent();
        _settings = settings;
        _registry = registry;
        _library = library;
        LoadSettings();
    }

    private void LoadSettings()
    {
        LibraryPathBox.Text = _settings.Settings.LibraryPath;
        ContextMenuToggle.IsChecked = _settings.Settings.ContextMenuEnabled;
        ThemeCombo.SelectedIndex = _settings.Settings.Theme switch
        {
            AppTheme.Light => 1,
            AppTheme.Dark => 2,
            _ => 0
        };
    }

    private void BrowseLibrary_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog
        {
            Title = "Select icon library folder",
            InitialDirectory = LibraryPathBox.Text
        };
        if (dlg.ShowDialog() == true)
            LibraryPathBox.Text = dlg.FolderName;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var newPath = LibraryPathBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(newPath))
        {
            MessageBox.Show("Library path cannot be empty.", "Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _settings.Settings.LibraryPath = newPath;
        _settings.Settings.Theme = ThemeCombo.SelectedIndex switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.System
        };

        var wantMenu = ContextMenuToggle.IsChecked == true;
        var exe = Environment.ProcessPath ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";

        try
        {
            if (wantMenu && !string.IsNullOrEmpty(exe))
            {
                _registry.Install(exe);
                _settings.Settings.ContextMenuEnabled = true;
            }
            else
            {
                _registry.Uninstall();
                _settings.Settings.ContextMenuEnabled = false;
            }

            _library.UpdatePath(newPath);
            _settings.Save();
            ThemeHelper.ApplyTheme(this, _settings.Settings.Theme);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save settings:\n{ex.Message}", "Settings",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
