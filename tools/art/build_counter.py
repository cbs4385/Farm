"""The wide shop counter (obj_counter_wide, 32 x 16), drawn by the project instead of generated: it must run edge to edge so that counters placed
side by side read as one long counter (the generated version floated in the middle of its frame, with a gap between pieces).
Usage: python tools/art/build_counter.py   (writes Assets/_Project/Art/Placeholders/obj_counter_wide.png)"""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders", "obj_counter_wide.png")

W, H = 32, 16
OUTLINE = (84, 52, 36, 255)
TOP_LIGHT = (244, 220, 176, 255)
TOP = (226, 192, 142, 255)
TOP_EDGE = (190, 150, 106, 255)
FRONT = (178, 112, 66, 255)
FRONT_LIGHT = (200, 136, 84, 255)
FRONT_DARK = (146, 88, 52, 255)
SEAM = (122, 74, 44, 255)

img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
px = img.load()

def rect(x0, y0, x1, y1, c):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1): px[x, y] = c

# The top surface (seen slightly from above): rows 2 to 6, outlined on top.
rect(0, 2, W - 1, 2, OUTLINE)
rect(0, 3, W - 1, 3, TOP_LIGHT)
rect(0, 4, W - 1, 5, TOP)
rect(0, 6, W - 1, 6, TOP_EDGE)
# The front: rows 7 to 14, a lit band, the planks, a shade at the foot.
rect(0, 7, W - 1, 7, OUTLINE)
rect(0, 8, W - 1, 8, FRONT_LIGHT)
rect(0, 9, W - 1, 12, FRONT)
rect(0, 13, W - 1, 13, FRONT_DARK)
rect(0, 14, W - 1, 14, OUTLINE)
# Plank seams at fixed spacing that divide the 32 cells evenly, so two pieces side by side continue the pattern.
for x in (7, 15, 23, 31):
    rect(x, 9, x, 12, SEAM)
# A few knots and highlights on the top.
for x, y in ((5, 4), (13, 5), (21, 4), (28, 5)):
    px[x, y] = TOP_LIGHT
for x in (3, 11, 19, 27):
    px[x, 10] = FRONT_LIGHT

os.makedirs(os.path.dirname(OUT), exist_ok=True)
img.save(OUT)
print("wrote", OUT)
