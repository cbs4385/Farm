"""The things that stand in the village on the four festival days (playtest 2026-10-13: "there are no activities ... create activities with appropriate props"). Original pixel
art in the palette of the kit's timber and the existing furniture; every picture is drawn here with an ink outline added at the end. Written to Assets/_Project/Art/Placeholders
(prop_* pictures get their foot at the foot of the cell from TextureImportPostprocessor):
  obj_fest_floor              16 x 16  one cell of the dance floor (light planks)
  prop_fest_maypole           16 x 48  the maypole: a striped pole, a wreath of blossoms and six ribbons
  prop_fest_bunting_<season>  32 x 16  a string of flags, one palette for each festival
  prop_fest_bandstand         32 x 32  a small stage with a striped roof, a drum and a fiddler's stool
  prop_fest_scoreboard        32 x 32  the Tidewatch booth: awning, chalkboard of scores
  prop_fest_boat              32 x 16  a rowboat
  prop_fest_fishcrate         16 x 16  a crate of the day's catch
  prop_fest_stall             32 x 32  a market stall with a striped awning and pumpkins
  prop_fest_judging           32 x 16  the judging table: cloth, rosettes, a tiered stand
  prop_fest_pumpkins          16 x 16  a heap of pumpkins
  prop_fest_feasttable        32 x 16  a long table laid for the feast
  prop_fest_lantern           16 x 32  a lantern pole, and prop_fest_lantern_lit  the same, lit
  prop_fest_lanternrack       16 x 32  the rack where the player hangs a lantern, and prop_fest_lanternrack_lit
Run from the repository root: python tools/art/build_festival_props.py"""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = os.path.join(ROOT, 'Assets', '_Project', 'Art', 'Placeholders')


def c(h):
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), 255)


INK = c('230b07')
WOOD_L = c('c5814f'); WOOD = c('ad6a41'); WOOD_D = c('7b4b36'); WOOD_DD = c('572f27')
PLANK_L = c('e3b97c'); PLANK = c('d2a266'); PLANK_D = c('b98650')
CLOTH = c('f4ecd6'); CLOTH_D = c('d5c9a8')
RED = c('c0433a'); RED_D = c('8a2a26'); WHITE = c('f6f1e4'); BLUE = c('4a78b8'); BLUE_D = c('2f5185'); BLUE_L = c('86b0e0')
GREEN = c('5b9a4c'); GREEN_D = c('3a6a35'); YELLOW = c('e8c15a'); PINK = c('e89ab4'); PURPLE = c('9a6ab8'); ORANGE = c('e08a3a'); ORANGE_D = c('a85a22')
IRON = c('838a8c'); IRON_D = c('5d6466'); SLATE = c('3b4a45'); SLATE_L = c('55665f'); CHALK = c('e8efe6')
GLOW = c('f9dc82'); GLOW_D = c('e8a33a'); PAPER = c('d84a3a'); PAPER_L = c('f0745a'); PAPER_D = c('9a2e26')
CLEAR = (0, 0, 0, 0)


class Canvas:
    def __init__(self, w, h):
        self.w, self.h = w, h
        self.img = Image.new('RGBA', (w, h), CLEAR)
        self.px = self.img.load()

    def rect(self, x0, y0, x1, y1, col):
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                if 0 <= x < self.w and 0 <= y < self.h: self.px[x, y] = col

    def dot(self, x, y, col):
        if 0 <= x < self.w and 0 <= y < self.h: self.px[x, y] = col

    def outline(self):
        mark = []
        for y in range(self.h):
            for x in range(self.w):
                if self.px[x, y][3]: continue
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < self.w and 0 <= ny < self.h and self.px[nx, ny][3] and self.px[nx, ny] != INK:
                        mark.append((x, y)); break
        for x, y in mark: self.px[x, y] = INK
        return self

    def save(self, name):
        path = os.path.join(OUT, name + '.png')
        self.img.save(path)
        print('wrote', path, self.img.size)


def floor():
    k = Canvas(16, 16)
    k.rect(0, 0, 15, 15, PLANK)
    for y in (0, 4, 8, 12): k.rect(0, y, 15, y, PLANK_D)
    for (x, y) in [(3, 2), (9, 2), (13, 6), (5, 6), (2, 10), (11, 10), (7, 14), (14, 14)]: k.dot(x, y, PLANK_L); k.dot(x + 1, y, PLANK_L)
    for (x, y) in [(6, 1), (12, 5), (4, 9), (10, 13)]: k.dot(x, y + 1, PLANK_D)
    return k


def maypole():
    k = Canvas(16, 48)
    k.rect(7, 4, 8, 44, WHITE)
    for y in range(6, 44, 6): k.rect(7, y, 8, y + 2, RED)
    k.rect(7, 4, 7, 44, CLOTH_D) if False else None
    k.rect(5, 44, 10, 47, WOOD_D); k.rect(5, 44, 10, 44, WOOD)
    # the wreath of blossoms
    for (x, y, col) in [(3, 3, PINK), (5, 1, YELLOW), (7, 0, PINK), (9, 1, WHITE), (11, 3, PINK), (2, 5, WHITE), (12, 5, YELLOW), (4, 5, GREEN), (10, 5, GREEN), (6, 4, GREEN), (8, 4, GREEN)]:
        k.rect(x, y, x + 1, y + 1, col)
    # ribbons hanging from the wreath
    ribbons = [(2, PINK), (4, YELLOW), (5, BLUE_L), (10, PURPLE), (11, GREEN), (13, RED)]
    for i, (x, col) in enumerate(ribbons):
        length = 26 + (i % 3) * 3
        for y in range(7, 7 + length):
            k.dot(x + (1 if (y // 4) % 2 else 0) * (1 if x < 8 else -1), y, col)
    return k.outline()


def bunting(a, b, c3):
    k = Canvas(32, 16)
    k.rect(0, 2, 31, 2, WOOD_D)
    cols = [a, b, c3, b]
    for i in range(8):
        x = i * 4
        col = cols[i % 4]
        sag = (1, 2, 2, 1)[i % 4] if False else 0
        k.rect(x, 3, x + 2, 3, col); k.rect(x + 1, 4, x + 2, 4, col); k.rect(x + 1, 5, x + 1, 5, col)
        k.dot(x + 1, 3, tuple(min(255, v + 35) for v in col[:3]) + (255,))
    return k.outline()


def bandstand():
    k = Canvas(32, 32)
    k.rect(2, 22, 29, 30, WOOD_D); k.rect(2, 22, 29, 23, WOOD); k.rect(2, 30, 29, 30, WOOD_DD)
    for x in (4, 12, 20, 27): k.dot(x, 26, WOOD_DD)
    k.rect(3, 8, 4, 22, WOOD); k.rect(27, 8, 28, 22, WOOD)
    # the roof: red and white stripes
    for y in range(0, 8):
        for x in range(1 + (7 - y) // 2 if False else 1, 31):
            col = RED if (x // 3) % 2 == 0 else WHITE
            k.dot(x, y + 3, col)
    for x in range(0, 32): k.dot(x, 11, RED_D)
    # a drum and a stool, a fiddle on its stand
    k.rect(7, 18, 13, 21, RED_D); k.rect(7, 17, 13, 17, CLOTH); k.rect(8, 18, 12, 18, CLOTH_D)
    k.rect(18, 19, 19, 22, WOOD_D); k.rect(17, 18, 20, 18, WOOD_L)
    k.rect(23, 12, 24, 22, WOOD_D); k.rect(22, 14, 25, 17, YELLOW); k.dot(23, 15, WOOD_DD)
    return k.outline()


def scoreboard():
    k = Canvas(32, 32)
    # awning: blue and white stripes
    for x in range(1, 31):
        col = BLUE if (x // 3) % 2 == 0 else WHITE
        k.rect(x, 2, x, 7, col)
    k.rect(1, 8, 30, 8, BLUE_D)
    for x in range(1, 31, 3): k.dot(x, 9, BLUE if (x // 3) % 2 == 0 else WHITE)
    k.rect(2, 9, 3, 30, WOOD); k.rect(28, 9, 29, 30, WOOD)
    k.rect(4, 12, 27, 24, WOOD_DD); k.rect(5, 13, 26, 23, SLATE); k.rect(5, 13, 26, 13, SLATE_L)
    # chalk: a fish, tally marks and a list
    for (x, y) in [(7, 18), (8, 17), (9, 17), (10, 17), (11, 18), (10, 19), (9, 19), (8, 19), (12, 17), (12, 19), (13, 18)]: k.dot(x, y, CHALK)
    for i in range(4): k.rect(16 + i * 2, 15, 16 + i * 2, 18, CHALK)
    k.rect(16, 21, 25, 21, CHALK); k.rect(16, 23, 22, 23, CHALK)
    k.rect(4, 25, 27, 27, WOOD_L); k.rect(4, 27, 27, 27, WOOD_D)
    k.rect(2, 28, 29, 30, WOOD_D); k.rect(2, 28, 29, 28, WOOD)
    return k.outline()


def boat():
    k = Canvas(32, 16)
    for y in range(6, 13):
        inset = max(0, 4 - (y - 6) * 1 if y < 10 else 0) if False else (0 if y < 9 else y - 8)
        k.rect(2 + inset, y, 29 - inset, y, WOOD if y % 2 == 0 else WOOD_D)
    k.rect(2, 6, 29, 6, WOOD_L)
    k.rect(5, 4, 26, 5, WOOD_DD)
    k.rect(8, 8, 9, 8, WOOD_DD); k.rect(15, 8, 16, 8, WOOD_DD)
    k.rect(11, 1, 11, 8, WOOD_L); k.rect(10, 1, 12, 1, WOOD_D)      # an oar
    k.rect(20, 3, 20, 7, WOOD_L)
    return k.outline()


def fishcrate():
    k = Canvas(16, 16)
    k.rect(1, 6, 14, 14, WOOD); k.rect(1, 6, 14, 6, WOOD_L); k.rect(1, 10, 14, 10, WOOD_D); k.rect(1, 14, 14, 14, WOOD_DD)
    for (x, y, col) in [(3, 3, BLUE_L), (7, 2, c('a8c8d8')), (11, 3, BLUE_L)]:
        k.rect(x, y, x + 3, y + 3, col); k.dot(x + 3, y + 1, ORANGE if False else INK); k.dot(x, y + 2, WHITE)
    return k.outline()


def stall():
    k = Canvas(32, 32)
    for x in range(0, 32):
        col = RED if (x // 4) % 2 == 0 else WHITE
        k.rect(x, 3, x, 9, col)
    for x in range(0, 32, 4): k.rect(x, 10, x + 3, 10, RED if (x // 4) % 2 == 0 else WHITE)
    k.rect(0, 2, 31, 2, RED_D)
    k.rect(2, 11, 3, 29, WOOD); k.rect(28, 11, 29, 29, WOOD)
    k.rect(4, 22, 27, 29, WOOD_D); k.rect(4, 22, 27, 23, WOOD_L)
    # goods: pumpkins, a basket of apples, jars
    for x0 in (5, 12):
        k.rect(x0, 17, x0 + 5, 21, ORANGE); k.rect(x0, 17, x0 + 5, 17, ORANGE_D if False else c('f0a458')); k.rect(x0 + 2, 15, x0 + 3, 16, GREEN_D)
    k.rect(20, 18, 26, 21, WOOD_L); k.dot(21, 17, RED); k.dot(23, 17, RED); k.dot(25, 17, RED); k.rect(21, 18, 25, 18, RED)
    k.rect(28 - 1, 19, 28, 21, BLUE_L) if False else None
    return k.outline()


def judging():
    k = Canvas(32, 16)
    k.rect(1, 6, 30, 13, WOOD_D); k.rect(1, 6, 30, 6, WOOD)
    k.rect(0, 5, 31, 8, CLOTH); k.rect(0, 8, 31, 8, CLOTH_D)
    for x in range(1, 31, 4): k.rect(x, 9, x + 1, 9, CLOTH_D)
    # a three-step stand and rosettes
    k.rect(11, 3, 20, 5, WOOD_L); k.rect(13, 1, 18, 2, WOOD); k.rect(14, 0, 17, 0, WOOD_L)
    for (x, y, col) in [(14, 1, BLUE), (11, 3, RED), (19, 3, YELLOW)]:
        k.rect(x, y, x + 2, y + 1, col); k.dot(x + 1, y + 2, col)
    for (x, col) in [(4, BLUE), (7, RED), (24, YELLOW), (27, PURPLE)]:
        k.rect(x, 3, x + 1, 4, col); k.dot(x, 5, col) if False else None
    return k.outline()


def pumpkins():
    k = Canvas(16, 16)
    for (x, y, w, col) in [(1, 8, 7, ORANGE), (8, 8, 7, ORANGE), (4, 3, 7, c('f0a458'))]:
        k.rect(x, y, x + w - 1, y + 5, col); k.rect(x, y, x + w - 1, y, c('f6bf7a')); k.rect(x + w // 2 - 1, y + 1, x + w // 2 - 1, y + 5, ORANGE_D)
        k.rect(x + w // 2 - 1, y - 2, x + w // 2, y - 1, GREEN_D)
    return k.outline()


def feasttable():
    k = Canvas(32, 16)
    k.rect(0, 6, 31, 12, CLOTH); k.rect(0, 6, 31, 6, WHITE); k.rect(0, 12, 31, 12, CLOTH_D)
    for x in range(1, 31, 4): k.rect(x, 13, x + 1, 14, CLOTH_D)
    k.rect(2, 14, 3, 15, WOOD_DD); k.rect(28, 14, 29, 15, WOOD_DD)
    # dishes: a roast, a pie, bowls, candles
    k.rect(4, 3, 9, 6, WOOD_L); k.rect(5, 2, 8, 3, ORANGE_D); k.rect(5, 3, 8, 5, c('b0602a'))
    k.rect(12, 4, 17, 6, c('d8c08a')); k.rect(13, 3, 16, 4, c('c9a45a')); k.dot(14, 3, RED)
    k.rect(21, 4, 24, 6, BLUE_L); k.rect(21, 4, 24, 4, WHITE)
    k.rect(27, 2, 27, 5, WHITE); k.dot(27, 1, GLOW); k.dot(27, 0, GLOW_D)
    k.rect(19, 3, 19, 5, WHITE); k.dot(19, 2, GLOW)
    return k.outline()


def lantern(lit):
    k = Canvas(16, 32)
    k.rect(7, 8, 8, 31, WOOD); k.rect(7, 8, 7, 31, WOOD_L)
    k.rect(4, 4, 11, 5, WOOD_D); k.rect(7, 2, 8, 3, WOOD_D)
    k.rect(6, 28, 9, 31, WOOD_DD)
    # the paper lantern hanging from the arm
    top = 8
    body = GLOW if lit else PAPER
    shade = GLOW_D if lit else PAPER_D
    light = c('fff0b8') if lit else PAPER_L
    k.rect(5, top, 10, top, WOOD_DD)
    k.rect(4, top + 1, 11, top + 9, body); k.rect(5, top + 10, 10, top + 11, body)
    k.rect(4, top + 1, 4, top + 9, shade); k.rect(11, top + 1, 11, top + 9, shade)
    k.rect(6, top + 2, 9, top + 7, light)
    for y in (top + 3, top + 6): k.rect(4, y, 11, y, shade)
    k.rect(7, top + 12, 8, top + 13, WOOD_DD)
    if lit:
        for (x, y) in [(2, 12), (13, 14), (3, 18), (12, 10)]: k.dot(x, y, c('fbe9a8'))
    return k.outline()


def lanternrack(lit):
    k = Canvas(16, 32)
    k.rect(1, 6, 2, 31, WOOD); k.rect(13, 6, 14, 31, WOOD)
    k.rect(0, 4, 15, 6, WOOD_D); k.rect(0, 4, 15, 4, WOOD_L)
    k.rect(1, 28, 14, 31, WOOD_D)
    # three lanterns hung from the beam: unlit paper or lit
    for i, x in enumerate((3, 7, 11)):
        body = [PAPER, c('4a78b8'), GREEN][i] if not lit else [GLOW, c('c8e0ff'), c('d8f0a0')][i]
        k.rect(x + 1, 7, x + 1, 9, WOOD_DD)
        k.rect(x, 10, x + 2, 18, body); k.rect(x, 10, x, 18, tuple(max(0, v - 40) for v in body[:3]) + (255,))
        k.rect(x + 1, 11, x + 1, 16, tuple(min(255, v + 40) for v in body[:3]) + (255,))
        k.rect(x, 14, x + 2, 14, tuple(max(0, v - 40) for v in body[:3]) + (255,))
    if lit:
        for (x, y) in [(0, 20), (15, 22), (5, 22), (10, 21)]: k.dot(x, y, c('fbe9a8'))
    return k.outline()


floor().save('obj_fest_floor')
maypole().save('prop_fest_maypole')
bunting(PINK, YELLOW, WHITE).save('prop_fest_bunting_spring')
bunting(BLUE, WHITE, YELLOW).save('prop_fest_bunting_summer')
bunting(ORANGE, RED, YELLOW).save('prop_fest_bunting_fall')
bunting(GLOW, PAPER, GLOW_D).save('prop_fest_bunting_winter')
bandstand().save('prop_fest_bandstand')
scoreboard().save('prop_fest_scoreboard')
boat().save('prop_fest_boat')
fishcrate().save('prop_fest_fishcrate')
stall().save('prop_fest_stall')
judging().save('prop_fest_judging')
pumpkins().save('prop_fest_pumpkins')
feasttable().save('prop_fest_feasttable')
lantern(False).save('prop_fest_lantern')
lantern(True).save('prop_fest_lantern_lit')
lanternrack(False).save('prop_fest_lanternrack')
lanternrack(True).save('prop_fest_lanternrack_lit')
