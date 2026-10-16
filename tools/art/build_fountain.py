"""The village square's big fountain (owner, 2026-10-16: "enlarge the fountain to fill 95% of the inner circle of the square", then "remove the arrow from the top of the fountain ...
animate the fountain so that it looks like it is spitting water"). A round stone basin seen from the front with a two-tier fountain rising from the middle, drawn here (original art).
`obj_fountain.png` is the first of six frames (`obj_fountain_1.png` to `obj_fountain_5.png` are the others), 96 x 112 pixels each: a rounded jet that pulses at the top, drops that fly
out from it in arcs and land in the upper bowl, water that runs from the bowls into the pool and rings that spread where it lands. The basin's rim is an ellipse 93 x 70 pixels, 95% of
the inside of the paving ring (`obj_plaza_ring.png`: 98 x 74), and the basin's middle is the middle of the picture, which is where the village scene puts it (the centre of the square);
FrameAnimator plays the frames. Run from the repository root: py tools/art/build_fountain.py"""
import math
import os
from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
ART = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")

W, H = 96, 112
FRAMES = 6
INK = (0x2a, 0x2c, 0x34, 255)
STONE_L, STONE, STONE_D, STONE_DD = (0xc4, 0xbe, 0xb0, 255), (0xa2, 0x9c, 0x90, 255), (0x7a, 0x76, 0x6e, 255), (0x58, 0x56, 0x52, 255)
WATER_D, WATER, WATER_L, FOAM = (0x2f, 0x6a, 0x92, 255), (0x4a, 0x8c, 0xb8, 255), (0x70, 0xb4, 0xdc, 255), (0xdc, 0xf0, 0xf8, 255)
MOSS = (0x6a, 0x84, 0x58, 255)

JET_RISE = [0, 2, 4, 3, 1, 2]                                   # how much higher the jet stands in each frame


def ellipse(d, cx, cy, rx, ry, fill, outline=None):
    d.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=fill, outline=outline)


def render(frame):
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    cx, cy = 47.5, 56.0          # the middle of the basin and of the picture (the rim is 93 x 70 round it)
    rx, ry = 46.5, 35.0
    phase = frame / float(FRAMES)

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
    for j, (x, y, w) in enumerate([(18, 70, 9), (68, 72, 11), (28, 48, 7), (60, 45, 8), (40, 78, 7), (14, 56, 5), (79, 58, 5)]):     # ripples, drifting with the frames
        x = x + int(round(math.sin((phase + j * 0.17) * 2 * math.pi) * 2))
        d.line([(x, y), (x + w, y)], fill=FOAM if (frame + j) % 3 else WATER_L)
        d.line([(x + 2, y + 1), (x + w - 2, y + 1)], fill=WATER_D)
    for (x, y) in [(10, 66), (11, 67), (84, 62), (85, 63), (35, 90), (36, 91), (60, 91)]:       # a little moss on the rim
        d.point((x, y), fill=MOSS)

    # the fountain: a foot, a lower bowl, a column, an upper bowl
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

    # water running over the rim of the upper bowl into the lower, in dashes that move down with the frames
    for dx in (-9, -5, 5, 9, -2, 2):
        for seg in range(3):
            y = cy - 29 + ((seg * 5 + frame * 3 + abs(dx)) % 15)
            if y < cy - 17:
                d.line([(cx + dx, y), (cx + dx, y + 2)], fill=FOAM if seg % 2 == 0 else WATER_L)
    # and from the lower bowl into the pool
    for dx in (-21, -17, -11, 11, 17, 21, -6, 6):
        for seg in range(2):
            y = cy - 12 + ((seg * 7 + frame * 4 + abs(dx)) % 14)
            d.line([(cx + dx, y), (cx + dx, y + 2)], fill=FOAM if seg % 2 == 0 else WATER_L)
    for (x, y) in [(cx - 21, cy - 1), (cx + 21, cy - 1), (cx - 14, cy + 3), (cx + 14, cy + 3), (cx - 7, cy + 10), (cx + 8, cy + 10)]:     # rings where it lands, spreading
        w = 2 + (frame + int(x)) % 3
        d.line([(x - w, y), (x + w, y)], fill=FOAM)
        d.line([(x - w + 1, y + 1), (x + w - 1, y + 1)], fill=WATER_L)

    # the spout: a stone nozzle on top of the upper bowl and a rounded jet that pulses
    d.rectangle([cx - 2, cy - 38, cx + 2, cy - 31], fill=STONE, outline=INK)
    top = cy - 44 - JET_RISE[frame]
    d.rectangle([cx - 1, top + 2, cx + 1, cy - 38], fill=FOAM)
    d.line([(cx - 1, top + 2), (cx - 1, cy - 38)], fill=WATER_L)
    ellipse(d, cx, top + 2, 2, 2, FOAM)                         # a rounded top, not a point
    d.point((cx - 3, top + 7), fill=WATER_L); d.point((cx + 3, top + 9), fill=WATER_L)
    # drops spat out in arcs, falling into the upper bowl; each drop is further along its arc in every frame
    ytop, ybowl = cy - 39, cy - 31
    for sgn in (-1, 1):
        for reach, lift in ((10, 12), (17, 8)):
            for j in range(3):
                t = (phase + j / 3.0 + (0.11 if reach > 10 else 0.0)) % 1.0
                x = cx + sgn * reach * t
                y = ytop - lift * 4 * t * (1 - t) + (ybowl - ytop) * t * t
                d.rectangle([round(x), round(y), round(x) + 1, round(y) + 1], fill=FOAM)
                tx, ty = cx + sgn * reach * max(0.0, t - 0.08), ytop - lift * 4 * max(0.0, t - 0.08) * (1 - max(0.0, t - 0.08)) + (ybowl - ytop) * max(0.0, t - 0.08) ** 2
                d.point((round(tx), round(ty)), fill=WATER_L)
    return img


def main():
    for frame in range(FRAMES):
        name = "obj_fountain.png" if frame == 0 else f"obj_fountain_{frame}.png"
        render(frame).save(os.path.join(ART, name))
        print("wrote", name)


if __name__ == "__main__":
    main()
