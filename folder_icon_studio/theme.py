"""Aetheria glassmorphism tokens for PyQt6."""

CANVAS = "#03040c"
GLASS = "rgba(20, 22, 38, 0.92)"
GLASS_LIGHT = "rgba(255, 255, 255, 0.08)"
GLASS_HOVER = "rgba(255, 255, 255, 0.14)"
BORDER = "rgba(255, 255, 255, 0.16)"
BORDER_STRONG = "rgba(255, 255, 255, 0.35)"
PRIMARY = "#a855f7"
PRIMARY_DIM = "rgba(168, 85, 247, 0.35)"
TEXT = "#ffffff"
TEXT_DIM = "rgba(255, 255, 255, 0.72)"
TEXT_FAINT = "rgba(255, 255, 255, 0.45)"
SUCCESS = "#10b981"
RADIUS = "14px"

STYLESHEET = f"""
QMainWindow, QWidget#central {{
    background-color: {CANVAS};
    color: {TEXT};
    font-family: "Segoe UI Variable", "Segoe UI", sans-serif;
    font-size: 13px;
}}

QFrame#glassPanel {{
    background-color: {GLASS};
    border: 1px solid {BORDER};
    border-radius: 18px;
}}

QLabel#title {{
    font-size: 22px;
    font-weight: 600;
    color: {TEXT};
}}

QLabel#subtitle {{
    color: {TEXT_FAINT};
    font-size: 12px;
}}

QLineEdit, QComboBox {{
    background-color: {GLASS_LIGHT};
    border: 1px solid {BORDER};
    border-radius: 10px;
    padding: 8px 12px;
    color: {TEXT};
    selection-background-color: {PRIMARY_DIM};
}}

QLineEdit:focus {{
    border: 1px solid {PRIMARY};
}}

QPushButton {{
    background-color: {GLASS_LIGHT};
    border: 1px solid {BORDER};
    border-radius: 10px;
    padding: 8px 16px;
    color: {TEXT};
}}

QPushButton:hover {{
    background-color: {GLASS_HOVER};
    border: 1px solid {BORDER_STRONG};
}}

QPushButton#primary {{
    background-color: {PRIMARY};
    border: 1px solid {PRIMARY};
    color: white;
    font-weight: 600;
}}

QPushButton#primary:hover {{
    background-color: #9333ea;
}}

QListWidget {{
    background-color: transparent;
    border: none;
    outline: none;
}}

QListWidget::item {{
    border-radius: 12px;
    padding: 6px;
    margin: 2px 4px;
}}

QListWidget::item:selected {{
    background-color: {PRIMARY_DIM};
    border: 1px solid {PRIMARY};
}}

QListWidget::item:hover {{
    background-color: {GLASS_HOVER};
}}

QScrollBar:vertical {{
    background: transparent;
    width: 10px;
    margin: 4px;
}}

QScrollBar::handle:vertical {{
    background: {BORDER};
    border-radius: 5px;
    min-height: 24px;
}}

QScrollBar::add-line:vertical, QScrollBar::sub-line:vertical {{
    height: 0;
}}

QStatusBar {{
    background: transparent;
    color: {TEXT_FAINT};
    border-top: 1px solid {BORDER};
}}
"""

MIME_ICON = "application/x-foldericonstudio-icon"
