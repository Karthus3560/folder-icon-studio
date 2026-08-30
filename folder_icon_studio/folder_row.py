"""Folder sidebar row: current icon → new icon."""

from __future__ import annotations

from pathlib import Path

from PyQt6.QtCore import Qt, pyqtSignal
from PyQt6.QtGui import QFont, QPixmap
from PyQt6.QtWidgets import QHBoxLayout, QLabel, QVBoxLayout, QWidget

from folder_icon_studio.theme import BORDER, PRIMARY, TEXT_FAINT

PREVIEW = 36


class IconPreview(QLabel):
    def __init__(self, caption: str, parent: QWidget | None = None) -> None:
        super().__init__(parent)
        self.setFixedSize(PREVIEW, PREVIEW)
        self.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.setStyleSheet(
            f"background: rgba(255,255,255,0.06); border: 1px solid {BORDER}; border-radius: 8px;"
        )
        self._caption = caption
        self._pix: QPixmap | None = None
        self._show_placeholder()

    def _show_placeholder(self, text: str = "—") -> None:
        self.setPixmap(QPixmap())
        self.setText(text)
        self.setStyleSheet(
            f"background: rgba(255,255,255,0.06); border: 1px solid {BORDER};"
            f"border-radius: 8px; color: {TEXT_FAINT}; font-size: 14px;"
        )

    def set_pixmap(self, pix: QPixmap | None, *, missing: bool = False) -> None:
        if pix is None or pix.isNull():
            self._show_placeholder("?" if missing else "—")
            return
        scaled = pix.scaled(
            PREVIEW - 4,
            PREVIEW - 4,
            Qt.AspectRatioMode.KeepAspectRatio,
            Qt.TransformationMode.SmoothTransformation,
        )
        self.setText("")
        self.setPixmap(scaled)
        border = "#f43f5e" if missing else BORDER
        self.setStyleSheet(
            f"background: rgba(255,255,255,0.06); border: 1px solid {border}; border-radius: 8px;"
        )


class FolderRowWidget(QWidget):
    clicked = pyqtSignal(str)

    def __init__(self, folder_path: str, parent: QWidget | None = None) -> None:
        super().__init__(parent)
        self.folder_path = str(Path(folder_path).resolve())
        self._active = False

        root = QVBoxLayout(self)
        root.setContentsMargins(8, 6, 8, 6)
        root.setSpacing(4)

        self.name_lbl = QLabel(Path(self.folder_path).name)
        self.name_lbl.setFont(QFont("Segoe UI", 10, QFont.Weight.DemiBold))
        self.name_lbl.setToolTip(self.folder_path)
        root.addWidget(self.name_lbl)

        row = QHBoxLayout()
        row.setSpacing(6)

        cur_col = QVBoxLayout()
        cur_cap = QLabel("Current")
        cur_cap.setStyleSheet(f"color: {TEXT_FAINT}; font-size: 9px;")
        cur_cap.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.current_preview = IconPreview("current")
        cur_col.addWidget(cur_cap)
        cur_col.addWidget(self.current_preview)
        row.addLayout(cur_col)

        arrow = QLabel("→")
        arrow.setStyleSheet(f"color: {TEXT_FAINT}; font-size: 16px;")
        arrow.setAlignment(Qt.AlignmentFlag.AlignCenter)
        row.addWidget(arrow)

        new_col = QVBoxLayout()
        new_cap = QLabel("New")
        new_cap.setStyleSheet(f"color: {TEXT_FAINT}; font-size: 9px;")
        new_cap.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.new_preview = IconPreview("new")
        new_col.addWidget(new_cap)
        new_col.addWidget(self.new_preview)
        row.addLayout(new_col)

        row.addStretch()
        root.addLayout(row)
        self._apply_frame()

    def set_active(self, active: bool) -> None:
        self._active = active
        self._apply_frame()

    def _apply_frame(self) -> None:
        if self._active:
            self.setStyleSheet(
                f"FolderRowWidget {{ background: rgba(168,85,247,0.15);"
                f"border: 1px solid {PRIMARY}; border-radius: 12px; }}"
            )
        else:
            self.setStyleSheet(
                "FolderRowWidget { background: rgba(255,255,255,0.04);"
                "border: 1px solid rgba(255,255,255,0.08); border-radius: 12px; }"
            )

    def set_current_icon(self, pix: QPixmap | None, *, missing: bool = False) -> None:
        self.current_preview.set_pixmap(pix, missing=missing)

    def set_new_icon(self, pix: QPixmap | None) -> None:
        self.new_preview.set_pixmap(pix)
        if pix and not pix.isNull():
            self.name_lbl.setText(f"{Path(self.folder_path).name}  •  pending")

    def clear_new_icon(self) -> None:
        self.new_preview.set_pixmap(None)
        self.name_lbl.setText(Path(self.folder_path).name)

    def mousePressEvent(self, event) -> None:  # noqa: N802
        if event.button() == Qt.MouseButton.LeftButton:
            self.clicked.emit(self.folder_path)
        super().mousePressEvent(event)
