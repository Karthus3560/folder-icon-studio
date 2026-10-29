"""Generate starter folder icons (run once at build time)."""

from __future__ import annotations

from pathlib import Path

from PIL import Image, ImageDraw

COLORS = [
    ("amber", (255, 193, 7), (255, 160, 0)),
    ("blue", (33, 150, 243), (25, 118, 210)),
    ("cyan", (0, 188, 212), (0, 151, 167)),
    ("deep-orange", (255, 87, 34), (244, 81, 30)),
    ("deep-purple", (103, 58, 183), (81, 45, 168)),
    ("green", (76, 175, 80), (56, 142, 60)),
    ("indigo", (63, 81, 181), (48, 63, 159)),
    ("lime", (205, 220, 57), (175, 180, 43)),
    ("orange", (255, 152, 0), (245, 124, 0)),
    ("pink", (233, 30, 99), (194, 24, 91)),
    ("purple", (156, 39, 176), (123, 31, 162)),
    ("red", (244, 67, 54), (211, 47, 47)),
    ("teal", (0, 150, 136), (0, 121, 107)),
    ("yellow", (255, 235, 59), (251, 192, 45)),
    ("brown", (121, 85, 72), (93, 64, 55)),
    ("blue-grey", (96, 125, 139), (69, 90, 100)),
    ("coral", (255, 111, 97), (230, 74, 59)),
    ("forest", (46, 125, 50), (27, 94, 32)),
    ("gold", (255, 193, 7), (212, 160, 23)),
    ("lavender", (179, 157, 219), (149, 117, 205)),
    ("mint", (102, 187, 106), (67, 160, 71)),
    ("navy", (26, 35, 126), (13, 71, 161)),
    ("rose", (240, 98, 146), (216, 27, 96)),
    ("slate", (69, 90, 100), (38, 50, 56)),
    ("sky", (3, 169, 244), (2, 136, 209)),
    ("violet", (142, 36, 170), (106, 27, 154)),
]

SIZES = (16, 24, 32, 48, 64, 128, 256)


def _darken(rgb: tuple[int, int, int], amount: float) -> tuple[int, int, int]:
    return tuple(max(0, int(c * (1 - amount))) for c in rgb)


def _draw_folder(size: int, primary: tuple[int, int, int], accent: tuple[int, int, int]) -> Image.Image:
    s = size / 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    body = (int(24 * s), int(80 * s), int(232 * s), int(224 * s))
    tab = (int(24 * s), int(48 * s), int(120 * s), int(96 * s))
    d.rounded_rectangle(body, radius=int(14 * s), fill=primary)
    d.rounded_rectangle(tab, radius=int(8 * s), fill=_darken(accent, 0.1))
    return img


def generate_all(output_dir: Path) -> int:
    output_dir.mkdir(parents=True, exist_ok=True)
    for name, primary, accent in COLORS:
        images = [_draw_folder(sz, primary, accent) for sz in SIZES]
        path = output_dir / f"folder-{name}.ico"
        images[0].save(
            path,
            format="ICO",
            sizes=[(im.width, im.height) for im in images],
            append_images=images[1:],
        )
        for im in images:
            im.close()
    return len(COLORS)


if __name__ == "__main__":
    out = Path(__file__).resolve().parent / "assets" / "starter_icons"
    n = generate_all(out)
    print(f"Generated {n} starter icons in {out}")
