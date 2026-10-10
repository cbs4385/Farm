"""The farm's greenhouse picture (2026-10-10): a 144 x 80 front view (nine cells wide, five tall) drawn here in the palette of the Cozy Village kit's farm buildings
(the barn and the coop): a dark outline, warm timber, a plank skirting, panes of pale glass between timber mullions, a glazed roof with a ridge and a vent, plants
showing through, and a glazed double door under a transom, with the door's centre at pixel 72. The kit has no greenhouse, so it is drawn rather than imported.
Original art. Written to Assets/_Project/Art/Placeholders/prop_bld_greenhouse.png; then run build_building_masks.py (its door pixel for 'greenhouse' is 72).
Run from the repository root: python tools/art/build_greenhouse.py"""
import os
from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = os.path.join(ROOT, 'Assets', '_Project', 'Art', 'Placeholders', 'prop_bld_greenhouse.png')

W, H = 144, 80


def c(h, a=255):
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)


INK = c('230b07')
WOOD_L = c('c5814f')
WOOD = c('ad6a41')
WOOD_D = c('7b4b36')
WOOD_DD = c('572f27')
GLASS_L = c('d7efe9')
GLASS = c('a8d3cf')
GLASS_D = c('7fb0b2')
GLASS_DD = c('5e8e94')
SHINE = c('f4fbf6')
LEAF = c('5b7a4c')
LEAF_D = c('3f5a37')
LEAF_L = c('8aa860')
SOIL = c('6b4a3a')
SOIL_D = c('4a3228')
TOMATO = c('b25638')
STONE = c('838a8c')
STONE_D = c('636a6a')


def main():
    img = Image.new('RGBA', (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    def rect(x0, y0, x1, y1, col):
        d.rectangle([x0, y0, x1, y1], fill=col)

    def hline(x0, x1, y, col):
        d.line([(x0, y), (x1, y)], fill=col)

    def vline(x, y0, y1, col):
        d.line([(x, y0), (x, y1)], fill=col)

    def glazed_wall(x0, y0, x1, y1, step):
        """Panes with timber mullions between x0..x1 and y0..y1 (inclusive), the plants behind them."""
        rect(x0, y0, x1, y1, GLASS)
        rect(x0, y0, x1, y0 + 2, GLASS_L)                       # light from above
        # plants standing behind the glass, along the bottom
        for i, x in enumerate(range(x0 + 3, x1 - 3, 7)):
            top = y1 - 9 - (i * 5) % 7
            rect(x, top, x + 4, y1, LEAF)
            rect(x + 1, top, x + 2, top + 2, LEAF_L)
            rect(x + 3, top + 3, x + 4, y1, LEAF_D)
            if i % 3 == 1: rect(x + 2, top + 4, x + 3, top + 5, TOMATO)
        rect(x0, y1 - 2, x1, y1, SOIL)
        hline(x0, x1, y1 - 2, SOIL_D)
        # a glint on every other pane
        for x in range(x0 + step, x1, step * 2):
            for k in range(5): d.point((x + 3 + k, y0 + 4 + k), fill=SHINE)
            for k in range(3): d.point((x + 5 + k, y0 + 3 + k), fill=SHINE)
        # mullions
        for x in range(x0, x1 + 1, step): rect(x, y0, x + 1, y1, WOOD)
        for x in range(x0, x1 + 1, step): vline(x + 1, y0, y1, WOOD_D)
        rect(x1 - 1, y0, x1, y1, WOOD)
        vline(x1, y0, y1, WOOD_D)

    # ---- the wings (low, a lean-to roof each side) ----
    for side in (0, 1):
        x0, x1 = (4, 33) if side == 0 else (110, 139)
        # skirting
        rect(x0, 66, x1, 79, INK)
        rect(x0 + 1, 67, x1 - 1, 79, WOOD)
        for y in (71, 75): hline(x0 + 1, x1 - 1, y, WOOD_D)
        hline(x0 + 1, x1 - 1, 67, WOOD_L)
        # glass
        rect(x0, 50, x1, 66, INK)
        glazed_wall(x0 + 1, 51, x1 - 1, 65, 10)
        # the lean-to roof: a glazed band sloping up to the middle block
        for y in range(36, 50):
            inner = x1 if side == 0 else x0
            rect(min(x0, inner), y, max(x0, inner), y, GLASS)
        rect(x0, 34, x1, 49, INK)
        rect(x0 + 1, 35, x1 - 1, 48, GLASS_D)
        rect(x0 + 1, 35, x1 - 1, 37, GLASS)
        rect(x0 + 1, 46, x1 - 1, 48, GLASS_DD)
        for x in range(x0 + 1, x1, 10): vline(x, 35, 48, WOOD_D)
        for k in range(8): d.point((x0 + 5 + k, 44 - k // 2), fill=SHINE)
        rect(x0, 33, x1, 34, WOOD_DD)                            # the eave beam
        hline(x0, x1, 33, WOOD_D)

    # ---- the middle block ----
    mx0, mx1 = 30, 113
    rect(mx0, 62, mx1, 79, INK)
    rect(mx0 + 1, 63, mx1 - 1, 79, WOOD)
    hline(mx0 + 1, mx1 - 1, 63, WOOD_L)
    for y in (67, 71, 75): hline(mx0 + 1, mx1 - 1, y, WOOD_D)
    for x in range(mx0 + 1, mx1, 12): vline(x, 64, 79, WOOD_D)
    # stone footing
    rect(mx0 - 1, 77, mx1 + 1, 79, INK)
    rect(mx0, 77, mx1, 78, STONE)
    for x in range(mx0, mx1, 6): d.point((x, 78), fill=STONE_D)
    hline(mx0, mx1, 79, WOOD_DD)

    rect(mx0, 34, mx1, 62, INK)
    glazed_wall(mx0 + 1, 35, mx1 - 1, 61, 12)

    # ---- the door: a glazed double door with a timber frame under a transom ----
    dx0, dx1 = 62, 81
    rect(dx0 - 2, 44, dx1 + 2, 79, INK)
    rect(dx0 - 1, 45, dx1 + 1, 79, WOOD_D)
    rect(dx0, 47, dx1, 79, WOOD)
    # transom
    rect(dx0, 47, dx1, 52, GLASS_D)
    rect(dx0, 47, dx1, 48, GLASS)
    for x in (dx0 + 5, dx0 + 10, dx0 + 15): vline(x, 47, 52, WOOD)
    hline(dx0, dx1, 53, WOOD_DD)
    # the two leaves
    for lx0, lx1 in ((dx0 + 1, dx0 + 8), (dx0 + 11, dx1 - 1)):
        rect(lx0, 55, lx1, 76, WOOD_L)
        rect(lx0 + 1, 56, lx1 - 1, 66, GLASS)
        rect(lx0 + 1, 56, lx1 - 1, 57, GLASS_L)
        rect(lx0 + 1, 62, lx1 - 1, 66, LEAF)
        rect(lx0 + 2, 62, lx1 - 2, 63, LEAF_L)
        hline(lx0, lx1, 67, WOOD_D)
        rect(lx0 + 1, 69, lx1 - 1, 75, WOOD)
        hline(lx0 + 1, lx1 - 1, 69, WOOD_DD)
        for x in (lx0 + 1, lx1 - 1): vline(x, 55, 76, WOOD_D)
    vline(dx0 + 9, 55, 79, WOOD_DD)
    vline(dx0 + 10, 55, 79, WOOD_DD)
    d.point((dx0 + 7, 68), fill=c('e8c15a')); d.point((dx0 + 12, 68), fill=c('e8c15a'))       # the handles
    rect(dx0 - 4, 78, dx1 + 4, 79, WOOD_L)                                                 # the sill
    hline(dx0 - 4, dx1 + 4, 79, WOOD_D)

    # ---- the high glazed roof of the middle block, with its ridge ----
    pts = [(24, 35), (120, 35), (104, 8), (40, 8)]
    d.polygon(pts, fill=INK)
    d.polygon([(26, 34), (118, 34), (103, 10), (41, 10)], fill=GLASS_D)
    # panes: lighter towards the top, darker at the eave
    d.polygon([(33, 21), (110, 21), (103, 10), (41, 10)], fill=GLASS)
    d.polygon([(41, 10), (103, 10), (101, 13), (43, 13)], fill=GLASS_L)
    d.polygon([(26, 34), (118, 34), (116, 30), (28, 30)], fill=GLASS_DD)
    # rafters fanning down from the ridge
    for i in range(9):
        top_x = 41 + i * 62 / 8
        bot_x = 26 + i * 92 / 8
        d.line([(top_x, 10), (bot_x, 34)], fill=WOOD, width=1)
    for y in (21, 28): d.line([(26 + (34 - y) * 0.15, y), (118 - (34 - y) * 0.15, y)], fill=WOOD_D, width=1)
    for k in range(7): d.point((50 + k, 18 - k // 2), fill=SHINE); d.point((82 + k, 19 - k // 2), fill=SHINE)
    # the ridge board and its cap
    rect(39, 6, 105, 9, INK)
    rect(40, 7, 104, 8, WOOD_L)
    hline(40, 104, 8, WOOD_D)
    # a vent: a timber-framed pane propped open on the left of the roof
    d.polygon([(52, 12), (68, 12), (70, 20), (50, 20)], fill=INK)
    d.polygon([(53, 13), (67, 13), (68, 19), (52, 19)], fill=WOOD)
    d.polygon([(55, 14), (65, 14), (66, 18), (54, 18)], fill=GLASS_L)
    d.point((57, 15), fill=SHINE); d.point((58, 15), fill=SHINE)
    hline(53, 67, 13, WOOD_L)
    # the eave beam over the middle block
    rect(24, 35, 120, 37, INK)
    rect(25, 35, 119, 36, WOOD_D)
    hline(25, 119, 35, WOOD)

    # tidy: a soft shade under the eaves so the glass reads as set back
    for x in range(mx0 + 1, mx1):
        if img.getpixel((x, 38))[3] and img.getpixel((x, 38)) != INK: img.putpixel((x, 38), GLASS_DD)

    img.save(OUT)
    print('wrote', OUT, img.size)


if __name__ == '__main__':
    main()
