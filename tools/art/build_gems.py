"""Gems, gem clusters and jewellery (playtest 2026-10-13), from the one crystal the Caves and Mines Props sample has (crystal_cluster_large.png, StationSupply, kept in
AssetPacks/). The blue cluster is recoloured by hue (saturation scaled, value kept) into one cluster per gem, shrunk to the game's 16 x 16 cell; the gem items are its tallest
crystal; the rings and pendants are drawn here (original pixel art) and take the gem's own colour from its recoloured crystal.
Writes to Assets/_Project/Art/Placeholders: obj_<gem>_node.png (16x16), item_gem_<gem>.png (16x16), item_jewelry_<gem>_ring.png and item_jewelry_<gem>_pendant.png (16x16).
Run from the repository root: python tools/art/build_gems.py"""
import colorsys, os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
SRC = os.path.join(ROOT, 'AssetPacks', 'Caves-and-Mines-Props-16px-FREE-Sample', 'Caves-and-Mines-Props-16px-FREE-Sample', 'sprites', '1x', 'crystal_cluster_large.png')
OUT = os.path.join(ROOT, 'Assets', '_Project', 'Art', 'Placeholders')

# id: (hue in degrees, saturation scale, value scale)
GEMS = {
    'amethyst': (278, 1.0, 0.95),
    'topaz': (38, 1.15, 1.15),
    'aquamarine': (186, 0.85, 1.1),
    'emerald': (140, 1.0, 0.9),
    'ruby': (352, 1.1, 0.95),
    'diamond': (200, 0.12, 1.35),
}
INK = (35, 11, 7, 255)
GOLD = (232, 193, 90, 255); GOLD_D = (176, 130, 50, 255); STEEL = (160, 168, 176, 255); STEEL_D = (100, 108, 118, 255)


def tint(img, hue, sat, val):
    out = img.copy()
    px = out.load()
    for y in range(out.height):
        for x in range(out.width):
            r, g, b, a = px[x, y]
            if a == 0: continue
            h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
            if s > 0.25:                                   # the crystal; the stones at its foot stay grey
                h = hue / 360.0
                s = min(1.0, s * sat)
                v = min(1.0, v * val)
            elif sat < 0.5:
                v = min(1.0, v * 1.05)
            r, g, b = colorsys.hsv_to_rgb(h, s, v)
            px[x, y] = (int(r * 255), int(g * 255), int(b * 255), a)
    return out


def shrink(img, box, size):
    crop = img.crop(box)
    scale = min(size[0] / crop.width, size[1] / crop.height)
    w, h = max(1, round(crop.width * scale)), max(1, round(crop.height * scale))
    small = crop.resize((w, h), Image.BOX)
    px = small.load()
    for y in range(h):
        for x in range(w):                                  # hard edges: no half-transparent pixels in pixel art
            r, g, b, a = px[x, y]
            px[x, y] = (r, g, b, 255 if a >= 128 else 0)
    canvas = Image.new('RGBA', size, (0, 0, 0, 0))
    canvas.paste(small, ((size[0] - w) // 2, size[1] - h), small)           # standing on the bottom edge
    return canvas


def mid_color(img):
    px = [p for p in img.getdata() if p[3] and max(p[:3]) - min(p[:3]) > 20 or p[3] and sum(p[:3]) > 600]
    px = px or [p for p in img.getdata() if p[3]]
    r = sum(p[0] for p in px) // len(px); g = sum(p[1] for p in px) // len(px); b = sum(p[2] for p in px) // len(px)
    return (r, g, b, 255)


def shade(c, f):
    return (min(255, int(c[0] * f)), min(255, int(c[1] * f)), min(255, int(c[2] * f)), 255)


def ring(gem):
    img = Image.new('RGBA', (16, 16), (0, 0, 0, 0)); px = img.load()
    cx, cy = 7.5, 10.0
    for y in range(16):
        for x in range(16):
            d = ((x - cx) ** 2 + (y - cy) ** 2) ** 0.5
            if 3.0 <= d <= 5.6: px[x, y] = INK
            if 3.9 <= d <= 4.7: px[x, y] = GOLD if (x + y) % 5 else GOLD_D
    for (x, y) in [(6, 3), (7, 3), (8, 3), (9, 3), (5, 4), (10, 4), (5, 5), (10, 5), (6, 6), (7, 6), (8, 6), (9, 6)]: px[x, y] = INK
    for (x, y) in [(6, 4), (7, 4), (8, 4), (9, 4), (6, 5), (7, 5), (8, 5), (9, 5)]: px[x, y] = gem
    px[6, 4] = shade(gem, 1.5); px[7, 4] = shade(gem, 1.25); px[9, 5] = shade(gem, 0.7)
    return img


def pendant(gem):
    img = Image.new('RGBA', (16, 16), (0, 0, 0, 0)); px = img.load()
    for i in range(6):                                       # the chain, two arms meeting
        for (x, y) in [(2 + i, 1 + i), (13 - i, 1 + i)]:
            px[x, y] = INK
            if (x, y + 0) and i % 2 == 0: px[x, y] = STEEL
    for (x, y) in [(7, 7), (8, 7)]: px[x, y] = INK
    for (x, y) in [(6, 8), (9, 8), (5, 9), (10, 9), (5, 10), (10, 10), (6, 11), (9, 11), (7, 12), (8, 12)]: px[x, y] = INK
    for (x, y) in [(7, 8), (8, 8), (6, 9), (7, 9), (8, 9), (9, 9), (6, 10), (7, 10), (8, 10), (9, 10), (7, 11), (8, 11)]: px[x, y] = gem
    px[7, 8] = shade(gem, 1.5); px[6, 9] = shade(gem, 1.25); px[8, 11] = shade(gem, 0.7)
    return img


src = Image.open(SRC).convert('RGBA')
for gem_id, (hue, sat, val) in GEMS.items():
    t = tint(src, hue, sat, val)
    shrink(t, (1, 17, 47, 63), (16, 16)).save(os.path.join(OUT, 'obj_%s_node.png' % gem_id))
    icon = shrink(t, (15, 16, 35, 50), (16, 16))              # the tallest crystal on its own
    icon.save(os.path.join(OUT, 'item_gem_%s.png' % gem_id))
    colour = mid_color(icon)
    ring(colour).save(os.path.join(OUT, 'item_jewelry_%s_ring.png' % gem_id))
    pendant(colour).save(os.path.join(OUT, 'item_jewelry_%s_pendant.png' % gem_id))
    print('wrote', gem_id)
