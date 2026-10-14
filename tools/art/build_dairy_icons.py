"""Icons for the milk, egg and cheese items (playtest 2026-10-14): cheese, goat cheese, omelette, baked custard, cheesy potato, goat cheese salad.
Original 16 x 16 pixel art drawn from the character maps below. Writes item_<id>.png into Assets/_Project/Art/Placeholders and lists the names in final_art.txt so the
placeholder generator leaves them alone. Run from the repository root: python tools/art/build_dairy_icons.py"""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = os.path.join(ROOT, 'Assets', '_Project', 'Art', 'Placeholders')

PAL = {
    '.': (0, 0, 0, 0), 'k': (52, 33, 22, 255),
    'y': (240, 200, 80, 255), 'Y': (214, 160, 50, 255), 'o': (160, 108, 40, 255),     # cheese: light, mid, rind
    'w': (250, 247, 235, 255), 'g': (214, 210, 196, 255),                                 # goat cheese and plates
    'e': (252, 236, 150, 255), 'E': (236, 178, 60, 255), 'b': (176, 112, 50, 255),      # egg, browned
    'c': (246, 214, 132, 255), 'C': (200, 140, 70, 255),                                 # custard and crust
    'p': (222, 178, 112, 255), 'P': (176, 120, 66, 255),                                 # potato
    'G': (96, 160, 70, 255), 'D': (58, 112, 52, 255), 'r': (200, 70, 60, 255),         # kale, dark kale, pepper
}

ICONS = {
    'item_artisan_cheese': [
        "................",
        "................",
        "................",
        "......kkkkk.....",
        "....kkyyyyYkk...",
        "...kyyyyyyyYYk..",
        "..kkkkkkkkkkkkk.",
        "..koYYoYyyoYYok.",
        "..koyykoYYykyok.",
        "..koYykyyYykYok.",
        "..koyyyyYYyyYok.",
        "..kkkkkkkkkkkkk.",
        "................",
        "................",
        "................",
        "................",
    ],
    'item_artisan_goat_cheese': [
        "................",
        "................",
        "................",
        "................",
        "......kkkk......",
        "....kkwwwwkk....",
        "...kwwwwwwwgk...",
        "..kwwwwwwwwwgk..",
        "..kwwwwwwwwggk..",
        "..kgwwwwwwwggk..",
        "...kggwwwwggk...",
        "....kkgggggk....",
        "......kkkkk.....",
        "................",
        "................",
        "................",
    ],
    'item_food_omelette': [
        "................",
        "................",
        "................",
        "................",
        "....kkkkkkkk....",
        "..kkEEeeeeEEkk..",
        ".kEeeeeeeeeeeEk.",
        ".kEeeGeeeeeGeEk.",
        ".kEeeeeeerreeEk.",
        ".kbEeeeeeeeeEbk.",
        "..kkbEEEEEEbkk..",
        "....kkkkkkkk....",
        "................",
        "................",
        "................",
        "................",
    ],
    'item_food_custard': [
        "................",
        "................",
        "................",
        "................",
        "....kkkkkkkk....",
        "...kCcccccbCk...",
        "..kCccccccccCk..",
        "..kCcccbcccccCk.",
        "..kkccccccccckk.",
        "...kCCCCCCCCCk..",
        "...kCCCCCCCCCk..",
        "....kkkkkkkkk...",
        "................",
        "................",
        "................",
        "................",
    ],
    'item_food_cheesy_potato': [
        "................",
        "................",
        "................",
        "....kkkkkkkk....",
        "..kkppppppppkk..",
        ".kppyyyyyyyyppk.",
        ".kpyyYyyyyYyypk.",
        ".kpyYyyyyyyyYpk.",
        ".kPpyyyyYyyypPk.",
        ".kPPppyyyyppPPk.",
        "..kkPPPPPPPPkk..",
        "....kkkkkkkk....",
        "................",
        "................",
        "................",
        "................",
    ],
    'item_food_goat_cheese_salad': [
        "................",
        "................",
        "................",
        "...kk..kkk......",
        "..kGGkkGDGk.....",
        ".kGDGGGGGGGkk...",
        ".kGGGwwGDGGGGk..",
        ".kGDGwwwGGGDGk..",
        ".kGGGGwGGDGGGk..",
        "..kGGGGGGGGGk...",
        "..kggggggggggk..",
        "...kkkkkkkkkk...",
        "................",
        "................",
        "................",
        "................",
    ],
}

if __name__ == "__main__":
    for name, rows in ICONS.items():
        assert len(rows) == 16 and all(len(r) == 16 for r in rows), name
        im = Image.new("RGBA", (16, 16))
        for y, row in enumerate(rows):
            for x, ch in enumerate(row):
                im.putpixel((x, y), PAL[ch])
        im.save(os.path.join(OUT, name + ".png"))
    final = os.path.join(OUT, "final_art.txt")
    have = set(open(final).read().split())
    with open(final, "a") as f:
        for n in ICONS:
            if n not in have: f.write(n + "\n")
    sheet = Image.new("RGBA", (16 * 6 + 7 * 4, 24), (214, 190, 140, 255))
    for i, n in enumerate(ICONS):
        im = Image.open(os.path.join(OUT, n + ".png"))
        sheet.paste(im, (4 + i * 20, 4), im)
    os.makedirs(os.path.join(ROOT, "Builds"), exist_ok=True)
    sheet.resize((sheet.width * 8, sheet.height * 8), Image.NEAREST).save(os.path.join(ROOT, "Builds", "dairy_icons_sheet.png"))
    print("done")
