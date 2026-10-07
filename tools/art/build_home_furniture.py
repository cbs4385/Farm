"""Farmhouse furniture (the bed, fireplace, rug, couch and armchair) and the farm mailbox, drawn by the project (playtest 2026-10-06: the bed was tiny, the mailbox hard to see, the house bare).
Writes into Assets/_Project/Art/Placeholders: obj_bed_double (32 x 32, two cells square), obj_fireplace (32 x 32), obj_rug_large (32 x 32, walkable),
obj_mailbox_tall (16 x 32, a bold red box on a white post with a raised yellow flag).
Usage: python tools/art/build_home_furniture.py"""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")

OUTLINE = (84, 52, 36, 255)
CLEAR = (0, 0, 0, 0)


class Canvas:
    def __init__(self, w, h):
        self.img = Image.new("RGBA", (w, h), CLEAR)
        self.px = self.img.load()
        self.w, self.h = w, h

    def rect(self, x0, y0, x1, y1, c):
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                if 0 <= x < self.w and 0 <= y < self.h: self.px[x, y] = c

    def box(self, x0, y0, x1, y1, fill, edge=OUTLINE):
        self.rect(x0, y0, x1, y1, edge)
        self.rect(x0 + 1, y0 + 1, x1 - 1, y1 - 1, fill)

    def save(self, name):
        # Image rows run top to bottom here; the drawings below use y = 0 at the top.
        self.img.save(os.path.join(OUT, name + ".png"))


def bed_double():
    c = Canvas(32, 32)
    wood, wood_l = (150, 92, 54, 255), (186, 124, 76, 255)
    c.box(1, 1, 30, 30, wood)                              # the frame
    c.rect(2, 2, 29, 4, wood_l)                            # the headboard, lit
    c.rect(2, 5, 29, 5, (118, 70, 42, 255))
    quilt, quilt_l, quilt_d = (90, 140, 196, 255), (126, 174, 222, 255), (66, 110, 160, 255)
    c.box(3, 14, 28, 28, quilt, (52, 80, 120, 255))        # the quilt
    c.rect(4, 15, 27, 15, quilt_l)
    for x in range(5, 28, 6):                              # stitched squares
        for y in range(18, 28, 5):
            c.rect(x, y, x + 2, y + 2, quilt_l)
    c.rect(4, 27, 27, 27, quilt_d)
    c.rect(3, 12, 28, 13, (244, 238, 226, 255))            # the turned-down sheet
    c.rect(3, 13, 28, 13, (206, 198, 184, 255))
    for x0 in (5, 17):                                     # two pillows
        c.box(x0, 6, x0 + 9, 11, (250, 246, 238, 255), (150, 140, 128, 255))
        c.rect(x0 + 2, 8, x0 + 7, 8, (222, 214, 200, 255))
    c.rect(1, 29, 3, 30, OUTLINE); c.rect(28, 29, 30, 30, OUTLINE)   # feet
    c.save("obj_bed_double")


def fireplace():
    c = Canvas(32, 32)
    stone, stone_l, stone_d = (148, 142, 146, 255), (182, 176, 178, 255), (112, 106, 112, 255)
    c.rect(0, 0, 31, 4, (110, 70, 44, 255))                # the mantel shelf
    c.rect(0, 0, 31, 0, OUTLINE); c.rect(0, 4, 31, 4, OUTLINE)
    c.rect(1, 1, 30, 2, (172, 118, 74, 255))
    c.rect(2, 5, 29, 31, OUTLINE)                          # the chimney breast
    c.rect(3, 5, 28, 30, stone)
    for y in range(6, 30, 5):                              # stone courses
        c.rect(3, y, 28, y, stone_d)
        off = 0 if (y // 5) % 2 else 4
        for x in range(3 + off, 29, 8): c.rect(x, y, x, y + 4, stone_d)
    c.rect(3, 5, 28, 5, stone_l)
    c.rect(7, 14, 24, 30, OUTLINE)                         # the opening
    c.rect(8, 15, 23, 30, (36, 26, 28, 255))
    c.rect(8, 15, 23, 15, (60, 44, 44, 255))
    c.rect(9, 27, 22, 29, (92, 58, 36, 255))               # logs
    c.rect(9, 27, 22, 27, (126, 82, 50, 255))
    flame = [((16, 17), (255, 196, 70, 255)), ((12, 21), (240, 130, 40, 255)), ((19, 21), (240, 130, 40, 255)), ((15, 20), (255, 232, 140, 255))]
    for (fx, fy), col in flame:
        c.rect(fx - 1, fy, fx + 1, 26, col)
        c.rect(fx, fy - 2, fx, fy - 1, col)
    c.rect(11, 24, 21, 26, (236, 120, 36, 255))
    c.rect(13, 25, 19, 26, (255, 214, 96, 255))
    c.save("obj_fireplace")


def rug_large():
    c = Canvas(32, 32)
    red, red_d, cream, blue = (176, 62, 58, 255), (130, 40, 44, 255), (236, 216, 170, 255), (70, 98, 150, 255)
    c.rect(1, 1, 30, 30, red_d)
    c.rect(2, 2, 29, 29, cream)
    c.rect(3, 3, 28, 28, red)
    c.rect(5, 5, 26, 26, blue)
    c.rect(6, 6, 25, 25, red)
    for i in range(4):                                      # a diamond in the middle
        c.rect(15 - i, 12 + i, 16 + i, 12 + i, cream)
        c.rect(15 - i, 19 - i, 16 + i, 19 - i, cream)
    for y in (0, 31):                                       # fringe
        for x in range(2, 30, 2): c.rect(x, y, x, y, cream)
    c.save("obj_rug_large")


def couch():
    # Playtest 2026-10-06: the old couch was a 16 x 16 teal smudge that nobody recognised. A front view, two cells wide: backrest, two seat
    # cushions, armrests with a patchwork blanket on one, and legs. Warm red so it cannot be mistaken for the teal armchair.
    c = Canvas(32, 16)
    red, red_l, red_d = (178, 66, 62, 255), (214, 104, 92, 255), (124, 42, 48, 255)
    gold, gold_l, gold_d = (232, 196, 128, 255), (248, 224, 164, 255), (190, 150, 92, 255)
    c.box(2, 1, 29, 9, red)                                 # the backrest
    c.rect(3, 2, 28, 3, red_l)
    c.rect(3, 8, 28, 8, red_d)
    c.rect(2, 1, 3, 1, CLEAR); c.rect(28, 1, 29, 1, CLEAR)    # rounded top corners
    c.box(4, 9, 27, 13, gold, OUTLINE)                      # the seat: two cushions
    c.rect(5, 10, 14, 10, gold_l); c.rect(17, 10, 26, 10, gold_l)
    c.rect(5, 12, 14, 12, gold_d); c.rect(17, 12, 26, 12, gold_d)
    c.rect(15, 9, 16, 13, OUTLINE)
    c.box(0, 5, 4, 14, red_d)                               # the armrests
    c.rect(1, 6, 3, 6, red)
    c.box(27, 5, 31, 14, red_d)
    c.rect(28, 6, 30, 6, red)
    blanket = [(70, 112, 170, 255), (240, 232, 208, 255)]   # a patchwork blanket over the right arm
    for i, y in enumerate(range(7, 14)):
        for j, x in enumerate(range(28, 31)):
            c.rect(x, y, x, y, blanket[(i + j) % 2])
    c.rect(1, 15, 2, 15, OUTLINE); c.rect(29, 15, 30, 15, OUTLINE)    # legs
    c.rect(6, 14, 8, 14, OUTLINE); c.rect(23, 14, 25, 14, OUTLINE)
    c.save("obj_couch")


def armchair():
    # Playtest 2026-10-07: like the old couch, the AI-drawn armchair was a small teal blob. A crisp front view: tall tufted backrest, a gold seat
    # cushion (the same gold as the couch's, so they read as a set), padded arms and legs. Green, so it differs from the red couch.
    c = Canvas(16, 16)
    green, green_l, green_d = (86, 140, 98, 255), (130, 184, 134, 255), (52, 92, 66, 255)
    gold, gold_l, gold_d = (232, 196, 128, 255), (248, 224, 164, 255), (190, 150, 92, 255)
    c.box(2, 1, 13, 9, green)                               # the backrest
    c.rect(3, 2, 12, 3, green_l)
    c.rect(3, 8, 12, 8, green_d)
    c.rect(2, 1, 2, 1, CLEAR); c.rect(13, 1, 13, 1, CLEAR)    # rounded top corners
    for x in (5, 8, 11): c.rect(x, 5, x, 5, green_d)        # tufting buttons
    c.box(3, 9, 12, 12, gold)                               # the seat cushion
    c.rect(4, 10, 11, 10, gold_l)
    c.rect(4, 12, 11, 12, gold_d)
    c.box(0, 6, 3, 13, green_d)                             # the padded arms
    c.rect(1, 7, 2, 7, green)
    c.box(12, 6, 15, 13, green_d)
    c.rect(13, 7, 14, 7, green)
    c.rect(1, 14, 2, 15, OUTLINE); c.rect(13, 14, 14, 15, OUTLINE)    # legs
    c.rect(5, 13, 6, 13, OUTLINE); c.rect(9, 13, 10, 13, OUTLINE)
    c.save("obj_armchair")


def mailbox_tall():
    c = Canvas(16, 32)
    post, post_d = (244, 240, 230, 255), (176, 168, 156, 255)
    c.rect(6, 14, 9, 28, OUTLINE)                           # the post
    c.rect(7, 14, 8, 28, post)
    c.rect(8, 14, 8, 28, post_d)
    c.rect(4, 28, 11, 30, OUTLINE)                          # the foot
    c.rect(5, 28, 10, 29, (128, 100, 70, 255))
    red, red_l, red_d = (214, 48, 44, 255), (246, 104, 84, 255), (150, 28, 32, 255)
    c.rect(1, 3, 14, 14, OUTLINE)                           # the box, with a rounded top
    c.rect(2, 4, 13, 13, red)
    c.rect(2, 4, 13, 5, red_l)
    c.rect(2, 12, 13, 13, red_d)
    c.rect(1, 3, 2, 3, CLEAR); c.rect(13, 3, 14, 3, CLEAR)
    c.rect(5, 8, 10, 11, OUTLINE)                           # the door, with a white letter in its slot
    c.rect(6, 9, 9, 10, (36, 24, 24, 255))
    c.rect(5, 6, 10, 7, (250, 250, 244, 255))
    c.rect(5, 6, 10, 6, (200, 200, 190, 255))
    c.rect(12, 0, 13, 9, OUTLINE)                           # the raised flag, bright yellow so it shows from far away
    c.rect(13, 1, 13, 8, (255, 214, 40, 255))
    c.rect(10, 1, 12, 4, OUTLINE)
    c.rect(11, 2, 12, 3, (255, 214, 40, 255))
    c.save("obj_mailbox_tall")


if __name__ == "__main__":
    bed_double(); fireplace(); rug_large(); mailbox_tall(); couch(); armchair()
    final = os.path.join(OUT, "final_art.txt")
    have = set(open(final).read().split())
    with open(final, "a") as f:
        for n in ("obj_bed_double", "obj_fireplace", "obj_rug_large", "obj_mailbox_tall"):
            if n not in have: f.write(n + "\n")
    print("done")
