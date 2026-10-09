"""Cuts the ground decoration out of the purchased Mini Farm pack's tileset: tufts of grass and small flowers for each season (green for spring and summer,
orange for fall, pale blue for winter). One 16 x 16 picture each, in Assets/_Project/Resources/Decor/decor_<season>_<t|f><n>.png (t = tuft, f = flower), which
GroundDecor scatters over the grass at run time. Run from the repository root: python tools/art/import_pack_decor.py"""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
TILESET = os.path.join(ROOT, "AssetPacks", "mini-farm asset pack 1.0", "mini-farm 1.0", "tileset", "tileset.png")
OUT = os.path.join(ROOT, "Assets", "_Project", "Resources", "Decor")

# (column, row) of each 16 x 16 cell in the tileset
SETS = {
    "spring": {"t": [(7, 0), (9, 0), (11, 0), (9, 1), (7, 2), (9, 2), (11, 2), (9, 3), (11, 3), (6, 4), (7, 4), (6, 5), (7, 5), (11, 5)],
               "f": [(6, 0), (6, 1), (7, 1), (6, 2), (6, 3), (7, 3), (9, 4)]},
    "fall": {"t": [(6, 6), (7, 6), (11, 6), (6, 7), (7, 7), (6, 8), (7, 8), (11, 8)],
             "f": [(9, 6), (9, 7)]},
    "sand": {"t": [(8, 0), (8, 7), (8, 8), (7, 4), (6, 5), (9, 3)], "f": []},     # pebbles, driftwood and dune grass for the beach
    "winter": {"t": [(6, 9), (7, 9), (10, 9), (11, 9), (6, 10), (10, 10), (11, 10), (10, 11), (11, 11), (10, 12)],
               "f": [(7, 10), (9, 10)]},
}


def main():
    sheet = Image.open(TILESET).convert("RGBA")
    os.makedirs(OUT, exist_ok=True)
    for old in os.listdir(OUT):
        if old.startswith("decor_") and old.endswith(".png"):
            os.remove(os.path.join(OUT, old))
    count = 0
    for season, kinds in SETS.items():
        for kind, cells in kinds.items():
            for i, (c, r) in enumerate(cells):
                tile = sheet.crop((c * 16, r * 16, c * 16 + 16, r * 16 + 16))
                tile.save(os.path.join(OUT, "decor_%s_%s%d.png" % (season, kind, i)))
                count += 1
    print("wrote", count, "decor pictures")


if __name__ == "__main__":
    main()
