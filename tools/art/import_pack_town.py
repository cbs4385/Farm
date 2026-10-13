"""Brings three buildings of the Tiny Top-Down Village pack (Pixel-Deck) into the game so that the clinic, the general store and the Community Hall are easy to tell
apart from the other shops (playtest 2026-10-13): the Doctor's House (a red cross on the door and on a flag), the General Store (its name on the sign) and the Town Hall
(a flag, a balcony and double doors). The pictures are 80 x 80 pixels, five of the game's 16-pixel cells wide and tall, and come in at their own size (nearest-neighbour
scaling would break the pixels). They replace Assets/_Project/Art/Placeholders/prop_bld_<key>.png. DOOR_PX in build_building_masks.py holds the pixel of each picture's
door, measured against the pictures; run that script afterwards to rebuild the masks.
Run from the repository root: python tools/art/import_pack_town.py
Licence: see docs/ASSET_LICENSES.md (commercial use allowed, credit "Pixel Art Assets by Pixel-Deck.com" appreciated)."""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
PACK = os.path.join(ROOT, "AssetPacks", "Tiny-Top-Down-Village-58-Buildings-v1.0", "Tiny Top-Down Village")
OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")

BUILDINGS = {
    "clinic": "Doctors House.png",
    "general": "General Store.png",
    "hall": "Town Hall.png",
}


def main():
    for key, name in BUILDINGS.items():
        image = Image.open(os.path.join(PACK, name)).convert("RGBA")
        image.save(os.path.join(OUT, "prop_bld_%s.png" % key))
        print("wrote prop_bld_%s" % key, image.size)


if __name__ == "__main__":
    main()
