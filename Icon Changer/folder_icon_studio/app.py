"""Folder Icon Studio — glass UI with drag-and-drop icon assignment."""

from __future__ import annotations

import json
import os
import sys
from pathlib import Path

from PyQt6.QtCore import (
    QMimeData,
    QPoint,
    QSize,
    Qt,
    QTimer,
    pyqtSignal,
)
from PyQt6.QtGui import (
    QAction,
    QColor,
    QDrag,
    QFont,
    QGuiApplication,
    QIcon,
    QPainter,
    QRadialGradient,
    QPixmap,
)
from PyQt6.QtWidgets import (
    QAbstractItemView,
    QApplication,
    QComboBox,
    QFileDialog,
    QFrame,
    QHBoxLayout,
    QLabel,
    QLineEdit,
    QListWidget,
    QListWidgetItem,
    QMainWindow,
    QMessageBox,
    QPushButton,
    QSplitter,
    QStatusBar,
    QVBoxLayout,
    QWidget,
)

from folder_icon_studio.folder_row import FolderRowWidget
from folder_icon_studio.icon_library import (
    BUNDLED_DIR_NAME,
    MY_ICONS_DIR_NAME,
    default_library_path,
    ensure_library,
    import_image,
    list_categories,
    list_icon_entries,
    list_icons,
    seed_bundled_icons,
    seed_starter_icons,
)
from folder_icon_studio.metadata import register_publisher_metadata
from folder_icon_studio.theme import MIME_ICON, STYLESHEET
from folder_icon_studio.thumbnail_cache import ThumbnailCache
from folder_icon_studio.win32_icons import apply_icon, get_folder_icon_state, restore_default

APP_NAME = "Folder Icon Studio"
SETTINGS_DIR = Path.home() / "AppData" / "Roaming" / "FolderIconStudio"
SETTINGS_FILE = SETTINGS_DIR / "settings.json"
ICON_CELL = 88
FOLDER_ROW_HEIGHT = 108
BATCH_SIZE = 40
THUMB_BATCH = 12


def _assets_root() -> Path:
    if getattr(sys, "frozen", False):
        return Path(sys._MEIPASS) / "folder_icon_studio" / "assets"
    return Path(__file__).resolve().parent / "assets"


def _starter_assets_dir() -> Path:
    return _assets_root() / "starter_icons"


def _bundled_assets_dir() -> Path:
    return _assets_root() / "bundled"


def _app_icon_path() -> Path:
    return _assets_root() / "app_icon.ico"


def _app_icon() -> QIcon:
    path = _app_icon_path()
    return QIcon(str(path)) if path.is_file() else QIcon()


def _enable_hd() -> None:
    os.environ.setdefault("QT_ENABLE_HIGHDPI_SCALING", "1")
    os.environ.setdefault("QT_SCALE_FACTOR_ROUNDING_POLICY", "PassThrough")
    QGuiApplication.setHighDpiScaleFactorRoundingPolicy(
        Qt.HighDpiScaleFactorRoundingPolicy.PassThrough
    )


class OrbBackground(QWidget):
    """Nebula orbs behind glass panels."""

    def paintEvent(self, event) -> None:  # noqa: N802
        p = QPainter(self)
        p.setRenderHint(QPainter.RenderHint.Antialiasing)
        p.fillRect(self.rect(), QColor("#03040c"))

        orbs = [
            (0.15, 0.12, 280, QColor(124, 58, 237, 90)),
            (0.85, 0.08, 240, QColor(30, 64, 175, 80)),
            (0.72, 0.78, 260, QColor(219, 39, 119, 70)),
            (0.2, 0.82, 200, QColor(5, 150, 105, 55)),
        ]
        for ox, oy, radius, color in orbs:
            cx = int(self.width() * ox)
            cy = int(self.height() * oy)
            grad = QRadialGradient(cx, cy, radius)
            grad.setColorAt(0, QColor(0, 0, 0, 0))
            grad.setColorAt(0.35, color)
            grad.setColorAt(1, QColor(0, 0, 0, 0))
            p.setBrush(grad)
            p.setPen(Qt.PenStyle.NoPen)
            p.drawEllipse(QPoint(cx, cy), radius, radius)


class GlassPanel(QFrame):
    def __init__(self, parent: QWidget | None = None) -> None:
        super().__init__(parent)
        self.setObjectName("glassPanel")


class FolderListWidget(QListWidget):
    """Sidebar folders — drop icons here; shows current → new previews."""

    icon_dropped = pyqtSignal(str, str)
    folder_selected = pyqtSignal(str)

    def __init__(self, parent: QWidget | None = None) -> None:
        super().__init__(parent)
        self.setAcceptDrops(True)
        self.setDragDropMode(QAbstractItemView.DragDropMode.DropOnly)
        self.setSpacing(6)
        self._rows: dict[str, FolderRowWidget] = {}
        self._selected: str | None = None
        self.itemSelectionChanged.connect(self._on_list_selection)

    def dragEnterEvent(self, event) -> None:  # noqa: N802
        if event.mimeData().hasFormat(MIME_ICON):
            event.acceptProposedAction()

    def dragMoveEvent(self, event) -> None:  # noqa: N802
        if event.mimeData().hasFormat(MIME_ICON):
            event.acceptProposedAction()

    def dropEvent(self, event) -> None:  # noqa: N802
        item = self.itemAt(event.position().toPoint())
        if not item:
            return
        icon_path = bytes(event.mimeData().data(MIME_ICON)).decode("utf-8")
        folder_path = item.data(Qt.ItemDataRole.UserRole)
        if folder_path and icon_path:
            self.select_folder(folder_path)
            self.icon_dropped.emit(folder_path, icon_path)
            event.acceptProposedAction()

    def add_folder(self, path: str) -> str:
        resolved = str(Path(path).resolve())
        if resolved in self._rows:
            return resolved
        item = QListWidgetItem()
        item.setData(Qt.ItemDataRole.UserRole, resolved)
        item.setSizeHint(QSize(0, FOLDER_ROW_HEIGHT))
        item.setToolTip(resolved)
        widget = FolderRowWidget(resolved)
        widget.clicked.connect(self.select_folder)
        self.addItem(item)
        self.setItemWidget(item, widget)
        self._rows[resolved] = widget
        return resolved

    def select_folder(self, folder_path: str) -> None:
        folder_path = str(Path(folder_path).resolve())
        self._selected = folder_path
        for path, widget in self._rows.items():
            widget.set_active(path == folder_path)
        for i in range(self.count()):
            item = self.item(i)
            if item and item.data(Qt.ItemDataRole.UserRole) == folder_path:
                self.setCurrentItem(item)
                break
        self.folder_selected.emit(folder_path)

    def selected_folder(self) -> str | None:
        return self._selected

    def _on_list_selection(self) -> None:
        item = self.currentItem()
        if item:
            path = item.data(Qt.ItemDataRole.UserRole)
            if path:
                self.select_folder(path)

    def set_current_preview(self, folder_path: str, pix: QPixmap | None, *, missing: bool = False) -> None:
        widget = self._rows.get(folder_path)
        if widget:
            widget.set_current_icon(pix, missing=missing)

    def set_new_preview(self, folder_path: str, pix: QPixmap | None) -> None:
        widget = self._rows.get(folder_path)
        if widget:
            widget.set_new_icon(pix)

    def clear_new_preview(self, folder_path: str) -> None:
        widget = self._rows.get(folder_path)
        if widget:
            widget.clear_new_icon()

    def folder_paths(self) -> list[str]:
        return list(self._rows.keys())

    def clear_all(self) -> None:
        self._rows.clear()
        self._selected = None
        self.clear()


class IconGridWidget(QListWidget):
    """Large icon grid — drag icons to folders."""

    def __init__(self, cache: ThumbnailCache, parent: QWidget | None = None) -> None:
        super().__init__(parent)
        self.cache = cache
        self.cache.loaded.connect(self._on_thumb)
        self.setViewMode(QListWidget.ViewMode.IconMode)
        self.setResizeMode(QListWidget.ResizeMode.Adjust)
        self.setMovement(QListWidget.Movement.Static)
        self.setSpacing(12)
        self.setIconSize(QSize(ICON_CELL - 10, ICON_CELL - 10))
        self.setGridSize(QSize(ICON_CELL + 8, ICON_CELL + 28))
        self.setUniformItemSizes(True)
        self.setDragEnabled(True)
        self.setSelectionMode(QAbstractItemView.SelectionMode.SingleSelection)
        self._paths: dict[str, QListWidgetItem] = {}
        self._pending: list[Path] = []
        self._batch_timer = QTimer(self)
        self._batch_timer.timeout.connect(self._load_batch)

    def set_icon_paths(self, paths: list[Path]) -> None:
        self.clear()
        self._paths.clear()
        self._pending = list(paths)
        self._batch_timer.start(0)

    def _load_batch(self) -> None:
        if not self._pending:
            self._batch_timer.stop()
            return
        batch = self._pending[:BATCH_SIZE]
        self._pending = self._pending[BATCH_SIZE:]
        for path in batch:
            key = str(path.resolve())
            item = QListWidgetItem(path.stem)
            item.setData(Qt.ItemDataRole.UserRole, key)
            item.setToolTip(key)
            item.setSizeHint(QSize(ICON_CELL + 8, ICON_CELL + 28))
            self.addItem(item)
            self._paths[key] = item
            cached = self.cache.get(path)
            if cached:
                item.setIcon(QIcon(cached))
            else:
                self.cache.request(path)
        if not self._pending:
            self._batch_timer.stop()

    def _on_thumb(self, key: str, pix: QPixmap) -> None:
        item = self._paths.get(key)
        if item and not pix.isNull():
            item.setIcon(QIcon(pix))

    def startDrag(self, supportedActions) -> None:  # noqa: N802
        item = self.currentItem()
        if not item:
            return
        path = item.data(Qt.ItemDataRole.UserRole)
        if not path:
            return
        drag = QDrag(self)
        mime = QMimeData()
        mime.setData(MIME_ICON, path.encode("utf-8"))
        drag.setMimeData(mime)
        icon = item.icon()
        if not icon.isNull():
            drag.setPixmap(icon.pixmap(self.iconSize()))
        drag.exec(Qt.DropAction.CopyAction)


class MainWindow(QMainWindow):
    def __init__(self) -> None:
        super().__init__()
        self.setWindowTitle(APP_NAME)
        self.resize(1280, 820)
        self.setMinimumSize(1000, 680)

        self.settings = self._load_settings()
        self.library = Path(self.settings.get("library_path", str(default_library_path())))
        ensure_library(self.library)
        seed_starter_icons(self.library, _assets_root())
        bundled = seed_bundled_icons(self.library, _bundled_assets_dir())

        self.assignments: dict[str, str] = {}
        self.current_icons: dict[str, str] = {}
        self.all_entries: list = []
        self.cache = ThumbnailCache(max_items=1200, thumb_size=ICON_CELL - 10)

        self._build_ui()
        self._reload_icons()
        msg = f"Library: {self.library}"
        if bundled:
            msg += f" — seeded {bundled} bundled icons (first run)"
        msg += " — drag icons onto folders, then Save"
        self.statusBar().showMessage(msg)

    def _load_settings(self) -> dict:
        try:
            if SETTINGS_FILE.is_file():
                return json.loads(SETTINGS_FILE.read_text(encoding="utf-8"))
        except Exception:
            pass
        return {}

    def _save_settings(self) -> None:
        SETTINGS_DIR.mkdir(parents=True, exist_ok=True)
        self.settings["library_path"] = str(self.library)
        SETTINGS_FILE.write_text(json.dumps(self.settings, indent=2), encoding="utf-8")

    def _build_ui(self) -> None:
        root = OrbBackground()
        root.setObjectName("central")
        self.setCentralWidget(root)
        layout = QVBoxLayout(root)
        layout.setContentsMargins(20, 16, 20, 12)
        layout.setSpacing(12)

        # Header
        header = QHBoxLayout()
        title_col = QVBoxLayout()
        title = QLabel(APP_NAME)
        title.setObjectName("title")
        subtitle = QLabel("Drag icons onto folders in the sidebar, then Save")
        subtitle.setObjectName("subtitle")
        title_col.addWidget(title)
        title_col.addWidget(subtitle)
        header.addLayout(title_col)
        header.addStretch()

        self.btn_load_parent = QPushButton("Load folders from path…")
        self.btn_add_folder = QPushButton("Add one folder…")
        self.btn_import = QPushButton("Import image")
        self.btn_library = QPushButton("Open library")
        self.btn_settings = QPushButton("Settings")
        self.btn_sources = QPushButton("Icon sources")
        for btn in (self.btn_load_parent, self.btn_add_folder, self.btn_import,
                    self.btn_library, self.btn_sources, self.btn_settings):
            header.addWidget(btn)
        layout.addLayout(header)

        # Toolbar row
        tool = QHBoxLayout()
        self.category_filter = QComboBox()
        self.category_filter.setMinimumWidth(200)
        self.category_filter.currentTextChanged.connect(self._filter_icons)
        cat_lbl = QLabel("Category")
        cat_lbl.setObjectName("subtitle")
        tool.addWidget(cat_lbl)
        tool.addWidget(self.category_filter)
        self.search = QLineEdit()
        self.search.setPlaceholderText("Search icons…")
        self.search.textChanged.connect(self._filter_icons)
        self.icon_count = QLabel("")
        self.icon_count.setObjectName("subtitle")
        tool.addWidget(self.search, stretch=1)
        tool.addWidget(self.icon_count)
        layout.addLayout(tool)

        # Main split
        splitter = QSplitter(Qt.Orientation.Horizontal)
        splitter.setHandleWidth(8)

        left_panel = GlassPanel()
        left_lay = QVBoxLayout(left_panel)
        left_lay.setContentsMargins(14, 14, 14, 14)
        left_title = QLabel("Folders")
        left_title.setFont(QFont("Segoe UI", 12, QFont.Weight.DemiBold))
        left_hint = QLabel("Click a folder to focus it · drag icon to assign · Current → New")
        left_hint.setObjectName("subtitle")
        left_lay.addWidget(left_title)
        left_lay.addWidget(left_hint)
        self.folder_list = FolderListWidget()
        self.folder_list.icon_dropped.connect(self._on_icon_dropped)
        self.folder_list.folder_selected.connect(self._on_folder_selected)
        self.cache.loaded.connect(self._on_thumb_for_folder)
        left_lay.addWidget(self.folder_list)

        folder_actions = QHBoxLayout()
        self.btn_clear_pending = QPushButton("Clear pending")
        self.btn_restore_selected = QPushButton("Restore selected")
        folder_actions.addWidget(self.btn_clear_pending)
        folder_actions.addWidget(self.btn_restore_selected)
        left_lay.addLayout(folder_actions)
        splitter.addWidget(left_panel)

        right_panel = GlassPanel()
        right_lay = QVBoxLayout(right_panel)
        right_lay.setContentsMargins(14, 14, 14, 14)
        right_title = QLabel("Icon library")
        right_title.setFont(QFont("Segoe UI", 12, QFont.Weight.DemiBold))
        right_lay.addWidget(right_title)
        self.icon_grid = IconGridWidget(self.cache)
        right_lay.addWidget(self.icon_grid)
        splitter.addWidget(right_panel)
        splitter.setSizes([320, 900])

        layout.addWidget(splitter, stretch=1)

        # Footer
        footer = QHBoxLayout()
        self.btn_restore = QPushButton("Restore default")
        self.btn_save = QPushButton("Save changes")
        self.btn_save.setObjectName("primary")
        footer.addStretch()
        footer.addWidget(self.btn_restore)
        footer.addWidget(self.btn_save)
        layout.addLayout(footer)

        self.setStatusBar(QStatusBar())

        self.btn_load_parent.clicked.connect(self._load_parent)
        self.btn_add_folder.clicked.connect(self._add_folder)
        self.btn_import.clicked.connect(self._import_icon)
        self.btn_library.clicked.connect(self._open_library)
        self.btn_settings.clicked.connect(self._settings)
        self.btn_sources.clicked.connect(self._show_icon_sources)
        self.btn_save.clicked.connect(self._save)
        self.btn_restore.clicked.connect(self._restore)
        self.btn_clear_pending.clicked.connect(self._clear_pending)
        self.btn_restore_selected.clicked.connect(self._restore_selected)

    def _reload_icons(self) -> None:
        self.all_entries = list_icon_entries(self.library, recursive=True)
        categories = ["All categories", *list_categories(self.library)]
        current = self.category_filter.currentText()
        self.category_filter.blockSignals(True)
        self.category_filter.clear()
        self.category_filter.addItems(categories)
        idx = self.category_filter.findText(current)
        if idx >= 0:
            self.category_filter.setCurrentIndex(idx)
        self.category_filter.blockSignals(False)
        self._filter_icons()

    def _filter_icons(self) -> None:
        q = self.search.text().strip().lower()
        cat = self.category_filter.currentText()
        filtered = self.all_entries
        if cat and cat != "All categories":
            filtered = [e for e in filtered if e.category == cat]
        if q:
            filtered = [
                e for e in filtered
                if q in e.name.lower() or q in str(e.path).lower() or q in e.category.lower()
            ]
        paths = [e.path for e in filtered]
        self.icon_grid.set_icon_paths(paths)
        self.icon_count.setText(f"{len(filtered)} / {len(self.all_entries)} icons")

    def _load_parent(self) -> None:
        path = QFileDialog.getExistingDirectory(self, "Select parent folder")
        if not path:
            return
        parent = Path(path)
        self.folder_list.clear_all()
        self.assignments.clear()
        self.current_icons.clear()
        count = 0
        for child in sorted(parent.iterdir()):
            if child.is_dir() and not child.name.startswith("."):
                self._register_folder(str(child.resolve()))
                count += 1
        self.statusBar().showMessage(f"Loaded {count} folders from {parent}")

    def _add_folder(self) -> None:
        path = QFileDialog.getExistingDirectory(self, "Select folder")
        if not path:
            return
        resolved = str(Path(path).resolve())
        if resolved in self.folder_list.folder_paths():
            self.folder_list.select_folder(resolved)
            self.statusBar().showMessage("Folder already in list — selected for editing")
            return
        self._register_folder(resolved)
        self.folder_list.select_folder(resolved)

    def _register_folder(self, folder_path: str) -> None:
        self.folder_list.add_folder(folder_path)
        state = get_folder_icon_state(folder_path)
        cur = state.get("current_icon")
        if cur:
            self.current_icons[folder_path] = cur
            missing = state.get("missing", False)
            pix = self.cache.get(Path(cur))
            if pix is not None:
                self.folder_list.set_current_preview(folder_path, pix, missing=missing)
            else:
                self.cache.request(Path(cur))
        else:
            self.folder_list.set_current_preview(folder_path, None)

    def _on_folder_selected(self, folder_path: str) -> None:
        name = Path(folder_path).name
        if folder_path in self.assignments:
            pending = Path(self.assignments[folder_path]).stem
            self.statusBar().showMessage(f"Editing: {name} — pending icon: {pending} (Save to apply)")
        else:
            self.statusBar().showMessage(f"Editing: {name} — drag a new icon onto this folder")

    def _on_icon_dropped(self, folder_path: str, icon_path: str) -> None:
        self.assignments[folder_path] = icon_path
        pix = self.cache.get(Path(icon_path))
        if pix is not None:
            self.folder_list.set_new_preview(folder_path, pix)
        else:
            self.folder_list.set_new_preview(folder_path, None)
            self.cache.request(Path(icon_path))
        name = Path(folder_path).name
        icon_name = Path(icon_path).stem
        self.statusBar().showMessage(f"{name}: new icon → {icon_name} (Save to apply)")

    def _on_thumb_for_folder(self, key: str, pix: QPixmap) -> None:
        for folder, icon in self.assignments.items():
            if icon == key:
                self.folder_list.set_new_preview(folder, pix)
        for folder, cur in self.current_icons.items():
            if cur == key:
                missing = not Path(cur).is_file()
                self.folder_list.set_current_preview(folder, pix, missing=missing)

    def _clear_pending(self) -> None:
        folder = self.folder_list.selected_folder()
        if not folder:
            QMessageBox.information(self, APP_NAME, "Select a folder in the sidebar first.")
            return
        self.assignments.pop(folder, None)
        self.folder_list.clear_new_preview(folder)
        self.statusBar().showMessage(f"Cleared pending change for {Path(folder).name}")

    def _restore_selected(self) -> None:
        folder = self.folder_list.selected_folder()
        if not folder:
            QMessageBox.information(self, APP_NAME, "Select a folder in the sidebar first.")
            return
        name = Path(folder).name
        if QMessageBox.question(
            self, APP_NAME, f"Restore default icon for '{name}'?"
        ) != QMessageBox.StandardButton.Yes:
            return
        try:
            restore_default(folder)
            self.assignments.pop(folder, None)
            self.folder_list.clear_new_preview(folder)
            self._refresh_folder_current(folder)
            self.statusBar().showMessage(f"Restored default icon for {name}")
        except Exception as ex:
            QMessageBox.critical(self, APP_NAME, str(ex))

    def _refresh_folder_current(self, folder_path: str) -> None:
        state = get_folder_icon_state(folder_path)
        cur = state.get("current_icon")
        if cur:
            self.current_icons[folder_path] = cur
            pix = self.cache.get(Path(cur))
            if pix is not None:
                self.folder_list.set_current_preview(folder_path, pix, missing=state.get("missing", False))
            else:
                self.cache.request(Path(cur))
        else:
            self.current_icons.pop(folder_path, None)
            self.folder_list.set_current_preview(folder_path, None)

    def _save(self) -> None:
        if not self.assignments:
            QMessageBox.information(self, APP_NAME, "Drag icons onto folders first, then Save.")
            return

        ok, fail = 0, []
        saved_folders: list[str] = []
        for folder, icon in list(self.assignments.items()):
            try:
                apply_icon(folder, icon)
                ok += 1
                saved_folders.append(folder)
            except Exception as ex:
                fail.append(f"{Path(folder).name}: {ex}")

        if fail:
            QMessageBox.warning(
                self, APP_NAME,
                f"Applied {ok} icon(s).\n\nFailed:\n" + "\n".join(fail[:12]),
            )
        else:
            QMessageBox.information(self, APP_NAME, f"Applied {ok} folder icon(s).")

        for folder in saved_folders:
            self.assignments.pop(folder, None)
            self.folder_list.clear_new_preview(folder)
            self._refresh_folder_current(folder)
        self.statusBar().showMessage(f"Saved {ok} folder icon(s)")

    def _restore(self) -> None:
        folders = self.folder_list.folder_paths()
        if not folders:
            QMessageBox.information(self, APP_NAME, "Load folders first.")
            return
        if QMessageBox.question(
            self, APP_NAME, f"Restore default icons for all {len(folders)} folder(s)?"
        ) != QMessageBox.StandardButton.Yes:
            return
        ok = 0
        for folder in folders:
            try:
                restore_default(folder)
                self.assignments.pop(folder, None)
                self.folder_list.clear_new_preview(folder)
                self._refresh_folder_current(folder)
                ok += 1
            except Exception as ex:
                QMessageBox.critical(self, APP_NAME, f"Failed on {folder}:\n{ex}")
                return
        QMessageBox.information(self, APP_NAME, f"Restored {ok} folder(s).")

    def _import_icon(self) -> None:
        path, _ = QFileDialog.getOpenFileName(
            self, "Import image", str(self.library),
            "Images (*.png *.jpg *.jpeg *.ico *.bmp *.webp);;All (*.*)",
        )
        if not path:
            return
        try:
            dest = import_image(Path(path), self.library)
            self._reload_icons()
            self.statusBar().showMessage(f"Imported {dest.name}")
        except Exception as ex:
            QMessageBox.critical(self, APP_NAME, str(ex))

    def _open_library(self) -> None:
        os.startfile(self.library)

    def _settings(self) -> None:
        path = QFileDialog.getExistingDirectory(self, "Icon library folder", str(self.library))
        if path:
            self.library = Path(path)
            ensure_library(self.library)
            seed_starter_icons(self.library, _assets_root())
            seed_bundled_icons(self.library, _bundled_assets_dir())
            self._save_settings()
            self._reload_icons()
            self.statusBar().showMessage(f"Library: {self.library}")

    def _show_icon_sources(self) -> None:
        sources = _assets_root() / "ICON_SOURCES.md"
        text = sources.read_text(encoding="utf-8") if sources.is_file() else (
            "Bundled icons are in the _Bundled folder (by category).\n"
            "Your imports go to My Icons folder."
        )
        manifest_path = self.library / BUNDLED_DIR_NAME / "manifest.json"
        if manifest_path.is_file():
            text += f"\n\nLibrary manifest: {manifest_path}"
        QMessageBox.information(
            self,
            "Free icon sources",
            text[:4000] + ("…" if len(text) > 4000 else ""),
        )


def main() -> None:
    if not sys.platform.startswith("win"):
        print("Windows only.")
        sys.exit(1)

    log_dir = Path.home() / "AppData" / "Local" / "FolderIconStudio"
    log_dir.mkdir(parents=True, exist_ok=True)
    crash_log = log_dir / "crash.log"

    try:
        register_publisher_metadata()
        _enable_hd()
        app = QApplication(sys.argv)
        app.setStyle("Fusion")
        app.setStyleSheet(STYLESHEET)
        app_icon = _app_icon()
        if not app_icon.isNull():
            app.setWindowIcon(app_icon)
        font = QFont("Segoe UI", 10)
        font.setHintingPreference(QFont.HintingPreference.PreferFullHinting)
        app.setFont(font)
        win = MainWindow()
        if not app_icon.isNull():
            win.setWindowIcon(app_icon)
        win.show()
        sys.exit(app.exec())
    except Exception:
        import traceback
        crash_log.write_text(traceback.format_exc(), encoding="utf-8")
        try:
            QMessageBox.critical(
                None,
                APP_NAME,
                f"The app crashed on startup.\n\nDetails saved to:\n{crash_log}",
            )
        except Exception:
            pass
        sys.exit(1)


if __name__ == "__main__":
    main()
