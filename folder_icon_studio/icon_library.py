"""Icon library: scan, categorize, import, and seed bundled packs."""

from __future__ import annotations

import json
import shutil
from dataclasses import dataclass
from pathlib import Path

from PIL import Image

ICON_SIZES = (16, 24, 32, 48, 64, 128, 256)
IMAGE_EXTS = {".png", ".jpg", ".jpeg", ".ico", ".bmp", ".webp", ".gif", ".svg"}

BUNDLED_DIR_NAME = "_Bundled"
MY_ICONS_DIR_NAME = "My Icons"
STARTERS_DIR_NAME = "_Starters"


@dataclass(frozen=True)
class IconEntry:
    path: Path
    category: str
    bundled: bool

    @property
    def name(self) -> str:
        return self.path.stem


def default_library_path() -> Path:
    return Path.home() / "Pictures" / "Folder Icons"


def ensure_library(path: Path | None = None) -> Path:
    lib = path or default_library_path()
    lib.mkdir(parents=True, exist_ok=True)
    (lib / MY_ICONS_DIR_NAME).mkdir(exist_ok=True)
    return lib


def icon_category(path: Path, library: Path) -> str:
    try:
        rel = path.resolve().relative_to(library.resolve())
        if not rel.parts:
            return "Other"
        top = rel.parts[0]
        if top == BUNDLED_DIR_NAME and len(rel.parts) >= 2:
            return rel.parts[1]
        if top == MY_ICONS_DIR_NAME:
            return MY_ICONS_DIR_NAME
        if top == STARTERS_DIR_NAME:
            return "Starter colors"
    except ValueError:
        pass
    return "Other"


def list_icon_entries(library: Path, *, recursive: bool = True) -> list[IconEntry]:
    if not library.is_dir():
        return []

    found: dict[str, IconEntry] = {}
    patterns = ("*.ico", "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.webp", "*.gif")
    iterator = library.rglob if recursive else library.glob

    for pattern in patterns:
        for path in iterator(pattern):
            if not path.is_file() or path.name.startswith("."):
                continue
            key = str(path.resolve()).lower()
            cat = icon_category(path, library)
            bundled = BUNDLED_DIR_NAME in path.parts
            found[key] = IconEntry(path=path, category=cat, bundled=bundled)

    return sorted(found.values(), key=lambda e: (e.category.lower(), e.name.lower()))


def list_icons(library: Path, *, recursive: bool = True) -> list[Path]:
    return [e.path for e in list_icon_entries(library, recursive=recursive)]


def list_categories(library: Path) -> list[str]:
    from folder_icon_studio.icon_categorizer import category_sort_key

    cats = {e.category for e in list_icon_entries(library)}
    return sorted(cats, key=category_sort_key)


def _unique_path(dest: Path) -> Path:
    if not dest.exists():
        return dest
    stem, suffix = dest.stem, dest.suffix
    n = 1
    while True:
        candidate = dest.with_name(f"{stem}-{n}{suffix}")
        if not candidate.exists():
            return candidate
        n += 1


def _write_multi_size_ico(source: Image.Image, output: Path) -> None:
    images: list[Image.Image] = []
    for size in ICON_SIZES:
        img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
        fitted = source.copy()
        fitted.thumbnail((size, size), Image.Resampling.LANCZOS)
        ox = (size - fitted.width) // 2
        oy = (size - fitted.height) // 2
        img.paste(fitted, (ox, oy), fitted if fitted.mode == "RGBA" else None)
        images.append(img)

    images[0].save(
        output,
        format="ICO",
        sizes=[(im.width, im.height) for im in images],
        append_images=images[1:],
    )
    for im in images:
        im.close()


def import_image(source: Path, library: Path) -> Path:
    source = source.resolve()
    library = ensure_library(library)
    my_dir = library / MY_ICONS_DIR_NAME
    my_dir.mkdir(exist_ok=True)
    ext = source.suffix.lower()

    if ext == ".ico":
        dest = _unique_path(my_dir / source.name)
        shutil.copy2(source, dest)
        return dest

    if ext == ".svg":
        raise ValueError("SVG is not supported. Export as PNG first.")

    if ext not in IMAGE_EXTS:
        raise ValueError(f"Unsupported file type: {ext}")

    with Image.open(source) as img:
        img = img.convert("RGBA")
        safe = "".join(c if c.isalnum() or c in "-_" else "_" for c in source.stem) or "imported"
        dest = _unique_path(my_dir / f"{safe}.ico")
        _write_multi_size_ico(img, dest)
    return dest


def _copy_tree_if_missing(src: Path, dest_root: Path) -> int:
    count = 0
    if not src.is_dir():
        return count
    for item in src.rglob("*"):
        if not item.is_file():
            continue
        rel = item.relative_to(src)
        dest = dest_root / rel
        if dest.exists():
            continue
        dest.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(item, dest)
        count += 1
    return count


def _copy_tree_all(src: Path, dest_root: Path) -> int:
    count = 0
    if not src.is_dir():
        return count
    for item in src.rglob("*"):
        if not item.is_file():
            continue
        rel = item.relative_to(src)
        dest = dest_root / rel
        dest.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(item, dest)
        count += 1
    return count


def _bundled_version(path: Path) -> str:
    manifest = path / "manifest.json"
    if not manifest.is_file():
        return ""
    try:
        data = json.loads(manifest.read_text(encoding="utf-8"))
    except Exception:
        return ""
    return str(data.get("version", ""))


def seed_starter_icons(library: Path, assets_root: Path) -> int:
    ensure_library(library)
    starters_dest = library / STARTERS_DIR_NAME
    return _copy_tree_if_missing(assets_root / "starter_icons", starters_dest)


def seed_bundled_icons(library: Path, bundled_assets: Path) -> int:
    ensure_library(library)
    if not bundled_assets.is_dir():
        return 0

    dest = library / BUNDLED_DIR_NAME
    src_version = _bundled_version(bundled_assets)
    dest_version = _bundled_version(dest)
    has_icons = any(dest.rglob("*.ico")) if dest.is_dir() else False

    if src_version and dest_version == src_version and has_icons:
        return 0

    if dest.is_dir():
        shutil.rmtree(dest)

    return _copy_tree_all(bundled_assets, dest)


def bundled_license_path(library: Path) -> Path | None:
    p = library / BUNDLED_DIR_NAME / "LICENSE.txt"
    return p if p.is_file() else None


def bundled_manifest(library: Path) -> dict | None:
    p = library / BUNDLED_DIR_NAME / "manifest.json"
    if not p.is_file():
        return None
    try:
        return json.loads(p.read_text(encoding="utf-8"))
    except Exception:
        return None
