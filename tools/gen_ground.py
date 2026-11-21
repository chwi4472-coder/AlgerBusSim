"""Ground materials: asphalt, sidewalk, kerb, road markings, sky ramp.

Run:  python tools/gen_ground.py [out_dir]
"""

import json
import os
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

import artlib as A

TILE = 512


def asphalt(seed=1):
    rng = np.random.default_rng(seed)
    fine = A.fbm(TILE, TILE, base=96, octaves=3, seed=seed)
    coarse = A.fbm(TILE, TILE, base=8, octaves=5, seed=seed + 1)

    base = np.array([0.216, 0.212, 0.212], np.float32)
    arr = base[None, None, :] * (0.78 + 0.42 * coarse[..., None]) * (0.88 + 0.24 * fine[..., None])
    img = A.to_img(arr)

    # aggregate speckle
    speck = (rng.random((TILE, TILE)) > 0.985).astype(np.float32)
    img = A.overlay(img, "#8A8A86", speck * 0.5, 0.35)

    # repair patches
    d = ImageDraw.Draw(img.convert("RGBA"), "RGBA")
    img = img.convert("RGBA")
    d = ImageDraw.Draw(img, "RGBA")
    for _ in range(4):
        cx, cy = int(rng.integers(0, TILE)), int(rng.integers(0, TILE))
        w = int(rng.integers(60, 190))
        h = int(rng.integers(40, 140))
        d.rounded_rectangle([cx, cy, cx + w, cy + h], radius=12,
                            fill=(44, 43, 42, 55))

    # tar seams (periodic so the tile keeps tiling)
    for _ in range(3):
        y = int(rng.integers(0, TILE))
        pts = [(x, y + int(6 * np.sin(x / 47.0))) for x in range(0, TILE + 1, 8)]
        d.line(pts, fill=(26, 25, 25, 120), width=int(rng.integers(3, 6)))

    # cracks
    for _ in range(5):
        x, y = rng.integers(0, TILE), rng.integers(0, TILE)
        pts = [(x, y)]
        for _ in range(9):
            x += rng.integers(-18, 19)
            y += rng.integers(6, 22)
            pts.append((x, y))
        d.line(pts, fill=(18, 18, 18, 160), width=1)

    img = img.convert("RGB")
    img = A.grain(img, 0.018, seed + 7)
    return img


def sidewalk(seed=2):
    rng = np.random.default_rng(seed)
    W = H = TILE
    img = Image.new("RGB", (W, H), A.rgb("#B4AC9C"))
    d = ImageDraw.Draw(img)
    slab = W // 4
    for gy in range(4):
        for gx in range(4):
            c = A.jitter("#B8B0A0", 9, rng)
            x0, y0 = gx * slab, gy * slab
            d.rectangle([x0 + 1, y0 + 1, x0 + slab - 2, y0 + slab - 2], fill=c)
            # bevel
            d.line([(x0 + 2, y0 + 2), (x0 + slab - 2, y0 + 2)], fill=A.shade(c, 1.07), width=2)
            d.line([(x0 + 2, y0 + 2), (x0 + 2, y0 + slab - 2)], fill=A.shade(c, 1.04), width=2)
            d.line([(x0 + 2, y0 + slab - 2), (x0 + slab - 2, y0 + slab - 2)],
                   fill=A.shade(c, 0.88), width=2)

    # joints
    for i in range(5):
        p = i * slab
        d.line([(p, 0), (p, H)], fill=(92, 88, 80), width=3)
        d.line([(0, p), (W, p)], fill=(92, 88, 80), width=3)

    img = A.grain(img, 0.02, seed + 1)
    stain = A.fbm(H, W, base=5, octaves=4, seed=seed + 3)
    stain = np.clip((stain - 0.55) * 2.0, 0, 1)
    img = A.overlay(img, "#6E6656", A.blur_mask(stain, 5), 0.22)

    # occasional weeds in the joints
    d = ImageDraw.Draw(img)
    for _ in range(3):
        x, y = rng.integers(0, W), rng.integers(0, H)
        d.line([(x, y), (x + 2, y - 7)], fill=(74, 96, 54), width=2)
        d.line([(x, y), (x - 3, y - 6)], fill=(84, 108, 60), width=2)
    return img


def kerb(seed=3):
    W, H = 256, 128
    img = A.vgrad(W, H, "#C0B8A8", "#A69C8A")
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, W, 26], fill=A.rgb("#C8C0B0"))
    d.line([(0, 26), (W, 26)], fill=(90, 86, 78), width=3)
    d.line([(0, 2), (W, 2)], fill=(230, 226, 216), width=2)
    for x in range(0, W, 64):
        d.line([(x, 0), (x, H)], fill=(96, 92, 84), width=2)
    img = A.grain(img, 0.03, seed)
    dirt = A.fbm(H, W, base=4, octaves=4, seed=seed + 2)
    dirt = np.clip((dirt - 0.5) * 1.8, 0, 1) * np.linspace(0, 1, H)[:, None]
    img = A.overlay(img, "#5E5646", A.blur_mask(dirt, 3), 0.3)
    return img


def markings(seed=4):
    """1024x1024 alpha atlas, 4x2 tiles of 256."""
    W = H = 1024
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    white = (238, 236, 228, 255)

    def cell(i):
        return (i % 4) * 256, (i // 4) * 256

    # 0 dashed lane line (vertical, one dash per tile)
    x, y = cell(0)
    d.rectangle([x + 112, y + 40, x + 144, y + 216], fill=white)

    # 1 solid line
    x, y = cell(1)
    d.rectangle([x + 120, y, x + 136, y + 256], fill=white)

    # 2 double yellow (no overtaking, intercity)
    x, y = cell(2)
    d.rectangle([x + 104, y, x + 116, y + 256], fill=(228, 196, 84, 255))
    d.rectangle([x + 140, y, x + 152, y + 256], fill=(228, 196, 84, 255))

    # 3 zebra bar
    x, y = cell(3)
    for i in range(4):
        d.rectangle([x + i * 64 + 6, y + 8, x + i * 64 + 50, y + 248], fill=white)

    # 4 straight arrow
    x, y = cell(4)
    d.rectangle([x + 110, y + 96, x + 146, y + 236], fill=white)
    d.polygon([(x + 128, y + 18), (x + 176, y + 104), (x + 80, y + 104)], fill=white)

    # 5 left arrow
    x, y = cell(5)
    d.rectangle([x + 120, y + 110, x + 156, y + 240], fill=white)
    d.rectangle([x + 40, y + 96, x + 156, y + 130], fill=white)
    d.polygon([(x + 24, y + 113), (x + 82, y + 62), (x + 82, y + 164)], fill=white)

    # 6 right arrow
    x, y = cell(6)
    d.rectangle([x + 100, y + 110, x + 136, y + 240], fill=white)
    d.rectangle([x + 100, y + 96, x + 216, y + 130], fill=white)
    d.polygon([(x + 232, y + 113), (x + 174, y + 62), (x + 174, y + 164)], fill=white)

    # 7 BUS lane text
    x, y = cell(7)
    f = A.font("arialbd.ttf", 120)
    A.text(d, (x + 128, y + 128), "BUS", f, white)

    # weather the paint so it is not perfectly clean
    arr = np.asarray(img, np.float32) / 255.0
    wear = A.fbm(H, W, base=32, octaves=4, seed=seed)
    a = arr[..., 3] * np.clip(0.55 + 0.9 * wear, 0, 1)
    arr[..., 3] = a
    img = A.to_img(arr)

    rects = {
        "mark_dash": [0, 0, 256, 256],
        "mark_solid": [256, 0, 256, 256],
        "mark_double_yellow": [512, 0, 256, 256],
        "mark_zebra": [768, 0, 256, 256],
        "mark_arrow_straight": [0, 256, 256, 256],
        "mark_arrow_left": [256, 256, 256, 256],
        "mark_arrow_right": [512, 256, 256, 256],
        "mark_bus": [768, 256, 256, 256],
    }
    return img, rects


def sky_ramp(seed=5):
    W, H = 64, 512
    stops = [
        (0.00, "#1F4E7A"),   # zenith
        (0.35, "#4F86AE"),
        (0.62, "#9CC0D2"),
        (0.80, "#D6DFDC"),
        (0.90, "#E8DCC6"),   # Mediterranean haze
        (1.00, "#C9B79A"),
    ]
    arr = np.zeros((H, W, 3), np.float32)
    for i in range(H):
        t = i / (H - 1)
        for k in range(len(stops) - 1):
            t0, c0 = stops[k]
            t1, c1 = stops[k + 1]
            if t0 <= t <= t1:
                f = (t - t0) / max(1e-6, t1 - t0)
                arr[i, :, :] = np.array(A.mix(c0, c1, f), np.float32) / 255.0
                break
    img = A.to_img(arr)
    # faint banding fix
    img = A.grain(img, 0.004, seed)
    return img


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    out = sys.argv[1] if len(sys.argv) > 1 else "Assets/StreamingAssets/art"
    os.makedirs(out, exist_ok=True)

    A.save(asphalt(), os.path.join(out, "ground_asphalt.png"))
    A.save(sidewalk(), os.path.join(out, "ground_sidewalk.png"))
    A.save(kerb(), os.path.join(out, "ground_kerb.png"))
    mk, rects = markings()
    A.save(mk, os.path.join(out, "ground_markings.png"))
    A.save(sky_ramp(), os.path.join(out, "sky_ramp.png"))

    with open(os.path.join(out, "ground_markings.rects.json"), "w", encoding="utf-8") as fh:
        json.dump({"texture": "ground_markings", "sprites": rects}, fh, indent=1)

    prev = os.path.join("tools", "_preview")
    os.makedirs(prev, exist_ok=True)
    sheet = Image.new("RGB", (TILE * 3, TILE), (30, 30, 32))
    sheet.paste(asphalt(), (0, 0))
    sheet.paste(sidewalk(), (TILE, 0))
    sheet.paste(kerb().resize((TILE, TILE), Image.LANCZOS), (TILE * 2, 0))
    sheet.save(os.path.join(prev, "ground.png"))
    print("ground textures written to", out)


if __name__ == "__main__":
    main()
