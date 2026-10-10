"""Brings the purchased Cozy Village kit's buildings into the game at their own size (a 96-pixel shop is six of the game's 16-pixel cells wide, a 64-pixel house
four), as Assets/_Project/Art/Placeholders/prop_bld_<key>.png. MapBuilder stands each one on its building's footprint with the door under the kit's door.
Run from the repository root: python tools/art/import_pack_buildings.py
Licence: see docs/ASSET_LICENSES.md (purchased; credit "Art by MutterPixel Studio")."""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
KIT = os.path.join(ROOT, "AssetPacks", "Cozy Village World Builder Kit – Towns & Farms", "Cozy Village World Builder Kit – Towns & Farms", "buildings")
OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")

SHOPS = "Cozy Village Shop Buildings (Animated)"
HOUSES = "Village Houses"

BUILDINGS = {
    "general": (SHOPS, "spr_village_shop_2.png"),
    "carpenter": (SHOPS, "spr_village_shop_1.png"),
    "blacksmith": (SHOPS, "spr_village_blacksmith_shop.png"),
    "library": (HOUSES, "spr_cozy_village_house_9.png"),
    "saloon": (SHOPS, "spr_village_inn.png"),
    "clinic": (HOUSES, "spr_cozy_village_house_10.png"),
    "hall": (HOUSES, "spr_cozy_village_house_8.png"),
    "cottage1": (HOUSES, "spr_cozy_village_house_1.png"),
    "cottage2": (HOUSES, "spr_cozy_village_house_2.png"),
    "cottage3": (HOUSES, "spr_cozy_village_house_3.png"),
    "cottage4": (HOUSES, "spr_cozy_village_house_5.png"),
    "farmhouse": ("..\\Farm\\Farm Buildings", "spr_farm_house.png"),
    "coop": ("..\\Farm\\Farm Buildings", "spr_chicken_coop.png"),
    "barn": ("..\\Farm\\Farm Buildings", "spr_farm_barn.png"),
}


def main():
    for key, (folder, name) in BUILDINGS.items():
        image = Image.open(os.path.join(KIT, folder, name)).convert("RGBA")
        image.save(os.path.join(OUT, "prop_bld_%s.png" % key))
        print("wrote prop_bld_%s" % key, image.size)


if __name__ == "__main__":
    main()
