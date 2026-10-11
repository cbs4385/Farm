"""The mine's entrance (2026-10-11), replacing the boulder the cave mouth used to be drawn with: a 32 x 32 front view (two cells wide and two tall) of a dark opening in a
rock face, held up by timber posts and a lintel with braces, a few rail sleepers running out of it, and a lantern. Palette of the kit's buildings (the same timber as the barn).
Original art. Written to Assets/_Project/Art/Placeholders/prop_mine_entrance.png.
Run from the repository root: python tools/art/build_mine_entrance.py [output.png]"""
import os, sys
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(ROOT, 'Assets', '_Project', 'Art', 'Placeholders', 'prop_mine_entrance.png')


def c(h):
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), 255)


INK = c('230b07')
WOOD_L = c('c5814f'); WOOD = c('ad6a41'); WOOD_D = c('7b4b36'); WOOD_DD = c('572f27')
ROCK_L = c('9aa0a2'); ROCK = c('7c8486'); ROCK_D = c('5d6466'); ROCK_DD = c('414849')
DARK = c('15110f'); DARK2 = c('0b0908'); DEEP = c('1f1a18')
IRON = c('838a8c'); GOLD = c('e8c15a'); GLOW = c('f3dc8a'); GRASS = c('5b7a4c'); GRASS_L = c('8aa860'); DIRT = c('6b4a3a')

W, H = 32, 32
img = Image.new('RGBA', (W, H), (0, 0, 0, 0))
px = img.load()


def rect(x0, y0, x1, y1, col):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            if 0 <= x < W and 0 <= y < H: px[x, y] = col


# the rock face: a rounded mound, y 2..31
for y in range(2, 32):
    t = (y - 2) / 29.0
    half = int(15.5 * (0.45 + 0.55 * min(1.0, t * 2.2)) ** 0.8)
    for x in range(16 - half - 1, 16 + half + 1):
        if 0 <= x < W: px[x, y] = INK
for y in range(3, 32):
    t = (y - 2) / 29.0
    half = int(15.5 * (0.45 + 0.55 * min(1.0, t * 2.2)) ** 0.8)
    for x in range(16 - half, 16 + half):
        if 0 <= x < W:
            shade = ROCK
            if y < 8 or x < 16 - half + 3: shade = ROCK_L
            elif x > 16 + half - 4: shade = ROCK_D
            px[x, y] = shade
# stone texture
for (x, y) in [(5, 14), (6, 14), (8, 20), (9, 20), (24, 15), (25, 15), (26, 22), (4, 25), (5, 25), (27, 27), (20, 7), (21, 7), (11, 6), (12, 6), (7, 9), (25, 11)]:
    if px[x, y][3]: px[x, y] = ROCK_D
for (x, y) in [(10, 12), (22, 18), (6, 17), (26, 25), (14, 5), (18, 5)]:
    if px[x, y][3]: px[x, y] = ROCK_L
# turf on top
rect(10, 3, 21, 4, GRASS)
for x in (11, 13, 16, 19): px[x, 3] = GRASS_L

# the opening: x 8..23, y 11..31, with a rounded top corner pair
rect(8, 11, 23, 31, INK)
rect(9, 12, 22, 31, DARK)
rect(11, 14, 20, 31, DEEP)
rect(13, 17, 18, 31, DARK2)
for x in (9, 22): px[x, 12] = INK

# rails and sleepers running out
rect(8, 31, 23, 31, DIRT)
for x in (10, 14, 18, 22): px[x, 31] = WOOD_D
rect(12, 28, 12, 31, IRON); rect(19, 28, 19, 31, IRON)
for y in (29, 31): rect(11, y, 20, y, WOOD_DD)

# posts (x 5..8 and 23..26), lintel (y 8..11) and braces
for x0 in (5, 23):
    rect(x0 - 1, 10, x0 + 3, 31, INK)
    rect(x0, 11, x0 + 2, 31, WOOD)
    rect(x0, 11, x0, 31, WOOD_L)
    rect(x0 + 2, 11, x0 + 2, 31, WOOD_D)
    for y in (16, 22, 27): px[x0 + 1, y] = WOOD_D
    rect(x0 - 1, 29, x0 + 3, 31, INK); rect(x0, 29, x0 + 2, 30, ROCK_D)       # stone footing
rect(3, 7, 28, 11, INK)
rect(4, 8, 27, 10, WOOD)
rect(4, 8, 27, 8, WOOD_L)
rect(4, 10, 27, 10, WOOD_D)
for x in (9, 15, 21): px[x, 9] = WOOD_D
# a cap beam over it
rect(5, 5, 26, 7, INK)
rect(6, 6, 25, 6, WOOD_D)
rect(6, 5, 25, 5, INK)
# diagonal braces under the lintel
for i in range(4):
    px[8 + i, 11 + i] = WOOD; px[9 + i, 11 + i] = WOOD_D
    px[23 - i, 11 + i] = WOOD; px[22 - i, 11 + i] = WOOD_D
# a lantern hung from the lintel and its glow on the post
rect(15, 11, 16, 12, INK); px[15, 11] = IRON; px[16, 11] = IRON
rect(14, 12, 17, 16, INK); rect(15, 13, 16, 15, GOLD); px[15, 13] = GLOW; px[16, 13] = GLOW
for (x, y) in [(13, 14), (18, 14), (14, 17), (17, 17)]: px[x, y] = c('7b6a3a')
# a sign board on the right post
rect(26, 18, 29, 22, INK); rect(27, 19, 28, 21, WOOD_L); px[27, 20] = WOOD_DD; px[28, 20] = WOOD_DD

img.save(OUT)
print('wrote', OUT, img.size)
