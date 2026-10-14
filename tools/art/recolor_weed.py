"""The weed of the farm's clutter had orange flowers and looked like a dandelion (playtest 2026-10-13: "these flowers still look like dandelions"; 0.6.1 had only recoloured the
ground decoration). Its orange and red flower pixels are turned purple (hue shifted, shading kept); the green leaves are untouched. Idempotent: purple pixels are not touched.
Run from the repository root: python tools/art/recolor_weed.py"""
import colorsys, os
from PIL import Image

PATH = os.path.join(os.path.dirname(__file__), '..', '..', 'Assets', '_Project', 'Art', 'Placeholders', 'obj_weed.png')
img = Image.open(PATH).convert('RGBA')
px = img.load()
changed = 0
for y in range(img.height):
    for x in range(img.width):
        r, g, b, a = px[x, y]
        if not a: continue
        h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
        if s > 0.25 and (h < 0.14 or h > 0.93):                # orange, yellow-orange and red; leaves are green (0.2 to 0.45)
            r2, g2, b2 = colorsys.hsv_to_rgb(0.77, min(1.0, s * 0.9), min(1.0, v * 1.05))
            px[x, y] = (int(r2 * 255), int(g2 * 255), int(b2 * 255), a)
            changed += 1
img.save(PATH)
print('recoloured', changed, 'pixels')
