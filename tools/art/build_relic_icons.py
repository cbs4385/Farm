"""The three sealing relics of the mythos layer (item icons, 16 x 16, original pixel art), found in a chest on the mine's 30th floor:
  item_mythos_relic_seal.png    a disc of gray ash with a gold ring and a dark eye
  item_mythos_relic_bell.png    a hollow bronze bell gone green at the lip
  item_mythos_relic_thread.png  a ball of green thread with a loose end
Written to Assets/_Project/Art/Placeholders. Run from the repository root: python tools/art/build_relic_icons.py"""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = os.path.join(ROOT, 'Assets', '_Project', 'Art', 'Placeholders')

OUTLINE = (0x1b, 0x1a, 0x1f, 255)

SEAL = ["................",
        ".....oooooo.....",
        "...ooaaaaaaoo...",
        "..oaaabbbbaaao..",
        ".oaabbbbbbbbaao.",
        ".oabbbbggbbbbao.",
        "oaabbbgccgbbbaao",
        "oabbbgccccgbbbao",
        "oabbbgccccgbbbao",
        "oaabbbgccgbbbaao",
        ".oabbbbggbbbbao.",
        ".oaabbbbbbbbaao.",
        "..oaaabbbbaaao..",
        "...ooaaaaaaoo...",
        ".....oooooo.....",
        "................"]
SEAL_COLOURS = {'o': OUTLINE, 'a': (0xc9, 0xc6, 0xc0, 255), 'b': (0x9a, 0x97, 0x92, 255), 'c': (0x2c, 0x28, 0x30, 255), 'g': (0xe0, 0xb8, 0x40, 255)}

BELL = ["................",
        ".......oo.......",
        "......oggo......",
        ".......oo.......",
        "......obbo......",
        ".....obbbbo.....",
        "....obbhbbbo....",
        "....obbhbbbo....",
        "...obbhbbbbbo...",
        "...obbhbbbbbo...",
        "..obbbbbbbbbbo..",
        "..obvbbbbbbvbo..",
        ".oddddddddddddo.",
        ".oooooooooooooo.",
        "......oggo......",
        "................"]
BELL_COLOURS = {'o': OUTLINE, 'b': (0xb0, 0x7a, 0x38, 255), 'h': (0xe6, 0xc0, 0x6a, 255), 'd': (0x6e, 0x48, 0x22, 255),
                'v': (0x4f, 0x9a, 0x6a, 255), 'g': (0xe0, 0xb8, 0x40, 255)}

THREAD = ["................",
          "....oooooooo....",
          "..ooglgggdggoo..",
          ".oglggdgggldggo.",
          ".ogggdgglggdglo.",
          "oglgdgglgggdgglo",
          "oggdgglgggdgglgo",
          "ogdgglgggdgglggo",
          "oddgglgggdgglgdo",
          "oggdgggldggdgglo",
          ".oggdgglgggdglo.",
          ".oggggdgglgddoo.",
          "..oodgglggdoogo.",
          "....oooooooogglo",
          ".............oo.",
          "................"]
THREAD_COLOURS = {'o': OUTLINE, 'g': (0x3f, 0x9a, 0x4a, 255), 'l': (0x8a, 0xd6, 0x7a, 255), 'd': (0x24, 0x62, 0x34, 255)}


def draw(rows, colours):
    img = Image.new('RGBA', (16, 16), (0, 0, 0, 0))
    for y, row in enumerate(rows):
        assert len(row) == 16, (y, row)
        for x, ch in enumerate(row):
            if ch != '.':
                img.putpixel((x, y), colours[ch])
    return img


for name, rows, colours in [('seal', SEAL, SEAL_COLOURS), ('bell', BELL, BELL_COLOURS), ('thread', THREAD, THREAD_COLOURS)]:
    img = draw(rows, colours)
    img.save(os.path.join(OUT, 'item_mythos_relic_%s.png' % name))
    print('wrote', name)
