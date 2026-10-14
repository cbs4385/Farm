"""The fallen log (obj_log) and the bush (obj_bush) of the farm's overgrowth, 16 x 16 pixel grids. Project-made, not AI-generated.
Run from the repository root: python tools/art/build_overgrowth.py"""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
PH = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")

PALETTE = {"o": (58, 38, 24), "b": (150, 96, 52), "B": (190, 128, 70), "h": (222, 170, 104), "c": (112, 78, 46),
           "d": (38, 86, 52), "g": (74, 140, 72), "l": (126, 190, 96), "m": (96, 120, 60), "r": (170, 60, 60)}

LOG = [
    "................",
    "................",
    "................",
    "................",
    "................",
    "................",
    "...oooooooooo...",
    "..ohhoBBBBBBbo..",
    "..ohcohBBBBbbo..",
    "..ohhobBbBbbco..",
    "..oooocbbbcccoo.",
    "...mmoooooooomm.",
    "..mgmmm..mmgmm..",
    "................",
    "................",
    "................",
]

BUSH = [
    "................",
    "................",
    "................",
    "......dddd......",
    "....ddgglgdd....",
    "...dgglllggdd...",
    "..dgglgggrgldd..",
    "..dgllggggllgd..",
    "..dggggmgggggd..",
    "..ddgggggggldd..",
    "...dddgggdddd...",
    "....cddddddc....",
    ".....c.cc.c.....",
    "................",
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


draw(LOG, "obj_log.png")
draw(BUSH, "obj_bush.png")
