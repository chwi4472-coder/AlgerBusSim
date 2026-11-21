"""Shared procedural drawing helpers for the AlgerBusSim art pipeline.

Everything here is deterministic (seeded) so regenerating the art produces
byte-identical atlases.  numpy is used for noise, Pillow for drawing.
"""

import os
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

from arabic import shape as shape_arabic

FONT_DIR = r"C:\Windows\Fonts"
_FONT_CACHE = {}


# ---------------------------------------------------------------- colour ----

def rgb(value):
    """(r,g,b) or (r,g,b,a) from '#rrggbb' / '#rrggbbaa' / tuple."""
    if isinstance(value, tuple):
        return value
    v = value.lstrip("#")
    if len(v) == 6:
        return tuple(int(v[i:i + 2], 16) for i in (0, 2, 4))
    if len(v) == 8:
        return tuple(int(v[i:i + 2], 16) for i in (0, 2, 4, 6))
    raise ValueError("bad colour " + value)


def mix(a, b, t):
    a, b = rgb(a), rgb(b)
    n = min(len(a), len(b))
    return tuple(int(round(a[i] + (b[i] - a[i]) * t)) for i in range(n))


def shade(c, factor):
    c = rgb(c)
    return tuple(max(0, min(255, int(round(v * factor)))) for v in c)


def jitter(c, amount, rng):
    c = rgb(c)
    return tuple(max(0, min(255, int(round(v + rng.uniform(-amount, amount))))) for v in c)


# ----------------------------------------------------------------- noise ----

def pnoise(h, w, gx, gy, rng):
    """Periodic value noise in [0,1], tileable in both axes."""
    gx = max(1, int(gx))
    gy = max(1, int(gy))
    grid = rng.random((gy, gx)).astype(np.float32)

    ys = np.linspace(0, gy, h, endpoint=False)
    xs = np.linspace(0, gx, w, endpoint=False)
    y0 = np.floor(ys).astype(int)
    x0 = np.floor(xs).astype(int)
    fy = ys - y0
    fx = xs - x0
    y1 = (y0 + 1) % gy
    x1 = (x0 + 1) % gx
    y0 %= gy
    x0 %= gx

    fy = (fy * fy * (3 - 2 * fy))[:, None]
    fx = (fx * fx * (3 - 2 * fx))[None, :]

    a = grid[np.ix_(y0, x0)]
    b = grid[np.ix_(y0, x1)]
    c = grid[np.ix_(y1, x0)]
    d = grid[np.ix_(y1, x1)]

    top = a + (b - a) * fx
    bot = c + (d - c) * fx
    return (top + (bot - top) * fy).astype(np.float32)


def fbm(h, w, base=4, octaves=4, seed=0, gain=0.5, lacunarity=2):
    rng = np.random.default_rng(seed)
    total = np.zeros((h, w), np.float32)
    amp = 1.0
    norm = 0.0
    gx, gy = base, max(1, int(base * h / max(1, w)))
    for _ in range(octaves):
        total += amp * pnoise(h, w, gx, gy, rng)
        norm += amp
        amp *= gain
        gx = int(gx * lacunarity)
        gy = int(gy * lacunarity)
    return total / max(norm, 1e-6)


def streak(h, w, seed=0, count=26, vertical=True):
    """Weathering streaks (rain run-off).  Returns float mask 0..1."""
    rng = np.random.default_rng(seed)
    acc = np.zeros((h, w), np.float32)
    for _ in range(count):
        if vertical:
            x = rng.integers(0, w)
            width = rng.integers(2, max(3, w // 90))
            length = rng.integers(h // 4, h)
            y0 = rng.integers(0, h - length + 1)
            band = np.zeros((h, w), np.float32)
            band[y0:y0 + length, x:x + width] = 1.0
            fade = np.linspace(1.0, 0.15, length).astype(np.float32)[:, None]
            band[y0:y0 + length, x:x + width] *= fade
            acc = np.maximum(acc, band)
        else:
            y = rng.integers(0, h)
            height = rng.integers(2, max(3, h // 90))
            length = rng.integers(w // 4, w)
            x0 = rng.integers(0, w - length + 1)
            band = np.zeros((h, w), np.float32)
            band[y:y + height, x0:x0 + length] = 1.0
            acc = np.maximum(acc, band)
    return np.clip(acc, 0, 1)


def to_img(arr):
    """float array (h,w) or (h,w,3|4) -> PIL image."""
    a = np.clip(arr, 0, 1)
    if a.ndim == 2:
        return Image.fromarray((a * 255).astype(np.uint8), "L")
    if a.shape[2] == 3:
        return Image.fromarray((a * 255).astype(np.uint8), "RGB")
    return Image.fromarray((a * 255).astype(np.uint8), "RGBA")


def from_img(img):
    return np.asarray(img.convert("RGB"), np.float32) / 255.0


def multiply(img, noise, amount=1.0, lo=0.0):
    """Multiply an RGB image by a float mask (0..1)."""
    a = from_img(img)
    m = (1.0 - amount) + amount * (lo + (1.0 - lo) * noise)[..., None]
    return to_img(a * m)


def overlay(img, colour, mask, alpha=1.0):
    """Overlay a flat colour using a float mask."""
    a = from_img(img)
    c = np.array(rgb(colour)[:3], np.float32) / 255.0
    m = np.clip(mask * alpha, 0, 1)[..., None]
    return to_img(a * (1 - m) + c * m)


def grain(img, amount=0.03, seed=0):
    rng = np.random.default_rng(seed)
    a = from_img(img)
    n = rng.normal(0, amount, a.shape[:2]).astype(np.float32)[..., None]
    return to_img(np.clip(a + n, 0, 1))


def blur_mask(mask, radius):
    im = to_img(mask)
    return np.asarray(im.filter(ImageFilter.GaussianBlur(radius)), np.float32) / 255.0


# ---------------------------------------------------------------- shapes ----

def vgrad(w, h, top, bottom, gamma=1.0):
    t = np.linspace(0, 1, h, dtype=np.float32)[:, None] ** gamma
    t = np.repeat(t, w, axis=1)
    a = np.array(rgb(top)[:3], np.float32) / 255.0
    b = np.array(rgb(bottom)[:3], np.float32) / 255.0
    out = a[None, None, :] * (1 - t[..., None]) + b[None, None, :] * t[..., None]
    return to_img(out)


def rounded_mask(size, radius, ss=4):
    w, h = size
    m = Image.new("L", (w * ss, h * ss), 0)
    d = ImageDraw.Draw(m)
    d.rounded_rectangle([0, 0, w * ss - 1, h * ss - 1], radius=radius * ss, fill=255)
    return m.resize((w, h), Image.LANCZOS)


def rect_mask(size, box=None, radius=0):
    w, h = size
    m = Image.new("L", (w, h), 0)
    d = ImageDraw.Draw(m)
    if box is None:
        box = (0, 0, w - 1, h - 1)
    if radius:
        d.rounded_rectangle(box, radius=radius, fill=255)
    else:
        d.rectangle(box, fill=255)
    return m


def soft_shadow(size, box, radius=6, spread=10, strength=0.55):
    """Returns a float mask usable with overlay() to darken."""
    w, h = size
    m = Image.new("L", (w, h), 0)
    d = ImageDraw.Draw(m)
    b = (box[0] - spread, box[1] - spread, box[2] + spread, box[3] + spread)
    d.rounded_rectangle(b, radius=radius, fill=int(255 * strength))
    m = m.filter(ImageFilter.GaussianBlur(spread * 0.6))
    return np.asarray(m, np.float32) / 255.0


def darken_rgba(layer, mask, strength=1.0, colour=(0, 0, 0)):
    """Darken an RGBA layer with a float mask, preserving the layer's alpha."""
    a = np.asarray(layer, np.float32) / 255.0
    m = np.clip(mask * strength, 0, 1)[..., None]
    c = np.array(colour, np.float32) / 255.0
    rgb = a[..., :3] * (1 - m) + c[None, None, :] * m
    out = np.concatenate([rgb, a[..., 3:4]], axis=2)
    return to_img(out)


def paint_rgba(layer, mask, colour, strength=1.0):
    """Paint a flat colour over an RGBA layer with a float mask, keeping alpha."""
    a = np.asarray(layer, np.float32) / 255.0
    m = np.clip(mask * strength, 0, 1)[..., None]
    c = np.array(rgb(colour)[:3], np.float32) / 255.0
    rgb = a[..., :3] * (1 - m) + c[None, None, :] * m
    alpha = np.clip(a[..., 3:4] + m, 0, 1)
    return to_img(np.concatenate([rgb, alpha], axis=2))


def inner_shadow(size, box, radius=0, depth=6):
    """Dark band just inside a rectangle (used for window recesses)."""
    w, h = size
    m = Image.new("L", (w, h), 0)
    d = ImageDraw.Draw(m)
    d.rounded_rectangle(box, radius=radius, fill=255)
    outer = m
    inner = Image.new("L", (w, h), 0)
    ImageDraw.Draw(inner).rounded_rectangle(
        (box[0] + depth, box[1] + depth, box[2] - depth, box[3] - depth),
        radius=max(0, radius - depth), fill=255)
    band = np.asarray(outer, np.float32) - np.asarray(inner, np.float32)
    band = np.clip(band / 255.0, 0, 1)
    return blur_mask(band, depth * 0.5)


# ------------------------------------------------------------------ text ----

def font(name, size):
    key = (name, size)
    if key not in _FONT_CACHE:
        _FONT_CACHE[key] = ImageFont.truetype(os.path.join(FONT_DIR, name), size)
    return _FONT_CACHE[key]


def text(draw, xy, s, f, fill, anchor="mm", rtl=None, stroke=0, stroke_fill=None,
         spacing=0):
    """Draw text, shaping Arabic when needed."""
    if rtl is None:
        rtl = any(0x0600 <= ord(c) <= 0x06FF for c in s)
    if rtl:
        s = shape_arabic(s)
    draw.text(xy, s, font=f, fill=fill, anchor=anchor,
              stroke_width=stroke, stroke_fill=stroke_fill, spacing=spacing)


def text_size(s, f, rtl=None):
    if rtl is None:
        rtl = any(0x0600 <= ord(c) <= 0x06FF for c in s)
    if rtl:
        s = shape_arabic(s)
    return f.getbbox(s)


def fit_font(s, name, max_size, max_width, rtl=None):
    size = max_size
    while size > 8:
        f = font(name, size)
        bb = text_size(s, f, rtl)
        if bb[2] - bb[0] <= max_width:
            return f
        size -= 1
    return font(name, 8)


# ------------------------------------------------------------------ save ----

def save(img, path, low_scale=None):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    if img.mode == "RGBA":
        img.save(path, "PNG", optimize=True)
    else:
        img.convert("RGB").save(path, "PNG", optimize=True)
    if low_scale:
        low_dir = os.path.join(os.path.dirname(path), "low")
        os.makedirs(low_dir, exist_ok=True)
        w, h = img.size
        low = img.resize((max(1, int(w * low_scale)), max(1, int(h * low_scale))),
                         Image.LANCZOS)
        low.save(os.path.join(low_dir, os.path.basename(path)), "PNG", optimize=True)


def desaturate(img, amount=0.5):
    g = img.convert("L").convert("RGB")
    return Image.blend(img.convert("RGB"), g, amount)
