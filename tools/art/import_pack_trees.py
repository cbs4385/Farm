"""Replaces the 16 x 16 tree picture with the purchased Mini Farm pack's 32 x 32 pine (playtest 2026-10-09: "the trees feel very small"), and sets the sprite's
pivot so the trunk stands at the foot of its cell while the crown rises over the cells above. Run from the repository root.
The tree is a scene object with a one-cell collider (MapBuilder) and a choppable node (a Tilemap tile, one cell), so only the picture grows."""
import os
import re
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
PACK = os.path.join(ROOT, "AssetPacks", "mini-farm asset pack 1.0", "mini-farm 1.0", "crops", "tree.png")
OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders", "obj_tree.png")
PIVOT_Y = 0.375          # the shadow under the trunk is about 4 pixels above the bottom edge; a Tilemap puts the pivot at the middle of the cell, 8 pixels above its foot


def main():
    Image.open(PACK).convert("RGBA").save(OUT)
    meta_path = OUT + ".meta"
    meta = open(meta_path, encoding="utf-8", newline="").read()
    meta = re.sub(r"(  alignment: )\d+(\r?\n  spritePivot: \{x: 0.5, y: )[0-9.]+(\})", r"\g<1>9\g<2>%s\g<3>" % PIVOT_Y, meta, count=1)
    open(meta_path, "w", encoding="utf-8", newline="").write(meta)
    print("wrote obj_tree (32 x 32, pivot y %s)" % PIVOT_Y)


if __name__ == "__main__":
    main()
