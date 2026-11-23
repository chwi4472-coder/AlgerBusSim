"""Generates every original texture used by AlgerBusSim.

No third-party art is embedded: everything here is drawn from primitives
and open fonts (Barlow Semi Condensed, Noto Naskh Arabic - both OFL).
Run:  python tools/make_art.py
"""
import math
import os
import random

import numpy as np
from PIL import Image, ImageDraw, ImageFont
import arabic_reshaper
from bidi.algorithm import get_display

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ART = os.path.join(ROOT, "Assets", "StreamingAssets", "art")
FONTS = os.path.join(ROOT, "Assets", "Resources", "Fonts")

LATIN_BOLD = os.path.join(FONTS, "BarlowSemiCondensed-Bold.ttf")
LATIN_SEMI = os.path.join(FONTS, "BarlowSemiCondensed-SemiBold.ttf")
LATIN_REG = os.path.join(FONTS, "BarlowSemiCondensed-Regular.ttf")
ARABIC = os.path.join(FONTS, "NotoNaskhArabic.ttf")

NAVY = (16, 52, 104, 255)
BLUE = (27, 95, 168, 255)
GREEN = (0, 122, 61, 255)
RED = (206, 17, 38, 255)
WHITE = (255, 255, 255, 255)
AMBER = (255, 180, 58, 255)


def font(path, size):
    return ImageFont.truetype(path, size)


def ar(text):
    return get_display(arabic_reshaper.reshape(text))


def save(img, name):
    path = os.path.join(ART, name)
    img.save(path, optimize=True)
    print("wrote", name, img.size)


def decal_algerbus():
    W, H = 1536, 256
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.text((40, 22), "ALGERBUS", font=font(LATIN_BOLD, 142), fill=NAVY)
    d.rectangle([44, 192, 480, 204], fill=GREEN)
    d.rectangle([44, 210, 480, 216], fill=RED)
    d.text((660, 42), ar("حافلة الجزائر"), font=font(ARABIC, 82), fill=NAVY)
    d.text((664, 154), "ALGER  ·  DZ", font=font(LATIN_SEMI, 50), fill=BLUE)
    save(img, "decal_algerbus.png")


def bus_destination():
    img = Image.new("RGBA", (1024, 256), (8, 9, 10, 255))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([8, 8, 1016, 248], radius=14, outline=(40, 40, 40, 255), width=4)
    d.text((38, 24), "M0", font=font(LATIN_BOLD, 150), fill=AMBER)
    d.text((250, 38), "EL HARRACH", font=font(LATIN_SEMI, 104), fill=AMBER)
    d.text((252, 152), ar("الحراش"), font=font(ARABIC, 62), fill=(255, 205, 120, 255))
    # LED matrix grid
    grid = Image.new("RGBA", img.size, (0, 0, 0, 0))
    gd = ImageDraw.Draw(grid)
    for y in range(0, 256, 4):
        gd.line([(0, y), (1024, y)], fill=(0, 0, 0, 58))
    for x in range(0, 1024, 4):
        gd.line([(x, 0), (x, 256)], fill=(0, 0, 0, 32))
    img = Image.alpha_composite(img, grid)
    save(img, "bus_dest_m0.png")


def bus_plate():
    img = Image.new("RGBA", (512, 128), (238, 238, 232, 255))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, 511, 127], outline=(30, 30, 30, 255), width=6)
    d.rectangle([8, 8, 503, 119], outline=(30, 30, 30, 255), width=2)
    d.text((36, 12), "4821-118-16", font=font(LATIN_BOLD, 82), fill=(24, 24, 24, 255))
    d.text((404, 8), "DZ", font=font(LATIN_BOLD, 26), fill=(24, 24, 24, 255))
    save(img, "bus_plate_dz.png")


def bus_icon(d, x, y, w, h, color, inner=NAVY):
    d.rounded_rectangle([x, y + h * 0.18, x + w, y + h], radius=int(w * 0.16), fill=color)
    d.rounded_rectangle([x + w * 0.10, y + h * 0.30, x + w * 0.90, y + h * 0.56],
                        radius=int(w * 0.05), fill=inner)
    d.ellipse([x + w * 0.16, y + h * 0.80, x + w * 0.34, y + h * 0.98], fill=inner)
    d.ellipse([x + w * 0.66, y + h * 0.80, x + w * 0.84, y + h * 0.98], fill=inner)
    d.rectangle([x + w * 0.46, y, x + w * 0.54, y + h * 0.18], fill=color)


def stop_panel():
    W, H = 512, 1024
    img = Image.new("RGBA", (W, H), (250, 250, 247, 255))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([0, 0, W - 1, H - 1], radius=22, fill=(250, 250, 247, 255),
                        outline=(210, 210, 204, 255), width=3)
    d.rounded_rectangle([0, 0, W - 1, 236], radius=22, fill=NAVY)
    d.rectangle([0, 190, W - 1, 236], fill=NAVY)
    # bus pictogram
    bus_icon(d, 52, 46, 120, 150, WHITE)
    d.text((190, 52), "ARRÊT", font=font(LATIN_BOLD, 62), fill=WHITE)
    d.text((190, 118), "DE BUS", font=font(LATIN_BOLD, 62), fill=WHITE)
    d.text((40, 268), ar("محطة الحافلات"), font=font(ARABIC, 78), fill=NAVY)
    d.line([(40, 372), (472, 372)], fill=(200, 200, 196, 255), width=3)
    d.text((40, 396), "LIGNE  M0", font=font(LATIN_BOLD, 54), fill=BLUE)
    d.text((40, 466), "BAB EL OUED", font=font(LATIN_SEMI, 44), fill=(40, 40, 40, 255))
    d.text((40, 516), "EL HARRACH", font=font(LATIN_SEMI, 44), fill=(40, 40, 40, 255))
    d.text((40, 590), ar("باب الوادي"), font=font(ARABIC, 50), fill=(40, 40, 40, 255))
    d.line([(206, 628), (300, 628)], fill=(90, 90, 90, 255), width=5)
    d.polygon([(292, 618), (312, 628), (292, 638)], fill=(90, 90, 90, 255))
    d.text((330, 590), ar("الحراش"), font=font(ARABIC, 50), fill=(40, 40, 40, 255))
    d.line([(40, 672), (472, 672)], fill=(200, 200, 196, 255), width=3)
    d.rounded_rectangle([40, 700, 472, 806], radius=12, fill=(238, 242, 236, 255))
    d.text((62, 712), "TICKET 30 DA", font=font(LATIN_BOLD, 48), fill=GREEN)
    d.text((62, 764), ar("التذكرة 30 دج"), font=font(ARABIC, 36), fill=GREEN)
    d.text((40, 852), "ALGERBUS", font=font(LATIN_BOLD, 58), fill=NAVY)
    d.text((40, 922), ar("حافلة الجزائر"), font=font(ARABIC, 42), fill=BLUE)
    save(img, "stop_panel.png")


def icon_sheet():
    S = 128
    img = Image.new("RGBA", (S * 8, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    def at(i):
        return i * S

    # 0 door
    x = at(0)
    d.rounded_rectangle([x + 26, 18, x + 102, 110], radius=8, outline=WHITE, width=8)
    d.line([(x + 64, 18), (x + 64, 110)], fill=WHITE, width=8)
    d.ellipse([x + 48, 60, x + 58, 70], fill=WHITE)
    d.ellipse([x + 70, 60, x + 80, 70], fill=WHITE)
    # 1 horn
    x = at(1)
    d.polygon([(x + 26, 52), (x + 58, 40), (x + 58, 88), (x + 26, 76)], fill=WHITE)
    d.rounded_rectangle([x + 18, 52, x + 30, 76], radius=4, fill=WHITE)
    for r in (24, 38):
        d.arc([x + 54 - r, 64 - r, x + 54 + r, 64 + r], -50, 50, fill=WHITE, width=8)
    # 2 camera
    x = at(2)
    d.rounded_rectangle([x + 18, 42, x + 110, 96], radius=10, outline=WHITE, width=8)
    d.rectangle([x + 44, 30, x + 74, 44], fill=WHITE)
    d.ellipse([x + 50, 54, x + 86, 90], outline=WHITE, width=8)
    d.ellipse([x + 60, 64, x + 76, 80], fill=WHITE)
    # 3 pause
    x = at(3)
    d.rounded_rectangle([x + 34, 24, x + 54, 104], radius=6, fill=WHITE)
    d.rounded_rectangle([x + 74, 24, x + 94, 104], radius=6, fill=WHITE)
    # 4 fuel
    x = at(4)
    d.rounded_rectangle([x + 24, 22, x + 70, 108], radius=6, outline=WHITE, width=8)
    d.rectangle([x + 34, 34, x + 60, 58], fill=WHITE)
    d.line([(x + 70, 44), (x + 92, 44), (x + 100, 56), (x + 100, 88)], fill=WHITE, width=8)
    d.ellipse([x + 92, 84, x + 108, 100], fill=WHITE)
    d.line([(x + 40, 108), (x + 54, 108)], fill=WHITE, width=8)
    # 5 coin
    x = at(5)
    d.ellipse([x + 18, 18, x + 110, 110], outline=WHITE, width=9)
    d.text((x + 44, 26), "D", font=font(LATIN_BOLD, 66), fill=WHITE)
    # 6 person
    x = at(6)
    d.ellipse([x + 48, 18, x + 80, 50], fill=WHITE)
    d.rounded_rectangle([x + 34, 58, x + 94, 108], radius=24, fill=WHITE)
    # 7 clock
    x = at(7)
    d.ellipse([x + 18, 18, x + 110, 110], outline=WHITE, width=9)
    d.line([(x + 64, 64), (x + 64, 38)], fill=WHITE, width=8)
    d.line([(x + 64, 64), (x + 84, 74)], fill=WHITE, width=8)
    save(img, "ui_icons.png")


def ui_round():
    img = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([2, 2, 125, 125], radius=28, fill=(255, 255, 255, 255))
    save(img, "ui_round.png")


def logo():
    img = Image.new("RGBA", (1024, 360), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([28, 40, 168, 300], radius=26, fill=(255, 255, 255, 235))
    bus_icon(d, 52, 82, 92, 150, NAVY, inner=(255, 255, 255, 235))
    d.text((196, 24), "ALGERBUS", font=font(LATIN_BOLD, 128), fill=WHITE)
    d.text((202, 168), ar("حافلة الجزائر"), font=font(ARABIC, 74), fill=(255, 255, 255, 240))
    d.rectangle([202, 276, 600, 288], fill=GREEN)
    d.rectangle([202, 296, 600, 302], fill=RED)
    d.text((202, 310), "SIMULATEUR DE BUS  ·  ALGER", font=font(LATIN_SEMI, 34),
           fill=(235, 240, 248, 230))
    save(img, "logo_algerbus.png")


def noise_texture(name, base, amp, size=512, waves=False, seed=7):
    rng = np.random.default_rng(seed)
    yy, xx = np.mgrid[0:size, 0:size].astype(np.float32)
    n = rng.normal(0.0, 1.0, (size, size))
    # smooth the noise a little
    k = 5
    n = np.apply_along_axis(lambda r: np.convolve(r, np.ones(k) / k, mode="same"), 0, n)
    n = np.apply_along_axis(lambda r: np.convolve(r, np.ones(k) / k, mode="same"), 1, n)
    n = n / (np.abs(n).max() + 1e-6)
    if waves:
        w = 0.55 + 0.45 * np.sin(xx / size * math.pi * 6 + np.sin(yy / size * math.pi * 2) * 1.4)
        w = 0.72 + 0.28 * w
    else:
        w = 1.0
    img = np.zeros((size, size, 4), dtype=np.float32)
    for c in range(3):
        img[:, :, c] = np.clip(base[c] * w + n * amp, 0, 255)
    img[:, :, 3] = 255
    out = Image.fromarray(img.astype(np.uint8), "RGBA")
    save(out, name)


def flag_dz():
    W, H = 384, 256
    img = Image.new("RGBA", (W, H), (255, 255, 255, 255))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, W // 2, H], fill=(0, 98, 51, 255))
    # crescent built as a mask so the two flag fields stay clean
    mask = Image.new("L", (W, H), 0)
    md = ImageDraw.Draw(mask)
    md.ellipse([120, 52, 292, 204], fill=255)
    md.ellipse([160, 40, 340, 216], fill=0)
    red = Image.new("RGBA", (W, H), (206, 17, 38, 255))
    img.paste(red, (0, 0), mask)
    # five-point star
    cx, cy, r = 232, 128, 34
    pts = []
    for i in range(10):
        a = -math.pi / 2 + i * math.pi / 5
        rr = r if i % 2 == 0 else r * 0.42
        pts.append((cx + math.cos(a) * rr, cy + math.sin(a) * rr))
    d.polygon(pts, fill=(206, 17, 38, 255))
    save(img, "flag_dz.png")


def plaque_grande_poste():
    W, H = 1024, 256
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([0, 0, W - 1, H - 1], radius=10, fill=(238, 232, 216, 255),
                        outline=(120, 100, 70, 255), width=6)
    d.text((48, 34), "GRANDE POSTE", font=font(LATIN_BOLD, 92), fill=NAVY)
    d.text((52, 146), ar("البريد المركزي"), font=font(ARABIC, 70), fill=NAVY)
    d.text((760, 190), "ALGER", font=font(LATIN_SEMI, 40), fill=(120, 90, 50, 255))
    save(img, "plaque_grande_poste.png")


def main():
    os.makedirs(ART, exist_ok=True)
    random.seed(11)
    decal_algerbus()
    bus_destination()
    bus_plate()
    stop_panel()
    icon_sheet()
    ui_round()
    logo()
    flag_dz()
    plaque_grande_poste()
    noise_texture("ground_sand.png", (196, 174, 138), 16, seed=3)
    noise_texture("water.png", (24, 78, 96), 9, waves=True, seed=5)


if __name__ == "__main__":
    main()
