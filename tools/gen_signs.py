"""Algerian road signage atlas (French/Arabic bilingual).

2048x2048, 4x4 cells of 512 px.  All text is baked with the Arabic shaper.
Run:  python tools/gen_signs.py [out_dir]
"""

import json
import os
import sys

from PIL import Image, ImageDraw, ImageFilter

import artlib as A

W = H = 2048
CELL = 512
RED = "#C8102E"
BLUE = "#0B4C8C"
GREEN = "#0F6B43"
YELLOW = "#E4B824"
WHITE = "#F4F2EC"
BLACK = "#202020"


def cell(i):
    return (i % 4) * CELL, (i // 4) * CELL


def disc(d, cx, cy, r, fill, outline=None, width=0):
    d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=fill, outline=outline, width=width)


def octagon(d, cx, cy, r, fill, outline, width):
    import math
    pts = [(cx + r * math.cos(math.pi / 8 + k * math.pi / 4),
            cy + r * math.sin(math.pi / 8 + k * math.pi / 4)) for k in range(8)]
    d.polygon(pts, fill=fill)
    d.line(pts + [pts[0]], fill=outline, width=width, joint="curve")


def triangle(d, cx, cy, r, fill, outline, width, down=False):
    s = -1 if down else 1
    pts = [(cx, cy + s * r), (cx - r * 0.95, cy - s * r * 0.72), (cx + r * 0.95, cy - s * r * 0.72)]
    d.polygon(pts, fill=fill)
    d.line(pts + [pts[0]], fill=outline, width=width, joint="curve")


def bus_pictogram(d, cx, cy, s, col):
    w, h = 2.2 * s, 1.6 * s
    d.rounded_rectangle([cx - w / 2, cy - h / 2, cx + w / 2, cy + h / 2], radius=0.22 * s, fill=col)
    # windscreen + windows cut out in the background colour
    d.rectangle([cx - w / 2 + 0.18 * s, cy - h / 2 + 0.2 * s,
                 cx - w / 2 + 0.62 * s, cy + 0.15 * s], fill=BLUE)
    d.rectangle([cx - w / 2 + 0.72 * s, cy - h / 2 + 0.2 * s,
                 cx + w / 2 - 0.14 * s, cy + 0.15 * s], fill=BLUE)
    # wheels
    for dx in (-0.62, 0.62):
        disc(d, cx + dx * s, cy + h / 2 - 0.06 * s, 0.22 * s, BLACK)
        disc(d, cx + dx * s, cy + h / 2 - 0.06 * s, 0.10 * s, "#9A9A96")


def pedestrian_pictogram(d, cx, cy, s, col):
    disc(d, cx, cy - 1.05 * s, 0.30 * s, col)
    d.polygon([(cx - 0.30 * s, cy - 0.62 * s), (cx + 0.30 * s, cy - 0.62 * s),
               (cx + 0.18 * s, cy + 0.15 * s), (cx - 0.16 * s, cy + 0.15 * s)], fill=col)
    d.line([(cx - 0.05 * s, cy + 0.10 * s), (cx - 0.55 * s, cy + 1.05 * s)], fill=col,
           width=int(0.22 * s))
    d.line([(cx + 0.05 * s, cy + 0.10 * s), (cx + 0.45 * s, cy + 0.75 * s)], fill=col,
           width=int(0.22 * s))
    d.line([(cx + 0.45 * s, cy + 0.75 * s), (cx + 0.40 * s, cy + 1.10 * s)], fill=col,
           width=int(0.20 * s))
    d.line([(cx - 0.30 * s, cy - 0.45 * s), (cx - 0.70 * s, cy + 0.05 * s)], fill=col,
           width=int(0.18 * s))


def pump_pictogram(d, cx, cy, s, col):
    d.rounded_rectangle([cx - 0.75 * s, cy - 1.0 * s, cx + 0.25 * s, cy + 1.0 * s],
                        radius=0.12 * s, fill=col)
    d.rectangle([cx - 0.55 * s, cy - 0.78 * s, cx + 0.05 * s, cy - 0.30 * s], fill=BLUE)
    d.line([(cx + 0.25 * s, cy - 0.55 * s), (cx + 0.72 * s, cy - 0.55 * s)], fill=col,
           width=int(0.16 * s))
    d.line([(cx + 0.72 * s, cy - 0.55 * s), (cx + 0.72 * s, cy + 0.35 * s)], fill=col,
           width=int(0.16 * s))
    d.line([(cx - 0.75 * s, cy + 1.0 * s), (cx + 0.35 * s, cy + 1.0 * s)], fill=col,
           width=int(0.16 * s))


def street_plate(d, x, y, french, arabic, wilaya="16", w=470, h=190):
    d.rounded_rectangle([x, y, x + w, y + h], radius=10, fill="#12305E")
    d.rounded_rectangle([x + 8, y + 8, x + w - 8, y + h - 8], radius=6,
                        outline=(236, 232, 220), width=4)
    f1 = A.fit_font(french, "arialbd.ttf", 42, w - 170)
    A.text(d, (x + (w - 96) / 2, y + 52), french, f1, (244, 242, 234), rtl=False)
    f2 = A.fit_font(arabic, "arialbd.ttf", 42, w - 170)
    A.text(d, (x + (w - 96) / 2, y + 120), arabic, f2, (244, 242, 234), rtl=True)
    # wilaya badge
    disc(d, x + w - 54, y + h / 2, 34, "#F4F2EC")
    f3 = A.font("arialbd.ttf", 34)
    A.text(d, (x + w - 54, y + h / 2), wilaya, f3, "#12305E")


def build():
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    rects = {}

    def add(name, i):
        x, y = cell(i)
        rects[name] = [x, y, CELL, CELL]
        return x, y

    # 0 — STOP
    x, y = add("sign_stop", 0)
    cx, cy = x + 256, y + 232
    octagon(d, cx, cy, 236, RED, WHITE, 16)
    f = A.font("arialbd.ttf", 118)
    A.text(d, (cx, cy - 18), "STOP", f, WHITE)
    f2 = A.font("arialbd.ttf", 62)
    A.text(d, (cx, cy + 92), "قف", f2, WHITE, rtl=True)

    # 1 — speed 50
    x, y = add("sign_speed50", 1)
    cx, cy = x + 256, y + 256
    disc(d, cx, cy, 236, WHITE, RED, 46)
    f = A.font("arialbd.ttf", 210)
    A.text(d, (cx, cy - 4), "50", f, BLACK)

    # 2 — no entry
    x, y = add("sign_noentry", 2)
    cx, cy = x + 256, y + 256
    disc(d, cx, cy, 236, RED)
    d.rounded_rectangle([cx - 150, cy - 44, cx + 150, cy + 44], radius=8, fill=WHITE)

    # 3 — yield
    x, y = add("sign_yield", 3)
    cx, cy = x + 256, y + 250
    triangle(d, cx, cy, 240, WHITE, RED, 44, down=True)

    # 4 — parking
    x, y = add("sign_parking", 4)
    cx, cy = x + 256, y + 256
    d.rounded_rectangle([cx - 200, cy - 200, cx + 200, cy + 200], radius=26, fill=BLUE,
                        outline=WHITE, width=12)
    f = A.font("arialbd.ttf", 300)
    A.text(d, (cx, cy - 6), "P", f, WHITE)

    # 5 — bus stop
    x, y = add("sign_busstop", 5)
    cx, cy = x + 256, y + 210
    d.rounded_rectangle([cx - 210, cy - 190, cx + 210, cy + 190], radius=26, fill=BLUE,
                        outline=WHITE, width=12)
    bus_pictogram(d, cx, cy - 30, 74, WHITE)
    f = A.fit_font("ARRÊT BUS", "arialbd.ttf", 52, 360)
    A.text(d, (cx, cy + 96), "ARRÊT BUS", f, WHITE)
    f2 = A.fit_font("محطة الحافلات", "arialbd.ttf", 42, 350)
    A.text(d, (cx, cy + 146), "محطة الحافلات", f2, WHITE, rtl=True)

    # 6 — pedestrian crossing
    x, y = add("sign_crossing", 6)
    cx, cy = x + 256, y + 256
    d.rounded_rectangle([cx - 210, cy - 210, cx + 210, cy + 210], radius=26, fill=BLUE,
                        outline=WHITE, width=12)
    for i in range(5):
        d.rectangle([cx - 170 + i * 68, cy + 96, cx - 130 + i * 68, cy + 178], fill=WHITE)
    pedestrian_pictogram(d, cx, cy - 30, 92, WHITE)

    # 7 — roundabout (ring of three arrows)
    x, y = add("sign_roundabout", 7)
    cx, cy = x + 256, y + 256
    triangle(d, cx, cy, 240, WHITE, RED, 40)
    import math
    ring = 88
    ry = cy + 22
    d.arc([cx - ring, ry - ring, cx + ring, ry + ring], 12, 348, fill=BLACK, width=22)
    for k in range(3):
        a = math.radians(k * 120 + 24)
        px, py = cx + ring * math.cos(a), ry + ring * math.sin(a)
        tx, ty = -math.sin(a), math.cos(a)
        s = 30
        p1 = (px + tx * s, py + ty * s)
        p2 = (px - tx * s * 0.35 - ty * s * 0.85, py - ty * s * 0.35 + tx * s * 0.85)
        p3 = (px - tx * s * 0.35 + ty * s * 0.85, py - ty * s * 0.35 - tx * s * 0.85)
        d.polygon([p1, p2, p3], fill=BLACK)

    # 8 — hospital
    x, y = add("sign_hospital", 8)
    cx, cy = x + 256, y + 256
    d.rounded_rectangle([cx - 200, cy - 200, cx + 200, cy + 200], radius=26, fill="#1E6BB0",
                        outline=WHITE, width=12)
    d.rectangle([cx - 46, cy - 140, cx + 46, cy + 140], fill=WHITE)
    d.rectangle([cx - 140, cy - 46, cx + 140, cy + 46], fill=WHITE)

    # 9 — fuel
    x, y = add("sign_fuel", 9)
    cx, cy = x + 256, y + 250
    d.rounded_rectangle([cx - 200, cy - 200, cx + 200, cy + 200], radius=26, fill="#1E6BB0",
                        outline=WHITE, width=12)
    pump_pictogram(d, cx, cy - 10, 96, WHITE)

    # 10 — police
    x, y = add("sign_police", 10)
    d.rounded_rectangle([x + 24, y + 150, x + 488, y + 362], radius=14, fill="#F4F2EC",
                        outline="#1A2A44", width=8)
    f = A.font("arialbd.ttf", 76)
    A.text(d, (x + 256, y + 208), "POLICE", f, "#1A2A44")
    f2 = A.font("arialbd.ttf", 52)
    A.text(d, (x + 256, y + 296), "الشرطة", f2, "#1A2A44", rtl=True)

    # 11 — no parking
    x, y = add("sign_noparking", 11)
    cx, cy = x + 256, y + 256
    disc(d, cx, cy, 236, BLUE, RED, 44)
    f = A.font("arialbd.ttf", 230)
    A.text(d, (cx, cy - 6), "P", f, WHITE)
    d.line([(cx - 168, cy + 168), (cx + 168, cy - 168)], fill=RED, width=44)

    # 12/13/15 — street plates
    x, y = add("plate_didouche", 12)
    street_plate(d, x + 20, y + 150, "RUE DIDOUCHE MOURAD", "شارع ديدوش مراد")
    x, y = add("plate_zighoud", 13)
    street_plate(d, x + 20, y + 150, "BD ZIGHOUD YOUCEF", "شارع زيغود يوسف")
    x, y = add("plate_martyrs", 14)
    street_plate(d, x + 20, y + 150, "PLACE DES MARTYRS", "ساحة الشهداء")
    x, y = add("plate_babeloued", 15)
    street_plate(d, x + 20, y + 150, "BAB EL OUED", "باب الوادي")

    # 14 already used above — put the gantry in its own extra cell row
    # (we reuse the same atlas with a 4x4 grid, so gantry goes in slot 14 region)

    return img, rects


def build_gantry():
    """Direction gantry, 512x256, kept as a separate small texture."""
    img = Image.new("RGBA", (512, 256), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([6, 6, 506, 250], radius=14, fill=GREEN)
    d.rounded_rectangle([16, 16, 496, 240], radius=8, outline=(240, 238, 230), width=6)

    f1 = A.fit_font("ALGER CENTRE", "arialbd.ttf", 56, 300)
    A.text(d, (256, 92), "ALGER CENTRE", f1, (244, 242, 234), rtl=False)
    f2 = A.fit_font("الجزائر الوسطى", "arialbd.ttf", 48, 300)
    A.text(d, (256, 168), "الجزائر الوسطى", f2, (244, 242, 234), rtl=True)

    d.line([(410, 210), (470, 210)], fill=(244, 242, 234), width=10)
    d.polygon([(492, 210), (452, 188), (452, 232)], fill=(244, 242, 234))
    return img


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    out = sys.argv[1] if len(sys.argv) > 1 else "Assets/StreamingAssets/art"
    os.makedirs(out, exist_ok=True)

    img, rects = build()
    A.save(img, os.path.join(out, "signs.png"))
    A.save(build_gantry(), os.path.join(out, "sign_gantry.png"))

    with open(os.path.join(out, "signs.rects.json"), "w", encoding="utf-8") as fh:
        json.dump({"texture": "signs", "sprites": rects}, fh, indent=1)

    prev = os.path.join("tools", "_preview")
    os.makedirs(prev, exist_ok=True)
    img.resize((768, 768), Image.LANCZOS).save(os.path.join(prev, "signs.png"))
    print("sign atlas written to", out, "with", len(rects), "sprites")


if __name__ == "__main__":
    main()
