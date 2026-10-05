"""Draws the farm animals: chicken, duck, rabbit (coop) and cow, goat, sheep (barn), and writes them as C# pixel grids
(Assets/_Project/Scripts/Gameplay/Animals/AnimalSpriteData.cs, read by Farm.Gameplay.AnimalSprites).

Every animal has: down (front) and up (back) and left (side; right is the side mirrored in code), each with two walking frames (0, 1);
eat0/eat1 (front view, head lowered, a two-frame munch, shown after the trough feeds them and when it grazes); idle0/idle1 (front view fidgets: birds
flap, the rest blink and turn their heads, played now and then while standing); and sleep (side view, lying down, night).
Standing still shows walk frame 0. Project-made, original.

Letters: o outline, b body, h light, s shade, w white/belly, k dark (patches, hooves, wool-face), a accent (beak, horn), r red (comb),
p pink (muzzle, ears, udder), e eye, y legs and feet.

Usage: python tools/art/build_animal_sprites.py [output.cs]
"""
import math
import os
import sys

OUT_DEFAULT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "_Project", "Scripts", "Gameplay", "Animals", "AnimalSpriteData.cs")
SIZE = 16

OUTLINE = (58, 40, 32)
EYE = (20, 16, 20)

PALETTES = {
    "chicken": dict(b=(246, 244, 238), h=(255, 255, 255), s=(206, 204, 198), a=(240, 186, 60), r=(214, 52, 46), y=(232, 178, 60), w=(255, 255, 255), k=(160, 150, 140), p=(240, 150, 150)),
    "duck":    dict(b=(250, 214, 70), h=(255, 240, 150), s=(214, 168, 46), a=(238, 130, 40), r=(214, 52, 46), y=(238, 130, 40), w=(255, 245, 200), k=(150, 110, 40), p=(240, 150, 150)),
    "rabbit":  dict(b=(205, 196, 188), h=(234, 228, 222), s=(160, 150, 144), a=(240, 186, 60), r=(214, 52, 46), y=(190, 180, 172), w=(250, 248, 244), k=(120, 110, 106), p=(238, 158, 170)),
    "cow":     dict(b=(246, 242, 236), h=(255, 255, 255), s=(208, 202, 196), a=(236, 226, 190), r=(214, 52, 46), y=(60, 50, 48), w=(255, 255, 255), k=(70, 60, 58), p=(240, 164, 160)),
    "goat":    dict(b=(196, 170, 132), h=(224, 202, 166), s=(150, 126, 96), a=(230, 222, 196), r=(214, 52, 46), y=(70, 56, 46), w=(244, 240, 230), k=(80, 64, 52), p=(232, 164, 160)),
    "sheep":   dict(b=(248, 246, 240), h=(255, 255, 255), s=(206, 202, 196), a=(236, 226, 190), r=(214, 52, 46), y=(76, 66, 66), w=(252, 250, 246), k=(92, 82, 82), p=(236, 170, 170)),
}


class Canvas:
    def __init__(self):
        self.g = [["."] * SIZE for _ in range(SIZE)]

    def px(self, x, y, ch):
        x, y = int(round(x)), int(round(y))
        if 0 <= x < SIZE and 0 <= y < SIZE:
            self.g[y][x] = ch

    def get(self, x, y):
        return self.g[y][x] if 0 <= x < SIZE and 0 <= y < SIZE else "."

    def ell(self, cx, cy, rx, ry, ch):
        for y in range(SIZE):
            for x in range(SIZE):
                if ((x - cx) / rx) ** 2 + ((y - cy) / ry) ** 2 <= 1.0:
                    self.g[y][x] = ch

    def rect(self, x0, y0, x1, y1, ch):
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                self.px(x, y, ch)

    def shade(self, chars="bw"):
        """Light on the top edge of a body shape and shade along its bottom edge."""
        src = [row[:] for row in self.g]
        for y in range(SIZE):
            for x in range(SIZE):
                c = src[y][x]
                if c not in chars:
                    continue
                above = src[y - 1][x] if y > 0 else "."
                below = src[y + 1][x] if y < SIZE - 1 else "."
                if above == ".":
                    self.g[y][x] = "h"
                elif below == "." or below == "o":
                    self.g[y][x] = "s"

    def outline(self):
        src = [row[:] for row in self.g]
        for y in range(SIZE):
            for x in range(SIZE):
                if src[y][x] != ".":
                    continue
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < SIZE and 0 <= ny < SIZE and src[ny][nx] not in (".", "o"):
                        self.g[y][x] = "o"
                        break

    def rows(self):
        return ["".join(r) for r in self.g]


def finish(c, shade_chars="b"):
    c.shade(shade_chars)
    c.outline()
    return c.rows()


# ---------------------------------------------------------------------------------------------------------------------------------
# Birds: chicken and duck

def bird(kind, view, frame, pose=None):
    c = Canvas()
    duck = kind == "duck"
    body_rx = 4.2 if duck else 3.8
    if pose == "sleep":
        c.ell(8, 11.5, 4.6, 2.6, "b")
        c.ell(5.5, 10, 2.1, 1.9, "b")                     # head tucked on the body
        c.px(4, 10, "e")
        c.px(3, 11, "a"); c.px(2, 11, "a")
        if not duck: c.px(5, 8, "r"); c.px(6, 8, "r")
        c.ell(10.5, 11.5, 2.4, 1.4, "s")
        return finish(c, "b")
    if view == "left":
        c.ell(9, 9.5, body_rx, 3.3, "b")
        if duck:
            c.px(13, 7, "b"); c.px(13, 6, "h")
        else:
            c.px(12, 7, "b"); c.px(13, 6, "b"); c.px(13, 5, "h"); c.px(12, 6, "b")
        c.ell(9.5, 9.8, 2.2, 1.5, "s")                      # wing
        hx = 5
        c.ell(hx, 6, 2.2, 2.2, "b")
        if duck:
            c.rect(1, 6, 3, 7, "a")
        else:
            c.px(4, 3, "r"); c.px(5, 3, "r"); c.px(5, 4, "r"); c.px(3, 8, "r")
            c.px(1, 6, "a"); c.px(2, 6, "a")
        c.px(4, 5, "e")
        step = frame * 2
        for lx in (7 - step // 2 * 0 - (1 if frame else 0), 10 + (1 if frame else 0)):
            c.px(lx, 12, "y"); c.px(lx, 13, "y"); c.px(lx, 14, "y"); c.px(lx - 1, 14, "y")
        return finish(c, "b")
    if view in ("down", "up"):
        head_dy = pose == "eat"
        hy = 5 + (2 + frame if head_dy else 0)
        c.ell(8, 9.5, body_rx - 0.3, 3.6, "b")
        if pose == "idle":                                                  # wings spread, beating up and down
            wy = 10.5 - frame * 3
            c.ell(3.4, wy, 2.2, 1.1, "b"); c.ell(12.6, wy, 2.2, 1.1, "b")
            c.ell(3.4, wy + 0.4, 1.4, 0.6, "s"); c.ell(12.6, wy + 0.4, 1.4, 0.6, "s")
        else:
            c.ell(4.8, 9.6, 0.9, 2, "s"); c.ell(11.2, 9.6, 0.9, 2, "s")    # wings at the sides
        c.ell(8, hy, 2.4, 2.2, "b")
        if view == "down":
            if duck:
                c.rect(7, hy + 1, 9, hy + 1, "a")
            else:
                c.px(7, hy - 3, "r"); c.px(8, hy - 3, "r"); c.px(9, hy - 3, "r"); c.px(8, hy - 2, "r")
                c.px(8, hy + 1, "a"); c.px(8, hy + 2, "r")
            c.px(7, hy, "e"); c.px(9, hy, "e")
            fl = 14 - (frame if pose != "eat" else 0)
            fr = 13 + (frame if pose != "eat" else 0)
            for lx, bottom in ((7, fl), (9, fr)):
                c.px(lx, bottom, "y"); c.px(lx - 1, bottom, "y"); c.px(lx + 1, bottom, "y")
                c.px(lx, bottom - 1, "y")
        else:
            if not duck:
                c.px(7, hy - 3, "r"); c.px(8, hy - 3, "r"); c.px(9, hy - 3, "r")
            c.ell(8, 8.2, 2.3, 2.8, "s")                    # tail plumes over the back
            c.px(8, 5, "h"); c.px(8, 6, "h") if False else None
            fl = 14 - frame
            fr = 13 + frame
            for lx, bottom in ((7, fl), (9, fr)):
                c.px(lx, bottom, "y"); c.px(lx - 1, bottom, "y"); c.px(lx + 1, bottom, "y")
                c.px(lx, bottom - 1, "y")
        return finish(c, "b")
    raise ValueError(view)


# ---------------------------------------------------------------------------------------------------------------------------------
# Rabbit

def rabbit(view, frame, pose=None):
    c = Canvas()
    if pose == "sleep":
        c.ell(9, 11.5, 4.6, 2.6, "b")
        c.ell(5, 10.5, 2.2, 2, "b")
        c.ell(6, 8.6, 0.8, 1.2, "b"); c.px(6, 8, "p")        # ears folded back
        c.px(4, 10, "e"); c.px(3, 11, "p")
        c.ell(13, 11, 1.3, 1.3, "w")
        return finish(c, "b")
    if view == "left":
        c.ell(9.5, 10, 4, 2.8, "b")
        c.ell(13.3, 9.6, 1.4, 1.4, "w")                       # tail
        c.ell(5, 8, 2.4, 2.2, "b")
        c.ell(5, 3.8, 0.9, 2.6, "b"); c.px(5, 3, "p"); c.px(5, 4, "p")
        c.ell(7.2, 4.2, 0.9, 2.4, "b"); c.px(7, 4, "p")
        c.px(4, 7, "e"); c.px(2, 8, "p")
        hind = 11 + (frame)
        c.ell(hind, 12.4, 2, 1, "s")                          # hind foot
        c.px(5 - frame, 12, "b"); c.px(5 - frame, 13, "b"); c.px(4 - frame, 13, "w"); c.px(5 - frame, 14, "w")
        return finish(c, "bw")
    hy = 6.5 + (2 + frame if pose == "eat" else 0)
    if view in ("down", "up"):
        c.ell(8, 11, 3.6, 3, "b")
        c.ell(8, hy, 2.6, 2.4, "b")
        c.ell(6.4, 2.6 + (hy - 6.5), 0.9, 2.8, "b"); c.ell(9.6, 2.6 + (hy - 6.5), 0.9, 2.8, "b")
        if view == "down":
            c.px(6, 2 + (hy - 6.5), "p"); c.px(10, 2 + (hy - 6.5), "p"); c.px(6, 3 + (hy - 6.5), "p"); c.px(10, 3 + (hy - 6.5), "p")
            c.px(7, hy - 0.4, "e"); c.px(9, hy - 0.4, "e"); c.px(8, hy + 0.8, "p")
            lf = 14 - (frame if pose != "eat" else 0); rf = 14 - ((1 - frame) if pose != "eat" else 0)
            c.px(6, lf, "w"); c.px(7, lf, "w"); c.px(9, rf, "w"); c.px(10, rf, "w")
        else:
            c.ell(8, 12, 1.6, 1.5, "w")                       # the tail puff
            lf = 14 - frame; rf = 14 - (1 - frame)
            c.px(6, lf, "w"); c.px(7, lf, "w"); c.px(9, rf, "w"); c.px(10, rf, "w")
        return finish(c, "b")
    raise ValueError(view)


# ---------------------------------------------------------------------------------------------------------------------------------
# Quadrupeds: cow, goat, sheep

def quad(kind, view, frame, pose=None):
    c = Canvas()
    cow, goat, sheep = kind == "cow", kind == "goat", kind == "sheep"
    if pose == "sleep":
        c.ell(9, 10.5, 5.4, 2.8, "w" if sheep else "b")
        if sheep:
            c.ell(6.5, 9, 2.2, 1.8, "w"); c.ell(11, 9, 2.4, 1.8, "w")
        c.ell(4.2, 11.2, 2.3, 1.9, "k" if sheep else "b")
        c.px(3, 10, "e")
        if cow:
            c.px(2, 12, "p"); c.px(3, 12, "p"); c.ell(9, 9.5, 1.5, 1.1, "k"); c.px(5, 8, "a")
        if goat:
            c.px(4, 8, "a"); c.px(3, 8, "a")
        if not sheep:
            c.px(14, 10, "k")
        return finish(c, "bw")
    if view == "left":
        if sheep:
            for cx, cy, rx, ry in ((9, 8, 4.6, 3.1), (6.6, 6.4, 2.1, 1.8), (10.4, 5.8, 2.3, 1.7), (12.6, 7.6, 1.9, 2.1)):
                c.ell(cx, cy, rx, ry, "w")
        else:
            c.ell(9, 8.2, 5.2 if cow else 4.5, 3.1 if cow else 2.8, "b")
        c.ell(3.8, 7.2, 2.3, 2.3, "k" if sheep else "b")
        if sheep:
            c.px(5, 5, "k"); c.px(6, 5, "k")                  # ear
            c.px(3, 6, "e")
        if cow:
            c.ell(2.4, 8.4, 1.3, 1.1, "p")
            c.px(4, 4, "a"); c.px(3, 4, "a")                  # horn
            c.px(5, 5, "b"); c.px(3, 6, "e")
            c.ell(8, 7, 1.7, 1.3, "k"); c.ell(11.5, 9, 1.3, 1.1, "k")
            c.px(10, 12, "p"); c.px(11, 12, "p")             # udder
            for yy in range(6, 10): c.px(14, yy, "b")
            c.px(14, 10, "k")
        if goat:
            c.px(4, 4, "a"); c.px(5, 3, "a"); c.px(3, 4, "a")
            c.px(2, 9, "w"); c.px(2, 10, "w")                 # beard
            c.px(3, 6, "e"); c.px(5, 5, "b")
            c.px(13, 6, "b"); c.px(13, 5, "h")                # tail up
        if not sheep and not cow and not goat:
            pass
        stride = frame
        legs = (5 - stride, 7 + stride, 11 + stride, 13 - stride) if not sheep else (5 - stride, 7 + stride, 11 + stride, 13 - stride)
        top = 10 if not sheep else 11
        for lx in legs:
            for yy in range(top, 14): c.px(lx, yy, "y" if (sheep or goat) else "b")
            c.px(lx, 14, "y")
        return finish(c, "bw")
    if view in ("down", "up"):
        hdy = (2 + frame) if pose == "eat" else 0
        if sheep:
            for cx, cy, rx, ry in ((8, 9.6, 4.6, 3.6), (5.6, 7.4, 1.9, 1.8), (10.4, 7.4, 1.9, 1.8), (8, 6.2, 2.6, 1.6)):
                c.ell(cx, cy, rx, ry, "w")
        else:
            c.ell(8, 9.6, 4.8 if cow else 4.2, 3.6 if cow else 3.3, "b")
        hy = 5 + hdy
        c.ell(8, hy, 2.5, 2.7, "k" if sheep else "b")
        if view == "down":
            if cow:
                c.px(5, hy - 2, "a"); c.px(11, hy - 2, "a"); c.px(5, hy - 1, "b"); c.px(11, hy - 1, "b")
                c.ell(8, hy + 1.6, 1.7, 1.1, "p"); c.px(7, hy - 0.2, "e"); c.px(9, hy - 0.2, "e")
                c.ell(6, 9, 1.4, 1.3, "k")
            elif goat:
                c.px(6, hy - 3, "a"); c.px(10, hy - 3, "a"); c.px(5, hy - 2, "a"); c.px(11, hy - 2, "a")
                c.px(7, hy, "e"); c.px(9, hy, "e"); c.px(8, hy + 1, "p"); c.px(8, hy + 2, "w"); c.px(8, hy + 3, "w")
            else:
                c.px(5, hy - 1, "k"); c.px(11, hy - 1, "k"); c.px(7, hy, "e"); c.px(9, hy, "e")
            lf = 14 - frame if pose != "eat" else 14
            rf = 13 + frame if pose != "eat" else 14
            for lx, bottom in ((6, lf), (10, rf)):
                for yy in range(12, bottom + 1): c.px(lx, yy, "y")
        else:
            if cow:
                c.px(5, hy - 2, "a"); c.px(11, hy - 2, "a"); c.ell(6, 9, 1.4, 1.3, "k")
            if goat:
                c.px(6, hy - 3, "a"); c.px(10, hy - 3, "a")
            c.rect(8, 8, 8, 12, "k" if not sheep else "s")      # the tail down the back
            lf = 14 - frame; rf = 13 + frame
            for lx, bottom in ((6, lf), (10, rf)):
                for yy in range(12, bottom + 1): c.px(lx, yy, "y")
        return finish(c, "bw")
    raise ValueError(view)


def draw(kind, key):
    """key: down0 down1 up0 up1 left0 left1 eat0 eat1 sleep"""
    pose = None
    if key == "sleep":
        view, frame, pose = "left", 0, "sleep"
    elif key.startswith("eat"):
        view, frame, pose = "down", int(key[-1]), "eat"
    elif key.startswith("idle"):
        frame = int(key[-1])
        if kind in ("chicken", "duck"):
            return bird(kind, "down", frame, "idle")
        base = rabbit("down", 0) if kind == "rabbit" else quad(kind, "down", 0)
        if frame == 0:                                   # a blink: the eyes close
            return [row.replace("e", "s") for row in base]
        return [row[::-1] for row in base]               # the head turns the other way
    else:
        view, frame = key[:-1], int(key[-1])
    if kind in ("chicken", "duck"):
        return bird(kind, view, frame, pose)
    if kind == "rabbit":
        return rabbit(view, frame, pose)
    return quad(kind, view, frame, pose)


KINDS = ["chicken", "duck", "rabbit", "cow", "goat", "sheep"]
KEYS = ["down0", "down1", "up0", "up1", "left0", "left1", "eat0", "eat1", "idle0", "idle1", "sleep"]


def to_rgb(ch, palette):
    if ch == "o": return OUTLINE
    if ch == "e": return EYE
    return palette[ch]


def write_cs(path):
    lines = []
    lines.append("// GENERATED by tools/art/build_animal_sprites.py: do not edit by hand; change the script and run it again.")
    lines.append("using System.Collections.Generic;")
    lines.append("using UnityEngine;")
    lines.append("")
    lines.append("namespace Farm.Gameplay")
    lines.append("{")
    lines.append("    // The farm animals' pixel pictures (16 x 16, one string per row). See AnimalSprites for how they are shown.")
    lines.append("    public static class AnimalSpriteData")
    lines.append("    {")
    lines.append("        static Color32 C(int r, int g, int b) => new Color32((byte)r, (byte)g, (byte)b, 255);")
    lines.append("")
    lines.append("        public static readonly string[] Keys = { %s };" % ", ".join('"%s"' % k for k in KEYS))
    lines.append("")
    lines.append("        public static readonly Dictionary<string, Dictionary<char, Color32>> Palettes = new Dictionary<string, Dictionary<char, Color32>>")
    lines.append("        {")
    for kind in KINDS:
        pal = dict(PALETTES[kind])
        entries = ["['o'] = C(%d, %d, %d)" % OUTLINE, "['e'] = C(%d, %d, %d)" % EYE] + ["['%s'] = C(%d, %d, %d)" % ((k,) + v) for k, v in pal.items()]
        lines.append('            ["%s"] = new Dictionary<char, Color32> { %s },' % (kind, ", ".join(entries)))
    lines.append("        };")
    lines.append("")
    lines.append("        public static readonly Dictionary<string, Dictionary<string, string[]>> Grids = new Dictionary<string, Dictionary<string, string[]>>")
    lines.append("        {")
    for kind in KINDS:
        lines.append('            ["%s"] = new Dictionary<string, string[]>' % kind)
        lines.append("            {")
        for key in KEYS:
            rows = draw(kind, key)
            lines.append('                ["%s"] = new[] { %s },' % (key, ", ".join('"%s"' % r for r in rows)))
        lines.append("            },")
    lines.append("        };")
    lines.append("    }")
    lines.append("}")
    with open(path, "w", newline="\n") as f:
        f.write("\n".join(lines) + "\n")
    print("wrote", os.path.abspath(path))


def preview(path, scale=6):
    from PIL import Image
    cols = len(KEYS)
    img = Image.new("RGBA", (cols * (SIZE * scale + 6) + 6, len(KINDS) * (SIZE * scale + 6) + 6), (118, 150, 84, 255))
    for r, kind in enumerate(KINDS):
        for cidx, key in enumerate(KEYS):
            rows = draw(kind, key)
            pal = PALETTES[kind]
            tile = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
            for y, row in enumerate(rows):
                for x, ch in enumerate(row):
                    if ch != ".":
                        tile.putpixel((x, y), to_rgb(ch, pal) + (255,))
            tile = tile.resize((SIZE * scale, SIZE * scale), Image.NEAREST)
            img.paste(tile, (6 + cidx * (SIZE * scale + 6), 6 + r * (SIZE * scale + 6)), tile)
    img.save(path)
    print("preview", path)


if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "--preview":
        preview(sys.argv[2])
    else:
        write_cs(sys.argv[1] if len(sys.argv) > 1 else OUT_DEFAULT)
