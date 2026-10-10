"""The altar of Harrow Wood (ritual scenes, 2026-10-10): a 32 x 32 stone altar, a slab on a plinth on two worn steps, with a carved ring on its face and a dark hollow on its top
where offerings are laid. Original art, drawn here. Written to Assets/_Project/Art/Placeholders/obj_altar.png (the old one was 16 x 16 and read as a small cone).
The picture is centred on the cell it stands on; only that cell is solid."""
import os
from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = os.path.join(ROOT, 'Assets', '_Project', 'Art', 'Placeholders', 'obj_altar.png')

INK = (0x24, 0x22, 0x2b, 255)
STONE_L = (0x9a, 0x99, 0xa6, 255)
STONE = (0x74, 0x73, 0x80, 255)
STONE_D = (0x56, 0x55, 0x62, 255)
STONE_DD = (0x3e, 0x3d, 0x49, 255)
HOLLOW = (0x2e, 0x2a, 0x34, 255)
STAIN = (0x4a, 0x2e, 0x36, 255)
RUNE = (0xc9, 0xb0, 0x62, 255)
RUNE_D = (0x8f, 0x7a, 0x44, 255)
MOSS = (0x5b, 0x7a, 0x4c, 255)
MOSS_D = (0x3f, 0x5a, 0x37, 255)


def main():
    img = Image.new('RGBA', (32, 32), (0, 0, 0, 0))
    px = img.load()

    def rect(x0, y0, x1, y1, c):
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1): px[x, y] = c

    # two steps
    rect(1, 25, 30, 28, INK); rect(2, 25, 29, 27, STONE_D); rect(2, 25, 29, 25, STONE); rect(24, 26, 29, 27, STONE_DD)
    rect(3, 22, 28, 25, INK); rect(4, 22, 27, 24, STONE); rect(4, 22, 27, 22, STONE_L); rect(23, 23, 27, 24, STONE_D)
    # the plinth and the slab
    rect(5, 11, 26, 22, INK); rect(6, 12, 25, 21, STONE_D)
    rect(6, 12, 20, 21, STONE); rect(6, 12, 8, 21, STONE_L); rect(21, 12, 25, 21, STONE_DD)
    # the top face of the slab, with the hollow where offerings are laid
    rect(3, 7, 28, 12, INK); rect(4, 8, 27, 11, STONE_L); rect(4, 11, 27, 11, STONE_D)
    rect(8, 8, 23, 10, HOLLOW); rect(10, 9, 21, 9, STAIN); rect(12, 8, 19, 8, STAIN)
    # a ring carved on the face, with a line through it
    ring = [(14, 14), (15, 14), (16, 14), (17, 14), (13, 15), (18, 15), (12, 16), (19, 16), (12, 17), (19, 17), (13, 18), (18, 18), (14, 19), (15, 19), (16, 19), (17, 19)]
    for (x, y) in ring: px[x, y] = RUNE
    for y in range(13, 21): px[15, y] = RUNE_D; px[16, y] = RUNE_D if y not in (16, 17) else RUNE
    for (x, y) in [(8, 15), (9, 16), (8, 17), (23, 15), (22, 16), (23, 17)]: px[x, y] = RUNE_D
    # moss in the cracks of the steps
    for (x, y) in [(3, 26), (4, 26), (3, 27), (8, 27), (9, 27), (5, 23), (6, 23), (26, 27), (27, 26)]: px[x, y] = MOSS
    for (x, y) in [(4, 27), (9, 28), (6, 24)]: px[x, y] = MOSS_D
    img.save(OUT)
    print('wrote', OUT)


if __name__ == '__main__':
    main()
