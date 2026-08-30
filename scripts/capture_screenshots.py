"""Capture README screenshots (run on Windows with display)."""

import sys
from pathlib import Path
from typing import Callable

from PyQt6.QtCore import QTimer
from PyQt6.QtGui import QFont
from PyQt6.QtWidgets import QApplication

_ROOT = Path(__file__).resolve().parent.parent
if str(_ROOT) not in sys.path:
    sys.path.insert(0, str(_ROOT))

from folder_icon_studio.app import MainWindow, _app_icon, _enable_hd  # noqa: E402
from folder_icon_studio.theme import STYLESHEET  # noqa: E402


def main() -> int:
    out = _ROOT / "docs" / "screenshots"
    out.mkdir(parents=True, exist_ok=True)

    _enable_hd()
    app = QApplication(sys.argv)
    app.setStyle("Fusion")
    app.setStyleSheet(STYLESHEET)
    icon = _app_icon()
    if not icon.isNull():
        app.setWindowIcon(icon)

    font = QFont("Segoe UI", 10)
    font.setHintingPreference(QFont.HintingPreference.PreferFullHinting)
    app.setFont(font)

    win = MainWindow()
    if not icon.isNull():
        win.setWindowIcon(icon)
    win.resize(1280, 820)

    library = Path.home() / "Pictures" / "Folder Icons"
    for folder in sorted(library.iterdir()) if library.is_dir() else []:
        if folder.is_dir() and not folder.name.startswith("."):
            win.folder_list.add_folder(str(folder))
            win._refresh_folder_current(str(folder.resolve()))
            if len(win.folder_list.folder_paths()) >= 4:
                break

    shots: list[tuple[str, Callable[[], None]]] = []

    def queue_shots() -> None:
        shots.append(("01-main-window.png", lambda: None))
        shots.append(
            (
                "02-folders-category.png",
                lambda: win.category_filter.setCurrentText("Folders"),
            )
        )
        shots.append(
            (
                "03-windows-system-category.png",
                lambda: win.category_filter.setCurrentText("Windows System"),
            )
        )
        shots.append(
            (
                "04-development-category.png",
                lambda: win.category_filter.setCurrentText("Development"),
            )
        )
        capture_next()

    def capture_next() -> None:
        if not shots:
            app.quit()
            return
        name, setup = shots.pop(0)
        setup()
        win._filter_icons()

        def snap() -> None:
            path = out / name
            win.grab().save(str(path))
            print(f"Saved {path}")
            QTimer.singleShot(800, capture_next)

        QTimer.singleShot(1200, snap)

    win.show()
    QTimer.singleShot(3500, queue_shots)
    return app.exec()


if __name__ == "__main__":
    raise SystemExit(main())
