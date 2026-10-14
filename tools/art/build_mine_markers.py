"""The four mine markers (playtest 2026-10-13), all made from the world map's mine entrance (prop_mine_entrance.png) so the way in and the ways about inside look alike:
  exit  - the opening shows a light blue interior (daylight outside)
  down  - a dark interior and an arrow down
  up    - a lighter interior and an arrow up
  lift  - a neutral grey interior with an arrow up and an arrow down
Only the pixels of the opening are recoloured; the timber, rock, lantern and rails are the entrance's own. Written to Assets/_Project/Resources/Mine/prop_mine_{exit,down,up,lift}.png.
Run from the repository root: python tools/art/build_mine_markers.py"""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
SRC = os.path.join(ROOT, 'Assets', '_Project', 'Art', 'Placeholders', 'prop_mine_entrance.png')
OUT = os.path.join(ROOT, 'Assets', '_Project', 'Resources', 'Mine')

# the three darks of the opening (outer, middle, deepest), and what each becomes
OPENING = {(21, 17, 15, 255): 0, (31, 26, 24, 255): 1, (11, 9, 8, 255): 2}
INK = (35, 11, 7, 255)
PALETTES = {
    'exit': [(150, 200, 235, 255), (172, 216, 245, 255), (198, 232, 252, 255)],
    'down': [(21, 17, 15, 255), (31, 26, 24, 255), (11, 9, 8, 255)],
    'up': [(84, 92, 112, 255), (100, 108, 130, 255), (120, 128, 150, 255)],
    'lift': [(112, 112, 116, 255), (130, 130, 134, 255), (150, 150, 154, 255)],
}
ARROW = (250, 236, 150, 255)
UP = ["..#..", ".###.", "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.."]
DOWN = list(reversed(UP))
TOP = 18          # arrows sit under the hanging lantern, above the rails


def stamp(px, bitmap, left):
    cells = {(left + x, TOP + y) for y, row in enumerate(bitmap) for x, ch in enumerate(row) if ch == '#'}
    for (x, y) in cells:                                  # an ink edge so the arrow reads on any interior
        for dx in (-1, 0, 1):
            for dy in (-1, 0, 1):
                if (x + dx, y + dy) not in cells and px[x + dx, y + dy][3]: px[x + dx, y + dy] = INK
    for (x, y) in cells: px[x, y] = ARROW


for kind, palette in PALETTES.items():
    img = Image.open(SRC).convert('RGBA')
    px = img.load()
    for y in range(img.height):
        for x in range(img.width):
            idx = OPENING.get(px[x, y])
            if idx is not None and 9 <= x <= 22: px[x, y] = palette[idx]
    if kind == 'down': stamp(px, DOWN, 14)
    elif kind == 'up': stamp(px, UP, 14)
    elif kind == 'lift':
        stamp(px, UP, 10)
        stamp(px, DOWN, 17)
    path = os.path.join(OUT, 'prop_mine_%s.png' % kind)
    img.save(path)
    print('wrote', path)
