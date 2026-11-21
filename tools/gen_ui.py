"""UI atlas: 9-slice panel, buttons, icons, coin, flag.

1024x1024.

Run:  python tools/gen_ui.py [out_dir]
"""

import json
import os
import sys

from PIL import Image, ImageDraw, ImageFilter

import artlib as A

WHITE = (244, 242, 234, 255)
PANEL = (255, 253, 247, 255)
PANEL_EDGE = (58, 66, 80, 255)
BTN = (44, 52, 66, 255)
BTN_PRESS = (30, 36, 48, 255)
ACCENT = (20, 90, 160, 255)
ACCENT2 = (0, 99, 51, 255)
INK = (30, 36, 44, 255)
MUTED = (120, 126, 134, 255)


def panel(d, x, y, s=512):
    d.rounded_rectangle([x, y, x + s, y + s], radius=46, fill=PANEL + (255,))
    d.rounded_rectangle([x, y, x + s, y + s], radius=46, fill=None, outline=PANEL_EDGE, width=12)
    g = A.blur_mask(A.fbm(s, s, base=5, octaves=2, seed=1), 40)
    d.rectangle([x + 12, y + 12, x + s - 12, y + s - 12], fill=None) if False else None
    inner = A.vgrad(s, s, (252, 250, 244, 255), (240, 238, 230, 255), gamma=1.0)
    img_layer = inner.convert("RGBA")
    d_ = ImageDraw.Draw(img_layer)
    d_.rounded_rectangle([x + 14, y + 14, x + s - 14, y + s - 14], radius=36, fill=None)
    # composite via paste on a temp is complex; draw gradient with ImageDraw not supported -> use paste
    return img_layer


def draw_panel(img, x, y, s=512):
    grad = A.vgrad(s, s, (252, 250, 244, 255), (238, 236, 228, 255), gamma=1.0).convert("RGBA")
    r = grad.resize((s, s), Image.LANCZOS)
    img.paste(r, (x, y), r)
    d = ImageDraw.Draw(img, "RGBA")
    d.rounded_rectangle([x, y, x + s, y + s], radius=44, outline=PANEL_EDGE, width=12)
    d.rounded_rectangle([x + 10, y + 10, x + s - 10, y + s - 10], radius=34, outline=(220, 216, 204, 255), width=4)
    return img


def icon_bus(d, x, y, s=256):
    body = (44, 52, 66, 255)
    d.rounded_rectangle([x + 20, y + 30, x + s - 20, y + s - 20], radius=26, fill=body)
    d.rectangle([x + 30, y + 42, x + s - 34, y + s - 60], fill=(84, 96, 116, 255))
    d.rectangle([x + 40, y + 58, x + s - 40, y + 92], fill=(30, 40, 56, 255))
    d.rectangle([x + 30, y + 100, x + s - 34, y + s - 60], fill=(84, 96, 116, 255))
    d.ellipse([x + 46, y + s - 78, x + 88, y + s - 36], fill=(26, 30, 38, 255))
    d.ellipse([x + s - 88, y + s - 78, x + s - 46, y + s - 36], fill=(26, 30, 38, 255))


def icon_money(d, x, y, s=256):
    d.ellipse([x + 40, y + 30, x + s - 40, y + s - 30], fill=(226, 180, 66, 255))
    d.ellipse([x + 40, y + 30, x + s - 40, y + s - 30], fill=None, outline=(170, 130, 30, 255), width=8)
    f = A.font("arialbd.ttf", int(s * 0.42))
    d.text((x + s / 2 - 20, y + s / 2 - 18), "1", font=f, fill=(120, 84, 18, 255))


def icon_fuel(d, x, y, s=256):
    d.rounded_rectangle([x + 30, y + 30, x + s - 30, y + s - 40], radius=16, fill=(40, 60, 92, 255))
    d.rectangle([x + s - 62, y + 62, x + s - 14, y + 62], fill=(200, 60, 44, 255))
    d.line([(x + s - 14, 62), (x + s - 14, 110)], fill=(200, 60, 44, 255), width=10)
    d.line([(x + s - 14, 110), (x + s - 40, 110)], fill=(200, 60, 44, 255), width=10)


def icon_passenger(d, x, y, s=256):
    d.ellipse([x + 60, y + 40, x + 130, y + 110], fill=(235, 235, 232, 255))
    d.polygon([(x + 55, y + 115), (x + 135, y + 115), (x + 145, y + 185), (x + 45, y + 185)], fill=(58, 92, 128, 255))
    d.rectangle([x + 70, y + 150, x + 115, y + 185], fill=(46, 72, 104, 255))


def icon_clock(d, x, y, s=256):
    d.ellipse([x + 30, y + 30, x + s - 30, y + s - 30], fill=(38, 46, 58, 255))
    d.ellipse([x + 44, y + 44, x + s - 44, y + s - 44], fill=None, outline=(250, 250, 246, 255), width=10)
    d.line([(x + s / 2, y + s / 2), (x + s / 2, y + 70)], fill=(250, 250, 246, 255), width=10)
    d.line([(x + s / 2, y + s / 2), (x + 86, y + 110)], fill=(250, 250, 246, 255), width=10)


def icon_door(d, x, y, s=256):
    d.rounded_rectangle([x + 30, y + 20, x + s - 30, y + s - 20], radius=18, fill=(36, 44, 60, 255))
    d.rectangle([x + 80, y + 20, x + 88, y + s - 20], fill=(230, 228, 220, 255))
    d.rectangle([x + s - 88, y + 20, x + s - 80, y + s - 20], fill=(230, 228, 220, 255))


def icon_ticket(d, x, y, s=256):
    d.rounded_rectangle([x + 26, y + 26, x + s - 26, y + s - 26], radius=20, fill=(20, 90, 160, 255))
    d.rounded_rectangle([x + 40, y + 40, x + s - 40, y + s - 40], radius=12, fill=None, outline=(244, 242, 234, 255), width=6)


def icon_star(d, x, y, s=256):
    pts = []
    import math
    for i in range(10):
        r = s / 2 * (2.0 if i % 2 else 1.0)
        a = math.pi / 2 + i * math.pi / 5
        pts.append((x + s / 2 + r * math.cos(a), y + s / 2 + r * math.sin(a)))
    d.polygon(pts, fill=(232, 176, 40, 255))


def icon_flag(d, x, y, s=256):
    d.line([(x + 40, y + s - 60), (x + 40, y + 60)], fill=(30, 36, 44, 255), width=10)
    d.polygon([(x + 40, 70), (x + 120, 90), (x + 40, 112)], fill=(214, 49, 69, 255))
    d.rectangle([x + 10, y + 40, x + 130, y + 46], fill=(30, 36, 44, 255))


def build():
    img = Image.new("RGBA", (1024, 1024), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    draw_panel(img, 0, 0, 512)
    draw_panel(img, 512, 0, 512)
    draw_panel(img, 0, 512, 256)
    icon_bus(d, 256, 512)
    icon_money(d, 512, 512)
    icon_fuel(d, 0, 256, 256)
    icon_passenger(d, 256, 256, 256)
    icon_clock(d, 512, 256, 256)
    icon_door(d, 256, 0, 256)
    icon_ticket(d, 512, 0, 256)
    icon_star(d, 0, 256, 256)
    icon_flag(d, 256, 256, 256)
    d.ellipse([768, 768, 960, 960], fill=(226, 180, 66, 255))
    d.text((800, 850), "1", font=A.font("arialbd.ttf", 90), fill=(120, 84, 18, 255))
    return img


def main():
    import json as _json
    sys.stdout.reconfigure(encoding="utf-8")
    out = sys.argv[1] if len(sys.argv) > 1 else "Assets/StreamingAssets/art"
    os.makedirs(out, exist_ok=True)
    img = build()
    A.save(img, os.path.join(out, "ui.png"))
    rects = {
        "ui_panel": [0, 0, 512, 512, "panel"],
        "ui_panel_small": [512, 512, 256, 256, "panel"],
        "ui_btn": [512, 256, 256, 256, "btn"],
        "ui_btn_press": [0, 256, 256, 256, "btnpress"],
        "icon_bus": [256, 512, 256, 256, "icon"],
        "icon_money": [512, 512, 256, 256, "icon"],
        "icon_fuel": [0, 256, 256, 256, "icon"],
        "icon_passenger": [256, 256, 256, 256, "icon"],
        "icon_clock": [512, 256, 256, 256, "icon"],
        "icon_door": [256, 0, 256, 256, "icon"],
        "icon_ticket": [512, 0, 256, 256, "icon"],
        "icon_star": [0, 256, 256, 256, "icon"],
        "icon_flag": [256, 256, 256, 256, "icon"],
        "ui_coin": [768, 768, 192, 192, "coin"],
    }
    with open(os.path.join(out, "ui.rects.json"), "w", encoding="utf-8") as fh:
        _json.dump({"texture": "ui", "sprites": rects}, fh, indent=1)
    print("ui atlas written to", out)


if __name__ == "__main__":
    main()
