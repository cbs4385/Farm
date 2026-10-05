"""Builds the water tile set: a seamless open-water tile (four ripple variants) and a shore tile for every combination of land
neighbours, so a pond or the sea reads as one body of water with a shoreline instead of a grid of separate puddles.

Names (read by Farm.Gameplay.WaterShore): tile_water_m<mask>.png, and tile_water_m0v<k>.png for the open-water variants.
Mask bits: N=1 E=2 S=4 W=8 (land on that side), NE=16 SE=32 SW=64 NW=128 (land only on that diagonal; set only when both adjacent
sides are water). Project-made, original.

Usage: python tools/art/build_water_tiles.py [output_dir]
"""
import os
import sys
from PIL import Image

OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "_Project", "Art", "Placeholders")

BASE = (84, 156, 150, 255)
SHADE = (70, 138, 136, 255)
LIGHT = (124, 192, 180, 255)
FOAM = (178, 220, 210, 255)
SHORE = (44, 96, 98, 255)
SPARK = (232, 247, 242, 255)

N, E, S, W, NE, SE, SW, NW = 1, 2, 4, 8, 16, 32, 64, 128
DIAGONALS = [(NE, N, E), (SE, S, E), (SW, S, W), (NW, N, W)]

# Ripples: (x, y, length) dashes of light water and single sparkle pixels, kept clear of the edges.
RIPPLES = [
    ([(3, 4, 4), (9, 10, 5)], [(7, 6)]),
    ([(6, 3, 3), (2, 11, 4), (11, 8, 3)], []),
    ([(4, 7, 5), (10, 12, 3)], [(5, 9)]),
    ([(8, 5, 4), (3, 12, 4), (12, 2, 2)], [(11, 11)]),
]


def canonical(mask):
    """Drops diagonal bits that a side bit already covers."""
    for diag, a, b in DIAGONALS:
        if mask & a or mask & b:
            mask &= ~diag
    return mask


def all_masks():
    seen = set()
    for m in range(256):
        seen.add(canonical(m))
    return sorted(seen)


def draw(mask, variant):
    img = Image.new("RGBA", (16, 16), BASE)
    px = img.load()
    dashes, sparks = RIPPLES[variant]
    if mask & 0x0F:
        dashes, sparks = RIPPLES[(mask * 7) % 4][0][:1], []        # one dash in the open middle of a shore tile
        dashes = [(5, 8, 4)] if mask & (N | S) != (N | S) else [(5, 8, 3)]
    for x in range(16):
        for y in range(16):
            if ((x * 73856093 ^ y * 19349663 ^ variant * 83492791) >> 3) % 9 == 0:
                px[x, y] = SHADE                                  # a faint grain so the water is not flat
    for (x, y, n) in dashes:
        for i in range(n):
            px[x + i, y] = LIGHT
    for (x, y) in sparks:
        px[x, y] = SPARK

    for x in range(16):
        for y in range(16):
            d = 99
            if mask & N: d = min(d, y)
            if mask & S: d = min(d, 15 - y)
            if mask & W: d = min(d, x)
            if mask & E: d = min(d, 15 - x)
            for diag, a, b in DIAGONALS:
                if mask & diag:
                    cx = 0 if diag in (SW, NW) else 15
                    cy = 0 if diag in (NE, NW) else 15
                    d = min(d, abs(x - cx) + abs(y - cy))
            if d == 0: px[x, y] = SHORE
            elif d == 1: px[x, y] = FOAM
            elif d == 2: px[x, y] = LIGHT
    return img


def main():
    os.makedirs(OUT, exist_ok=True)
    written = 0
    for mask in all_masks():
        if mask == 0:
            for v in range(4):
                draw(0, v).save(os.path.join(OUT, f"tile_water_m0v{v}.png")); written += 1
        else:
            draw(mask, 0).save(os.path.join(OUT, f"tile_water_m{mask}.png")); written += 1
    print("wrote", written, "water tiles to", os.path.abspath(OUT))


if __name__ == "__main__":
    main()
