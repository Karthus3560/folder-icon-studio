using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using FolderIconStudio.Models;
using FolderIconStudio.Services;
using Microsoft.Win32;

namespace FolderIconStudio.ViewModels;

public sealed class IconViewModel : INotifyPropertyChanged
{
    private bool _isSelected;
    private bool _isFavorite;

    public IconItem Item { get; }
    public ImageSource? Preview { get; }

    public string DisplayName => Item.DisplayName;
    public string FilePath => Item.FilePath;
    public bool IsMissing => Item.IsMissing;

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    public bool IsFavorite
    {
        get => _isFavorite;
        set { _isFavorite = value; OnPropertyChanged(); }
    }

    public IconViewModel(IconItem item, ImageSource? preview)
    {
        Item = item;
        Preview = preview;
        _isFavorite = item.IsFavorite;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly SettingsService _settings;
    private readonly IconLibraryService _library;
    private readonly FolderIconService _folderIcons;
    private readonly RegistryService _registry;

    private string _searchText = "";
    private string? _targetFolder;
    private IconViewModel? _selectedIcon;
    private string _statusText = "Select a folder icon";
    private bool _hasTargetFolder;
    private bool _hasCustomIcon;
    private bool _iconMissing;

    private bool _hasRecentIcons;

    public ObservableCollection<IconViewModel> AllIcons { get; } = [];
    public ObservableCollection<IconViewModel> RecentIcons { get; } = [];
    public ObservableCollection<IconViewModel> FavoriteIcons { get; } = [];
    public ObservableCollection<IconViewModel> FilteredIcons { get; } = [];

    public bool HasRecentIcons
    {
        get => _hasRecentIcons;
        private set { _hasRecentIcons = value; OnPropertyChanged(); }
    }

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
    }

    public string? TargetFolder
    {
        get => _targetFolder;
        set
        {
            _targetFolder = value;
            _hasTargetFolder = !string.IsNullOrEmpty(value) && Directory.Exists(value);
            OnPropertyChanged();
            OnPropertyChanged(nameof(TargetFolderDisplay));
            OnPropertyChanged(nameof(HasTargetFolder));
            RefreshFolderState();
        }
    }

    public string TargetFolderDisplay => string.IsNullOrEmpty(_targetFolder)
        ? "Browse mode — pick a folder to customize"
        : _targetFolder!;

    public bool HasTargetFolder
    {
        get => _hasTargetFolder;
        private set { _hasTargetFolder = value; OnPropertyChanged(); }
    }

    public bool HasCustomIcon
    {
        get => _hasCustomIcon;
        private set { _hasCustomIcon = value; OnPropertyChanged(); }
    }

    public bool IconMissing
    {
        get => _iconMissing;
        private set { _iconMissing = value; OnPropertyChanged(); }
    }

    public IconViewModel? SelectedIcon
    {
        get => _selectedIcon;
        set
        {
            if (_selectedIcon != null)
                _selectedIcon.IsSelected = false;
            _selectedIcon = value;
            if (_selectedIcon != null)
                _selectedIcon.IsSelected = true;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSave));
        }
    }

    public bool CanSave => SelectedIcon != null && HasTargetFolder && !SelectedIcon.IsMissing;

    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    public ICommand SaveCommand { get; }
    public ICommand RestoreCommand { get; }
    public ICommand BrowseFileCommand { get; }
    public ICommand OpenLibraryCommand { get; }
    public ICommand SelectIconCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand PickFolderCommand { get; }

    public MainViewModel(
        SettingsService settings,
        IconLibraryService library,
        FolderIconService folderIcons,
        RegistryService registry)
    {
        _settings = settings;
        _library = library;
        _folderIcons = folderIcons;
        _registry = registry;

        SaveCommand = new RelayCommand(Save, () => CanSave);
        RestoreCommand = new RelayCommand(Restore, () => HasTargetFolder);
        BrowseFileCommand = new RelayCommand(BrowseFile);
        OpenLibraryCommand = new RelayCommand(OpenLibrary);
        SelectIconCommand = new RelayCommand<IconViewModel>(SelectIcon);
        ToggleFavoriteCommand = new RelayCommand<IconViewModel>(ToggleFavorite);
        PickFolderCommand = new RelayCommand(PickFolder);

        _library.LibraryChanged += (_, _) => ReloadIcons();
        ReloadIcons();
    }

    public void ReloadIcons()
    {
        AllIcons.Clear();
        RecentIcons.Clear();
        FavoriteIcons.Clear();

        var items = _library.GetAllIcons(_settings);
        var state = HasTargetFolder ? _folderIcons.GetState(_targetFolder!) : null;

        foreach (var item in items)
        {
            if (state?.CurrentIconPath != null &&
                string.Equals(item.FilePath, state.CurrentIconPath, StringComparison.OrdinalIgnoreCase))
            {
                // will select after load
            }

            var missing = !File.Exists(item.FilePath);
            var vm = new IconViewModel(
                new IconItem
                {
                    FilePath = item.FilePath,
                    DisplayName = item.DisplayName,
                    IsStarter = item.IsStarter,
                    IsFavorite = item.IsFavorite,
                    IsMissing = missing
                },
                missing ? null : IconImageHelper.LoadBestSizeIcon(item.FilePath, 96));

            AllIcons.Add(vm);
            if (vm.IsFavorite)
                FavoriteIcons.Add(vm);
        }

        foreach (var path in _settings.Settings.RecentlyUsed)
        {
            var vm = AllIcons.FirstOrDefault(i =>
                string.Equals(i.FilePath, path, StringComparison.OrdinalIgnoreCase));
            if (vm != null)
                RecentIcons.Add(vm);
        }

        ApplyFilter();
        HasRecentIcons = RecentIcons.Count > 0;

        if (state?.CurrentIconPath != null)
        {
            var current = AllIcons.FirstOrDefault(i =>
                string.Equals(i.FilePath, state.CurrentIconPath, StringComparison.OrdinalIgnoreCase));
            if (current != null)
                SelectedIcon = current;
        }
    }

    private void ApplyFilter()
    {
        FilteredIcons.Clear();
        var query = string.IsNullOrWhiteSpace(SearchText)
            ? AllIcons
            : AllIcons.Where(i => i.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var icon in query)
            FilteredIcons.Add(icon);
    }

    private void RefreshFolderState()
    {
        if (!HasTargetFolder)
        {
            HasCustomIcon = false;
            IconMissing = false;
            return;
        }

        var state = _folderIcons.GetState(_targetFolder!);
        HasCustomIcon = state.HasCustomIcon;
        IconMissing = state.IconFileMissing;

        if (IconMissing)
            StatusText = "The current icon file is missing. Choose a new icon or restore default.";
        else if (HasCustomIcon)
            StatusText = "This folder has a custom icon. Click an icon to preview.";
        else
            StatusText = "Click an icon to preview on this folder.";
    }

    private void SelectIcon(IconViewModel? vm)
    {
        if (vm == null || vm.IsMissing)
            return;

        SelectedIcon = vm;

        if (HasTargetFolder)
        {
            try
            {
                _folderIcons.PreviewIcon(_targetFolder!, vm.FilePath);
                StatusText = $"Previewing: {vm.DisplayName}";
            }
            catch (Exception ex)
            {
                StatusText = $"Preview failed: {ex.Message}";
                if (ex is UnauthorizedAccessException or IOException)
                {
                    MessageBox.Show($"Could not preview icon on this folder:\n{ex.Message}",
                        "Folder Icon Studio", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        else
        {
            StatusText = $"Selected: {vm.DisplayName} — pick a target folder to apply";
        }
    }

    private void Save()
    {
        if (SelectedIcon == null || !HasTargetFolder)
            return;

        try
        {
            _folderIcons.ApplyIcon(_targetFolder!, SelectedIcon.FilePath);
            _settings.AddRecent(SelectedIcon.FilePath);
            RefreshFolderState();
            StatusText = $"Saved icon: {SelectedIcon.DisplayName}";
            ReloadIcons();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save icon:\n{ex.Message}", "Folder Icon Studio",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Restore()
    {
        if (!HasTargetFolder)
            return;

        try
        {
            _folderIcons.RestoreDefault(_targetFolder!);
            SelectedIcon = null;
            RefreshFolderState();
            StatusText = "Restored default folder icon.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to restore default:\n{ex.Message}", "Folder Icon Studio",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BrowseFile()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Images|*.png;*.jpg;*.jpeg;*.ico;*.svg|Icon files (*.ico)|*.ico|All files|*.*",
            Title = "Import icon image"
        };

        if (dlg.ShowDialog() != true)
            return;

        try
        {
            var dest = _library.ImportImage(dlg.FileName);
            _settings.AddRecent(dest);
            ReloadIcons();
            var imported = AllIcons.FirstOrDefault(i =>
                string.Equals(i.FilePath, dest, StringComparison.OrdinalIgnoreCase));
            if (imported != null)
                SelectIcon(imported);
            StatusText = $"Imported: {Path.GetFileName(dest)}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Import failed:\n{ex.Message}", "Folder Icon Studio",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void ImportDroppedFiles(string[] files)
    {
        foreach (var file in files)
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();
            if (ext is not (".png" or ".jpg" or ".jpeg" or ".ico" or ".svg"))
                continue;

            try
            {
                var dest = _library.ImportImage(file);
                _settings.AddRecent(dest);
                StatusText = $"Imported: {Path.GetFileName(dest)}";
            }
            catch (Exception ex)
            {
                StatusText = $"Import failed: {ex.Message}";
            }
        }

        ReloadIcons();
    }

    private void OpenLibrary()
    {
        var path = _library.LibraryPath;
        Directory.CreateDirectory(path);
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    private void ToggleFavorite(IconViewModel? vm)
    {
        if (vm == null)
            return;

        _settings.ToggleFavorite(vm.FilePath);
        vm.IsFavorite = _settings.IsFavorite(vm.FilePath);
        ReloadIcons();
    }

    private void PickFolder()
    {
        var dlg = new OpenFolderDialog
        {
            Title = "Select a folder to customize"
        };

        if (dlg.ShowDialog() == true)
            TargetFolder = dlg.FolderName;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();
}

public sealed class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;

    public RelayCommand(Action<T?> execute) => _execute = execute;

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute(parameter is T t ? t : default);
}
