"""Thumbnail loading on the main thread (Qt GUI objects are not thread-safe)."""

from __future__ import annotations

from collections import OrderedDict, deque
from pathlib import Path

from PIL import Image
from PyQt6.QtCore import QObject, QTimer, pyqtSignal
from PyQt6.QtGui import QIcon, QImage, QPixmap

IMAGE_EXTS = {".png", ".jpg", ".jpeg", ".ico", ".bmp", ".webp", ".gif"}


def _load_rgba(path: Path) -> Image.Image:
    with Image.open(path) as im:
        if path.suffix.lower() == ".ico" and getattr(im, "n_frames", 1) > 1:
            best: Image.Image | None = None
            best_area = 0
            for i in range(im.n_frames):
                im.seek(i)
                area = im.size[0] * im.size[1]
                if area > best_area:
                    best_area = area
                    best = im.copy()
            if best is not None:
                return best.convert("RGBA")
        return im.convert("RGBA")


def _pil_to_pixmap(img: Image.Image, size: int) -> QPixmap:
    thumb = img.copy()
    thumb.thumbnail((size, size), Image.Resampling.LANCZOS)
    data = thumb.tobytes("raw", "RGBA")
    qimg = QImage(
        data,
        thumb.width,
        thumb.height,
        thumb.width * 4,
        QImage.Format.Format_RGBA8888,
    ).copy()
    return QPixmap.fromImage(qimg)


class ThumbnailCache(QObject):
    loaded = pyqtSignal(str, QPixmap)

    def __init__(self, max_items: int = 500, thumb_size: int = 96) -> None:
        super().__init__()
        self.thumb_size = thumb_size
        self._cache: OrderedDict[str, QPixmap] = OrderedDict()
        self._max = max_items
        self._queue: deque[Path] = deque()
        self._queued: set[str] = set()
        self._timer = QTimer(self)
        self._timer.setInterval(10)
        self._timer.timeout.connect(self._process_batch)

    def get(self, path: Path) -> QPixmap | None:
        key = str(path.resolve())
        if key in self._cache:
            self._cache.move_to_end(key)
            return self._cache[key]
        return None

    def request(self, path: Path) -> None:
        key = str(path.resolve())
        if key in self._cache:
            self.loaded.emit(key, self._cache[key])
            return
        if key in self._queued:
            return
        self._queued.add(key)
        self._queue.append(path.resolve())
        if not self._timer.isActive():
            self._timer.start()

    def _process_batch(self) -> None:
        processed = 0
        per_tick = 12

        while self._queue and processed < per_tick:
            path = self._queue.popleft()
            key = str(path)
            self._queued.discard(key)

            if key in self._cache:
                self.loaded.emit(key, self._cache[key])
                processed += 1
                continue

            try:
                if not path.is_file():
                    processed += 1
                    continue
                rgba = _load_rgba(path)
                pix = _pil_to_pixmap(rgba, self.thumb_size)
                rgba.close()
                if pix.isNull():
                    processed += 1
                    continue
                self._cache[key] = pix
                self._cache.move_to_end(key)
                while len(self._cache) > self._max:
                    self._cache.popitem(last=False)
                self.loaded.emit(key, pix)
            except Exception:
                pass

            processed += 1

        if not self._queue:
            self._timer.stop()

    def icon(self, path: Path) -> QIcon:
        pix = self.get(path)
        return QIcon(pix) if pix else QIcon()
