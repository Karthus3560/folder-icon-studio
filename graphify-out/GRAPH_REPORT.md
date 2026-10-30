# Graph Report - Icon Changer  (2026-10-30)

## Corpus Check
- cluster-only mode — file stats not available

## Summary
- 970 nodes · 1980 edges · 51 communities (31 shown, 20 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 92 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `f348bfca`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- FolderIconService
- FolderIconStudio.Services
- Icon Changer/folder_icon_studio/icon_library.py
- AppSettings
- FolderListWidget
- DllImport
- FolderListWidget
- Window
- RoutedEventArgs
- SettingsService
- MainViewModel
- MainViewModel
- CultureInfo
- categories
- categories
- folder_icon_studio/icon_library.py
- folder_icon_studio/win32_icons.py
- MainWindow
- IconLibraryService
- folder_icon_studio/app.py
- Icon Changer/folder_icon_studio/win32_icons.py
- FolderRowWidget
- .ReloadIcons
- IconViewModel
- Icon Changer/folder_icon_studio/app.py
- folder_icon_studio/generate_starters.py
- RelayCommand
- MainWindow
- ThumbnailCache
- FolderIconStudio
- IconLibraryService
- .FillRoundedRect
- _app_icon
- IconViewModel
- SingleInstanceService
- ImageSource
- FirstRunDialog
- FirstRunDialog

## God Nodes (most connected - your core abstractions)
1. `MainViewModel` - 46 edges
2. `MainViewModel` - 44 edges
3. `MainWindow` - 29 edges
4. `MainWindow` - 27 edges
5. `IconLibraryService` - 27 edges
6. `FolderIconService` - 26 edges
7. `Window` - 23 edges
8. `Window` - 23 edges
9. `SettingsService` - 21 edges
10. `categories` - 21 edges

## Surprising Connections (you probably didn't know these)
- `MainWindow` --uses--> `ThumbnailCache`  [INFERRED]
  folder_icon_studio/app.py → Icon Changer/folder_icon_studio/thumbnail_cache.py
- `main()` --uses--> `MainWindow`  [INFERRED]
  scripts/capture_screenshots.py → Icon Changer/folder_icon_studio/app.py
- `FolderListWidget` --uses--> `FolderRowWidget`  [INFERRED]
  folder_icon_studio/app.py → Icon Changer/folder_icon_studio/folder_row.py
- `App` --references--> `FolderIconService`  [EXTRACTED]
  Icon Changer/FolderIconStudio/App.xaml.cs → FolderIconStudio/Services/FolderIconService.cs
- `MainViewModel` --references--> `FolderIconService`  [EXTRACTED]
  Icon Changer/FolderIconStudio/ViewModels/MainViewModel.cs → FolderIconStudio/Services/FolderIconService.cs

## Import Cycles
- None detected.

## Communities (51 total, 20 thin omitted)

### Community 0 - "FolderIconService"
Cohesion: 0.07
Nodes (14): FileAttributesInterop, PathHelper, ElevationHelper, ElevationJob, FolderPath, IconPath, Operation, FolderIconService (+6 more)

### Community 1 - "FolderIconStudio.Services"
Cohesion: 0.09
Nodes (8): FolderIconStudio.Helpers, FolderIconStudio.Native, FolderIconStudio.Models, FolderIconStudio.ViewModels, FolderIconStudio, FolderIconStudio.Views, FolderIconStudio.Services, RegistryService

### Community 2 - "Icon Changer/folder_icon_studio/icon_library.py"
Cohesion: 0.08
Nodes (32): categorize_icon(), category_sort_key(), _matches(), _normalize_name(), _ico_size(), main(), prepare_bundled(), _safe_filename() (+24 more)

### Community 3 - "AppSettings"
Cohesion: 0.05
Nodes (40): AppSettings, ContextMenuEnabled, Favorites, FirstRunCompleted, LibraryPath, RecentlyUsed, Theme, AppTheme (+32 more)

### Community 4 - "FolderListWidget"
Cohesion: 0.06
Nodes (6): FolderListWidget, GlassPanel, IconGridWidget, OrbBackground, FolderRowWidget, IconPreview

### Community 5 - "DllImport"
Cohesion: 0.09
Nodes (10): ThemeHelper, DwmInterop, MARGINS, ShellInterop, ThemeHelper, DwmInterop, FileAttributesInterop, MARGINS (+2 more)

### Community 6 - "FolderListWidget"
Cohesion: 0.06
Nodes (7): FolderListWidget, GlassPanel, IconGridWidget, OrbBackground, _load_rgba(), _pil_to_pixmap(), ThumbnailCache

### Community 7 - "Window"
Cohesion: 0.10
Nodes (20): Window, BrowseFileCommand, DisplayName, FilteredIcons, IconMissing, IsFavorite, IsSelected, OpenLibraryCommand (+12 more)

### Community 8 - "RoutedEventArgs"
Cohesion: 0.10
Nodes (15): ContextMenuToggle, LibraryPathBox, ThemeCombo, Window, SettingsWindow, CheckBox, ComboBox, TextBox (+7 more)

### Community 9 - "SettingsService"
Cohesion: 0.16
Nodes (10): Application, App, MainWindow, ViewModel, RegistryService, SettingsService, Settings, SingleInstanceService (+2 more)

### Community 10 - "MainViewModel"
Cohesion: 0.07
Nodes (22): MainViewModel, AllIcons, BrowseFileCommand, CanSave, FavoriteIcons, FilteredIcons, HasCustomIcon, HasRecentIcons (+14 more)

### Community 11 - "MainViewModel"
Cohesion: 0.08
Nodes (22): MainViewModel, AllIcons, BrowseFileCommand, CanSave, FavoriteIcons, FilteredIcons, HasCustomIcon, HasRecentIcons (+14 more)

### Community 12 - "CultureInfo"
Cohesion: 0.14
Nodes (12): BoolToVisibilityConverter, Instance, CountToVisibilityConverter, Instance, FavoriteColorConverter, Instance, BoolToVisibilityConverter, Instance (+4 more)

### Community 13 - "categories"
Cohesion: 0.08
Nodes (25): categories, Adobe Creative, Apps & Utilities, Chat & Social, Development, Devices & Printers, Documents & Notes, Drives & Cloud (+17 more)

### Community 14 - "categories"
Cohesion: 0.08
Nodes (25): categories, Adobe Creative, Apps & Utilities, Chat & Social, Development, Devices & Printers, Documents & Notes, Drives & Cloud (+17 more)

### Community 16 - "folder_icon_studio/icon_library.py"
Cohesion: 0.20
Nodes (17): bundled_license_path(), bundled_manifest(), _bundled_version(), _copy_tree_all(), _copy_tree_if_missing(), default_library_path(), ensure_library(), icon_category() (+9 more)

### Community 17 - "folder_icon_studio/win32_icons.py"
Cohesion: 0.21
Nodes (11): apply_icon(), get_attributes(), get_current_icon_path(), get_folder_icon_state(), _long_path(), notify_folder_changed(), _prepare_folder(), _read_ini() (+3 more)

### Community 18 - "MainWindow"
Cohesion: 0.15
Nodes (5): _app_icon_path(), _assets_root(), _bundled_assets_dir(), MainWindow, _starter_assets_dir()

### Community 20 - "folder_icon_studio/app.py"
Cohesion: 0.15
Nodes (6): _enable_hd(), main(), bundled_publisher_json(), register_publisher_metadata(), bundled_publisher_json(), register_publisher_metadata()

### Community 21 - "Icon Changer/folder_icon_studio/win32_icons.py"
Cohesion: 0.32
Nodes (11): apply_icon(), get_attributes(), get_current_icon_path(), get_folder_icon_state(), _long_path(), notify_folder_changed(), _prepare_folder(), _read_ini() (+3 more)

### Community 23 - ".ReloadIcons"
Cohesion: 0.17
Nodes (6): IconItem, DisplayName, FilePath, IsFavorite, IsMissing, IsStarter

### Community 24 - "IconViewModel"
Cohesion: 0.14
Nodes (8): IconViewModel, DisplayName, FilePath, IsFavorite, IsMissing, IsSelected, Item, Preview

### Community 25 - "Icon Changer/folder_icon_studio/app.py"
Cohesion: 0.23
Nodes (7): _app_icon(), _app_icon_path(), _assets_root(), _bundled_assets_dir(), _enable_hd(), main(), _starter_assets_dir()

### Community 26 - "folder_icon_studio/generate_starters.py"
Cohesion: 0.18
Nodes (6): _darken(), _draw_folder(), generate_all(), _darken(), _draw_folder(), generate_all()

### Community 27 - "RelayCommand"
Cohesion: 0.15
Nodes (4): RelayCommand, RelayCommand, RelayCommand, RelayCommand

### Community 29 - "ThumbnailCache"
Cohesion: 0.23
Nodes (3): _load_rgba(), _pil_to_pixmap(), ThumbnailCache

### Community 30 - "FolderIconStudio"
Cohesion: 0.24
Nodes (10): FolderIconStudio, net8.0-windows, System.Drawing.Common (8.0.10), Microsoft.NET.Sdk, FolderIconStudio, System.Drawing.Common (8.0.10), StarterIconGenerator, System.Drawing.Common (8.0.10) (+2 more)

### Community 33 - "_app_icon"
Cohesion: 0.24
Nodes (7): _app_icon(), main(), capture_next(), queue_shots(), main(), capture_next(), queue_shots()

### Community 34 - "IconViewModel"
Cohesion: 0.20
Nodes (8): IconViewModel, DisplayName, FilePath, IsFavorite, IsMissing, IsSelected, Item, Preview

### Community 37 - "FirstRunDialog"
Cohesion: 0.47
Nodes (3): Window, FirstRunDialog, RegisterContextMenu

### Community 38 - "FirstRunDialog"
Cohesion: 0.47
Nodes (3): Window, FirstRunDialog, RegisterContextMenu

## Knowledge Gaps
- **122 isolated node(s):** `FolderPath`, `IconPath`, `Operation`, `FolderPath`, `IconPath` (+117 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 321 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **20 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `MainViewModel` connect `MainViewModel` to `FolderIconService`, `FolderIconStudio.Services`, `IconViewModel`, `Window`, `SettingsService`, `.ReloadIcons`, `IconViewModel`, `RelayCommand`, `IconLibraryService`?**
  _High betweenness centrality (0.048) - this node is a cross-community bridge._
- **Are the 3 inferred relationships involving `MainWindow` (e.g. with `ThumbnailCache` and `main()`) actually correct?**
  _`MainWindow` has 3 INFERRED edges - model-reasoned connections that need verification._
- **What connects `FolderPath`, `IconPath`, `Operation` to the rest of the system?**
  _122 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `FolderIconService` be split into smaller, more focused modules?**
  _Cohesion score 0.06519114688128773 - nodes in this community are weakly interconnected._
- **Why does `MainViewModel` connect `MainViewModel` to `FolderIconService`, `FolderIconStudio.Services`, `IconViewModel`, `Window`, `SettingsService`, `MainViewModel`, `.ReloadIcons`, `IconViewModel`, `RelayCommand`, `IconLibraryService`?**
  _High betweenness centrality (0.046) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `IconLibraryService` (e.g. with `.Application_Startup()` and `.Application_Startup()`) actually correct?**
  _`IconLibraryService` has 2 INFERRED edges - model-reasoned connections that need verification._
- **Should `FolderIconStudio.Services` be split into smaller, more focused modules?**
  _Cohesion score 0.08852459016393442 - nodes in this community are weakly interconnected._