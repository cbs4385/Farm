"""Which cells of the ground each building picture really covers (playtest 2026-10-10: the coop and barn blocked empty space around their pictures).

Reads Assets/_Project/Art/Placeholders/prop_bld_<style>.png and writes Assets/_Project/Resources/BuildingMasks.json: for each style the pixel of its door and, for
every cell row above the door row (bottom first), which cells hold enough of the picture to block. Columns are cells relative to the door cell (dx0 is the first).
The picture stands with its foot at the foot of the door row and its door under the door cell, so one mask serves every place a building can stand.

Run again when a building picture or a door pixel changes (the door pixels are the numbers below, measured by hand against the pictures).
"""
import json
import math
import os
import sys

from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
ART = os.path.join(ROOT, 'Assets', '_Project', 'Art', 'Placeholders')
OUT = os.path.join(ROOT, 'Assets', '_Project', 'Resources', 'BuildingMasks.json')

DOOR_PX = {
    'general': 39, 'blacksmith': 33, 'carpenter': 48, 'library': 57, 'saloon': 45, 'clinic': 50, 'hall': 47,
    'cottage1': 26, 'cottage2': 32, 'cottage3': 24, 'cottage4': 33, 'farmhouse': 48,
    'coop': 22, 'barn': 48,
}
CELL = 16
SOLID_SHARE = 0.20       # a cell blocks when this much of it is picture


def mask_for(style, door_px):
    im = Image.open(os.path.join(ART, f'prop_bld_{style}.png')).convert('RGBA')
    w, h = im.size
    px = im.load()
    left = 0.5 - door_px / CELL                       # the picture's left edge, in cells from the door cell's left edge
    dx0 = math.floor(left)
    dx1 = math.ceil(left + w / CELL) - 1
    rows = []
    for dy in range(math.ceil(h / CELL)):
        row = ''
        for dx in range(dx0, dx1 + 1):
            x0 = round((dx - left) * CELL)
            opaque = 0
            for yy in range(h - dy * CELL - CELL, h - dy * CELL):
                if yy < 0:
                    continue
                for xx in range(x0, x0 + CELL):
                    if 0 <= xx < w and px[xx, yy][3] > 128:
                        opaque += 1
            row += '#' if opaque / (CELL * CELL) >= SOLID_SHARE else '.'
        rows.append(row)
    while rows and set(rows[-1]) == {'.'}:
        rows.pop()
    return {'doorPx': door_px, 'dx0': dx0, 'rows': rows}


def main():
    data = {style: mask_for(style, px) for style, px in sorted(DOOR_PX.items())}
    with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
        json.dump(data, f, indent=1)
        f.write('\n')
    for style, m in data.items():
        print(style, 'dx0', m['dx0'])
        for r in reversed(m['rows']):
            print('   ', r)
    return 0


if __name__ == '__main__':
    sys.exit(main())
