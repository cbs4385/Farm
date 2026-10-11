"""The torch's item picture (2026-10-11): 16 x 16, a wooden handle wrapped in cloth with a flame on top. Palette of the kit. Original art.
Written to Assets/_Project/Art/Placeholders/item_resource_torch.png (listed in final_art.txt so the placeholder generator leaves it alone).
Run from the repository root: python tools/art/build_torch.py [output.png]"""
import os, sys
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(ROOT, 'Assets', '_Project', 'Art', 'Placeholders', 'item_resource_torch.png')


def c(h):
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), 255)


INK = c('230b07'); WOOD_L = c('c5814f'); WOOD = c('ad6a41'); WOOD_D = c('7b4b36')
CLOTH = c('d8c9a0'); CLOTH_D = c('a8956a'); RED = c('c2401f'); ORANGE = c('f08a24'); YELLOW = c('f9d85a'); WHITE = c('fff6c8')

img = Image.new('RGBA', (16, 16), (0, 0, 0, 0))
px = img.load()


def rect(x0, y0, x1, y1, col):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1): px[x, y] = col


rect(6, 8, 9, 15, INK); rect(7, 9, 8, 15, WOOD); rect(7, 9, 7, 15, WOOD_L); rect(8, 12, 8, 15, WOOD_D)      # the handle
rect(5, 7, 10, 10, INK); rect(6, 8, 9, 9, CLOTH); rect(6, 9, 9, 9, CLOTH_D)                                 # the wrapped head
rect(5, 3, 10, 7, INK)                                                                                      # the flame's outline
rect(6, 4, 9, 7, RED); rect(6, 5, 9, 7, ORANGE); rect(7, 6, 8, 7, YELLOW); px[7, 7] = WHITE; px[8, 7] = WHITE
rect(7, 1, 8, 3, INK); px[7, 2] = ORANGE; px[8, 3] = RED; px[7, 3] = ORANGE; px[8, 2] = INK
px[7, 4] = ORANGE
img.save(OUT); print('wrote', OUT)
