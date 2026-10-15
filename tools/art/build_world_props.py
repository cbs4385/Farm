"""Three pieces of the world that playtesters could not read (2026-10-10), redrawn here in the warm palette of the kit's farm buildings. Original art.
  obj_chest.png     24 x 24  the chest a player sets down: bigger than before (it was a 16 x 16 smudge), a wooden body, a rounded lid, iron bands and a lock.
  prop_bench.png    32 x 16  the village benches (two cells wide): the kit's round bench came out of the 2 x 2 reduction as a black blob, so this is a front view.
  fx_mail.png       16 x 16  a small envelope that bobs over the mailbox when there is something in it.
  obj_toolrack.png  16 x 16  the rack in front of the farmhouse (also item_machine_toolrack.png): two posts and a rail with a hoe, an axe and a pickaxe hanging from it.
  obj_board.png     16 x 16  the help-wanted board: two posts, a plank board with pinned notes (the old one was a tilted brown slab).
Run from the repository root: python tools/art/build_world_props.py"""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = os.path.join(ROOT, 'Assets', '_Project', 'Art', 'Placeholders')


def c(h):
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), 255)


INK = c('230b07')
WOOD_L = c('c5814f')
WOOD = c('ad6a41')
WOOD_D = c('7b4b36')
WOOD_DD = c('572f27')
IRON = c('838a8c')
IRON_D = c('636a6a')
IRON_L = c('b6bdbf')
GOLD = c('e8c15a')
PAPER = c('ecdcb0')
PAPER_D = c('c9b284')
RED = c('b25638')
BLUE = c('5f86a8')
GREEN_L = c('8aa860')


def canvas(w, h):
    img = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    return img, img.load()


def rect(px, x0, y0, x1, y1, col):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            px[x, y] = col


def chest():
    img, px = canvas(24, 24)
    # shadow of the body on the ground, then the body (y 11..21) and the rounded lid (y 4..11)
    rect(px, 3, 22, 20, 22, c('1a1411'))
    rect(px, 2, 10, 21, 21, INK)
    rect(px, 3, 11, 20, 20, WOOD)
    rect(px, 3, 11, 20, 12, WOOD_D)                      # the shade under the lid
    rect(px, 3, 13, 5, 20, WOOD_L)                       # the lit left side
    rect(px, 18, 13, 20, 20, WOOD_D)
    for y in (15, 18): rect(px, 4, y, 19, y, WOOD_D)     # planks
    rect(px, 3, 20, 20, 20, WOOD_DD)
    # the lid: a barrel curve
    rect(px, 3, 3, 20, 10, INK)
    rect(px, 4, 4, 19, 10, WOOD_L)
    rect(px, 2, 6, 21, 10, INK)
    rect(px, 3, 6, 20, 10, WOOD_L)
    rect(px, 4, 3, 19, 3, INK)
    rect(px, 5, 4, 18, 5, WOOD_L)
    rect(px, 3, 8, 20, 10, WOOD)
    rect(px, 3, 10, 20, 10, WOOD_D)
    for x in range(5, 18, 4): px[x, 5] = WOOD
    # iron bands, front and over the lid
    for x0 in (5, 17):
        rect(px, x0, 4, x0 + 1, 21, IRON)
        rect(px, x0, 4, x0, 21, IRON_L)
        rect(px, x0 + 1, 4, x0 + 1, 21, IRON_D)
        px[x0, 8] = INK; px[x0 + 1, 15] = INK
    # the lock plate and keyhole
    rect(px, 10, 9, 13, 14, INK)
    rect(px, 11, 10, 12, 13, GOLD)
    px[11, 11] = INK; px[11, 12] = INK
    img.save(os.path.join(OUT, 'obj_chest.png'))


def toolrack():
    img, px = canvas(16, 16)
    for x0 in (0, 13):                                                        # the two posts, three wide with an outline each side
        rect(px, x0, 1, x0 + 2, 15, INK)
        rect(px, x0 + 1, 1, x0 + 1, 14, WOOD)
        px[x0 + 1, 15] = WOOD_D
    rect(px, 3, 2, 12, 5, INK)                                                # the rail between them
    rect(px, 3, 3, 12, 4, WOOD)
    rect(px, 3, 3, 12, 3, WOOD_L)
    rect(px, 3, 4, 12, 4, WOOD_D)
    # a hoe: a handle with a bent blade
    rect(px, 4, 5, 4, 12, WOOD_L)
    rect(px, 3, 12, 5, 12, IRON_L); px[3, 13] = IRON_D; px[4, 13] = INK; px[5, 13] = INK; px[3, 11] = INK
    # an axe: a handle with a steel head
    rect(px, 8, 5, 8, 14, WOOD_L)
    rect(px, 6, 8, 7, 11, IRON_L); rect(px, 6, 9, 6, 10, IRON_D); px[7, 8] = IRON; px[7, 11] = IRON_D
    px[6, 7] = INK; px[7, 7] = INK; px[5, 9] = INK; px[5, 10] = INK; px[6, 12] = INK; px[7, 12] = INK
    # a pickaxe: a handle with a curved double head
    rect(px, 11, 5, 11, 13, WOOD_L)
    rect(px, 9, 10, 12, 10, IRON_L); px[9, 11] = IRON_D; px[12, 11] = IRON_D
    px[8, 10] = INK; px[8, 11] = INK; px[13, 10] = INK
    px[3, 3] = IRON_L; px[12, 3] = IRON_L                                     # nails
    img.save(os.path.join(OUT, 'obj_toolrack.png'))
    img.save(os.path.join(OUT, 'item_machine_toolrack.png'))


def bench():
    img, px = canvas(32, 16)
    # back rest slats
    rect(px, 2, 2, 29, 8, INK)
    rect(px, 3, 3, 28, 4, WOOD_L)
    rect(px, 3, 5, 28, 5, WOOD_D)
    rect(px, 3, 6, 28, 7, WOOD)
    # uprights from the back rest to the ground behind the seat
    for x in (3, 4, 27, 28): rect(px, x, 8, x, 13, WOOD_DD)
    # the seat
    rect(px, 1, 9, 30, 12, INK)
    rect(px, 2, 10, 29, 11, WOOD_L)
    rect(px, 2, 12, 29, 12, WOOD_D)
    for x in (8, 15, 22): px[x, 10] = WOOD; px[x, 11] = WOOD
    # legs, with an iron brace
    for x0 in (3, 26):
        rect(px, x0 - 1, 13, x0 + 2, 15, INK)
        rect(px, x0, 13, x0 + 1, 14, WOOD_D)
        px[x0, 14] = WOOD
    rect(px, 5, 13, 26, 13, WOOD_DD)
    img.save(os.path.join(OUT, 'prop_bench.png'))


def board():
    img, px = canvas(16, 16)
    for x0 in (2, 11):                                                        # the posts, under the board
        rect(px, x0, 10, x0 + 2, 15, INK)
        rect(px, x0 + 1, 10, x0 + 1, 14, WOOD)
        px[x0 + 1, 15] = WOOD_D
    rect(px, 0, 0, 15, 11, INK)                                               # the board
    rect(px, 1, 1, 14, 10, WOOD)
    rect(px, 1, 1, 14, 1, WOOD_L)
    rect(px, 1, 10, 14, 10, WOOD_D)
    for x, y, col, pin in ((2, 3, PAPER, RED), (7, 2, PAPER_D, BLUE), (11, 4, PAPER, GREEN_L)):          # pinned notes
        rect(px, x, y, x + 2, y + 4, col)
        px[x + 1, y] = pin
        px[x + 1, y + 2] = WOOD_D
    px[8, 6] = WOOD_D
    img.save(os.path.join(OUT, 'obj_board.png'))


def mail():
    img, px = canvas(16, 16)
    rect(px, 1, 4, 14, 12, INK)
    rect(px, 2, 5, 13, 11, PAPER)
    rect(px, 2, 11, 13, 11, PAPER_D)
    for i in range(6):                                                        # the flap
        px[2 + i, 5 + i // 2] = PAPER_D; px[13 - i, 5 + i // 2] = PAPER_D
    px[7, 8] = RED; px[8, 8] = RED; px[7, 7] = RED; px[8, 7] = RED          # the seal
    px[7, 8] = RED
    rect(px, 11, 1, 12, 3, GOLD)                                              # a glint
    px[10, 2] = GOLD; px[13, 2] = GOLD
    img.save(os.path.join(OUT, 'fx_mail.png'))


if __name__ == '__main__':
    chest(); toolrack(); bench(); board(); mail()
    # Final art: the placeholder generator must never draw over these (it did, once: the rack went back to the plain picture).
    final = os.path.join(OUT, 'final_art.txt')
    have = set(open(final).read().split())
    with open(final, 'a') as f:
        for n in ('obj_toolrack', 'item_machine_toolrack'):
            if n not in have: f.write(n + '
')
    print('wrote obj_chest, obj_toolrack, prop_bench, obj_board, fx_mail in', OUT)
