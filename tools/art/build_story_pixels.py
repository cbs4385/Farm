"""The last flat-colour placeholders of the 0.7.4 work, replaced by project-made pixel art (no AI, no outside pack):
  item_mythos_brass_key.png   Juno's small brass key (item icon, 16 x 16)
  item_pet_cat_food.png       a bowl of fish-shaped biscuits (item icon, 16 x 16)
  obj_door_panel.png          one leaf of an interior door: planks, a frame and a brass handle (it is drawn narrow when the door is open)
  obj_jetty.png               one board-width of the beach landing: boards across, seams, nails
  obj_net.png                 a fishing net hung to dry on a rack, with cork floats
  ui_cursor.png               the square that shows where the tool will land: four golden corner brackets
  item_forage_mushroom.png, obj_mushroom.png, item_artisan_goat_cheese.png   a toadstool-capped mushroom (icon and the one growing on the ground) and a wheel of goat cheese,
                              redrawn: the old ones were a few unconnected blobs
  item_machine_{hay,sacks,barrels,wheelbarrow}.png   the backpack icons of the farm props, made from the pictures of the props themselves (obj_<name>.png)
Written to Assets/_Project/Art/Placeholders. Run from the repository root: python tools/art/build_story_pixels.py"""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = os.path.join(ROOT, 'Assets', '_Project', 'Art', 'Placeholders')

OUTLINE = (0x1b, 0x1a, 0x1f, 255)
CLEAR = (0, 0, 0, 0)


def save(name, img):
    img.save(os.path.join(OUT, name + '.png'))
    print('wrote', name)


def from_grid(rows, colours):
    img = Image.new('RGBA', (16, 16), CLEAR)
    for y, row in enumerate(rows):
        assert len(row) == 16, (y, row)
        for x, c in enumerate(row):
            if c != '.':
                img.putpixel((x, y), colours[c])
    return img


# ---- the brass key --------------------------------------------------------------------------------------------------
KEY = ["................",
       "................",
       "..oooo..........",
       ".oggggo.........",
       "oghhhggooooooo..",
       "oghoohggggggggo.",
       "oghoohggggddddo.",
       ".oggggoooooooooo",
       "..oooo..ogo.ogo.",
       "........ooo.ooo.",
       "................",
       "................",
       "................",
       "................",
       "................",
       "................"]
KEY_COLOURS = {'o': OUTLINE, 'g': (0xc8, 0x9a, 0x32, 255), 'h': (0xf0, 0xd0, 0x6a, 255), 'd': (0x8a, 0x64, 0x1e, 255)}

# ---- the cat's bowl -------------------------------------------------------------------------------------------------
BOWL = ["................",
        "................",
        "................",
        ".....oo..oo.....",
        "....obbo.obbo...",
        "...obbbboobbbbo.",
        "..oooooooooooooo",
        ".orrrrrrrrrrrrro",
        ".orrrhrrrrrrrrro",
        "..orrrrrrrrrrro.",
        "...oorrrrrrroo..",
        "....ooooooooo...",
        "................",
        "................",
        "................",
        "................"]
BOWL_COLOURS = {'o': OUTLINE, 'b': (0xd9, 0x8f, 0x4a, 255), 'r': (0xb8, 0x4a, 0x3c, 255), 'h': (0xe0, 0x7a, 0x66, 255)}

WOOD = (0x8a, 0x5a, 0x34, 255)
WOOD_LIGHT = (0xa8, 0x74, 0x46, 255)
WOOD_DARK = (0x5e, 0x3b, 0x22, 255)
BRASS = (0xc8, 0x9a, 0x32, 255)


def door_panel():
    img = Image.new('RGBA', (16, 16), CLEAR)
    for y in range(16):
        for x in range(16):
            edge = x in (0, 15) or y in (0, 15)
            if edge:
                img.putpixel((x, y), OUTLINE)
                continue
            c = WOOD
            if (x - 1) % 4 == 3: c = WOOD_DARK               # a seam between planks
            elif (x - 1) % 4 == 0: c = WOOD_LIGHT            # the lit edge of a plank
            if y in (1, 14): c = WOOD_DARK                   # the frame: a dark band top and bottom
            elif y in (2, 13): c = WOOD_LIGHT if c != WOOD_DARK else c
            img.putpixel((x, y), c)
    for dx, dy in ((12, 7), (13, 7), (12, 8), (13, 8), (12, 9), (13, 9)):          # a brass handle on the free edge
        img.putpixel((dx, dy), BRASS)
    for dx, dy in ((11, 7), (11, 8), (11, 9), (14, 7), (14, 8), (14, 9), (12, 6), (13, 6), (12, 10), (13, 10)):
        img.putpixel((dx, dy), OUTLINE)
    return img


def jetty():
    img = Image.new('RGBA', (16, 16), CLEAR)
    shades = [(0xa0, 0x6c, 0x40, 255), (0x94, 0x62, 0x38, 255), (0xa8, 0x74, 0x46, 255), (0x9a, 0x68, 0x3c, 255)]
    for y in range(16):
        board = y // 4
        for x in range(16):
            c = shades[board]
            if y % 4 == 3: c = WOOD_DARK                      # the gap between boards
            elif y % 4 == 0: c = tuple(min(255, v + 14) for v in c[:3]) + (255,)
            grain = (x * 7 + board * 5) % 11
            if grain == 0 and y % 4 in (1, 2): c = tuple(max(0, v - 18) for v in c[:3]) + (255,)
            img.putpixel((x, y), c)
    for board in range(4):                                    # a nail near each end of every board
        for nx in (1, 14):
            img.putpixel((nx, board * 4 + 1), WOOD_DARK)
    return img


def net():
    img = Image.new('RGBA', (16, 16), CLEAR)
    cord = (0x7c, 0x96, 0x86, 255)
    cork = (0xe6, 0xd8, 0xa8, 255)
    for y in range(2, 16):                                    # two posts
        for x in (1, 2, 13, 14):
            img.putpixel((x, y), WOOD if x in (1, 13) else WOOD_DARK)
        for x in (0, 3, 12, 15):
            if img.getpixel((x, y))[3] == 0: img.putpixel((x, y), OUTLINE)
    for x in range(0, 16):                                    # the crossbar
        for y in range(1, 5):
            img.putpixel((x, y), OUTLINE if y in (1, 4) else (WOOD_LIGHT if y == 2 else WOOD))
    for y in range(5, 13):                                    # the net hangs in a diamond mesh
        for x in range(3, 13):
            if (x + y) % 3 == 0 or (x - y) % 3 == 0:
                img.putpixel((x, y), cord)
    for x in (4, 7, 10):                                      # cork floats along the foot of the net
        for dx, dy in ((0, 0), (1, 0), (0, 1), (1, 1)):
            img.putpixel((x + dx, 13 + dy), cork)
        for dx in (-1, 2):
            img.putpixel((x + dx, 13), OUTLINE); img.putpixel((x + dx, 14), OUTLINE)
        for dx in (0, 1):
            img.putpixel((x + dx, 15), OUTLINE)
    return img


def cursor():
    img = Image.new('RGBA', (16, 16), CLEAR)
    gold = (0xf4, 0xcf, 0x4a, 255)
    glow = (0xff, 0xf0, 0xa0, 255)
    arm = 4
    for cx, cy, sx, sy in ((0, 0, 1, 1), (15, 0, -1, 1), (0, 15, 1, -1), (15, 15, -1, -1)):
        for i in range(arm):                                  # the two arms of a corner bracket, two pixels thick
            for t in range(2):
                img.putpixel((cx + sx * i, cy + sy * t), gold if i else glow)
                img.putpixel((cx + sx * t, cy + sy * i), gold if i else glow)
        for i in range(arm + 1):                              # a dark edge on the inside so that it reads on grass and on sand
            for t in (2,):
                ox, oy = cx + sx * i, cy + sy * t
                if 0 <= ox < 16 and 0 <= oy < 16 and img.getpixel((ox, oy))[3] == 0: img.putpixel((ox, oy), OUTLINE)
                ox, oy = cx + sx * t, cy + sy * i
                if 0 <= ox < 16 and 0 <= oy < 16 and img.getpixel((ox, oy))[3] == 0: img.putpixel((ox, oy), OUTLINE)
    return img


def from_small(rows, colours, bottom=False):
    """A picture drawn on a grid smaller than 16 x 16, centred (or standing on the bottom edge)."""
    w = max(len(r) for r in rows)
    h = len(rows)
    img = Image.new('RGBA', (16, 16), CLEAR)
    ox, oy = (16 - w) // 2, (16 - h) if bottom else (16 - h) // 2
    for y, row in enumerate(rows):
        for x, c in enumerate(row):
            if c != '.':
                img.putpixel((ox + x, oy + y), colours[c])
    return img


MUSHROOM = ["....oooooo....",
            "..ooRRRRRRoo..",
            ".oRRRwwRRRRRo.",
            "oRRRwwwRRRwRRo",
            "oRRRRRRRRRwwRo",
            "oRwRRRRRwRRRRo",
            ".oooooooooooo.",
            "...occCCcco...",
            "...ocCCCCco...",
            "...ocCCsCco...",
            "...ocCCsCco...",
            "....occcco....",
            "....oooooo...."]
MUSHROOM_COLOURS = {'o': OUTLINE, 'R': (0xb4, 0x4c, 0x3a, 255), 'w': (0xf2, 0xe6, 0xc8, 255), 'C': (0xea, 0xdc, 0xbc, 255), 'c': (0xd2, 0xc0, 0x9c, 255), 's': (0xb8, 0xa4, 0x80, 255)}

MUSHROOM_WORLD = ["..........oooo...",
                  "........ooRRRoo..",
                  ".......oRRwwRRRo.",
                  "......oRRRRRRwRRo",
                  "......ooooooooooo",
                  "oooooo...ocCsco..",
                  "oRRRRRoo.ocCsco..",
                  "oRwRRRRRo.occo...",
                  ".ooooooo...oo....",
                  "...ocCso.........",
                  "...ocCso.........",
                  "....ooo..........",
                  "................"]
MUSHROOM_WORLD = [r[:16] for r in MUSHROOM_WORLD]

CHEESE = ["....oooooooo....",
          "...owwwwwwwwo...",
          "..owwwhhwwwwwo..",
          ".owwwwwwwwwgwwo.",
          ".owwwwwwwwwwwwo.",
          ".owwwwwgwwwwwwo.",
          ".oddwwwwwwwwddo.",
          "..oddddddddddo..",
          "...oooooooooo..."]
CHEESE_COLOURS = {'o': OUTLINE, 'w': (0xf6, 0xf2, 0xe4, 255), 'h': (0xff, 0xff, 0xff, 255), 'd': (0xd4, 0xcc, 0xb4, 255), 'g': (0x7c, 0xa8, 0x5c, 255)}


def icon_from_object(name):
    """A backpack icon made from the world picture of the same thing: trimmed to its pixels and centred, 14 pixels at most."""
    src = Image.open(os.path.join(OUT, 'obj_' + name + '.png')).convert('RGBA')
    box = src.getbbox()
    crop = src.crop(box)
    w, h = crop.size
    if max(w, h) > 14:
        scale = 14 / max(w, h)
        crop = crop.resize((max(1, round(w * scale)), max(1, round(h * scale))), Image.NEAREST)
    icon = Image.new('RGBA', (16, 16), CLEAR)
    icon.paste(crop, ((16 - crop.width) // 2, (16 - crop.height) // 2), crop)
    return icon


if __name__ == '__main__':
    save('item_mythos_brass_key', from_grid(KEY, KEY_COLOURS))
    save('item_pet_cat_food', from_grid(BOWL, BOWL_COLOURS))
    save('obj_door_panel', door_panel())
    save('obj_jetty', jetty())
    save('obj_net', net())
    save('ui_cursor', cursor())
    save('item_forage_mushroom', from_small(MUSHROOM, MUSHROOM_COLOURS))
    save('obj_mushroom', from_small(MUSHROOM_WORLD, MUSHROOM_COLOURS, bottom=True))
    save('item_artisan_goat_cheese', from_small(CHEESE, CHEESE_COLOURS))
    for prop in ('hay', 'sacks', 'barrels', 'wheelbarrow'):
        save('item_machine_' + prop, icon_from_object(prop))
