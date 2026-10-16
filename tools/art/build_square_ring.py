"""An inlaid ring of paving stones round the fountain in the village square (playtest 2026-10-16: the square did not look like one): two concentric bands of paler and darker stones
on the cobbles, laid on the ground. `obj_plaza_ring.png`, 144 x 112 pixels (9 x 7 cells), drawn here, original art; it is centred on the
middle cell of the square (25, 17), where the fountain stands. Run from the repository root: py tools/art/build_square_ring.py"""
import math
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders", "obj_plaza_ring.png")
W, H = 144, 112
LIGHT, LIGHT_D = (0xc6, 0xbe, 0xae, 255), (0xa8, 0xa0, 0x92, 255)
DARK, DARK_D = (0x84, 0x7c, 0x72, 255), (0x6a, 0x63, 0x5a, 255)
JOINT = (0x5a, 0x54, 0x4c, 255)


def main():
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    px = img.load()
    cx, cy = W / 2 - 0.5, H / 2 - 0.5
    for y in range(H):
        for x in range(W):
            dx, dy = (x - cx) / 70.0, (y - cy) / 53.0
            r = math.hypot(dx, dy)
            if r > 1.0 or r < 0.70: continue
            angle = math.atan2(dy, dx)
            stone = int((angle + math.pi) / (2 * math.pi) * 48)
            band = 0 if r > 0.86 else 1                 # an outer band of pale stones and an inner band of dark ones
            edge = abs(r - 1.0) < 0.022 or abs(r - 0.86) < 0.018 or abs(r - 0.70) < 0.022
            seam = abs(((angle + math.pi) / (2 * math.pi) * 48) % 1.0 - 0.5) < 0.07 and not edge
            if edge or seam: px[x, y] = JOINT
            else:
                light = (stone + band) % 2 == 0
                px[x, y] = (LIGHT if light else LIGHT_D) if band == 0 else (DARK if light else DARK_D)
    img.save(OUT)
    print("wrote", OUT)


if __name__ == "__main__":
    main()
