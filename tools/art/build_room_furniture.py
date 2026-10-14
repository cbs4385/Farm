"""Furniture that was far too small for what it is (playtest 2026-10-13: "the beds in the hospital need to be upscaled ... same issue with store shelves ... make the stove larger because it
is not clear that you can cook food on it"). Original pixel art, in the palette of the kit's timber and the existing furniture; written to Assets/_Project/Art/Placeholders:
  obj_kitchen.png         32 x 32  a range cooker with a hood and flue, four rings, two pots and an oven (two cells wide, two tall; the cooking place of the farmhouse)
  obj_bed.png             16 x 32  a single bed (one cell wide, two tall), cut from the farmhouse's double bed so that it is the same bed (the clinic's beds)
  obj_shelf.png           16 x 32  a tall shelf of shop goods: jars, boxes, bottles (one cell wide, two tall; the stores)
  obj_bookshelf_tall.png  16 x 32  a tall shelf of books (the library and the hall)
Run from the repository root: python tools/art/build_room_furniture.py"""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = os.path.join(ROOT, 'Assets', '_Project', 'Art', 'Placeholders')


def c(h):
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), 255)


INK = c('230b07')
WOOD_L = c('c5814f'); WOOD = c('ad6a41'); WOOD_D = c('7b4b36'); WOOD_DD = c('572f27')
IRON_L = c('9aa0a2'); IRON = c('7c8486'); IRON_D = c('5d6466'); IRON_DD = c('414849')
CREAM_L = c('f4ecd6'); CREAM = c('e2d7b9'); CREAM_D = c('bdb08e')
RED = c('b8402f'); RED_D = c('842a22'); RED_L = c('dd6a4a')
FLAME = c('f3a431'); FLAME_L = c('f8dc6a')
GLASS = c('2a2a33'); GLASS_L = c('4a4a58')
STEAM = c('e8eef0')
CLEAR = (0, 0, 0, 0)


def canvas(w, h):
    img = Image.new('RGBA', (w, h), CLEAR)
    return img, img.load()


def rect(px, size, x0, y0, x1, y1, col):
    w, h = size
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            if 0 <= x < w and 0 <= y < h: px[x, y] = col


def kitchen():
    img, px = canvas(32, 32)
    s = img.size
    # flue and hood
    rect(px, s, 13, 0, 18, 7, INK); rect(px, s, 14, 0, 17, 7, IRON); rect(px, s, 14, 0, 14, 7, IRON_L); rect(px, s, 17, 0, 17, 7, IRON_D)
    rect(px, s, 5, 7, 26, 13, INK)
    rect(px, s, 6, 8, 25, 12, IRON); rect(px, s, 6, 8, 25, 8, IRON_L); rect(px, s, 6, 12, 25, 12, IRON_D)
    rect(px, s, 8, 10, 23, 10, IRON_D)
    # the body of the range
    rect(px, s, 2, 13, 29, 31, INK)
    rect(px, s, 3, 14, 28, 30, CREAM)
    rect(px, s, 3, 14, 28, 14, CREAM_L)
    rect(px, s, 3, 30, 28, 30, CREAM_D)
    # the hob: a dark iron top with four rings
    rect(px, s, 3, 14, 28, 19, IRON_DD)
    rect(px, s, 3, 14, 28, 14, IRON)
    for (cx, cy) in [(8, 17), (23, 17)]:
        rect(px, s, cx - 2, cy - 1, cx + 2, cy + 1, INK); rect(px, s, cx - 1, cy, cx + 1, cy, IRON_D)
    # a red pot with steam on the left ring, a flame under nothing on the right (lit)
    rect(px, s, 5, 12, 11, 17, INK); rect(px, s, 6, 13, 10, 16, RED); rect(px, s, 6, 13, 10, 13, RED_L); rect(px, s, 6, 16, 10, 16, RED_D)
    rect(px, s, 4, 14, 5, 14, INK); rect(px, s, 11, 14, 12, 14, INK)
    for (x, y) in [(7, 9), (9, 8), (8, 7), (10, 6)]: pass
    px[7, 11] = STEAM; px[9, 10] = STEAM
    rect(px, s, 21, 16, 25, 18, FLAME); rect(px, s, 22, 15, 24, 16, FLAME_L); px[23, 14] = FLAME_L
    rect(px, s, 14, 15, 17, 18, INK); rect(px, s, 15, 16, 16, 17, IRON_D)          # a rear ring
    # the knobs and the oven door
    for x in (7, 12, 19, 24): rect(px, s, x, 21, x + 1, 22, INK); px[x, 21] = IRON_L
    rect(px, s, 6, 24, 25, 29, INK); rect(px, s, 7, 25, 24, 28, GLASS); rect(px, s, 8, 25, 12, 25, GLASS_L); rect(px, s, 8, 26, 9, 26, GLASS_L)
    rect(px, s, 7, 23, 24, 23, IRON_L)                                              # the oven's handle
    # feet
    rect(px, s, 3, 31, 6, 31, INK); rect(px, s, 25, 31, 28, 31, INK)
    return img


def bed():
    src = Image.open(os.path.join(OUT, 'obj_bed_double.png')).convert('RGBA')
    img = Image.new('RGBA', (16, 32), CLEAR)
    img.paste(src.crop((0, 0, 12, 32)), (0, 0))
    img.paste(src.crop((28, 0, 32, 32)), (12, 0))
    return img


def tall_frame(px, s):
    rect(px, s, 0, 0, 15, 31, INK)
    rect(px, s, 1, 1, 14, 30, WOOD_DD)                      # the back
    rect(px, s, 1, 1, 2, 30, WOOD); rect(px, s, 1, 1, 1, 30, WOOD_L); rect(px, s, 13, 1, 14, 30, WOOD_D)
    for y in (1, 9, 17, 25):                                # the boards
        rect(px, s, 1, y, 14, y + 1, WOOD); rect(px, s, 1, y, 14, y, WOOD_L); rect(px, s, 1, y + 1, 14, y + 1, WOOD_D)
    rect(px, s, 1, 29, 14, 30, WOOD_D)


def shelf():
    img, px = canvas(16, 32)
    s = img.size
    tall_frame(px, s)
    jars = [c('d86a5a'), c('e8c15a'), c('7aa860'), c('6a9ad0')]
    for i, x in enumerate((3, 6, 9, 11)):                   # jars and bottles on the top shelf
        col = jars[i % 4]
        rect(px, s, x, 4, x + 1, 8, col); rect(px, s, x, 3, x + 1, 3, IRON_L); px[x, 4] = CREAM_L
    for (x, w, col) in [(3, 4, c('c9a45a')), (8, 5, c('b0833f'))]:             # boxes
        rect(px, s, x, 11, x + w - 1, 16, INK); rect(px, s, x + 1, 12, x + w - 2, 15, col); rect(px, s, x + 1, 12, x + w - 2, 12, CREAM)
    for i, x in enumerate((3, 6, 9)):                       # sacks and loaves
        rect(px, s, x, 19, x + 2, 24, INK); rect(px, s, x + 1, 20, x + 1, 23, [c('d9c08a'), c('a86a4a'), c('e8d8a8')][i])
    rect(px, s, 12, 21, 13, 24, c('7aa860'))
    for i, x in enumerate((3, 5, 7, 9, 11)):                # bottles on the bottom shelf
        rect(px, s, x, 27, x, 28, [c('3a6a4a'), c('8a3a3a'), c('3a4a8a'), c('8a6a2a'), c('3a6a4a')][i])
    return img


def bookshelf_tall():
    img, px = canvas(16, 32)
    s = img.size
    tall_frame(px, s)
    colours = [c('b8402f'), c('3a6a9a'), c('5a8a4a'), c('d8a83a'), c('7a4a8a'), c('c8c0a0'), c('8a3a3a'), c('3a8a8a')]
    for row, y in enumerate((3, 11, 19, 27)):
        top = y if y < 27 else 26
        x = 3
        i = row * 3
        while x < 13:
            w = 1 if (x + row) % 3 else 2
            rect(px, s, x, top, x + w - 1, top + 4 if y < 27 else top + 2, colours[i % 8]); px[x, top] = tuple(min(255, v + 40) for v in colours[i % 8][:3]) + (255,)
            x += w
            i += 1
    return img


def shrunk(img, percent):
    """The picture at `percent` of its size, standing on the bottom edge in the middle of the same canvas (the cells it covers do not change)."""
    w, h = img.size
    small = img.resize((w * percent // 100, h * percent // 100), Image.NEAREST)
    out = Image.new('RGBA', img.size, (0, 0, 0, 0))
    out.paste(small, ((w - small.width) // 2, h - small.height))
    return out


# playtest 2026-10-14: the range is "a bit large compared to the rest of the house furniture": drawn 25% smaller on the same two by two cells
for name, img in [('obj_kitchen', shrunk(kitchen(), 75)), ('obj_bed', bed()), ('obj_shelf', shelf()), ('obj_bookshelf_tall', bookshelf_tall())]:
    path = os.path.join(OUT, name + '.png')
    img.save(path)
    print('wrote', path, img.size)
