"""Brings pieces of the purchased "Cozy Village World Builder Kit" (MutterPixel Studio; AssetPacks/, git-ignored) into the game.

The kit is drawn on a 32-pixel grid and the game on a 16-pixel grid, so most pieces are halved: each 2 x 2 block of pixels becomes the pixel that is most common in
the block (a pixel-art filter: no blurred colors), and a block that is mostly empty stays empty. A few landmarks keep their own size (the clock tower).
  - Trees: Assets/_Project/Resources/Trees/tree_<season>_<n>.png (32 x 32; spring is used for summer too). SeasonalTrees picks them at run time.
  - Props: Assets/_Project/Art/Placeholders/prop_<name>.png, placed in the village and on the farm by MapBuilder.
Run from the repository root: python tools/art/import_pack_cozy.py
Licence: see docs/ASSET_LICENSES.md (purchased; credit "Art by MutterPixel Studio" in the credits)."""
import collections
import glob
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
KIT = os.path.join(ROOT, "AssetPacks", "Cozy Village World Builder Kit – Towns & Farms", "Cozy Village World Builder Kit – Towns & Farms")
TREES_OUT = os.path.join(ROOT, "Assets", "_Project", "Resources", "Trees")
PROPS_OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")

# spr_cozy_tree_<n>.png: 1 round green, 2 bare, 3 and 4 orange, 5 orange willow, 6 yellow willow, 7 green willow, 8 to 11 oaks and willows, 12 and 13 conifers, 14 small oak
TREES = {
    "spring": [1, 7, 8, 9, 10, 11, 14],
    "fall": [3, 4, 5, 6, 8, 10],
    "winter": [2, 13, 12],
}

# (output name, folder, file or pattern, "half" or "native")
PROPS = [
    ("prop_clock_tower", "Village Square", "spr_clock_tower.png", "native"),
    ("prop_fountain", "Village Square", "spr_water_fountain*.png", "half"),
    ("prop_bench", "Village Square", "spr_long_cozy_bench.png", "half"),
    ("prop_flag", "Village Square", "spr_flag_1.png", "half"),
    ("prop_flowerbox_1", "Village Square", "spr_town_plant_1.png", "half"),
    ("prop_flowerbox_2", "Village Square", "spr_town_plant_2.png", "half"),
    ("prop_lamp", "Outdoor Lights", "spr_outdoor_lamp.png", "half"),
    ("prop_stall", "buildings/Cozy Market Stalls", "spr_stall_3.png", "half"),
    ("prop_stall_blue", "buildings/Cozy Market Stalls", "spr_stall_1.png", "half"),
    ("prop_barrels", "buildings/Workshops", "spr_4_barrels.png", "half"),
    ("prop_barrel_stack", "buildings/Workshops", "spr_barrel_stack.png", "half"),
    ("prop_boxes", "buildings/Workshops", "spr_boxes.png", "half"),
    ("prop_wheelbarrow", "buildings/Workshops", "spr_wheel_barrow*.png", "half"),
    ("prop_hay", "Farm/Cozy Farm & Crops", "spr_hay_1.png", "half"),
    ("prop_sacks", "Farm/Farm Buildings", "spr_crop_sacks.png", "half"),
]


def half(img):
    """Halves a picture: every 2 x 2 block becomes its most common opaque color (transparent when fewer than two of its four pixels are opaque)."""
    img = img.convert("RGBA")
    w, h = img.size
    out = Image.new("RGBA", (w // 2, h // 2), (0, 0, 0, 0))
    src = img.load()
    dst = out.load()
    for y in range(h // 2):
        for x in range(w // 2):
            block = [src[2 * x + dx, 2 * y + dy] for dx in (0, 1) for dy in (0, 1)]
            opaque = [p for p in block if p[3] >= 128]
            if len(opaque) < 2:
                continue
            color = collections.Counter(opaque).most_common(1)[0][0]
            dst[x, y] = (color[0], color[1], color[2], 255)
    return out


def tight(img):
    """Crops transparent rows from the bottom and sides' empty margin so that a prop's foot is at the bottom of its picture; keeps the top as it is."""
    box = img.getbbox()
    if box is None:
        return img
    return img.crop((box[0], 0, box[2], box[3]))


def main():
    os.makedirs(TREES_OUT, exist_ok=True)
    for old in glob.glob(os.path.join(TREES_OUT, "tree_*.png")):
        os.remove(old)
    count = 0
    for season, numbers in TREES.items():
        for i, n in enumerate(numbers):
            half(Image.open(os.path.join(KIT, "Cozy Trees", "spr_cozy_tree_%d.png" % n))).save(os.path.join(TREES_OUT, "tree_%s_%d.png" % (season, i)))
            count += 1
    print("wrote", count, "trees")

    for name, folder, pattern, mode in PROPS:
        matches = sorted(glob.glob(os.path.join(KIT, folder.replace("/", os.sep), pattern)))
        if not matches:
            print("MISSING", name, folder, pattern)
            continue
        img = Image.open(matches[0]).convert("RGBA")
        if img.width == 2 * img.height:                 # a strip of two frames (the fountain): the first
            img = img.crop((0, 0, img.height, img.height))
        out = tight(half(img) if mode == "half" else img)
        out.save(os.path.join(PROPS_OUT, name + ".png"))
        print("wrote", name, img.size, "->", out.size)


if __name__ == "__main__":
    main()
