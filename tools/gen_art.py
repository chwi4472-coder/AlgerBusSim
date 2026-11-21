"""Regenerates the full Algerian art asset set and writes assets/manifest.json."""

import glob
import json
import os
import sys

from PIL import Image

sys.stdout.reconfigure(encoding="utf-8")

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "Assets", "StreamingAssets", "art")


def main():
    os.makedirs(OUT, exist_ok=True)
    generated = []

    for p in sorted(glob.glob(os.path.join(OUT, "*.png"))):
        name = os.path.basename(p)[:-4]
        rect_path = os.path.join(OUT, name + ".rects.json")
        rects = {}
        if os.path.exists(rect_path):
            with open(rect_path, "r", encoding="utf-8") as fh:
                rects = json.load(fh).get("sprites", {})
        generated.append({
            "key": name,
            "file": "art/" + name + ".png",
            "kind": "sprite",
            "sprites": rects,
        })
        low_path = os.path.join(OUT, "low", name + "_low.png")
        stale_path = os.path.join(OUT, "low", name + ".png")
        if os.path.exists(stale_path):
            os.remove(stale_path)
        if not os.path.exists(low_path) and os.path.exists(p):
            im = Image.open(p).convert("RGBA")
            w, h = im.size
            low = im.resize((max(1, w // 2), max(1, h // 2)), Image.LANCZOS).convert("RGBA")
            low.save(low_path, "PNG", optimize=True)
            generated[-1]["lowFile"] = "art/low/" + name + "_low.png"
        elif os.path.exists(stale_path):
            generated[-1]["lowFile"] = "art/low/" + name + ".png"

    with open(os.path.join(OUT, "manifest.json"), "w", encoding="utf-8") as fh:
        json.dump({"version": 1, "root": "art", "textures": generated}, fh, indent=1)

    print("art manifest written:", len(generated), "textures")


if __name__ == "__main__":
    main()
