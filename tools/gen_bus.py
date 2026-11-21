"""Bus exterior/interior atlases for the Algerian urban bus.

Generated from explicit rect lists (geometry-faithful, no eyeballing required).

  bus_urban_side   2048x1024  -> 12.0m x 3.0m (roof, windshield, doors, stripes)
  bus_urban_front  1024x1024  -> windshield + fascia + grille + destination board
  bus_urban_rear   1024x1024  -> tail lights + plate + sign band
  bus_interior     1024x1024  -> driver area, seats, floor, ceiling, doors
  bus_plate        1024x1024  -> vehicle registration plate

Run:  python tools/gen_bus.py [out_dir]
"""

import json
import os
import sys

from PIL import Image, ImageDraw

import artlib as A

T = (255, 255, 255, 255)


def side_texture():
    """2048x1024 = 12.0m (x) x 3.0m (y).  Door side: front door + rear door."""
    img = A.vgrad(2048, 1024, (247, 245, 238, 255), (239, 237, 230, 255), gamma=1.15)
    img = A.overlay(img, (0, 0, 0, 0), A.fbm(1024, 2048, base=5, octaves=3, seed=3), 0.04)

    d = ImageDraw.Draw(img, "RGBA")

    # roof + rooflight
    d.rectangle([0, 0, 2048, 40], fill=A.rgb("#EFEDE6") + (255,))
    d.rectangle([60, 8, 200, 32], fill=(206, 210, 216, 255))

    # body band and skirt
    d.rectangle([0, 40, 2048, 960], fill=A.rgb("#EFEDE6") + (255,))
    for ax0, ax1 in ((190, 430), (1558, 1798)):
        d.rectangle([ax0, 830, ax1, 1024], fill=(116, 118, 114, 255))

    # front fascia + windshield + pillars
    d.rectangle([0, 40, 340, 900], fill=A.rgb("#EFEDE6") + (255,))
    d.rectangle([0, 560, 340, 640], fill=(30, 36, 44, 255))
    for i in range(7):
        d.line([(0, 578 + i * 10), (340, 578 + i * 10)], fill=(122, 132, 148, 255), width=4)
    d.rectangle([20, 60, 320, 250], fill=(96, 120, 136, 255))
    d.rectangle([0, 40, 60, 900], fill=A.rgb("#EFEDE6") + (255,))
    d.rectangle([280, 40, 340, 900], fill=A.rgb("#EFEDE6") + (255,))

    # front door (blue) + driver window behind it
    d.rectangle([430, 50, 700, 900], fill=(27, 95, 168, 255))
    d.rectangle([432, 60, 698, 890], fill=(31, 102, 177, 255))
    d.line([(570, 50), (570, 900), (700, 900), (700, 50)], fill=(16, 60, 118, 255))
    d.line([(432, 200), (698, 200)], fill=(40, 88, 154, 255))
    d.line([(432, 420), (698, 420)], fill=(40, 88, 154, 255))
    d.rectangle([700, 60, 920, 900], fill=(96, 120, 136, 255))
    d.rectangle([700, 320, 920, 900], fill=A.rgb("#EFEDE6") + (255,))
    d.line([(710, 60), (910, 900)], fill=A.rgb("#EFEDE6") + (255,))

    # side window (upper band) + lower painted band
    d.rectangle([940, 60, 1280, 300], fill=(96, 120, 136, 255))
    d.rectangle([940, 300, 1280, 900], fill=A.rgb("#EFEDE6") + (255,))

    # rear door (blue)
    d.rectangle([1460, 50, 1740, 900], fill=(27, 95, 168, 255))
    d.rectangle([1462, 60, 1738, 890], fill=(31, 102, 177, 255))
    d.line([(1620, 50), (1620, 900), (1740, 900), (1740, 50)], fill=(16, 60, 118, 255))
    d.line([(1462, 200), (1738, 200)], fill=(40, 88, 154, 255))
    d.line([(1462, 420), (1738, 420)], fill=(40, 88, 154, 255))

    # livery stripes
    d.rectangle([0, 560, 2048, 640], fill=(0, 99, 51, 255))
    d.rectangle([0, 640, 2048, 720], fill=(27, 95, 168, 255))

    # brand band on the front door
    d.rectangle([430, 50, 700, 190], fill=(244, 242, 234, 255))
    f = A.font("arialbd.ttf", 92)
    A.text(d, (565, 120), "ALGERBUS", f, (20, 40, 70, 255))

    return img.convert("RGB")


def front_texture():
    img = Image.new("RGBA", (1024, 1024), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, 380, 900], fill=A.rgb("#EFEDE6") + (255,))
    d.rectangle([0, 560, 380, 640], fill=(30, 36, 44, 255))
    for i in range(7):
        d.line([(0, 578 + i * 10), (380, 578 + i * 10)], fill=(122, 132, 148, 255), width=4)
    d.rectangle([20, 60, 360, 380], fill=(96, 120, 136, 255))
    d.rectangle([0, 40, 60, 900], fill=A.rgb("#EFEDE6") + (255,))
    d.rectangle([300, 40, 360, 900], fill=A.rgb("#EFEDE6") + (255,))
    d.rectangle([0, 820, 380, 900], fill=(116, 118, 114, 255))
    d.ellipse([70, 420, 150, 500], fill=(252, 244, 214, 255), outline=(200, 190, 150, 255), width=6)
    d.rectangle([0, 260, 380, 340], fill=(16, 42, 84, 255))
    f = A.fit_font("ALGER BUS", "arialbd.ttf", 52, 250)
    A.text(d, (190, 302), "ALGER BUS", f, (244, 242, 234, 255), rtl=False)
    f2 = A.fit_font("الجزائر", "arialbd.ttf", 44, 250)
    A.text(d, (190, 312), "الجزائر", f2, (244, 242, 234, 255), rtl=True)
    return img.convert("RGB")


def rear_texture():
    img = Image.new("RGBA", (1024, 1024), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, 1024, 1024], fill=A.rgb("#EFEDE6") + (255,))
    d.rectangle([0, 420, 1024, 470], fill=(180, 24, 30, 255))
    for i in range(6):
        x = 60 + i * 160
        d.ellipse([x - 24, 432, x + 24, 456], fill=(255, 60, 54, 255))
    d.rectangle([60, 560, 300, 760], fill=(244, 242, 234, 255), outline=(70, 70, 72, 255), width=6)
    f = A.fit_font("16-1234-119", "arialbd.ttf", 40, 180)
    A.text(d, (180, 660), "16-1234-119", f, (20, 20, 24, 255), rtl=False)
    d.rectangle([0, 840, 1024, 1024], fill=(12, 60, 108, 255))
    f2 = A.font("arialbd.ttf", 40)
    A.text(d, (512, 930), "DZ 1234 DZ", f2, (244, 242, 234, 255), rtl=False)
    return img.convert("RGB")


def interior_texture():
    img = Image.new("RGBA", (1024, 1024), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, 1024, 1024], fill=(150, 132, 106, 255))
    d.rectangle([0, 0, 1024, 480], fill=(70, 96, 128, 255))
    d.ellipse([430, 90, 610, 270], fill=(230, 226, 214, 255))
    d.line([(430, 270), (610, 90)], fill=(30, 40, 56, 255), width=10)
    d.rectangle([250, 400, 360, 480], fill=(110, 126, 146, 255))
    for r in range(5):
        y0 = 520 + r * 150
        for s in range(2):
            sx = 60 + s * 200
            d.rectangle([sx, y0 + 40, sx + 150, y0 + 150], fill=(64, 84, 110, 255))
            d.rectangle([sx - 6, y0, sx + 156, y0 + 46], fill=(48, 64, 92, 255))
    d.rectangle([0, 860, 1024, 1024], fill=(96, 102, 110, 255))
    d.rectangle([0, 700, 1024, 860], fill=(70, 86, 104, 255))
    d.rectangle([0, 700, 1024, 860], fill=(70, 86, 104, 255), outline=(40, 50, 64, 255), width=8)
    return img.convert("RGB")


def plate_texture():
    img = Image.new("RGBA", (1024, 1024), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, 1024, 1024], fill=(244, 242, 234, 255), outline=(30, 30, 34, 255), width=10)
    f = A.fit_font("16-1234-119", "arialbd.ttf", 72, 800)
    A.text(d, (512, 360), "16-1234-119", f, (20, 20, 24, 255), rtl=False)
    d.rectangle([180, 520, 860, 700], fill=(30, 60, 120, 255))
    f2 = A.fit_font("DZR-2026", "arialbd.ttf", 50, 560)
    A.text(d, (512, 610), "DZR-2026", f2, (244, 242, 234, 255), rtl=False)
    return img.convert("RGB")


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    out = sys.argv[1] if len(sys.argv) > 1 else "Assets/StreamingAssets/art"
    os.makedirs(out, exist_ok=True)
    for name, fn in (("bus_urban_side", side_texture), ("bus_urban_front", front_texture),
                     ("bus_urban_rear", rear_texture), ("bus_interior", interior_texture),
                     ("bus_plate", plate_texture)):
        A.save(fn(), os.path.join(out, name + ".png"))

    rects = {
        "bus_urban_side": [0, 0, 2048, 1024, "side"],
        "bus_urban_front": [0, 0, 1024, 1024, "front"],
        "bus_urban_rear": [0, 0, 1024, 1024, "rear"],
        "bus_interior": [0, 0, 1024, 1024, "interior"],
        "bus_plate": [0, 0, 1024, 1024, "plate"],
    }
    with open(os.path.join(out, "bus.rects.json"), "w", encoding="utf-8") as fh:
        json.dump({"texture": "bus", "sprites": rects}, fh, indent=1)
    print("bus atlases written to", out)


if __name__ == "__main__":
    main()
