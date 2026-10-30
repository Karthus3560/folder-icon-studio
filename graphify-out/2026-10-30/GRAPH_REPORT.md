# Graph Report - Icon Changer  (2026-10-30)

## Corpus Check
- 86 files · ~26,870 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 1683 file(s) not represented in the graph (top: .ico 1676, (none) 7)

## Summary
- 993 nodes · 1982 edges · 63 communities (39 shown, 24 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 92 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `29dda51e`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- FolderIconService
- FolderIconStudio.Services
- Icon Changer/folder_icon_studio/icon_library.py
- AppSettings
- FolderListWidget
- DllImport
- ThumbnailCache
- Window
- RoutedEventArgs
- SettingsService
- MainViewModel
- MainViewModel
- CultureInfo
- categories
- categories
- folder_icon_studio/app.py
- folder_icon_studio/icon_library.py
- folder_icon_studio/win32_icons.py
- MainWindow
- IconLibraryService
- Icon Changer/folder_icon_studio/metadata.py
- Icon Changer/folder_icon_studio/win32_icons.py
- Window
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
- SettingsWindow
- IconViewModel
- App
- ImageSource
- FirstRunDialog
- FolderRowWidget
- ensure_library
- .ApplyTheme
- FolderListWidget
- IconGridWidget
- ._build_ui
- Icon Changer/FolderIconStudio/Models/AppModels.cs
- Icon Changer/FolderIconStudio/FolderIconStudio.csproj
- SettingsService
- Icon Changer/folder_icon_studio/generate_starters.py
- AppSettings
- IconItem

## God Nodes (most connected - your core abstractions)
1. `MainViewModel` - 46 edges
2. `MainViewModel` - 44 edges
3. `MainWindow` - 29 edges
4. `IconLibraryService` - 27 edges
5. `MainWindow` - 27 edges
6. `FolderIconService` - 26 edges
7. `Window` - 24 edges
8. `Window` - 24 edges
9. `SettingsService` - 21 edges
10. `categories` - 21 edges

## Surprising Connections (you probably didn't know these)
- `main()` --uses--> `MainWindow`  [INFERRED]
  scripts/capture_screenshots.py → Icon Changer/folder_icon_studio/app.py
- `FolderListWidget` --uses--> `FolderRowWidget`  [INFERRED]
  folder_icon_studio/app.py → Icon Changer/folder_icon_studio/folder_row.py
- `IconGridWidget` --uses--> `ThumbnailCache`  [INFERRED]
  folder_icon_studio/app.py → Icon Changer/folder_icon_studio/thumbnail_cache.py
- `MainWindow` --uses--> `ThumbnailCache`  [INFERRED]
  folder_icon_studio/app.py → Icon Changer/folder_icon_studio/thumbnail_cache.py
- `MainWindow` --references--> `MainViewModel`  [EXTRACTED]
  Icon Changer/FolderIconStudio/MainWindow.xaml.cs → FolderIconStudio/ViewModels/MainViewModel.cs

## Import Cycles
- None detected.

## Communities (63 total, 24 thin omitted)

### Community 0 - "FolderIconService"
Cohesion: 0.06
Nodes (19): FolderIconState, AppSetAttributes, CurrentIconPath, HasCustomIcon, IconFileMissing, FileAttributesInterop, PathHelper, ElevationHelper (+11 more)

### Community 1 - "FolderIconStudio.Services"
Cohesion: 0.08
Nodes (9): FolderIconStudio.Helpers, FolderIconStudio.Native, FolderIconStudio.Models, FolderIconStudio.ViewModels, FolderIconStudio, FolderIconStudio.Views, FolderIconStudio.Services, PathHelper (+1 more)

### Community 2 - "Icon Changer/folder_icon_studio/icon_library.py"
Cohesion: 0.08
Nodes (32): categorize_icon(), category_sort_key(), _matches(), _normalize_name(), _ico_size(), main(), prepare_bundled(), _safe_filename() (+24 more)

### Community 3 - "AppSettings"
Cohesion: 0.18
Nodes (11): AppSettings, ContextMenuEnabled, Favorites, FirstRunCompleted, LibraryPath, RecentlyUsed, Theme, AppTheme (+3 more)

### Community 4 - "FolderListWidget"
Cohesion: 0.05
Nodes (6): FolderRowWidget, IconPreview, FolderListWidget, GlassPanel, IconGridWidget, OrbBackground

### Community 5 - "DllImport"
Cohesion: 0.14
Nodes (7): DwmInterop, MARGINS, ShellInterop, DwmInterop, FileAttributesInterop, MARGINS, ShellInterop

### Community 6 - "ThumbnailCache"
Cohesion: 0.23
Nodes (3): _load_rgba(), _pil_to_pixmap(), ThumbnailCache

### Community 7 - "Window"
Cohesion: 0.09
Nodes (17): Window, BrowseFileCommand, DisplayName, FilteredIcons, HasRecentIcons, IconMissing, IsFavorite, IsSelected (+9 more)

### Community 8 - "RoutedEventArgs"
Cohesion: 0.14
Nodes (11): ContextMenuToggle, LibraryPathBox, ThemeCombo, Window, SettingsWindow, CheckBox, ComboBox, TextBox (+3 more)

### Community 9 - "SettingsService"
Cohesion: 0.20
Nodes (7): MainWindow, ViewModel, RegistryService, SettingsService, Settings, Application, App

### Community 10 - "MainViewModel"
Cohesion: 0.07
Nodes (22): MainViewModel, AllIcons, BrowseFileCommand, CanSave, FavoriteIcons, FilteredIcons, HasCustomIcon, HasRecentIcons (+14 more)

### Community 11 - "MainViewModel"
Cohesion: 0.07
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

### Community 15 - "folder_icon_studio/app.py"
Cohesion: 0.13
Nodes (6): _app_icon(), _enable_hd(), main(), main(), capture_next(), queue_shots()

### Community 16 - "folder_icon_studio/icon_library.py"
Cohesion: 0.22
Nodes (13): bundled_license_path(), bundled_manifest(), _bundled_version(), _copy_tree_all(), _copy_tree_if_missing(), icon_category(), IconEntry, import_image() (+5 more)

### Community 17 - "folder_icon_studio/win32_icons.py"
Cohesion: 0.26
Nodes (11): apply_icon(), get_attributes(), get_current_icon_path(), get_folder_icon_state(), _long_path(), notify_folder_changed(), _prepare_folder(), _read_ini() (+3 more)

### Community 18 - "MainWindow"
Cohesion: 0.14
Nodes (4): MainWindow, main(), capture_next(), queue_shots()

### Community 21 - "Icon Changer/folder_icon_studio/win32_icons.py"
Cohesion: 0.32
Nodes (11): apply_icon(), get_attributes(), get_current_icon_path(), get_folder_icon_state(), _long_path(), notify_folder_changed(), _prepare_folder(), _read_ini() (+3 more)

### Community 22 - "Window"
Cohesion: 0.09
Nodes (19): Window, BrowseFileCommand, DisplayName, FilteredIcons, HasRecentIcons, IconMissing, IsFavorite, IsSelected (+11 more)

### Community 23 - ".ReloadIcons"
Cohesion: 0.19
Nodes (6): IconItem, DisplayName, FilePath, IsFavorite, IsMissing, IsStarter

### Community 24 - "IconViewModel"
Cohesion: 0.22
Nodes (8): IconViewModel, DisplayName, FilePath, IsFavorite, IsMissing, IsSelected, Item, Preview

### Community 25 - "Icon Changer/folder_icon_studio/app.py"
Cohesion: 0.21
Nodes (9): bundled_publisher_json(), register_publisher_metadata(), _app_icon(), _app_icon_path(), _assets_root(), _bundled_assets_dir(), _enable_hd(), main() (+1 more)

### Community 26 - "folder_icon_studio/generate_starters.py"
Cohesion: 0.38
Nodes (3): _darken(), _draw_folder(), generate_all()

### Community 27 - "RelayCommand"
Cohesion: 0.15
Nodes (4): RelayCommand, RelayCommand, RelayCommand, RelayCommand

### Community 29 - "ThumbnailCache"
Cohesion: 0.16
Nodes (3): _load_rgba(), _pil_to_pixmap(), ThumbnailCache

### Community 30 - "FolderIconStudio"
Cohesion: 0.22
Nodes (6): net8.0-windows, System.Drawing.Common (8.0.10), Microsoft.NET.Sdk, net8.0-windows, System.Drawing.Common (8.0.10), Microsoft.NET.Sdk

### Community 31 - "IconLibraryService"
Cohesion: 0.18
Nodes (3): Application, IconLibraryService, LibraryPath

### Community 33 - "SettingsWindow"
Cohesion: 0.16
Nodes (8): ContextMenuToggle, LibraryPathBox, ThemeCombo, Window, SettingsWindow, CheckBox, ComboBox, TextBox

### Community 34 - "IconViewModel"
Cohesion: 0.20
Nodes (8): IconViewModel, DisplayName, FilePath, IsFavorite, IsMissing, IsSelected, Item, Preview

### Community 35 - "App"
Cohesion: 0.14
Nodes (3): App, SingleInstanceService, SingleInstanceService

### Community 37 - "FirstRunDialog"
Cohesion: 0.38
Nodes (3): Window, FirstRunDialog, RegisterContextMenu

### Community 39 - "ensure_library"
Cohesion: 0.32
Nodes (8): _app_icon_path(), _assets_root(), _bundled_assets_dir(), _starter_assets_dir(), default_library_path(), ensure_library(), seed_bundled_icons(), seed_starter_icons()

### Community 55 - "Icon Changer/FolderIconStudio/Models/AppModels.cs"
Cohesion: 0.20
Nodes (9): AppTheme, Dark, Light, System, FolderIconState, AppSetAttributes, CurrentIconPath, HasCustomIcon (+1 more)

### Community 56 - "Icon Changer/FolderIconStudio/FolderIconStudio.csproj"
Cohesion: 0.22
Nodes (6): net8.0-windows, System.Drawing.Common (8.0.10), Microsoft.NET.Sdk, net8.0-windows, System.Drawing.Common (8.0.10), Microsoft.NET.Sdk

### Community 58 - "Icon Changer/folder_icon_studio/generate_starters.py"
Cohesion: 0.38
Nodes (3): _darken(), _draw_folder(), generate_all()

### Community 59 - "AppSettings"
Cohesion: 0.29
Nodes (7): AppSettings, ContextMenuEnabled, Favorites, FirstRunCompleted, LibraryPath, RecentlyUsed, Theme

### Community 60 - "IconItem"
Cohesion: 0.33
Nodes (6): IconItem, DisplayName, FilePath, IsFavorite, IsMissing, IsStarter

## Knowledge Gaps
- **162 isolated node(s):** `net8.0-windows`, `System.Drawing.Common (8.0.10)`, `Microsoft.NET.Sdk`, `IsSelected`, `TargetFolderDisplay` (+157 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 361 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **24 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `MainViewModel` connect `MainViewModel` to `FolderIconService`, `FolderIconStudio.Services`, `IconViewModel`, `Window`, `SettingsService`, `MainViewModel`, `Window`, `.ReloadIcons`, `IconViewModel`, `RelayCommand`, `IconLibraryService`?**
  _High betweenness centrality (0.052) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `IconLibraryService` (e.g. with `.Application_Startup()` and `.Application_Startup()`) actually correct?**
  _`IconLibraryService` has 2 INFERRED edges - model-reasoned connections that need verification._
- **What connects `net8.0-windows`, `System.Drawing.Common (8.0.10)`, `Microsoft.NET.Sdk` to the rest of the system?**
  _162 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `FolderIconService` be split into smaller, more focused modules?**
  _Cohesion score 0.05894736842105263 - nodes in this community are weakly interconnected._
- **Why does `MainViewModel` connect `MainViewModel` to `FolderIconService`, `FolderIconStudio.Services`, `IconViewModel`, `SettingsService`, `Window`, `.ReloadIcons`, `IconViewModel`, `RelayCommand`, `IconLibraryService`?**
  _High betweenness centrality (0.050) - this node is a cross-community bridge._
- **Are the 3 inferred relationships involving `MainWindow` (e.g. with `ThumbnailCache` and `main()`) actually correct?**
  _`MainWindow` has 3 INFERRED edges - model-reasoned connections that need verification._
- **Should `FolderIconStudio.Services` be split into smaller, more focused modules?**
  _Cohesion score 0.07726075504828797 - nodes in this community are weakly interconnected._