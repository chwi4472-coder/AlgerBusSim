"""Generates the district facade tiles, shopfronts and roofs.

Each wall tile represents BAYS x FLOORS building modules and tiles seamlessly
in both axes, so a building of any size is a box with UV repeat.  This is the
mobile-friendly approach: high pixel density per square metre, almost no
triangles.

Run:  python tools/gen_facades.py [out_dir]
"""

import os
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

import artlib as A

SIZE = 1024
BAYS = 4
FLOORS = 4
BAY = SIZE // BAYS
FLOOR = SIZE // FLOORS


DISTRICTS = {
    "babeloued": dict(
        label="Bab El Oued", kind="popular",
        wall="#E9E3D5", wall2="#D8CFBD",
        trim="#F5F2EA", frame="#2E6DA8", shutter="#2E6DA8",
        glass_top="#6C8296", glass_bot="#232E38",
        balcony=0.55, shutter_p=0.80, laundry=0.55, dish=0.55, ac=0.35,
        stone=False, cables=4, rust=0.30, window_h=140, window_w=104,
        sill_h=12, french=False, bars=0.25,
    ),
    "centre": dict(
        label="Alger Centre", kind="colonial",
        wall="#F0EDE4", wall2="#DED9CC",
        trim="#FAF8F2", frame="#4A4A48", shutter="#E8E6DE",
        glass_top="#7A8C9C", glass_bot="#2A333B",
        balcony=0.85, shutter_p=0.55, laundry=0.10, dish=0.20, ac=0.15,
        stone=True, cables=1, rust=0.05, window_h=196, window_w=112,
        sill_h=16, french=True, bars=0.55,
    ),
    "hydra": dict(
        label="Hydra", kind="affluent",
        wall="#F6F3EC", wall2="#E6E1D4",
        trim="#FFFFFF", frame="#3B3B39", shutter="#F2EFE6",
        glass_top="#5E7A8E", glass_bot="#1E2830",
        balcony=0.70, shutter_p=0.25, laundry=0.02, dish=0.10, ac=0.30,
        stone=False, cables=0, rust=0.02, window_h=172, window_w=128,
        sill_h=14, french=True, bars=0.35,
    ),
    "elharrach": dict(
        label="El Harrach", kind="industrial",
        wall="#CFC6B4", wall2="#B3A894",
        trim="#DCD4C4", frame="#6B6257", shutter="#8A7F6D",
        glass_top="#5A6470", glass_bot="#20262C",
        balcony=0.15, shutter_p=0.35, laundry=0.15, dish=0.35, ac=0.45,
        stone=False, cables=5, rust=0.55, window_h=112, window_w=96,
        sill_h=8, french=False, bars=0.70,
    ),
    "kouba": dict(
        label="Kouba", kind="residential",
        wall="#E4DCC8", wall2="#CFC5AC",
        trim="#EDE7D6", frame="#4E5A63", shutter="#5F7B8C",
        glass_top="#64798C", glass_bot="#232C34",
        balcony=0.65, shutter_p=0.45, laundry=0.40, dish=0.45, ac=0.30,
        stone=False, cables=3, rust=0.20, window_h=150, window_w=108,
        sill_h=10, french=False, bars=0.40,
    ),
}

SHOP_SIGNS = {
    "babeloued": [
        ("ÉPICERIE EL BARAKA", "بقالة البركة", "#C0392B"),
        ("CAFÉ DES SPORTS", "مقهى الرياضة", "#1F6E43"),
        ("BOULANGERIE", "مخبزة", "#B8860B"),
        ("TAXI PHONE", "هاتف", "#2C3E50"),
    ],
    "centre": [
        ("PHARMACIE", "صيدلية", "#1E7A46"),
        ("LIBRAIRIE", "مكتبة", "#2C4A7A"),
        ("TABAC PRESSE", "تبغ وصحافة", "#8E2B2B"),
        ("AGENCE IMMOBILIÈRE", "وكالة عقارية", "#33475B"),
    ],
    "hydra": [
        ("BANQUE", "بنك", "#1B4E7A"),
        ("GALERIE", "معرض", "#4A4A48"),
        ("RESTAURANT", "مطعم", "#7A3B2E"),
        ("CABINET MÉDICAL", "عيادة", "#2F6B5A"),
    ],
    "elharrach": [
        ("ATELIER", "ورشة", "#6B5B3E"),
        ("PIÈCES AUTO", "قطع غيار", "#3E4A55"),
        ("DÉPÔT", "مستودع", "#5A4A3A"),
        ("QUINCAILLERIE", "خردوات", "#6E5236"),
    ],
    "kouba": [
        ("ALIMENTATION GÉNÉRALE", "مواد غذائية", "#B03A2E"),
        ("SALON DE THÉ", "صالون شاي", "#2E7D5B"),
        ("COIFFEUR", "حلاق", "#33475B"),
        ("PÂTISSERIE", "حلويات", "#A0642A"),
    ],
}

AWNINGS = ["#B33A32", "#2E6E4E", "#2C5C8A", "#C08A2E", "#7A4A8A"]


# ------------------------------------------------------------------ walls ----

def wall_base(cfg, seed):
    rng = np.random.default_rng(seed)
    mott = A.fbm(SIZE, SIZE, base=7, octaves=5, seed=seed, gain=0.55)
    base = np.array(A.rgb(cfg["wall"]), np.float32) / 255.0
    alt = np.array(A.rgb(cfg["wall2"]), np.float32) / 255.0
    t = (mott ** 1.4)[..., None]
    arr = base[None, None, :] * (1 - t) + alt[None, None, :] * t

    img = A.to_img(arr)
    img = A.grain(img, 0.012, seed + 1)

    # rain streaks
    st = A.streak(SIZE, SIZE, seed=seed + 2, count=22)
    img = A.overlay(img, "#4A4238", A.blur_mask(st, 1.2), 0.10 * (1 if cfg["kind"] != "affluent" else 0.35))

    # rust / damp blotches
    if cfg["rust"] > 0:
        blot = A.fbm(SIZE, SIZE, base=3, octaves=4, seed=seed + 3)
        blot = np.clip((blot - 0.62) * 3.2, 0, 1)
        img = A.overlay(img, "#8A5A32", A.blur_mask(blot, 6), cfg["rust"])

    if cfg["stone"]:
        img = stone_courses(img, cfg, seed + 4)

    return img.convert("RGBA")


def stone_courses(img, cfg, seed):
    rng = np.random.default_rng(seed)
    d = ImageDraw.Draw(img, "RGBA")
    course = 34
    y = 0
    row = 0
    while y < SIZE:
        d.line([(0, y), (SIZE, y)], fill=A.rgb(cfg["wall2"]) + (90,), width=2)
        d.line([(0, y + 1), (SIZE, y + 1)], fill=(255, 255, 255, 28), width=1)
        off = 0 if row % 2 == 0 else course * 2
        x = off
        while x < SIZE:
            d.line([(x, y), (x, y + course)], fill=A.rgb(cfg["wall2"]) + (55,), width=1)
            x += course * 2
        y += course
        row += 1
    # soften the joints so they read as mortar, not CAD lines
    return img.filter(ImageFilter.GaussianBlur(0.6))


def draw_window(layer, cfg, rng, bx, by, w, h, floor_idx):
    """Draw one window (and its accessories) into an RGBA layer."""
    d = ImageDraw.Draw(layer, "RGBA")
    x = bx + (BAY - w) // 2
    y = by + int(FLOOR * 0.20) if not cfg["french"] else by + int(FLOOR * 0.10)

    frame = A.rgb(cfg["frame"])
    trim = A.rgb(cfg["trim"])

    # recess shadow (drawn into the layer's alpha, never flattening it)
    shadow = A.soft_shadow(layer.size, (x - 4, y - 4, x + w + 4, y + h + 4),
                           radius=4, spread=9, strength=0.50)
    layer.paste(A.darken_rgba(layer, shadow, 0.85), (0, 0))

    # lintel + sill (stone trim)
    d.rectangle([x - 12, y - 20, x + w + 12, y - 8], fill=trim + (255,))
    d.rectangle([x - 12, y - 20, x + w + 12, y - 17], fill=A.shade(trim, 1.06) + (255,))
    d.rectangle([x - 14, y + h + 4, x + w + 14, y + h + 4 + cfg["sill_h"]], fill=trim + (255,))
    d.rectangle([x - 14, y + h + 4 + cfg["sill_h"], x + w + 14, y + h + 8 + cfg["sill_h"]],
                fill=A.shade(trim, 0.72) + (150,))

    # frame
    d.rounded_rectangle([x - 7, y - 7, x + w + 7, y + h + 7], radius=3, fill=frame + (255,))
    d.rectangle([x, y, x + w, y + h], fill=A.shade(frame, 0.82) + (255,))

    closed = rng.random() < cfg["shutter_p"] * 0.35

    if closed:
        draw_shutters(d, x, y, w, h, cfg, rng, closed=True)
    else:
        # glass
        glass = A.vgrad(w, h, cfg["glass_top"], cfg["glass_bot"], gamma=0.8)
        layer.paste(glass.convert("RGBA"), (x, y))

        # interior hint: curtain / room darkness
        g = ImageDraw.Draw(layer, "RGBA")
        if rng.random() < 0.55:
            curtain_col = rng.choice(["#D9C9A8", "#C9B7C4", "#AFC0B4", "#E2D9C6"])
            cw = int(w * rng.uniform(0.18, 0.34))
            side = 0 if rng.random() < 0.5 else w - cw
            g.rectangle([x + side, y, x + side + cw, y + h],
                        fill=A.rgb(curtain_col) + (105,))
        # reflection streak
        g.polygon([(x + int(w * 0.06), y + h), (x + int(w * 0.42), y),
                   (x + int(w * 0.62), y), (x + int(w * 0.26), y + h)],
                  fill=(255, 255, 255, 26))

        # mullions
        mw = 5 if w > 100 else 4
        g.rectangle([x + w // 2 - mw // 2, y, x + w // 2 + mw // 2, y + h], fill=frame + (255,))
        g.rectangle([x, y + int(h * 0.42), x + w, y + int(h * 0.42) + mw], fill=frame + (255,))

        # security bars (very common in Algiers)
        if rng.random() < cfg["bars"]:
            for i in range(6):
                bx2 = x + int(w * (i + 1) / 7)
                g.line([(bx2, y), (bx2, y + h)], fill=(46, 46, 46, 150), width=2)
            g.line([(x, y + h // 2), (x + w, y + h // 2)], fill=(46, 46, 46, 120), width=2)

        if rng.random() < cfg["shutter_p"]:
            draw_shutters(d, x, y, w, h, cfg, rng, closed=False)

    # balcony / railing
    if rng.random() < cfg["balcony"]:
        draw_balcony(layer, cfg, rng, bx, y + h + cfg["sill_h"] + 8, w)

    # satellite dish
    if rng.random() < cfg["dish"]:
        dx = bx + rng.integers(14, max(16, BAY - 56))
        dy = by + rng.integers(10, max(12, FLOOR - 70))
        dw, dh = 34, 26
        d.ellipse([dx, dy, dx + dw, dy + dh], fill=(196, 196, 192, 255),
                  outline=(120, 120, 118, 255), width=2)
        d.ellipse([dx + 6, dy + 5, dx + dw - 6, dy + dh - 5], fill=(168, 168, 165, 255))
        d.line([(dx + dw // 2, dy + dh), (dx + dw // 2 + 10, dy + dh + 16)],
               fill=(90, 90, 88, 220), width=3)
        d.ellipse([dx + dw // 2 + 7, dy + dh + 13, dx + dw // 2 + 14, dy + dh + 20],
                  fill=(60, 60, 58, 255))

    # split AC unit
    if rng.random() < cfg["ac"]:
        ax = bx + rng.integers(6, max(8, BAY - 50))
        ay = by + int(FLOOR * 0.62)
        d.rectangle([ax, ay, ax + 44, ay + 30], fill=(214, 214, 208, 255),
                    outline=(150, 150, 146, 255))
        for i in range(5):
            d.line([(ax + 4, ay + 6 + i * 5), (ax + 40, ay + 6 + i * 5)],
                   fill=(160, 160, 156, 255), width=1)
        d.rectangle([ax + 4, ay + 24, ax + 40, ay + 27], fill=(120, 120, 116, 255))

    # potted plant on a sill
    if rng.random() < 0.12:
        px = bx + rng.integers(10, max(12, BAY - 40))
        py = by + int(FLOOR * 0.20) + h + 2
        d.rectangle([px, py, px + 16, py + 14], fill=(150, 78, 48, 255))
        d.ellipse([px - 4, py - 12, px + 20, py + 6], fill=(58, 96, 52, 255))
        d.ellipse([px + 2, py - 18, px + 18, py + 2], fill=(74, 118, 62, 255))


def draw_shutters(d, x, y, w, h, cfg, rng, closed):
    col = A.rgb(cfg["shutter"])
    if closed:
        d.rectangle([x, y, x + w, y + h], fill=col + (255,))
        for i in range(0, h, 8):
            d.line([(x, y + i), (x + w, y + i)], fill=A.shade(col, 0.72) + (255,), width=2)
        d.line([(x + w // 2, y), (x + w // 2, y + h)], fill=A.shade(col, 0.55) + (255,), width=2)
    else:
        pw = 26
        for sx in (x - pw - 6, x + w + 6):
            d.rectangle([sx, y - 4, sx + pw, y + h + 4], fill=col + (255,),
                        outline=A.shade(col, 0.68) + (255,), width=2)
            for i in range(0, h + 8, 7):
                d.line([(sx + 2, y - 4 + i), (sx + pw - 2, y - 4 + i)],
                       fill=A.shade(col, 0.74) + (230,), width=1)
            d.rectangle([sx + pw // 2 - 1, y + 6, sx + pw // 2 + 1, y + 22],
                        fill=A.shade(col, 0.55) + (255,))


def draw_balcony(layer, cfg, rng, bx, slab_y, w):
    d = ImageDraw.Draw(layer, "RGBA")
    slab = A.rgb(cfg["trim"])
    x0 = bx + 4
    x1 = bx + BAY - 4

    d.rectangle([x0, slab_y, x1, slab_y + 14], fill=A.shade(slab, 0.95) + (255,))
    d.rectangle([x0, slab_y, x1, slab_y + 4], fill=A.shade(slab, 1.05) + (255,))
    d.rectangle([x0, slab_y + 14, x1, slab_y + 19], fill=(60, 55, 48, 90))

    rail_h = 50 if cfg["kind"] != "affluent" else 58
    if cfg["kind"] == "colonial":
        # stone balustrade
        d.rectangle([x0, slab_y - 8, x1, slab_y + 2], fill=slab + (255,))
        n = 7
        for i in range(n):
            px = x0 + int((x1 - x0) * (i + 0.5) / n)
            d.ellipse([px - 7, slab_y - 34, px + 7, slab_y - 6], fill=A.shade(slab, 0.92) + (255,))
        d.rectangle([x0, slab_y - 44, x1, slab_y - 34], fill=A.shade(slab, 1.02) + (255,))
    else:
        top = slab_y - rail_h
        d.rectangle([x0, top, x1, top + 4], fill=(44, 46, 48, 255))
        step = 11 if cfg["kind"] != "affluent" else 9
        for px in range(x0 + 4, x1 - 2, step):
            d.line([(px, top + 4), (px, slab_y - 1)], fill=(44, 46, 48, 235), width=2)
        d.line([(x0, top + rail_h // 2), (x1, top + rail_h // 2)], fill=(44, 46, 48, 180), width=1)
        # wrought-iron scroll motif
        if cfg["kind"] in ("popular", "residential"):
            for px in range(x0 + 12, x1 - 10, step * 2):
                d.arc([px - 7, top + 12, px + 7, top + 30], 0, 180,
                      fill=(44, 46, 48, 170), width=2)

    # laundry
    if rng.random() < cfg["laundry"]:
        cols = ["#E8E8E4", "#C0392B", "#2E6DA8", "#E2C044", "#7FA88C", "#C77B9A"]
        n = rng.integers(3, 6)
        top = slab_y - rail_h + 6
        for i in range(n):
            px = x0 + 10 + i * ((x1 - x0 - 20) // n)
            cw = int(rng.integers(12, 20))
            ch = int(rng.integers(16, 30))
            d.rectangle([px, top, px + cw, top + ch], fill=A.rgb(rng.choice(cols)) + (245,))
            d.line([(px, top - 3), (px + cw, top - 3)], fill=(60, 60, 58, 200), width=1)


def draw_cables(img, cfg, seed):
    rng = np.random.default_rng(seed)
    d = ImageDraw.Draw(img, "RGBA")
    for _ in range(cfg["cables"]):
        y = int(rng.integers(10, SIZE - 10))
        sag = rng.uniform(2, 7)
        pts = [(x, y + sag * np.sin(np.pi * x / SIZE) * 1.0) for x in range(0, SIZE + 1, 8)]
        d.line(pts, fill=(28, 26, 24, 115), width=2)
    return img


def wall_tile(cfg, variant, seed):
    img = wall_base(cfg, seed)
    rng = np.random.default_rng(seed + 77)

    # full-tile architectural layer
    layer = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    for f in range(FLOORS):
        for b in range(BAYS):
            w = cfg["window_w"] + (rng.integers(-10, 12) if variant else 0)
            h = cfg["window_h"] + (rng.integers(-8, 10) if variant else 0)
            draw_window(layer, cfg, rng, b * BAY, f * FLOOR, w, h, f)

    # ground-floor band is replaced by the shopfront strip at runtime, but the
    # tile must still read correctly if used alone
    img = Image.alpha_composite(img, layer)
    img = draw_cables(img, cfg, seed + 9)

    # parapet / floor band every 2 floors for the residential blocks
    if cfg["kind"] in ("residential", "industrial"):
        d = ImageDraw.Draw(img, "RGBA")
        for f in range(0, FLOORS, 2):
            y = f * FLOOR
            d.rectangle([0, y, SIZE, y + 8], fill=A.shade(A.rgb(cfg["trim"]), 0.94) + (255,))
            d.rectangle([0, y + 8, SIZE, y + 11], fill=(50, 46, 40, 60))

    # floor separation: a soft shadow at each slab line keeps the rhythm readable
    d = ImageDraw.Draw(img, "RGBA")
    for f in range(FLOORS):
        y = f * FLOOR
        d.rectangle([0, y, SIZE, y + 7], fill=(64, 58, 50, 18))
        d.rectangle([0, y + 7, SIZE, y + 9], fill=(64, 58, 50, 10))

    return img.convert("RGB")


# -------------------------------------------------------------- shopfronts ---

SHOP_W, SHOP_H = 1024, 320


def shopfront(cfg, district, seed):
    rng = np.random.default_rng(seed)
    img = A.vgrad(SHOP_W, SHOP_H, A.shade(cfg["wall"], 0.92), A.shade(cfg["wall2"], 0.80))
    img = A.grain(img, 0.014, seed).convert("RGBA")

    st = A.streak(SHOP_H, SHOP_W, seed=seed + 1, count=16)
    img = A.overlay(img, "#4A4238", A.blur_mask(st, 1.0), 0.14).convert("RGBA")

    d = ImageDraw.Draw(img, "RGBA")
    signs = SHOP_SIGNS[district]
    bay_w = SHOP_W // len(signs)

    for i, (fr, ar, colour) in enumerate(signs):
        bx = i * bay_w
        pad = 12
        opening = [bx + pad + 6, 96, bx + bay_w - pad - 6, SHOP_H - 14]

        # sign board
        d.rectangle([bx + pad, 34, bx + bay_w - pad, 88], fill=A.rgb(colour) + (255,))
        d.rectangle([bx + pad, 34, bx + bay_w - pad, 39], fill=A.shade(colour, 1.35) + (255,))
        f_fr = A.fit_font(fr, "arialbd.ttf", 26, bay_w - 2 * pad - 20)
        A.text(d, ((bx + pad + bx + bay_w - pad) // 2, 56), fr, f_fr, (250, 248, 242),
               rtl=False)
        f_ar = A.fit_font(ar, "arialbd.ttf", 24, bay_w - 2 * pad - 24)
        A.text(d, ((bx + pad + bx + bay_w - pad) // 2, 78), ar, f_ar, (250, 248, 242),
               rtl=True)

        # opening
        d.rectangle(opening, fill=(24, 22, 20, 255))
        if rng.random() < 0.55:
            # lit interior with shelf silhouettes
            warm = A.vgrad(opening[2] - opening[0], opening[3] - opening[1],
                           "#7A6440", "#2A241C")
            img.paste(warm.convert("RGBA"), (opening[0], opening[1]))
            dd = ImageDraw.Draw(img, "RGBA")
            for s in range(3):
                sy = opening[1] + 40 + s * 52
                dd.line([(opening[0] + 8, sy), (opening[2] - 8, sy)],
                        fill=(30, 26, 22, 220), width=6)
                for g in range(6):
                    gx = opening[0] + 14 + g * ((opening[2] - opening[0] - 24) // 6)
                    dd.rectangle([gx, sy - 22, gx + 12, sy - 4],
                                 fill=A.rgb(rng.choice(["#C8B48A", "#A8483A", "#5E7A4A",
                                                        "#D9D2C0", "#8A6A3A"])) + (235,))
        else:
            # roll-up shutter, dirty and slightly buckled
            sh = A.rgb(rng.choice(["#8C8C86", "#6E7A7E", "#9A8268"]))
            d.rectangle(opening, fill=sh + (255,))
            for y in range(opening[1], opening[3], 7):
                d.line([(opening[0], y), (opening[2], y)], fill=A.shade(sh, 0.78) + (255,),
                       width=2)
            for _ in range(6):
                gx = int(rng.integers(opening[0], opening[2] - 20))
                gy = int(rng.integers(opening[1], opening[3] - 30))
                d.line([(gx, gy), (gx + int(rng.integers(-14, 14)), gy + int(rng.integers(10, 26)))],
                       fill=(70, 66, 60, 70), width=2)
            d.rectangle([opening[0], opening[3] - 10, opening[2], opening[3]],
                        fill=A.shade(sh, 0.6) + (255,))

        # awning hangs in front of the opening
        aw = A.rgb(rng.choice(AWNINGS))
        d.polygon([(bx + pad - 2, 92), (bx + bay_w - pad + 2, 92),
                   (bx + bay_w - pad - 6, 124), (bx + pad + 6, 124)],
                  fill=aw + (255,))
        for k in range(bx + pad, bx + bay_w - pad, 18):
            d.line([(k, 92), (k + 4, 124)], fill=A.shade(aw, 0.78) + (210,), width=7)
        d.line([(bx + pad + 6, 124), (bx + bay_w - pad - 6, 124)],
               fill=A.shade(aw, 0.55) + (255,), width=4)
        # scalloped valance
        for k in range(bx + pad + 6, bx + bay_w - pad - 6, 22):
            d.arc([k, 118, k + 22, 136], 0, 180, fill=aw + (255,), width=6)
        # shadow cast by the awning
        d.rectangle([opening[0], 124, opening[2], 140], fill=(0, 0, 0, 70))

        # door frame
        d.rectangle([opening[0] - 5, opening[1] - 5, opening[2] + 5, opening[3] + 5],
                    outline=A.rgb(cfg["frame"]) + (255,), width=5)

        # step
        d.rectangle([opening[0] - 8, SHOP_H - 16, opening[2] + 8, SHOP_H],
                    fill=A.shade(cfg["trim"], 0.88) + (255,))

        # planter beside the door
        if rng.random() < 0.45:
            px = opening[0] + int(rng.integers(-8, 12))
            d.rectangle([px, SHOP_H - 34, px + 26, SHOP_H - 14], fill=(150, 84, 54, 255))
            d.ellipse([px - 4, SHOP_H - 62, px + 30, SHOP_H - 24], fill=(62, 100, 56, 255))
            d.ellipse([px + 4, SHOP_H - 70, px + 24, SHOP_H - 40], fill=(78, 122, 66, 255))

    # pavement shadow line at the very bottom
    d.rectangle([0, SHOP_H - 6, SHOP_W, SHOP_H], fill=(20, 18, 16, 90))
    return img.convert("RGB")


# ------------------------------------------------------------------ roofs ----

def roof_flat(seed):
    rng = np.random.default_rng(seed)
    n = A.fbm(512, 512, base=9, octaves=5, seed=seed)
    base = np.array(A.rgb("#8E8A80"), np.float32) / 255.0
    arr = base[None, None, :] * (0.72 + 0.55 * n[..., None])
    img = A.to_img(arr)
    img = A.grain(img, 0.05, seed + 1)
    d = ImageDraw.Draw(img.convert("RGBA"), "RGBA")
    img = img.convert("RGBA")
    d = ImageDraw.Draw(img, "RGBA")
    # tar seams and gravel patches
    for _ in range(7):
        x = int(rng.integers(0, 512))
        d.line([(x, 0), (x + int(rng.integers(-30, 30)), 512)],
               fill=(48, 46, 44, 90), width=int(rng.integers(2, 5)))
    for _ in range(5):
        cx, cy = int(rng.integers(40, 470)), int(rng.integers(40, 470))
        r = int(rng.integers(20, 60))
        d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(120, 112, 100, 40))
    return img.convert("RGB")


def roof_tile(seed):
    rng = np.random.default_rng(seed)
    W = H = 512
    img = Image.new("RGB", (W, H), A.rgb("#A9552F"))
    d = ImageDraw.Draw(img)
    row_h = 32
    for row, y in enumerate(range(0, H, row_h)):
        off = 0 if row % 2 == 0 else 24
        for x in range(-48, W + 48, 48):
            c = A.jitter("#B45C34", 14, rng)
            d.rounded_rectangle([x + off, y, x + off + 46, y + row_h - 2],
                                radius=8, fill=c)
            d.line([(x + off + 4, y + row_h - 4), (x + off + 42, y + row_h - 4)],
                   fill=A.shade(c, 0.7), width=2)
    img = A.grain(img, 0.03, seed)
    moss = A.fbm(H, W, base=4, octaves=4, seed=seed + 2)
    moss = np.clip((moss - 0.6) * 2.5, 0, 1)
    return A.overlay(img, "#5A6A44", A.blur_mask(moss, 4), 0.35)


# ------------------------------------------------------------------- main ----

def contact_sheet(paths, out):
    imgs = [Image.open(p) for p in paths]
    cols = 3
    rows = (len(imgs) + cols - 1) // cols
    cell = 320
    sheet = Image.new("RGB", (cols * cell, rows * cell), (30, 30, 32))
    for i, im in enumerate(imgs):
        im.thumbnail((cell - 8, cell - 8), Image.LANCZOS)
        x = (i % cols) * cell + (cell - im.width) // 2
        y = (i // cols) * cell + (cell - im.height) // 2
        sheet.paste(im, (x, y))
    sheet.save(out)


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    out = sys.argv[1] if len(sys.argv) > 1 else "Assets/StreamingAssets/art"
    made = []
    for i, (key, cfg) in enumerate(DISTRICTS.items()):
        for v in ("a", "b"):
            p = os.path.join(out, f"wall_{key}_{v}.png")
            A.save(wall_tile(cfg, variant=(v == "b"), seed=1000 + i * 37 + (0 if v == "a" else 11)),
                   p, low_scale=0.5)
            made.append(p)
        p = os.path.join(out, f"shop_{key}.png")
        A.save(shopfront(cfg, key, seed=2000 + i * 13), p)
        made.append(p)
        print("  ", key, "done")

    A.save(roof_flat(3001), os.path.join(out, "roof_flat.png"))
    A.save(roof_tile(3002), os.path.join(out, "roof_tile.png"), low_scale=0.5)
    made += [os.path.join(out, "roof_flat.png"), os.path.join(out, "roof_tile.png")]

    prev = os.path.join("tools", "_preview")
    os.makedirs(prev, exist_ok=True)
    contact_sheet([m for m in made if "wall_" in m], os.path.join(prev, "facades.png"))
    contact_sheet([m for m in made if "shop_" in m], os.path.join(prev, "shops.png"))
    print(f"wrote {len(made)} textures to {out}")


if __name__ == "__main__":
    main()
