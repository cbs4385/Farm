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
    'general': 40, 'blacksmith': 33, 'carpenter': 48, 'library': 57, 'saloon': 45, 'clinic': 41, 'hall': 40,          # clinic, general and hall: the Tiny Top-Down Village pictures (import_pack_town.py)
    'cottage1': 26, 'cottage2': 32, 'cottage3': 24, 'cottage4': 33, 'farmhouse': 48,
    'coop': 22, 'barn': 48, 'greenhouse': 72,
}
CELL = 16
SOLID_SHARE = 0.20       # a wall cell blocks when this much of it is picture
ROOF_SHARE = 0.50        # the roof rows (from the third up) block only where the picture really fills the cell: a sloping roof line must not catch a walker (playtest 2026-10-10)


def smooth(row):
    # no notches: a row blocks from its first to its last blocking cell, so a roof line or a gap never makes a pocket to be caught in
    if '#' not in row:
        return row
    a, b = row.index('#'), row.rindex('#')
    return '.' * a + '#' * (b - a + 1) + '.' * (len(row) - b - 1)


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
            row += '#' if opaque / (CELL * CELL) >= (SOLID_SHARE if dy < 2 else ROOF_SHARE) else '.'
        rows.append(smooth(row))
    while rows and set(rows[-1]) == {'.'}:
        rows.pop()
    if len(rows) > 2 and rows[-1].count('#') < rows[-2].count('#'):          # the ridge or tip of the roof narrows: it is drawn over a walker, it does not stop one
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
