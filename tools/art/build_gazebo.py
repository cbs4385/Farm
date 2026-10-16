"""The village square's gazebo (playtest 2026-10-16: "use a gazebo as a center piece"): a wooden pavilion on a stone base, an open front with two steps, a rail round the other
sides and a terracotta shingle roof with a gold finial, in the colours of the Cozy Village kit's buildings. 96 x 112 pixels (6 x 7 cells), drawn here, original art.
`prop_gazebo.png`; the foot of the picture (its bottom edge) is the front edge of the base. The base is five cells wide (x 8..87 of the picture) and three cells deep: the front
row (steps and the edge of the floor), the floor and the back rail, which is how `VillageSquare` makes it solid. Run from the repository root: py tools/art/build_gazebo.py"""
import os
from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders", "prop_gazebo.png")

W, H = 96, 112
INK = (0x2b, 0x1d, 0x14, 255)
WOOD_L, WOOD, WOOD_D = (0xb4, 0x80, 0x4a, 255), (0x8a, 0x58, 0x30, 255), (0x5e, 0x3a, 0x20, 255)
CREAM, CREAM_D = (0xee, 0xe0, 0xbd, 255), (0xc6, 0xb4, 0x8a, 255)
ROOF_L, ROOF, ROOF_D, ROOF_DD = (0xc4, 0x66, 0x44, 255), (0x9c, 0x47, 0x30, 255), (0x76, 0x34, 0x26, 255), (0x58, 0x26, 0x1c, 255)
STONE_L, STONE, STONE_D = (0xc0, 0xb8, 0xa6, 255), (0xa0, 0x98, 0x88, 255), (0x76, 0x6e, 0x62, 255)
FLOOR, FLOOR_D = (0xc8, 0x9e, 0x66, 255), (0xa6, 0x7e, 0x4c, 255)
SHADE = (0x3a, 0x28, 0x1c, 255)
GOLD, GOLD_D = (0xea, 0xc4, 0x54, 255), (0xb4, 0x8a, 0x2c, 255)


def lerp(a, b, t):
    return (a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t)


def main():
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    # ---- the base: a six-sided floor seen from the front, a stone skirt under its front edges and two steps ----
    floor = [(8, 88), (24, 100), (72, 100), (88, 88), (72, 76), (24, 76)]
    d.polygon([(8, 88), (24, 100), (72, 100), (88, 88), (88, 96), (72, 108), (24, 108), (8, 96)], fill=STONE, outline=INK)
    d.polygon([(8, 88), (24, 100), (24, 108), (8, 96)], fill=STONE_D, outline=INK)            # the left front face in shade
    d.polygon([(88, 88), (72, 100), (72, 108), (88, 96)], fill=STONE, outline=INK)
    for x in range(26, 72, 8):                                                                # stones in the skirt
        d.line([(x, 101), (x, 107)], fill=STONE_D)
    d.line([(25, 101), (71, 101)], fill=STONE_L)
    d.polygon(floor, fill=FLOOR, outline=INK)
    for y in range(78, 100, 4):                                                               # planks
        x0 = 24 if y < 88 else 24
        d.line([(x0 + (0 if y >= 88 else 2), y), (72 - (0 if y >= 88 else 2), y)], fill=FLOOR_D)
    d.line([(10, 88), (24, 99)], fill=FLOOR_D)
    d.line([(86, 88), (72, 99)], fill=FLOOR_D)
    # steps down the middle of the front
    d.rectangle([34, 100, 62, 104], fill=STONE_L, outline=INK)
    d.rectangle([30, 105, 66, 110], fill=STONE, outline=INK)
    d.line([(31, 106), (65, 106)], fill=STONE_L)
    d.line([(35, 101), (61, 101)], fill=CREAM)

    # ---- the back rail and the two back posts (behind the roof's shadow) ----
    shade = [(22, 70), (74, 70), (86, 82), (10, 82)]
    d.polygon([(24, 70), (72, 70), (86, 86), (10, 86)], fill=SHADE)
    for (x0, y0, x1, y1) in [(24, 78, 72, 78), (10, 88, 24, 76), (86, 88, 72, 76)]:           # rails: top bar
        d.line([(x0, y0 - 1), (x1, y1 - 1)], fill=WOOD_L, width=1)
        d.line([(x0, y0), (x1, y1)], fill=WOOD, width=2)
    for x in range(28, 72, 6):                                                                # balusters along the back
        d.line([(x, 79), (x, 85)], fill=WOOD_D)
    for (a, b) in [((10, 88), (24, 76)), ((86, 88), (72, 76))]:
        for t in (0.35, 0.7):
            p = lerp(a, b, t)
            d.line([(p[0], p[1]), (p[0], p[1] + 6)], fill=WOOD_D)
    # a bench along the back wall, inside
    d.rectangle([30, 82, 66, 86], fill=WOOD, outline=INK)
    d.line([(31, 83), (65, 83)], fill=WOOD_L)

    # ---- the posts ----
    def post(x, y0, y1):
        d.rectangle([x - 2, y1, x + 2, y0], fill=WOOD, outline=INK)
        d.line([(x - 1, y1 + 1), (x - 1, y0 - 1)], fill=WOOD_L)
        d.line([(x + 2, y1 + 1), (x + 2, y0 - 1)], fill=WOOD_D)
        d.rectangle([x - 3, y0 - 1, x + 3, y0 + 1], fill=CREAM, outline=INK)                  # foot
    for (x, foot, top) in [(26, 99, 68), (70, 99, 68), (10, 88, 62), (86, 88, 62), (26, 77, 62), (70, 77, 62)]:
        post(x, foot, top)

    # ---- the roof: an eight-sided cone seen from the front: five faces from the apex to the eave ----
    apex = (48, 5)
    eave = [(2, 54), (17, 63), (34, 68), (62, 68), (79, 63), (94, 54)]
    shades = [ROOF_DD, ROOF_D, ROOF_L, ROOF, ROOF_D]
    for i in range(5):
        d.polygon([apex, eave[i], eave[i + 1]], fill=shades[i])
    # shingle courses: curves parallel to the eave, a little paler on the sunny faces
    for t in (0.3, 0.52, 0.74):
        pts = [lerp(apex, v, t) for v in eave]
        for i in range(5):
            d.line([pts[i], pts[i + 1]], fill=ROOF_DD if shades[i] in (ROOF_D, ROOF_DD) else ROOF_D)
    for v in eave[1:5]:                                                                       # the ridges
        d.line([apex, lerp(apex, v, 1.0)], fill=ROOF_DD)
    d.polygon([apex] + eave, outline=INK)
    # the eave: a cream fascia board with a scalloped lower edge
    for i in range(5):
        a, b = eave[i], eave[i + 1]
        d.line([(a[0], a[1] + 1), (b[0], b[1] + 1)], fill=CREAM, width=3)
        d.line([(a[0], a[1] + 3), (b[0], b[1] + 3)], fill=CREAM_D, width=1)
        d.line([(a[0], a[1] + 4), (b[0], b[1] + 4)], fill=INK, width=1)
    # a gold finial and a small cupola cap on the top
    d.polygon([(45, 8), (51, 8), (48, 1)], fill=GOLD, outline=INK)
    d.rectangle([47, 0, 48, 4], fill=GOLD_D)
    d.ellipse([45, 8, 51, 13], fill=GOLD, outline=INK)

    img.save(OUT)
    print("wrote", OUT)


if __name__ == "__main__":
    main()
