"""Application publisher metadata (embedded + written to local app data)."""

from __future__ import annotations

import json
import sys
from pathlib import Path

PUBLISHER_NAME = "Ghaith S. Jawad"
PUBLISHER_EMAIL = "Safagsj3560@gmail.com"
PRODUCT_NAME = "Folder Icon Studio"
VERSION = "1.1.0"
COPYRIGHT = f"Copyright (C) 2026 {PUBLISHER_NAME}"

PUBLISHER_INFO = {
    "publisher": PUBLISHER_NAME,
    "email": PUBLISHER_EMAIL,
    "product": PRODUCT_NAME,
    "version": VERSION,
    "copyright": COPYRIGHT,
}


def bundled_publisher_json() -> Path:
    if getattr(sys, "frozen", False):
        return Path(sys._MEIPASS) / "folder_icon_studio" / "assets" / "publisher.json"
    return Path(__file__).resolve().parent / "assets" / "publisher.json"


def register_publisher_metadata() -> None:
    """Write publisher info to local app data so Windows/tools can read it."""
    app_dir = Path.home() / "AppData" / "Local" / "FolderIconStudio"
    app_dir.mkdir(parents=True, exist_ok=True)
    dest = app_dir / "publisher.json"
    if not dest.is_file():
        bundled = bundled_publisher_json()
        if bundled.is_file():
            dest.write_text(bundled.read_text(encoding="utf-8"), encoding="utf-8")
        else:
            dest.write_text(json.dumps(PUBLISHER_INFO, indent=2), encoding="utf-8")
