"""Copies the purchased Mini Farm pack's tool pictures over the project's tool item icons (16 x 16, same names, so the .meta files and the item data stay as
they are). Run from the repository root: python tools/art/import_pack_tools.py
Playtest 2026-10-09: "Just found the hoe. It looks exactly like an axe." The pack's hoe has a long handle and a thin blade, the axe a wide head.
Licence: see docs/ASSET_LICENSES.md (the pack is purchased; keep the receipt)."""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
TILESET = os.path.join(ROOT, "AssetPacks", "mini-farm asset pack 1.0", "mini-farm 1.0", "tileset", "tileset.png")
OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")
COLUMN = 11                       # x = 176
ROWS = {"item_tool_rod": 13, "item_tool_axe": 14, "item_tool_pickaxe": 15, "item_tool_hoe": 16, "item_tool_wateringcan": 17}


def main():
    sheet = Image.open(TILESET).convert("RGBA")
    for name, row in ROWS.items():
        tile = sheet.crop((COLUMN * 16, row * 16, COLUMN * 16 + 16, row * 16 + 16))
        tile.save(os.path.join(OUT, name + ".png"))
        print("wrote", name)


if __name__ == "__main__":
    main()
