"""Windows folder icon mechanics: desktop.ini, attributes, shell refresh."""

from __future__ import annotations

import ctypes
from ctypes import wintypes
from pathlib import Path

DESKTOP_INI = "desktop.ini"
MARKER = ".foldericonstudio"
SHELL_CLASS = ".ShellClassInfo"
ICON_KEY = "IconResource"

FILE_ATTRIBUTE_READONLY = 0x1
FILE_ATTRIBUTE_HIDDEN = 0x2
FILE_ATTRIBUTE_SYSTEM = 0x4
FILE_ATTRIBUTE_NORMAL = 0x80

SHCNE_UPDATEDIR = 0x00001000
SHCNE_ASSOCCHANGED = 0x08000000
SHCNE_UPDATEIMAGE = 0x00008000
SHCNF_PATHW = 0x0005
SHCNF_FLUSHNOWAIT = 0x3000
SHCNF_IDLIST = 0x0000

kernel32 = ctypes.windll.kernel32
shell32 = ctypes.windll.shell32


def _long_path(path: str | Path) -> str:
    p = str(Path(path).resolve())
    if p.startswith("\\\\?\\"):
        return p
    if p.startswith("\\\\"):
        return "\\\\?\\UNC\\" + p[2:]
    return "\\\\?\\" + p


def get_attributes(path: str | Path) -> int:
    attrs = kernel32.GetFileAttributesW(_long_path(path))
    if attrs == 0xFFFFFFFF:
        return 0
    return attrs


def set_attributes(path: str | Path, attrs: int) -> None:
    lp = _long_path(path)
    if not kernel32.SetFileAttributesW(lp, attrs):
        normal = str(Path(path).resolve())
        if not kernel32.SetFileAttributesW(normal, attrs):
            err = kernel32.GetLastError()
            raise OSError(err, f"SetFileAttributes failed on {path}")


def notify_folder_changed(folder: str | Path) -> None:
    folder = str(Path(folder).resolve())
    parent = str(Path(folder).parent)
    shell32.SHChangeNotify(SHCNE_UPDATEDIR, SHCNF_PATHW | SHCNF_FLUSHNOWAIT, folder, None)
    if parent and parent != folder:
        shell32.SHChangeNotify(SHCNE_UPDATEDIR, SHCNF_PATHW | SHCNF_FLUSHNOWAIT, parent, None)
    shell32.SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, None, None)
    shell32.SHChangeNotify(SHCNE_UPDATEIMAGE, SHCNF_FLUSHNOWAIT, None, None)


def _read_ini(path: Path) -> list[str]:
    if not path.is_file():
        return []
    text = path.read_text(encoding="utf-16")
    return text.replace("\r\n", "\n").replace("\r", "\n").split("\n")


def _write_ini(path: Path, lines: list[str]) -> None:
    content = "\r\n".join(lines)
    if not content.endswith("\r\n"):
        content += "\r\n"
    path.write_text(content, encoding="utf-16")


def _prepare_folder(folder: Path) -> None:
    attrs = get_attributes(folder)
    attrs &= ~FILE_ATTRIBUTE_READONLY
    set_attributes(folder, attrs)
    ini = folder / DESKTOP_INI
    if ini.is_file():
        set_attributes(ini, FILE_ATTRIBUTE_NORMAL)
    marker = folder / MARKER
    if marker.is_file():
        set_attributes(marker, FILE_ATTRIBUTE_NORMAL)


def apply_icon(folder: str | Path, icon_path: str | Path) -> None:
    folder = Path(folder).resolve()
    icon_path = Path(icon_path).resolve()
    if not folder.is_dir():
        raise FileNotFoundError(f"Folder not found: {folder}")
    if not icon_path.is_file():
        raise FileNotFoundError(f"Icon not found: {icon_path}")

    _prepare_folder(folder)
    ini = folder / DESKTOP_INI
    lines = _read_ini(ini)
    lines = [l for l in lines if not l.strip().lower().startswith(f"{ICON_KEY.lower()}=")]

    section = f"[{SHELL_CLASS}]"
    if not any(l.strip().lower() == section.lower() for l in lines):
        if lines and lines[-1].strip():
            lines.append("")
        lines.append(section)

    idx = next(i for i, l in enumerate(lines) if l.strip().lower() == section.lower())
    lines.insert(idx + 1, f"{ICON_KEY}={icon_path},0")
    _write_ini(ini, lines)

    attrs = get_attributes(folder)
    attrs |= FILE_ATTRIBUTE_SYSTEM | FILE_ATTRIBUTE_READONLY
    set_attributes(folder, attrs)

    set_attributes(ini, FILE_ATTRIBUTE_HIDDEN | FILE_ATTRIBUTE_SYSTEM)
    marker = folder / MARKER
    marker.write_text("", encoding="utf-8")
    set_attributes(marker, FILE_ATTRIBUTE_HIDDEN | FILE_ATTRIBUTE_SYSTEM)
    notify_folder_changed(folder)


def get_current_icon_path(folder: str | Path) -> str | None:
    """Read IconResource from desktop.ini, or None if default."""
    folder = Path(folder).resolve()
    ini = folder / DESKTOP_INI
    if not ini.is_file():
        return None
    try:
        for line in _read_ini(ini):
            stripped = line.strip()
            if stripped.lower().startswith(f"{ICON_KEY.lower()}="):
                value = stripped.split("=", 1)[1].strip()
                icon_path = value.split(",")[0].strip().strip('"')
                return icon_path if icon_path else None
    except OSError:
        return None
    return None


def get_folder_icon_state(folder: str | Path) -> dict:
    """Current icon path on disk and whether the file still exists."""
    path = get_current_icon_path(folder)
    if not path:
        return {"current_icon": None, "missing": False, "has_custom": False}
    missing = not Path(path).is_file()
    return {"current_icon": path, "missing": missing, "has_custom": True}


def restore_default(folder: str | Path) -> None:
    folder = Path(folder).resolve()
    ini = folder / DESKTOP_INI
    marker = folder / MARKER
    had_marker = marker.is_file()

    if not ini.is_file():
        if had_marker:
            attrs = get_attributes(folder)
            attrs &= ~(FILE_ATTRIBUTE_SYSTEM | FILE_ATTRIBUTE_READONLY)
            set_attributes(folder, attrs)
        notify_folder_changed(folder)
        return

    _prepare_folder(folder)
    lines = _read_ini(ini)
    lines = [l for l in lines if not l.strip().lower().startswith(f"{ICON_KEY.lower()}=")]
    meaningful = [
        l for l in lines
        if l.strip() and l.strip().lower() != f"[{SHELL_CLASS}]".lower()
    ]

    if meaningful:
        _write_ini(ini, lines)
    else:
        set_attributes(ini, FILE_ATTRIBUTE_NORMAL)
        ini.unlink(missing_ok=True)
        if had_marker:
            attrs = get_attributes(folder)
            attrs &= ~(FILE_ATTRIBUTE_SYSTEM | FILE_ATTRIBUTE_READONLY)
            set_attributes(folder, attrs)

    if marker.is_file():
        set_attributes(marker, FILE_ATTRIBUTE_NORMAL)
        marker.unlink(missing_ok=True)

    notify_folder_changed(folder)
