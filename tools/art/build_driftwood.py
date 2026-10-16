"""The picture of a piece of driftwood on the beach (node `driftwood`, `obj_driftwood.png`, 16 x 16). It is the long piece that used to be drawn on the sand as ground decoration
(`Resources/Decor/decor_sand_t2.png`, project art): the pebble and the two driftwood decorations were removed from the sand because they could not be picked up (playtest 2026-10-16),
and this is the real thing, picked up with an axe. Run from the repository root while that file still exists: py tools/art/build_driftwood.py"""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
SRC = os.path.join(ROOT, "Assets", "_Project", "Resources", "Decor", "decor_sand_t2.png")
OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders", "obj_driftwood.png")


def main():
    im = Image.open(SRC).convert("RGBA")
    print("source", im.size)
    im.save(OUT)
    print("wrote", OUT)


if __name__ == "__main__":
    main()
