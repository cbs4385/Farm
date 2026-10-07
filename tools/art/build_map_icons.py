"""Icons for the pictorial map in the game menu (playtest 2026-10-07: the map did not match the world; each building now has a small picture).
Writes 16 x 16 sprites called ui_map_* into Assets/_Project/Art/Placeholders (ui_ sprites are collected into the UiArt asset by Farm > Setup > Build UI Art).
Usage: python tools/art/build_map_icons.py"""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")

OUTLINE = (60, 40, 32, 255)
CLEAR = (0, 0, 0, 0)
WHITE = (250, 248, 240, 255)
GREY = (150, 150, 158, 255)
GREY_D = (96, 96, 108, 255)
RED = (212, 52, 48, 255)
RED_D = (150, 32, 36, 255)
WOOD = (176, 120, 74, 255)
WOOD_D = (120, 78, 48, 255)
CREAM = (240, 226, 190, 255)
GOLD = (252, 204, 64, 255)
GREEN = (74, 140, 84, 255)
GREEN_D = (42, 92, 58, 255)
BLUE = (110, 170, 214, 255)


class Canvas:
    def __init__(self):
        self.img = Image.new("RGBA", (16, 16), CLEAR)
        self.px = self.img.load()

    def dot(self, x, y, c):
        if 0 <= x < 16 and 0 <= y < 16:
            self.px[x, y] = c

    def rect(self, x0, y0, x1, y1, c):
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                self.dot(x, y, c)

    def box(self, x0, y0, x1, y1, fill, edge=OUTLINE):
        self.rect(x0, y0, x1, y1, edge)
        self.rect(x0 + 1, y0 + 1, x1 - 1, y1 - 1, fill)

    def save(self, name):
        self.img.save(os.path.join(OUT, name + ".png"))


def clinic():
    c = Canvas()
    c.box(1, 1, 14, 14, WHITE, GREY_D)
    c.rect(6, 3, 9, 12, RED)                       # a red cross
    c.rect(3, 6, 12, 9, RED)
    c.rect(6, 3, 6, 12, (240, 96, 84, 255)); c.rect(3, 6, 12, 6, (240, 96, 84, 255))
    c.save("ui_map_clinic")


def library():
    c = Canvas()
    c.rect(0, 3, 15, 13, OUTLINE)                  # an open book
    c.rect(1, 4, 7, 12, CREAM); c.rect(8, 4, 14, 12, CREAM)
    c.rect(7, 3, 8, 13, WOOD_D)
    for y in (6, 8, 10):
        c.rect(2, y, 6, y, GREY); c.rect(9, y, 13, y, GREY)
    c.rect(0, 13, 15, 14, (60, 90, 150, 255))      # the blue cover under it
    c.rect(1, 14, 14, 14, (40, 62, 110, 255))
    c.save("ui_map_library")


def store():
    c = Canvas()
    c.rect(1, 2, 14, 6, OUTLINE)                   # a striped awning with a scalloped edge
    for i, x in enumerate(range(2, 14, 2)):
        c.rect(x, 3, x + 1, 5, RED if i % 2 == 0 else WHITE)
        c.dot(x, 6, RED if i % 2 == 0 else WHITE); c.dot(x + 1, 6, RED if i % 2 == 0 else WHITE)
    c.box(2, 7, 13, 14, (214, 184, 132, 255))      # the shop front
    c.rect(4, 9, 7, 12, BLUE); c.rect(4, 9, 7, 9, WHITE)
    c.rect(9, 9, 12, 14, WOOD_D)                   # the door
    c.dot(11, 12, GOLD)
    c.save("ui_map_store")


def smith():
    c = Canvas()
    c.rect(0, 5, 4, 7, OUTLINE); c.rect(1, 6, 3, 6, GREY)          # the horn
    c.rect(3, 4, 14, 8, OUTLINE); c.rect(4, 5, 13, 7, GREY); c.rect(4, 5, 13, 5, (190, 190, 198, 255))
    c.rect(6, 9, 11, 11, OUTLINE); c.rect(7, 9, 10, 10, GREY_D)    # the waist
    c.rect(4, 12, 13, 14, OUTLINE); c.rect(5, 12, 12, 13, GREY_D)  # the foot
    for (x, y) in ((11, 1), (13, 2), (9, 2), (14, 4)):             # sparks
        c.dot(x, y, (255, 170, 50, 255))
    c.dot(12, 2, GOLD)
    c.save("ui_map_smith")


def carpenter():
    c = Canvas()
    c.rect(1, 12, 14, 14, OUTLINE); c.rect(2, 12, 13, 13, (214, 170, 110, 255))   # a plank
    c.rect(7, 5, 8, 12, WOOD_D)                                                    # a hammer
    c.rect(3, 2, 12, 5, OUTLINE); c.rect(4, 3, 11, 4, GREY); c.rect(4, 3, 11, 3, (190, 190, 198, 255))
    c.dot(13, 3, GREY_D); c.dot(13, 4, GREY_D)
    c.save("ui_map_carpenter")


def saloon():
    c = Canvas()
    c.rect(2, 3, 10, 14, OUTLINE)                                  # a beer mug
    c.rect(3, 5, 9, 13, (240, 170, 50, 255)); c.rect(3, 5, 4, 13, (255, 206, 96, 255))
    c.rect(2, 2, 10, 4, OUTLINE); c.rect(3, 2, 9, 3, WHITE)          # foam
    c.dot(2, 1, WHITE); c.dot(5, 1, WHITE); c.dot(8, 1, WHITE)
    c.rect(11, 5, 14, 12, OUTLINE); c.rect(12, 6, 13, 11, CLEAR)     # the handle
    c.rect(11, 6, 11, 11, OUTLINE)
    c.save("ui_map_saloon")


def hall():
    c = Canvas()
    c.rect(7, 0, 7, 4, OUTLINE); c.rect(8, 0, 11, 2, GOLD)            # a flag on the roof
    c.rect(1, 5, 14, 6, OUTLINE); c.rect(2, 5, 13, 5, RED)            # the roof
    c.box(2, 6, 13, 14, CREAM)
    for x in (3, 7, 11): c.rect(x, 7, x + 1, 13, WHITE)               # columns
    c.rect(6, 10, 9, 14, WOOD_D)                                      # the doors
    c.save("ui_map_hall")


def house():
    c = Canvas()
    c.rect(10, 1, 12, 5, OUTLINE); c.rect(11, 1, 11, 4, GREY_D)       # the chimney
    c.rect(0, 6, 15, 7, OUTLINE)                                      # a roof
    for i in range(6):
        c.rect(7 - i, 1 + i, 8 + i, 1 + i, RED)
        c.dot(7 - i - 1, 1 + i, OUTLINE); c.dot(8 + i + 1, 1 + i, OUTLINE)
    c.rect(1, 6, 14, 6, RED_D)
    c.box(2, 7, 13, 14, CREAM)
    c.rect(6, 9, 9, 14, WOOD_D)                                       # the door
    c.rect(3, 9, 5, 11, BLUE); c.rect(10, 9, 12, 11, BLUE)
    c.save("ui_map_house")


def coop():
    c = Canvas()
    c.rect(3, 6, 11, 12, OUTLINE); c.rect(4, 7, 10, 11, WHITE)        # a hen
    c.rect(1, 5, 3, 8, OUTLINE); c.rect(2, 6, 3, 7, WHITE)            # tail
    c.rect(10, 3, 13, 7, OUTLINE); c.rect(11, 4, 12, 6, WHITE)        # head
    c.rect(11, 2, 12, 3, RED); c.dot(11, 7, RED)                      # comb and wattle
    c.rect(13, 5, 14, 5, GOLD)                                        # beak
    c.dot(12, 4, OUTLINE)                                             # eye
    c.rect(6, 8, 8, 9, (230, 220, 200, 255))                          # wing
    c.rect(6, 13, 6, 14, (240, 150, 40, 255)); c.rect(8, 13, 8, 14, (240, 150, 40, 255))
    c.rect(5, 15, 7, 15, (240, 150, 40, 255)); c.rect(7, 15, 9, 15, (240, 150, 40, 255))
    c.save("ui_map_coop")


def barn():
    c = Canvas()
    c.rect(1, 2, 14, 6, OUTLINE)                                      # the gambrel roof
    c.rect(2, 3, 13, 5, (120, 60, 52, 255)); c.rect(4, 2, 11, 2, (150, 78, 66, 255))
    c.box(1, 6, 14, 14, RED)
    c.rect(4, 8, 11, 14, OUTLINE); c.rect(5, 9, 10, 14, (230, 226, 214, 255))   # a white-trimmed door with a cross
    for i in range(6):
        c.dot(5 + i, 9 + i, RED); c.dot(10 - i, 9 + i, RED)
    c.rect(7, 3, 8, 4, (230, 226, 214, 255))                          # a hayloft window
    c.save("ui_map_barn")


def greenhouse():
    c = Canvas()
    c.rect(1, 5, 14, 14, OUTLINE)
    c.rect(2, 6, 13, 13, (190, 228, 240, 255))                        # panes of glass
    c.rect(4, 1, 11, 4, OUTLINE); c.rect(5, 2, 10, 4, (190, 228, 240, 255))   # the raised roof
    c.rect(1, 5, 14, 5, WHITE)
    for x in (5, 8, 11): c.rect(x, 2, x, 13, WHITE)                   # the frame
    c.rect(2, 9, 13, 9, WHITE)
    c.rect(3, 11, 6, 13, GREEN); c.rect(9, 10, 12, 13, GREEN); c.rect(4, 10, 5, 10, GREEN_D)
    c.rect(2, 13, 13, 13, WOOD_D)
    c.save("ui_map_greenhouse")


def mine():
    c = Canvas()
    c.rect(1, 6, 14, 15, OUTLINE)                                     # a mine mouth in a rock face
    c.rect(2, 7, 13, 15, (130, 122, 116, 255)); c.rect(2, 7, 13, 8, (166, 158, 150, 255))
    c.rect(5, 8, 10, 15, OUTLINE); c.rect(6, 9, 9, 15, (28, 24, 28, 255))   # the dark opening
    c.rect(4, 7, 4, 15, WOOD_D); c.rect(11, 7, 11, 15, WOOD_D); c.rect(4, 7, 11, 7, WOOD_D)   # timber frame
    c.rect(11, 1, 12, 5, WOOD_D)                                      # a pickaxe above
    c.rect(8, 1, 14, 2, GREY); c.dot(8, 2, GREY_D); c.dot(14, 2, GREY_D)
    c.save("ui_map_mine")


def forest():
    c = Canvas()
    c.rect(7, 12, 8, 15, WOOD_D)                                      # trunk
    for (y0, half) in ((1, 2), (4, 4), (8, 6)):                       # a pine in three tiers
        for i in range(4):
            w = half * (i + 1) // 4 + 1
            c.rect(8 - w, y0 + i, 7 + w, y0 + i, GREEN if i < 2 else GREEN_D)
    c.rect(2, 11, 13, 11, GREEN_D)
    c.save("ui_map_forest")


def woods():
    c = Canvas()
    c.rect(7, 11, 8, 15, (84, 56, 44, 255))
    c.rect(3, 2, 12, 11, (38, 78, 60, 255))                          # a dark round crown
    c.rect(2, 4, 13, 9, (38, 78, 60, 255)); c.rect(4, 1, 11, 1, (38, 78, 60, 255))
    c.rect(4, 3, 7, 5, (62, 112, 84, 255)); c.rect(9, 6, 11, 8, (26, 56, 44, 255))
    c.save("ui_map_woods")


def beach():
    c = Canvas()
    c.rect(1, 3, 14, 13, OUTLINE)                                    # a scallop shell
    c.rect(2, 4, 13, 12, (250, 190, 190, 255))
    for x in range(3, 13, 2): c.rect(x, 4, x, 11, (226, 140, 150, 255))
    c.rect(1, 3, 3, 4, CLEAR); c.rect(12, 3, 14, 4, CLEAR)
    c.rect(5, 13, 10, 15, OUTLINE); c.rect(6, 13, 9, 14, (230, 160, 160, 255))
    c.save("ui_map_beach")


def here():
    c = Canvas()
    c.rect(3, 0, 12, 2, OUTLINE); c.rect(1, 2, 14, 9, OUTLINE); c.rect(3, 10, 12, 11, OUTLINE)    # a gold map pin
    c.rect(5, 12, 10, 13, OUTLINE); c.rect(7, 14, 8, 15, OUTLINE)
    c.rect(4, 1, 11, 1, GOLD); c.rect(2, 2, 13, 9, GOLD); c.rect(4, 10, 11, 10, GOLD)
    c.rect(6, 11, 9, 12, GOLD); c.rect(7, 13, 8, 14, GOLD)
    c.rect(3, 3, 5, 5, (255, 240, 170, 255))
    c.rect(6, 4, 9, 7, OUTLINE); c.rect(7, 5, 8, 6, WHITE)           # the hole in the pin
    c.save("ui_map_here")


ICONS = ("ui_map_clinic", "ui_map_library", "ui_map_store", "ui_map_smith", "ui_map_carpenter", "ui_map_saloon", "ui_map_hall", "ui_map_house",
         "ui_map_coop", "ui_map_barn", "ui_map_greenhouse", "ui_map_mine", "ui_map_forest", "ui_map_woods", "ui_map_beach", "ui_map_here")

if __name__ == "__main__":
    clinic(); library(); store(); smith(); carpenter(); saloon(); hall(); house()
    coop(); barn(); greenhouse(); mine(); forest(); woods(); beach(); here()
    final = os.path.join(OUT, "final_art.txt")
    have = set(open(final).read().split())
    with open(final, "a") as f:
        for n in ICONS:
            if n not in have: f.write(n + "\n")
    # a contact sheet to look at
    sheet = Image.new("RGBA", (16 * 8 + 8 * 9, 16 * 2 + 3 * 4), (214, 190, 140, 255))
    for i, n in enumerate(ICONS):
        im = Image.open(os.path.join(OUT, n + ".png"))
        sheet.paste(im, (4 + (i % 8) * 24, 4 + (i // 8) * 20), im)
    sheet.resize((sheet.width * 6, sheet.height * 6), Image.NEAREST).save(os.path.join(ROOT, "Builds", "map_icons_sheet.png"))
    print("done")
