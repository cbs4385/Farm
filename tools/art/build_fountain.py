"""The village square's big fountain (owner, 2026-10-16: "enlarge the fountain to fill 95% of the inner circle of the square"). A round stone basin seen from the front with a two-tier
fountain rising from the middle and water falling into the pool, drawn here (original art), `obj_fountain.png`, 96 x 112 pixels. The basin's rim is an ellipse 93 x 70 pixels, 95% of
the inside of the paving ring (`obj_plaza_ring.png`: 98 x 74), and the basin's middle is the middle of the picture, which is where the village scene puts it (the centre of the square).
Run from the repository root: py tools/art/build_fountain.py"""
import os
from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders", "obj_fountain.png")

W, H = 96, 112
INK = (0x2a, 0x2c, 0x34, 255)
STONE_L, STONE, STONE_D, STONE_DD = (0xc4, 0xbe, 0xb0, 255), (0xa2, 0x9c, 0x90, 255), (0x7a, 0x76, 0x6e, 255), (0x58, 0x56, 0x52, 255)
WATER_D, WATER, WATER_L, FOAM = (0x2f, 0x6a, 0x92, 255), (0x4a, 0x8c, 0xb8, 255), (0x70, 0xb4, 0xdc, 255), (0xdc, 0xf0, 0xf8, 255)
MOSS = (0x6a, 0x84, 0x58, 255)


def ellipse(d, cx, cy, rx, ry, fill, outline=None):
    d.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=fill, outline=outline)


def main():
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    cx, cy = 47.5, 56.0          # the middle of the basin and of the picture (the rim is 93 x 70 round it)
    rx, ry = 46.5, 35.0

    # the basin: a stone skirt under the rim (the front face of the wall), the rim, the inside wall at the back, the water
    ellipse(d, cx, cy + 8, rx, ry, STONE_DD, INK)
    d.rectangle([cx - rx + 1, cy, cx + rx - 1, cy + 8], fill=STONE_D)
    ellipse(d, cx, cy, rx, ry, STONE_L, INK)
    for i in range(-5, 6):                                   # joints between the stones of the rim, down the front face
        x = cx + i * 8.4
        k = (1 - ((x - cx) / rx) ** 2) ** 0.5
        d.line([(x, cy + ry * k + 1), (x, cy + 8 + ry * k - 2)], fill=STONE_DD)
    ellipse(d, cx, cy, rx - 6, ry - 6, STONE_D, INK)         # the inner wall
    ellipse(d, cx, cy + 3, rx - 8, ry - 9, WATER, None)      # the water, lower than the rim, so a band of wall shows at the back
    ellipse(d, cx, cy + 4, rx - 14, ry - 14, WATER_L, None)
    for (x, y, w) in [(18, 70, 9), (68, 72, 11), (28, 48, 7), (60, 45, 8), (40, 78, 7), (14, 56, 5), (79, 58, 5)]:     # ripples
        d.line([(x, y), (x + w, y)], fill=FOAM)
        d.line([(x + 2, y + 1), (x + w - 2, y + 1)], fill=WATER_D)
    # a little moss on the rim
    for (x, y) in [(10, 66), (11, 67), (84, 62), (85, 63), (35, 90), (36, 91), (60, 91)]:
        d.point((x, y), fill=MOSS)

    # the fountain: a foot, a lower bowl, a column, an upper bowl, a spout
    ellipse(d, cx, cy + 5, 17, 8, STONE_D, INK)               # foot in the water
    ellipse(d, cx, cy + 3, 15, 6, STONE, None)
    d.rectangle([cx - 5, cy - 14, cx + 5, cy + 3], fill=STONE, outline=INK)
    d.line([(cx - 3, cy - 13), (cx - 3, cy + 2)], fill=STONE_L)
    d.line([(cx + 4, cy - 13), (cx + 4, cy + 2)], fill=STONE_D)
    ellipse(d, cx, cy - 14, 24, 9, STONE_D, INK)              # the lower bowl, seen from above
    ellipse(d, cx, cy - 16, 22, 7, STONE_L, None)
    ellipse(d, cx, cy - 15, 19, 5, WATER, None)
    ellipse(d, cx, cy - 15, 14, 3, WATER_L, None)
    d.rectangle([cx - 3, cy - 30, cx + 3, cy - 15], fill=STONE, outline=INK)
    d.line([(cx - 1, cy - 29), (cx - 1, cy - 16)], fill=STONE_L)
    ellipse(d, cx, cy - 30, 11, 4, STONE_D, INK)              # the upper bowl
    ellipse(d, cx, cy - 31, 9, 3, WATER_L, None)
    d.rectangle([cx - 1, cy - 44, cx + 1, cy - 32], fill=FOAM)   # the jet
    d.polygon([(cx - 3, cy - 40), (cx + 3, cy - 40), (cx, cy - 47)], fill=FOAM)
    for k, (dx, top, bottom) in enumerate([(-8, -34, -20), (8, -34, -20), (-14, -28, -17), (14, -28, -17), (-4, -36, -26), (4, -36, -26)]):      # water falling from the bowls
        x = cx + dx
        d.line([(x, cy + top), (x - (1 if dx < 0 else -1) * 0, cy + bottom)], fill=FOAM if k % 2 == 0 else WATER_L)
    for (dx, y) in [(-19, -16), (19, -16), (-10, -9), (10, -9), (-22, -13), (22, -13)]:       # water falling from the lower bowl into the pool
        d.line([(cx + dx, cy + y), (cx + dx, cy + y + 9)], fill=FOAM)
        d.point((cx + dx, cy + y + 10), fill=WATER_L)
    for (x, y) in [(cx - 20, cy - 6), (cx + 20, cy - 6), (cx - 13, cy), (cx + 13, cy), (cx - 6, cy + 11), (cx + 7, cy + 11)]:           # rings where it lands
        d.line([(x - 3, y), (x + 3, y)], fill=FOAM)

    img.save(OUT)
    print("wrote", OUT, img.size)


if __name__ == "__main__":
    main()
