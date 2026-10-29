"""Copy project New Icons into smart categories under assets/bundled."""

from __future__ import annotations

import argparse
import json
import re
import shutil
import sys
from pathlib import Path

_ROOT = Path(__file__).resolve().parent.parent
if str(_ROOT) not in sys.path:
    sys.path.insert(0, str(_ROOT))

from folder_icon_studio.icon_categorizer import categorize_icon, category_sort_key

BUNDLED_VERSION = "3.0.0"


def _safe_filename(stem: str) -> str:
    cleaned = re.sub(r'[<>:"/\\|?*]', "_", stem).strip() or "icon"
    return cleaned


def _ico_size(path: Path) -> int:
    try:
        return path.stat().st_size
    except OSError:
        return 0


def prepare_bundled(
    source: Path,
    output: Path,
    *,
    clean: bool = True,
) -> dict[str, int]:
    if not source.is_dir():
        raise SystemExit(f"Missing icon source folder: {source}")

    if clean and output.exists():
        shutil.rmtree(output)
    output.mkdir(parents=True, exist_ok=True)

    # Best file per normalized stem (largest .ico wins duplicates).
    best: dict[str, tuple[Path, str, str]] = {}
    for ico in source.rglob("*.ico"):
        rel = ico.relative_to(source)
        rel_posix = rel.as_posix()
        source_folder = rel.parts[0] if len(rel.parts) > 1 else ""
        category = categorize_icon(ico.stem, rel_posix)
        key = ico.stem.lower()
        score = _ico_size(ico)
        current = best.get(key)
        if current is None or score > _ico_size(current[0]):
            best[key] = (ico, category, source_folder)

    categories: dict[str, int] = {}
    used_names: dict[str, set[str]] = {}
    stats = {"icons": 0, "categories": 0, "skipped_duplicates": 0}

    stats["skipped_duplicates"] = len(list(source.rglob("*.ico"))) - len(best)

    for stem_key in sorted(best, key=str.lower):
        ico, category, _source_folder = best[stem_key]
        cat_dir = output / category
        cat_dir.mkdir(parents=True, exist_ok=True)

        base = _safe_filename(ico.stem)
        used = used_names.setdefault(category, set())
        dest_name = f"{base}.ico"
        if dest_name.lower() in {n.lower() for n in used}:
            n = 2
            while f"{base} ({n}).ico".lower() in {x.lower() for x in used}:
                n += 1
            dest_name = f"{base} ({n}).ico"

        dest = cat_dir / dest_name
        shutil.copy2(ico, dest)
        used.add(dest_name)
        stats["icons"] += 1
        categories[category] = categories.get(category, 0) + 1

    stats["categories"] = len(categories)
    manifest = {
        "source": "Folder Icon Studio icon pack",
        "version": BUNDLED_VERSION,
        "icon_count": stats["icons"],
        "categorized": True,
        "categories": dict(
            sorted(categories.items(), key=lambda item: category_sort_key(item[0]))
        ),
    }
    (output / "manifest.json").write_text(
        json.dumps(manifest, indent=2),
        encoding="utf-8",
    )
    return stats


def main() -> int:
    parser = argparse.ArgumentParser(description="Prepare bundled icons from New Icons")
    parser.add_argument(
        "--source",
        type=Path,
        default=_ROOT / "New Icons",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=Path(__file__).resolve().parent / "assets" / "bundled",
    )
    args = parser.parse_args()

    stats = prepare_bundled(args.source, args.output)
    print(
        "Prepared {icons} icons in {categories} categories "
        "({skipped_duplicates} duplicate files merged) -> {output}".format(
            output=args.output,
            **stats,
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
