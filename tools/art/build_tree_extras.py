"""The acorn (item_resource_acorn) and the sapling it grows from (obj_sapling), drawn as 16 x 16 pixel grids. Project-made, not AI-generated.
Run from the repository root: python tools/art/build_tree_extras.py"""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
PH = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")

PALETTE = {"o": (58, 38, 24), "b": (150, 96, 52), "B": (190, 128, 70), "h": (222, 170, 104), "c": (112, 78, 46),
           "d": (38, 86, 52), "g": (74, 140, 72), "l": (126, 190, 96), "t": (112, 74, 44)}

ACORN = [
    "................",
    "................",
    "................",
    "......ooo.......",
    ".....occco......",
    "....occccco.....",
    "....oBccccco....",
    "....oooooooo....",
    "....obBBBhbo....",
    "....obBBhhbo....",
    ".....obBhhbo....",
    ".....obBhbo.....",
    "......obbo......",
    ".......oo.......",
    "................",
    "................",
]

SAPLING = [
    "................",
    "................",
    "................",
    ".....ll..ll.....",
    "....lgggllgg....",
    "....lggdgggd....",
    ".....gdd.dd.....",
    "......gld.......",
    ".......gd.......",
    ".......tt.......",
    ".......tt.......",
    ".......tt.......",
    "......dttd......",
    ".....dddddd.....",
    "................",
    "................",
]


def draw(rows, name):
    im = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch != ".":
                im.putpixel((x, y), PALETTE[ch] + (255,))
    im.save(os.path.join(PH, name))
    print("wrote", name)


if __name__ == "__main__":
    draw(ACORN, "item_resource_acorn.png")
    draw(SAPLING, "obj_sapling.png")
