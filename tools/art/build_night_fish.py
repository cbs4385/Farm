"""The night fish (playtest 2026-10-13: "the special fish don't appear to come at night"): seven more fish that bite only after 20:00, drawn by recolouring the day fish pictures
(hue shifted, darkened, with a few pale glowing pixels along the back). Writes item_fish_<id>.png to Assets/_Project/Art/Placeholders.
Run from the repository root: python tools/art/build_night_fish.py"""
import colorsys, os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
DIR = os.path.join(ROOT, 'Assets', '_Project', 'Art', 'Placeholders')
# id: (base fish, hue, saturation scale, value scale)
FISH = {
    'lanternfish': ('sardine', 175, 1.6, 0.85),
    'cuttlefish': ('squid', 285, 1.3, 0.75),
    'stingray': ('flounder', 215, 1.4, 0.55),
    'midnight_carp': ('carp', 235, 1.5, 0.55),
    'ghost_perch': ('perch', 190, 0.35, 1.25),
    'black_bass': ('pike', 255, 0.9, 0.45),
    'starfin': ('trout', 50, 1.5, 1.2),
}
GLOW = (235, 250, 190, 255)

for fid, (base, hue, sat, val) in FISH.items():
    img = Image.open(os.path.join(DIR, 'item_fish_%s.png' % base)).convert('RGBA')
    px = img.load()
    top = {}
    for y in range(img.height):
        for x in range(img.width):
            r, g, b, a = px[x, y]
            if a == 0: continue
            h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
            if s > 0.12 or v > 0.35:                                   # keep the dark outline
                h = hue / 360.0
                s = min(1.0, max(s, 0.25) * sat)
                v = min(1.0, v * val)
                r, g, b = colorsys.hsv_to_rgb(h, s, v)
                px[x, y] = (int(r * 255), int(g * 255), int(b * 255), 255)
            top.setdefault(x, y)
    for i, x in enumerate(sorted(top)):                               # a few lit pixels along the back
        if i % 3 == 1: px[x, top[x] + 1 if top[x] + 1 < img.height else top[x]] = GLOW
    img.save(os.path.join(DIR, 'item_fish_%s.png' % fid))
    print('wrote', fid)
