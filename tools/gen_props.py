"""Environmental props atlas (foliage, street furniture).

1024x1024 with a 4x4 grid of 256px sprites.

Run:  python tools/gen_props.py [out_dir]
"""

import json
import os
import sys

from PIL import Image, ImageDraw

import artlib as A

GREENS = ("#2E6B3A", "#3E7A45", "#4E8A52", "#577F4A", "#2A5C35")
BROWNS = ("#6B4A2E", "#7A5636", "#5E4128")
GREYS = ("#8A8C86", "#6E7070", "#9A9C98")


def palm_frond(d, cx, cy, s, col):
    base = (cx, cy)
    d.line([(cx, cy), (cx - 0.9 * s, cy - 2.4 * s)], fill=col, width=4)
    n = 7
    for i in range(n):
        t = i / (n - 1)
        length = 1.6 * s
        width = 0.34 * s * (1 - t * 0.6)
        px = cx - 0.9 * s + (0.9 * s * t)
        py = cy - 2.4 * s + (2.4 * s * t)
        pts = [(px, py), (px - width, py - length * 0.45), (px, py - length)]
        d.polygon(pts, fill=col)
        pts = [(px, py), (px + width, py - length * 0.45), (px, py - length)]
        d.polygon(pts, fill=col)


def palm_trunk(d, x, y, s, col):
    d.rectangle([x - s * 0.18, y - 0.1 * s, x + s * 0.18, y + 0.1 * s], fill=col)


def ficus(d, cx, cy, s, col):
    d.ellipse([cx - 0.9 * s, cy - 2.0 * s, cx + 0.9 * s, cy + 0.2 * s], fill=col)
    for i, (ox, oy) in enumerate(((0.15, 0.2), (-0.25, 0.15), (0.05, -0.35), (-0.35, -0.25))):
        d.ellipse([cx + ox * s - 0.25 * s, cy + oy * s - 0.35 * s, cx + ox * s + 0.25 * s, cy + oy * s + 0.1 * s],
                  fill=A.shade(col, 0.85))
    d.ellipse([cx - 0.5 * s, cy + 0.05 * s, cx + 0.5 * s, cy + 0.45 * s], fill=(60, 80, 55, 255))


def shrub(d, x, y, s, col):
    d.ellipse([x - 0.5 * s, y - 0.55 * s, x + 0.5 * s, y + 0.25 * s], fill=col)
    d.ellipse([x - 0.25 * s, y - 0.3 * s, x + 0.35 * s, y + 0.45 * s], fill=A.shade(col, 0.9))


def bin_img(d, x, y, s, col):
    d.rectangle([x - 0.7 * s, y - 0.7 * s, x + 0.7 * s, y + 0.7 * s], fill=col)
    d.rectangle([x - 0.7 * s, y - 0.7 * s, x + 0.7 * s, y - 0.45 * s], fill=A.shade(col, 1.2))
    d.rectangle([x - 0.25 * s, y - 0.45 * s, x + 0.25 * s, y + 0.7 * s], fill=(70, 74, 70, 255))


def bench_img(d, x, y, s, col):
    d.rounded_rectangle([x - 0.8 * s, y, x + 0.8 * s, y + 0.12 * s], radius=0.06 * s, fill=col)
    for dx in (-0.55, -0.18, 0.18, 0.55):
        d.rectangle([x + dx * s, y - 0.32 * s, x + dx * s, y], fill=A.shade(col, 0.85))


def lamp_img(d, x, y, s, col):
    d.rectangle([x - 0.12 * s, y - 0.5 * s, x + 0.12 * s, y], fill=(34, 34, 36, 255))
    d.rectangle([x - 0.2 * s, y - 0.32 * s, x + 0.2 * s, y - 0.05 * s], fill=(52, 56, 60, 255))


def traffic_light(d, x, y, s):
    for i, c in enumerate(((236, 84, 70, 255), (240, 190, 66, 255), (74, 196, 106, 255))):
        d.rectangle([x - 0.35 * s, y - 0.55 * s + i * 0.55 * s, x + 0.35 * s, y - 0.45 * s + i * 0.55 * s],
                    fill=c)
    d.rectangle([x - 0.45 * s, y - 0.62 * s, x + 0.45 * s, y - 0.1 * s], fill=(20, 20, 22, 255))


def kiosk_img(d, x, y, s):
    d.rectangle([x - 0.45 * s, y - 0.75 * s, x + 0.45 * s, y - 0.12 * s], fill=(150, 152, 148, 255))
    d.rectangle([x - 0.3 * s, y - 0.85 * s, x + 0.3 * s, y - 0.72 * s], fill=(40, 46, 60, 255))


def wall_fixture(d, x, y, s):
    d.ellipse([x - 0.22 * s, y - 0.5 * s, x + 0.22 * s, y - 0.05 * s], fill=(120, 96, 64, 255))
    d.ellipse([x - 0.08 * s, y - 0.62 * s, x + 0.08 * s, y - 0.42 * s], fill=(150, 128, 96, 255))


def dish_img(d, x, y, s, col=None):
    col = col or (176, 178, 182, 255)
    d.rectangle([x - 0.55 * s, y - 0.45 * s, x + 0.55 * s, y - 0.15 * s], fill=(60, 62, 66, 255))
    d.ellipse([x - 0.38 * s, y - 0.35 * s, x + 0.38 * s, y - 0.12 * s], fill=(176, 178, 182, 255))
    d.ellipse([x - 0.2 * s, y - 0.25 * s, x + 0.2 * s, y - 0.1 * s], fill=(150, 152, 156, 255))
    d.line([(x, y - 0.1 * s), (x + 0.3 * s, y + 0.3 * s)], fill=(90, 92, 96, 255), width=3)


def ac_img(d, x, y, s, col=None):
    col = col or (172, 172, 168, 255)
    d.rectangle([x - 0.5 * s, y - 0.55 * s, x + 0.5 * s, y - 0.05 * s], fill=(172, 172, 168, 255))
    d.rectangle([x - 0.3 * s, y - 0.62 * s, x + 0.3 * s, y - 0.5 * s], fill=(146, 148, 144, 255))
    for i in range(4):
        d.line([(x - 0.28 * s, y - 0.45 * s + i * 0.15 * s), (x + 0.28 * s, y - 0.45 * s + i * 0.15 * s)],
               fill=(140, 140, 136, 255), width=2)


def tank_img(d, x, y, s):
    d.rectangle([x - 0.5 * s, y - 0.5 * s, x + 0.5 * s, y + 0.1 * s], fill=(128, 128, 122, 255))
    d.rectangle([x - 0.5 * s, y - 0.5 * s, x + 0.5 * s, y - 0.2 * s], fill=(108, 110, 106, 255))
    d.rectangle([x - 0.35 * s, y - 0.32 * s, x + 0.35 * s, y - 0.2 * s], fill=(88, 90, 88, 255))


SPRITES = ["palm_frond", "palm_trunk", "ficus", "shrub", "dish", "ac", "lamp", "bin",
           "bench", "bollard", "traffic_light", "kiosk", "wall_fixture", "tank", "pole", "plank"]


def build():
    img = Image.new("RGBA", (1024, 1024), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    palm_frond(d, 150, 800, 130, (46, 107, 58, 255))
    palm_trunk(d, 150, 880, 26, (107, 74, 46, 255))
    ficus(d, 620, 780, 170, (62, 108, 70, 255))
    shrub(d, 880, 880, 80, (46, 107, 58, 255))
    dish_img(d, 240, 150, 70, (176, 178, 182, 255))
    ac_img(d, 720, 150, 70, (172, 172, 168, 255))
    lamp_img(d, 900, 820, 60, (52, 56, 60, 255))
    bin_img(d, 80, 130, 60, (138, 140, 134, 255))
    bench_img(d, 820, 130, 70, (96, 110, 130, 255))
    d.line([(1000, 60), (1000, 900)], fill=(90, 92, 96, 255), width=8)
    bollard_img(d, 120, 900, 34, (120, 122, 118, 255))
    traffic_light(d, 480, 300, 66)
    kiosk_img(d, 240, 300, 52)
    wall_fixture(d, 830, 620, 46)
    tank_img(d, 620, 300, 60, (128, 128, 122, 255))
    pole_img(d, 900, 150, 90, (110, 112, 108, 255))
    plank_img(d, 150, 620, 80, (150, 86, 60, 255))
    return img, SPRITES


def bollard_img(d, x, y, s, col):
    d.rectangle([x - 0.3 * s, y - 0.3 * s, x + 0.3 * s, y + 0.3 * s], fill=col)


def pole_img(d, x, y, s, col):
    d.rectangle([x - 0.06 * s, y - 0.7 * s, x + 0.06 * s, y + 0.7 * s], fill=col)


def plank_img(d, x, y, s, col):
    d.rectangle([x - 0.6 * s, y, x + 0.6 * s, y + s * 0.7], fill=col)


def main():
    import json as _json
    sys.stdout.reconfigure(encoding="utf-8")
    out = sys.argv[1] if len(sys.argv) > 1 else "Assets/StreamingAssets/art"
    os.makedirs(out, exist_ok=True)
    img, names = build()
    A.save(img, os.path.join(out, "props.png"))
    rects = {}
    for i, n in enumerate(names):
        rects[n] = [ (i % 4) * 256, (i // 4) * 256, 256, 256 ]
    with open(os.path.join(out, "props.rects.json"), "w", encoding="utf-8") as fh:
        _json.dump({"texture": "props", "sprites": rects}, fh, indent=1)
    print("props atlas written to", out, "with", len(names), "sprites")


if __name__ == "__main__":
    main()
