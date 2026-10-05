"""Avatar paper-doll layers (playtest request, 2026-10-05): the farmer is built from layers, each with several styles, that can be combined freely and
recoloured: body (two builds), hair (6 styles), shirt (5), pants (4) and accessory (7, including none). Every layer is a 16 x 32 grid per facing
(down, up, left; right is the mirror of left) drawn here from shared body geometry, so every combination lines up. The result is written as text to
Assets/_Project/Resources/Avatar/avatar_layers.txt (editable by hand, replaceable by final art with the same format) and previews go to Builds/.

Cell characters (lower case = the colour, upper case = its shade):  s skin  h hair  t shirt  p pants  a accessory  w white cloth  k dark (eyes)  b boots  . empty
Usage: python tools/art/build_avatar_layers.py [--preview]"""
import os, sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
OUT = os.path.join(ROOT, "Assets", "_Project", "Resources", "Avatar", "avatar_layers.txt")
W, H = 16, 32
FACINGS = ["down", "up", "left"]


def blank(): return [["."] * W for _ in range(H)]
def put(g, x, y, c):
    if 0 <= x < W and 0 <= y < H: g[y][x] = c
def hline(g, y, x0, x1, c):
    for x in range(x0, x1 + 1): put(g, x, y, c)
def rect(g, x0, y0, x1, y1, c):
    for y in range(y0, y1 + 1): hline(g, y, x0, x1, c)
def mirror(g): return [list(reversed(r)) for r in g]


# ----------------------------------------------------------------------------------------------------------------------------------------------
# bodies
# ----------------------------------------------------------------------------------------------------------------------------------------------
def head_front(g, face=True):
    hline(g, 8, 5, 10, "s"); hline(g, 9, 4, 11, "s")
    for y in range(10, 16): hline(g, y, 3, 12, "s")
    hline(g, 16, 4, 11, "s"); hline(g, 17, 5, 10, "s")
    if face:
        for y in (13, 14): put(g, 5, y, "k"); put(g, 10, y, "k")
        hline(g, 16, 7, 8, "S")
        put(g, 4, 15, "S"); put(g, 11, 15, "S")
    hline(g, 18, 6, 9, "S")


def head_left(g):
    hline(g, 8, 6, 10, "s"); hline(g, 9, 5, 11, "s")
    for y in range(10, 16): hline(g, y, 4, 11, "s")
    hline(g, 16, 5, 10, "s"); hline(g, 17, 6, 9, "s")
    put(g, 3, 14, "s")                                  # the nose
    for y in (13, 14): put(g, 5, y, "k")
    rect(g, 8, 13, 9, 15, "S")                          # the ear
    hline(g, 18, 6, 8, "S")


def body(build, facing):
    g = blank()
    fem = build == "feminine"
    if facing in ("down", "up"):
        head_front(g, face=(facing == "down"))
        if facing == "up": hline(g, 17, 5, 10, "S")
        if fem:
            rect(g, 5, 19, 10, 24, "s"); rect(g, 4, 25, 11, 26, "s")
            rect(g, 3, 19, 4, 26, "s"); rect(g, 11, 19, 12, 26, "s"); hline(g, 27, 3, 4, "s"); hline(g, 27, 11, 12, "s")
            rect(g, 5, 27, 7, 29, "s"); rect(g, 8, 27, 10, 29, "s"); rect(g, 7, 27, 8, 29, "S")
            rect(g, 4, 30, 7, 31, "b"); rect(g, 8, 30, 11, 31, "b"); hline(g, 31, 4, 11, "B")
        else:
            rect(g, 4, 19, 11, 26, "s")
            rect(g, 2, 19, 3, 26, "s"); rect(g, 12, 19, 13, 26, "s"); hline(g, 27, 2, 3, "s"); hline(g, 27, 12, 13, "s")
            rect(g, 4, 27, 7, 29, "s"); rect(g, 8, 27, 11, 29, "s"); rect(g, 7, 27, 8, 29, "S")
            rect(g, 3, 30, 7, 31, "b"); rect(g, 8, 30, 12, 31, "b"); hline(g, 31, 3, 12, "B")
    else:
        head_left(g)
        if fem:
            rect(g, 6, 19, 10, 26, "s"); rect(g, 6, 20, 8, 26, "S"); hline(g, 27, 6, 8, "s")
            rect(g, 6, 27, 9, 29, "s"); rect(g, 4, 30, 8, 31, "b"); hline(g, 31, 4, 8, "B")
        else:
            rect(g, 5, 19, 10, 26, "s"); rect(g, 6, 20, 9, 26, "S"); hline(g, 27, 6, 8, "s")
            rect(g, 5, 27, 9, 29, "s"); rect(g, 3, 30, 8, 31, "b"); hline(g, 31, 3, 8, "B")
    return g


# ----------------------------------------------------------------------------------------------------------------------------------------------
# shirts, pants
# ----------------------------------------------------------------------------------------------------------------------------------------------
def shirt(style, build, facing):
    g = blank()
    fem = build == "feminine"
    if facing in ("down", "up"):
        x0, x1 = (5, 10) if fem else (4, 11)
        ax0, ax1 = (3, 12) if fem else (2, 13)
        top = 19
        rect(g, x0, top, x1, 24, "t"); hline(g, 24, x0, x1, "T")
        if fem: rect(g, 4, 25, 11, 25, "t"); hline(g, 25, 4, 11, "T")
        if facing == "down": hline(g, 19, 7, 8, "s")              # the neckline
        if style == "tshirt":
            rect(g, ax0, 19, ax0 + 1, 21, "t"); rect(g, ax1 - 1, 19, ax1, 21, "t")
            hline(g, 21, ax0, ax0 + 1, "T"); hline(g, 21, ax1 - 1, ax1, "T")
        elif style == "longsleeve":
            rect(g, ax0, 19, ax0 + 1, 26, "t"); rect(g, ax1 - 1, 19, ax1, 26, "t")
            hline(g, 26, ax0, ax0 + 1, "T"); hline(g, 26, ax1 - 1, ax1, "T")
        elif style == "overalls":
            rect(g, ax0, 19, ax0 + 1, 22, "t"); rect(g, ax1 - 1, 19, ax1, 22, "t")      # short sleeves under the straps
            rect(g, 5, 22, 10, 26, "p")                                                 # the bib
            hline(g, 26, 5, 10, "P")
            for y in range(19, 22): put(g, 5, y, "p"); put(g, 10, y, "p")                # the straps
            put(g, 5, 22, "k"); put(g, 10, 22, "k")                                      # the buttons
            if facing == "up": rect(g, 5, 22, 10, 22, "p")
        elif style == "vest":
            rect(g, ax0, 19, ax0 + 1, 22, "w"); rect(g, ax1 - 1, 19, ax1, 22, "w")     # the shirt under it
            if facing == "down":
                rect(g, 7, 19, 8, 24, "w")                                              # open at the front
                for y in range(19, 25): put(g, 7, y, "w"); put(g, 8, y, "w")
                for y in range(19, 24): put(g, 6, y, "T")
                for y in range(19, 24): put(g, 9, y, "T")
        elif style == "dress":
            rect(g, ax0, 19, ax0 + 1, 21, "t"); rect(g, ax1 - 1, 19, ax1, 21, "t")
            rect(g, 4, 25, 11, 26, "t"); rect(g, 3, 27, 12, 28, "t"); hline(g, 28, 3, 12, "T"); hline(g, 26, 4, 11, "t")
            hline(g, 24, x0, x1, "t"); hline(g, 25, 4, 11, "T") if False else None
            put(g, 7, 22, "T"); put(g, 8, 22, "T")
    else:
        rect(g, 5 if not fem else 6, 19, 10, 24, "t"); hline(g, 24, 5 if not fem else 6, 10, "T")
        if fem: rect(g, 6, 25, 10, 25, "t"); hline(g, 25, 6, 10, "T")
        if style == "tshirt": rect(g, 6, 19, 8, 21, "T"); hline(g, 21, 6, 8, "T")
        elif style == "longsleeve": rect(g, 6, 19, 8, 26, "T"); hline(g, 26, 6, 8, "T")
        elif style == "overalls": rect(g, 6, 22, 9, 26, "p"); hline(g, 26, 6, 9, "P"); rect(g, 6, 19, 7, 21, "p"); put(g, 6, 22, "k"); rect(g, 6, 19, 8, 21, "T"); put(g, 6, 19, "p"); put(g, 6, 20, "p"); put(g, 6, 21, "p")
        elif style == "vest": rect(g, 6, 19, 8, 22, "w"); rect(g, 6, 22, 8, 24, "T")
        elif style == "dress":
            rect(g, 6, 25, 10, 26, "t"); rect(g, 5, 27, 10, 28, "t"); hline(g, 28, 5, 10, "T"); rect(g, 6, 19, 8, 21, "T")
    return g


def pants(style, build, facing):
    g = blank()
    fem = build == "feminine"
    if facing in ("down", "up"):
        hx0, hx1 = (4, 11)
        lx = [(5, 7), (8, 10)] if fem else [(4, 7), (8, 11)]
        if style == "trousers":
            rect(g, hx0, 25, hx1, 26, "p")
            for a, b in lx: rect(g, a, 27, b, 29, "p")
            rect(g, 7, 27, 8, 29, "P"); hline(g, 29, lx[0][0], lx[1][1], "P")
        elif style == "shorts":
            rect(g, hx0, 25, hx1, 26, "p")
            for a, b in lx: rect(g, a, 27, b, 27, "p")
            hline(g, 27, lx[0][0], lx[1][1], "P"); put(g, 7, 27, "P"); put(g, 8, 27, "P")
        elif style == "skirt":
            rect(g, 4, 25, 11, 26, "p"); rect(g, 3, 27, 12, 28, "p"); hline(g, 28, 3, 12, "P")
        elif style == "cuffed":
            rect(g, hx0, 25, hx1, 26, "p")
            for a, b in lx: rect(g, a, 27, b, 28, "p")
            rect(g, 7, 27, 8, 28, "P")
            for a, b in lx: rect(g, a, 29, b, 29, "w")
            hline(g, 28, lx[0][0], lx[1][1], "P")
    else:
        if style == "trousers":
            rect(g, 5 if not fem else 6, 25, 10, 26, "p"); rect(g, 5 if not fem else 6, 27, 9, 29, "p"); hline(g, 29, 5, 9, "P")
        elif style == "shorts":
            rect(g, 5 if not fem else 6, 25, 10, 26, "p"); rect(g, 5 if not fem else 6, 27, 9, 27, "P")
        elif style == "skirt":
            rect(g, 5, 25, 10, 26, "p"); rect(g, 4, 27, 10, 28, "p"); hline(g, 28, 4, 10, "P")
        elif style == "cuffed":
            rect(g, 5 if not fem else 6, 25, 10, 26, "p"); rect(g, 5 if not fem else 6, 27, 9, 28, "p"); hline(g, 28, 5, 9, "P"); hline(g, 29, 5, 9, "w")
    return g


# ----------------------------------------------------------------------------------------------------------------------------------------------
# hair
# ----------------------------------------------------------------------------------------------------------------------------------------------
def hair(style, facing):
    g = blank()
    if facing == "down":
        if style in ("short", "parted", "long", "ponytail", "bun", "curly"):
            hline(g, 7, 5, 10, "h"); hline(g, 8, 4, 11, "h"); hline(g, 9, 3, 12, "h")
            hline(g, 10, 3, 12, "h"); put(g, 3, 11, "h"); put(g, 12, 11, "h"); put(g, 3, 12, "h"); put(g, 12, 12, "h")
            hline(g, 10, 4, 11, "H") if style != "parted" else None
        if style == "short":
            hline(g, 10, 4, 11, "h"); hline(g, 10, 5, 6, "H"); hline(g, 10, 9, 10, "H")
        elif style == "parted":
            hline(g, 10, 3, 9, "h"); hline(g, 11, 3, 6, "h"); put(g, 10, 10, "H"); put(g, 11, 10, "H"); put(g, 7, 9, "H"); put(g, 7, 8, "H")
            put(g, 12, 11, "h")
        elif style == "long":
            rect(g, 2, 9, 3, 22, "h"); rect(g, 12, 9, 13, 22, "h"); hline(g, 22, 2, 3, "H"); hline(g, 22, 12, 13, "H")
            hline(g, 10, 4, 11, "h"); put(g, 7, 9, "H"); put(g, 8, 9, "H")
            put(g, 4, 11, "h"); put(g, 11, 11, "h")
        elif style == "ponytail":
            hline(g, 10, 4, 11, "h"); put(g, 5, 10, "H"); put(g, 10, 10, "H"); rect(g, 12, 7, 13, 9, "h"); put(g, 13, 10, "H")
        elif style == "bun":
            hline(g, 10, 4, 11, "h"); put(g, 5, 10, "H"); put(g, 10, 10, "H")
            rect(g, 6, 5, 9, 6, "h"); hline(g, 4, 7, 8, "h"); put(g, 6, 6, "H"); put(g, 9, 6, "H")
        elif style == "curly":
            hline(g, 6, 4, 11, "h"); hline(g, 7, 3, 12, "h"); hline(g, 8, 2, 13, "h"); hline(g, 9, 2, 13, "h"); hline(g, 10, 2, 13, "h")
            rect(g, 2, 11, 3, 14, "h"); rect(g, 12, 11, 13, 14, "h"); hline(g, 10, 4, 11, "h")
            for x in (4, 7, 10): put(g, x, 9, "H")
            put(g, 3, 12, "H"); put(g, 12, 12, "H")
    elif facing == "up":
        shape = {"short": 15, "parted": 15, "bun": 15, "ponytail": 15, "long": 22, "curly": 15}
        hline(g, 7, 5, 10, "h"); hline(g, 8, 4, 11, "h")
        for y in range(9, 17): hline(g, y, 3, 12, "h")
        hline(g, 17, 5, 10, "H") if style in ("short", "parted", "bun", "ponytail") else hline(g, 17, 4, 11, "h")
        hline(g, 16, 4, 11, "h")
        if style == "short": hline(g, 16, 5, 10, "H")
        if style == "parted": hline(g, 16, 4, 11, "H"); put(g, 4, 8, "H")
        if style == "long":
            for y in range(17, 23): hline(g, y, 3, 12, "h")
            hline(g, 22, 3, 12, "H"); hline(g, 17, 3, 12, "h")
            for y in range(10, 22, 2): put(g, 7, y, "H")
        if style == "ponytail":
            rect(g, 7, 17, 8, 24, "h"); rect(g, 6, 11, 9, 12, "H"); put(g, 7, 25, "H"); put(g, 8, 24, "H")
        if style == "bun":
            rect(g, 6, 5, 9, 6, "h"); hline(g, 4, 7, 8, "h"); put(g, 6, 6, "H"); put(g, 9, 6, "H"); hline(g, 7, 7, 8, "H")
        if style == "curly":
            hline(g, 6, 4, 11, "h"); hline(g, 7, 3, 12, "h"); hline(g, 8, 2, 13, "h")
            for y in range(9, 17): hline(g, y, 2, 13, "h")
            hline(g, 17, 4, 11, "h")
            for x in (4, 7, 10): put(g, x, 10, "H"); put(g, x + 1, 13, "H")
        if style == "short": put(g, 7, 9, "H"); put(g, 8, 9, "H")
    else:   # left profile (the face is on the left)
        hline(g, 7, 6, 10, "h"); hline(g, 8, 5, 11, "h"); hline(g, 9, 5, 11, "h")
        for y in range(10, 16): hline(g, y, 8, 11, "h")
        hline(g, 10, 4, 7, "h")
        if style == "short":
            put(g, 4, 10, "h"); hline(g, 16, 8, 10, "h"); put(g, 5, 10, "H"); put(g, 6, 10, "H")
        elif style == "parted":
            hline(g, 10, 4, 7, "h"); hline(g, 11, 4, 5, "h"); put(g, 6, 9, "H"); hline(g, 16, 8, 10, "h")
        elif style == "long":
            rect(g, 8, 16, 12, 22, "h"); hline(g, 22, 8, 12, "H"); rect(g, 9, 11, 12, 15, "h"); put(g, 4, 10, "h")
            for y in range(12, 22, 2): put(g, 10, y, "H")
        elif style == "ponytail":
            hline(g, 16, 8, 10, "h"); put(g, 4, 10, "h"); rect(g, 11, 10, 13, 12, "h"); rect(g, 12, 13, 13, 18, "h"); put(g, 12, 19, "H"); put(g, 13, 18, "H")
        elif style == "bun":
            hline(g, 16, 8, 10, "h"); put(g, 4, 10, "h"); rect(g, 8, 5, 11, 6, "h"); hline(g, 4, 9, 10, "h"); put(g, 8, 6, "H"); put(g, 11, 6, "H")
        elif style == "curly":
            hline(g, 6, 5, 11, "h"); hline(g, 7, 4, 12, "h"); hline(g, 8, 4, 12, "h"); hline(g, 9, 4, 12, "h")
            for y in range(10, 17): hline(g, y, 8, 12, "h")
            hline(g, 10, 4, 7, "h"); put(g, 4, 11, "h"); put(g, 10, 12, "H"); put(g, 8, 9, "H"); put(g, 6, 8, "H")
    return g


# ----------------------------------------------------------------------------------------------------------------------------------------------
# accessories
# ----------------------------------------------------------------------------------------------------------------------------------------------
def accessory(style, facing):
    g = blank()
    if style == "none": return g
    if facing in ("down", "up"):
        if style == "sunhat":
            hline(g, 5, 6, 9, "a"); hline(g, 6, 5, 10, "a"); hline(g, 7, 4, 11, "a"); hline(g, 8, 4, 11, "A"); hline(g, 8, 6, 9, "a")
            hline(g, 9, 1, 14, "a"); hline(g, 10, 1, 14, "a"); hline(g, 10, 2, 13, "A")
            put(g, 4, 7, "A"); put(g, 11, 7, "A")
        elif style == "cap":
            hline(g, 6, 5, 10, "a"); hline(g, 7, 4, 11, "a"); hline(g, 8, 3, 12, "a"); hline(g, 9, 3, 12, "a")
            if facing == "down": hline(g, 10, 3, 12, "A"); hline(g, 11, 5, 10, "A")
            else: hline(g, 10, 4, 11, "A"); put(g, 7, 8, "A"); put(g, 8, 8, "A")
        elif style == "beanie":
            hline(g, 5, 7, 8, "A"); hline(g, 6, 5, 10, "a"); hline(g, 7, 4, 11, "a"); hline(g, 8, 3, 12, "a")
            hline(g, 9, 3, 12, "A"); hline(g, 10, 3, 12, "A"); put(g, 6, 7, "A"); put(g, 9, 7, "A")
        elif style == "glasses":
            if facing == "down":
                for x0 in (4, 9):
                    hline(g, 12, x0, x0 + 2, "a"); hline(g, 15, x0, x0 + 2, "a")
                    put(g, x0, 13, "a"); put(g, x0, 14, "a"); put(g, x0 + 2, 13, "a"); put(g, x0 + 2, 14, "a")
                hline(g, 13, 7, 8, "a"); put(g, 3, 13, "a"); put(g, 12, 13, "a")
            else: hline(g, 13, 3, 12, "A")
        elif style == "scarf":
            hline(g, 18, 4, 11, "a"); hline(g, 19, 4, 11, "A") if facing == "up" else hline(g, 19, 4, 11, "a")
            if facing == "down": rect(g, 4, 20, 5, 24, "a"); hline(g, 24, 4, 5, "A"); put(g, 6, 19, "A"); hline(g, 18, 4, 11, "A")
            else: hline(g, 18, 4, 11, "A")
        elif style == "flower":
            put(g, 11, 8, "a"); put(g, 12, 8, "a"); put(g, 13, 8, "a"); put(g, 12, 7, "a"); put(g, 12, 9, "a"); put(g, 12, 8, "w")
    else:
        if style == "sunhat":
            hline(g, 5, 6, 9, "a"); hline(g, 6, 5, 10, "a"); hline(g, 7, 5, 11, "a"); hline(g, 8, 5, 11, "A"); hline(g, 8, 6, 9, "a")
            hline(g, 9, 2, 13, "a"); hline(g, 10, 2, 13, "A")
        elif style == "cap":
            hline(g, 6, 6, 10, "a"); hline(g, 7, 5, 11, "a"); hline(g, 8, 5, 11, "a"); hline(g, 9, 4, 11, "a"); hline(g, 10, 2, 5, "A"); put(g, 11, 10, "A")
        elif style == "beanie":
            hline(g, 5, 8, 9, "A"); hline(g, 6, 5, 10, "a"); hline(g, 7, 5, 11, "a"); hline(g, 8, 4, 11, "a"); hline(g, 9, 4, 11, "A"); hline(g, 10, 4, 11, "A")
        elif style == "glasses":
            hline(g, 12, 4, 6, "a"); hline(g, 15, 4, 6, "a"); put(g, 4, 13, "a"); put(g, 4, 14, "a"); put(g, 6, 13, "a"); put(g, 6, 14, "a"); hline(g, 13, 7, 10, "a")
        elif style == "scarf":
            hline(g, 18, 5, 10, "A"); hline(g, 19, 5, 10, "a"); rect(g, 9, 20, 10, 23, "a"); hline(g, 23, 9, 10, "A")
        elif style == "flower":
            put(g, 10, 8, "a"); put(g, 11, 8, "a"); put(g, 12, 8, "a"); put(g, 11, 7, "a"); put(g, 11, 9, "a"); put(g, 11, 8, "w")
    return g


BUILDS = ["feminine", "masculine"]
HAIRS = ["short", "parted", "long", "ponytail", "bun", "curly"]
SHIRTS = ["tshirt", "longsleeve", "overalls", "vest", "dress"]
PANTS = ["trousers", "shorts", "skirt", "cuffed"]
ACCESSORIES = ["none", "sunhat", "cap", "beanie", "glasses", "scarf", "flower"]


def all_layers():
    layers = []
    for b in BUILDS:
        for f in FACINGS: layers.append((f"body.{b}.{f}", body(b, f)))
    for h in HAIRS:
        for f in FACINGS: layers.append((f"hair.{h}.{f}", hair(h, f)))
    for b in BUILDS:
        for s in SHIRTS:
            for f in FACINGS: layers.append((f"shirt.{s}.{b}.{f}", shirt(s, b, f)))
        for p in PANTS:
            for f in FACINGS: layers.append((f"pants.{p}.{b}.{f}", pants(p, b, f)))
    for a in ACCESSORIES:
        for f in FACINGS: layers.append((f"accessory.{a}.{f}", accessory(a, f)))
    return layers


def write():
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    lines = ["# Avatar layers: 16 x 32 cells per facing (down, up, left; right is the mirror of left). Made by tools/art/build_avatar_layers.py; edit by hand if you like.",
             "# s skin  h hair  t shirt  p pants  a accessory  w white cloth  k dark  b boots  . empty (lower case = the colour, upper case = its shade)", ""]
    for name, g in all_layers():
        lines.append("@" + name)
        for r in g: lines.append("".join(r))
        lines.append("")
    open(OUT, "w", encoding="utf-8", newline="").write("\n".join(lines))
    print(len(all_layers()), "layers ->", OUT)


# ---- previews (the same compositing rules as the game) ------------------------------------------------------------------------------------------------
def hexcol(h): return tuple(int(h[i:i + 2], 16) for i in (1, 3, 5))
def shade(c, f=0.72): return tuple(max(0, int(v * f)) for v in c)

def compose(build, hair_s, shirt_s, pants_s, acc_s, colors, facing):
    layers = dict(all_layers())
    f = facing if facing != "right" else "left"
    grids = [layers[f"body.{build}.{f}"], layers[f"pants.{pants_s}.{build}.{f}"], layers[f"shirt.{shirt_s}.{build}.{f}"], layers[f"hair.{hair_s}.{f}"], layers[f"accessory.{acc_s}.{f}"]]
    if facing == "right": grids = [mirror(g) for g in grids]
    out = [[None] * W for _ in range(H)]
    cmap = {"s": colors["skin"], "h": colors["hair"], "t": colors["shirt"], "p": colors["pants"], "a": colors["acc"], "w": hexcol("#efe6d8"), "k": hexcol("#2b1d16"), "b": hexcol("#5a3b28")}
    for g in grids:
        for y in range(H):
            for x in range(W):
                c = g[y][x]
                if c == ".": continue
                base = cmap[c.lower()]
                out[y][x] = shade(base) if c.isupper() and c.lower() != "k" else base
    outline = hexcol("#2b1d16")
    res = [[None] * W for _ in range(H)]
    for y in range(H):
        for x in range(W):
            if out[y][x] is not None: res[y][x] = out[y][x]
            else:
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < W and 0 <= ny < H and out[ny][nx] is not None: res[y][x] = outline; break
    return res


def preview():
    from PIL import Image
    colors = {"skin": hexcol("#e8b48a"), "hair": hexcol("#6b4226"), "shirt": hexcol("#3f8f9b"), "pants": hexcol("#5a4a8c"), "acc": hexcol("#e0a83a")}
    def img(cells, s=8):
        im = Image.new("RGBA", (W, H), (0, 0, 0, 0))
        for y in range(H):
            for x in range(W):
                if cells[y][x]: im.putpixel((x, y), cells[y][x] + (255,))
        return im.resize((W * s, H * s), Image.NEAREST)
    os.makedirs(os.path.join(ROOT, "Builds"), exist_ok=True)
    def sheet(name, items):
        s = 8
        sheet_ = Image.new("RGBA", (len(items) * (W * s + 8) + 8, 4 * (H * s + 8) + 8), (62, 54, 62, 255))
        for i, kwargs in enumerate(items):
            for j, f in enumerate(["down", "up", "left", "right"]):
                im = img(compose(facing=f, **kwargs), s)
                sheet_.paste(im, (8 + i * (W * s + 8), 8 + j * (H * s + 8)), im)
        sheet_.save(os.path.join(ROOT, "Builds", name))
    base = dict(build="feminine", hair_s="long", shirt_s="tshirt", pants_s="trousers", acc_s="none", colors=colors)
    sheet("avatar_builds.png", [dict(base, build=b, hair_s=h) for b, h in (("feminine", "long"), ("masculine", "short"))])
    sheet("avatar_hair.png", [dict(base, build="masculine", hair_s=h) for h in HAIRS])
    sheet("avatar_shirts.png", [dict(base, build="masculine", hair_s="short", shirt_s=s) for s in SHIRTS])
    sheet("avatar_shirts_f.png", [dict(base, shirt_s=s, hair_s="ponytail") for s in SHIRTS])
    sheet("avatar_pants.png", [dict(base, build="masculine", hair_s="short", pants_s=p) for p in PANTS])
    sheet("avatar_acc.png", [dict(base, build="masculine", hair_s="short", acc_s=a) for a in ACCESSORIES])


if __name__ == "__main__":
    write()
    if "--preview" in sys.argv: preview()
