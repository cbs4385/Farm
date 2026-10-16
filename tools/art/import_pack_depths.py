"""Copies the purchased "CRAWLING DEPTHS" pack (AssetPacks/CRAWLING DEPTHS/) into Assets/_Project/Resources/Depths/ for the horror layer (Farm.Mythos, `DepthsArt`): the
tentacles of the ritual, the eyes, veins, statues, columns, urns, book altar, arcs and floor tiles that dress the mine from the relic chest's floor down. The files are copied UNCHANGED under plain names: the animation sheets stay sheets (frames are cut at run time by `DepthsArt`, so there is one file per sheet,
not one per frame). The pack's gifs, mouths, floating rocks, cliff, amalgams, full tileset and the two giant entity pictures (10 x 13 cells, larger than the boss floor's wall) are not used.
Run from the repository root: py tools/art/import_pack_depths.py
Licence: see docs/ASSET_LICENSES.md (the pack carries no licence text; the owner keeps the receipt / itch.io page)."""
import os
import shutil

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
PACK = os.path.join(ROOT, "AssetPacks", "CRAWLING DEPTHS", "CRAWLING DEPTHS")
OUT = os.path.join(ROOT, "Assets", "_Project", "Resources", "Depths")

FILES = {
    "tentacle_1": "Creatures/Tentacle 1 Sheet.png",      # 6 frames of 48 x 64
    "tentacle_2": "Creatures/Tentacle 2 Sheet.png",
    "eye_1": "Creatures/Eye 1 Sheet.png",                # 28 frames of 32 x 32
    "eye_2": "Creatures/Eye 2 Sheet.png",
    "littleeyes_1": "Structures & Details/Little Eyes 1 Sheet.png",    # 36 frames of 16 x 16
    "littleeyes_2": "Structures & Details/Little Eyes 2 Sheet.png",
    "vein_a1": "Structures & Details/Vein A1 Sheet.png",    # 24 frames of 16 x 16
    "vein_a2": "Structures & Details/Vein A2 Sheet.png",
    "vein_b1": "Structures & Details/Vein B1 Sheet.png",    # 32 frames of 16 x 16
    "vein_b2": "Structures & Details/Vein B2 Sheet.png",
    "statue_a1": "Structures & Details/Statue A1.png",     # 48 x 48
    "statue_a2": "Structures & Details/Statue A2.png",
    "statue_b1": "Structures & Details/Statue B1.png",
    "statue_b2": "Structures & Details/Statue B2.png",
    "statue_c1": "Structures & Details/Statue C1.png",
    "statue_c2": "Structures & Details/Statue C2.png",
    "column_short_1": "Structures & Details/Short Vein Column 1.png",   # 48 x 32
    "column_short_2": "Structures & Details/Short Vein Column 2.png",
    "column_tall_1": "Structures & Details/Tall Vein Column 1.png",     # 48 x 48
    "column_tall_2": "Structures & Details/Tall Vein Column 2.png",
    "urn_1": "Structures & Details/Urn 1.png",             # 16 x 16
    "urn_2": "Structures & Details/Urn 2.png",
    "book_altar": "Structures & Details/Book Altar.png",  # 16 x 32
    "arc_1": "Structures & Details/Amalgam Arc 1.png",    # 48 x 48
    "arc_2": "Structures & Details/Amalgam Arc 2.png",
    "floor_tiles": "Terrain/Tiles.png",                   # 5 x 4 cracked floor tiles of 16 x 16
}


def main():
    os.makedirs(OUT, exist_ok=True)
    for name, rel in FILES.items():
        shutil.copyfile(os.path.join(PACK, *rel.split("/")), os.path.join(OUT, name + ".png"))
        print("copied", name)


if __name__ == "__main__":
    main()
