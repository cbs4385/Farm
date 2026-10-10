"""The Keepers' robes (ritual scenes, 2026-10-10): one hooded figure per Keeper, 16 x 32, four facings (right is the mirror of left), drawn here from small templates.
Original art. Written to Assets/_Project/Resources/Outfits/npc_robe_<keeper>_idle_<facing>.png (loaded by NpcOutfits; the names start with npc_ and end with a facing, so the
character rig walks and swings them like any villager).

Cell codes: h robe light, R robe, r robe shade, s robe deep shade, d hood hollow, e eye glint, p pale hands, t trim, T trim shade, k cord, K cord knot, g satchel/strap, o lantern glow.
"""
import os
import sys

from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = os.path.join(ROOT, 'Assets', '_Project', 'Resources', 'Outfits')

W, H = 16, 32

# a deep forest green for every Keeper (the green hood of the joined ending); the leader's is nearly black with violet trim
ROBES = {
    'odalys': {'h': '#4d6a5c', 'R': '#34503f', 'r': '#243a2d', 's': '#182a21', 'trim': '#9a74c8', 'trimShade': '#6e4f9a', 'cord': '#cfc6a8'},
    'tilda': {'h': '#5e7a58', 'R': '#44603f', 'r': '#314a2e', 's': '#223420', 'trim': '#d9a441', 'trimShade': '#a87a2a', 'cord': '#e8dcb0'},
    'marcus': {'h': '#556a50', 'R': '#3c5238', 'r': '#2a3d28', 's': '#1c2a1b', 'trim': '#9b6b3c', 'trimShade': '#6e4a28', 'cord': '#b89a6a'},
    'dorian': {'h': '#4f6a58', 'R': '#37513f', 'r': '#263b2c', 's': '#192a1f', 'trim': '#7ea05a', 'trimShade': '#587a3e', 'cord': '#a89870'},
    'wren': {'h': '#58705c', 'R': '#3f5742', 'r': '#2c402f', 's': '#1d2d20', 'trim': '#b0504a', 'trimShade': '#823632', 'cord': '#d8c8a0'},
}
COMMON = {'d': '#0f1316', 'e': '#e9e4c8', 'p': '#d9c0a0', 'g': '#6b4a2b', 'o': '#ffd877'}


def mirror(half_rows, darken=True):
    """Left half rows (8 cells) to full rows: the right half is the left one mirrored, one tone darker (light comes from the top left)."""
    shade = {'h': 'R', 'R': 'r', 'r': 'r', 's': 's', 't': 'T', 'T': 'T'}
    rows = []
    for row in half_rows:
        right = ''.join((shade.get(c, c) if darken else c) for c in reversed(row))
        rows.append(row + right)
    return rows


def front(variant):
    half = [
        '........', '........',
        '......hh',     # 2 hood tip
        '....hhRR',
        '...hRRRR',
        '...hRRRR',
        '..hRRRdd',     # 6 the hollow
        '..hRRddd',
        '..hRRded',     # 8 eyes
        '..hRRddd',
        '...hRRdd',
        '...hRRRR',
        '..hRRRRR',     # 12 shoulders
        '.hhRRRRR',
        '.hRRRRRR',
        '.hRRrRRR',     # 15 the sleeve's edge
        '.hRRrRRR',
        '.hRRrRRR',
        '.hRRrRRR',
        '.ttkkkkk',     # 19 the cord
        '.hRRrpKk',     # 20 the hands, the knot
        '.hRRrppk',
        'hRRRrrRk',
        'hRRRRRRk',
        'hRRRRRRR',
        'hRRRRRRR',
        'hRRRRRRR',
        'hRRRRRRR',
        'hRRRRRRR',
        'hRRRRRRR',
        'hTTTTTTT',     # 30 the trim at the hem
        'rrrrrrrr',
    ]
    rows = mirror(half)
    rows = [list(r) for r in rows]
    apply_variant(rows, variant, 'down')
    return [''.join(r) for r in rows]


def back(variant):
    half = [
        '........', '........',
        '......hh',
        '....hhRR',
        '...hRRRR',
        '...hRRRR',
        '..hRRRRR',
        '..hRRRRR',
        '..hRRRRR',
        '..hRRRRR',
        '...hRRRR',
        '...hRRRR',
        '..hRRRRR',
        '.hhRRRRR',
        '.hRRRRRR',
        '.hRRRRRR',
        '.hRRRRRR',
        '.hRRRRRR',
        '.hRRRRRR',
        '.ttkkkkk',
        '.hRRRRRR',
        '.hRRRRRR',
        'hRRRRRRR',
        'hRRRRRRR',
        'hRRRRRRR',
        'hRRRRRRR',
        'hRRRRRRR',
        'hRRRRRRR',
        'hRRRRRRR',
        'hRRRRRRR',
        'hTTTTTTT',
        'rrrrrrrr',
    ]
    rows = [list(r) for r in mirror(half)]
    for y in range(3, 32):               # the seam down the back
        rows[y][7] = 'r'; rows[y][8] = 'r'
    # the sigil on the back: a ring with a line through it
    for (x, y) in [(6, 14), (7, 13), (8, 13), (9, 14), (6, 15), (9, 15), (6, 16), (9, 16), (7, 17), (8, 17)]:
        rows[y][x] = 't'
    for y in range(12, 19):
        rows[y][7] = 't'; rows[y][8] = 't' if y in (12, 18) else rows[y][8]
    apply_variant(rows, variant, 'up')
    return [''.join(r) for r in rows]


def left(variant):
    rows = [
        '................',
        '................',
        '.....hhhh.......',
        '...hhhRRRR......',
        '..hhRRRRRRr.....',
        '..hRRRRRRRr.....',
        '.hRRddRRRRr.....',
        '.hRddddRRRRr....',
        '.hRdedRRRRRr....',
        '.hRdddRRRRRr....',
        '..hRddRRRRRr....',
        '..hRRRRRRRRr....',
        '...hRRRRRRRrr...',
        '...hRRRRRRRrr...',
        '...hRRRrRRRRr...',
        '...hRRRrRRRRr...',
        '...hRRRrRRRRr...',
        '...hRRRrRRRRrr..',
        '...hRRRrRRRRrr..',
        '...ttkkkkkkkTT..',
        '...hRppRRRRRrr..',
        '...hRppRRRRRrr..',
        '..hRRRRRRRRRRrr.',
        '..hRRRRRRRRRRrr.',
        '..hRRRRRRRRRRrr.',
        '..hRRRRRRRRRRrr.',
        '..hRRRRRRRRRRrr.',
        '.hRRRRRRRRRRRrr.',
        '.hRRRRRRRRRRRrr.',
        '.hRRRRRRRRRRRrr.',
        '.hTTTTTTTTTTTTr.',
        '.rrrrrrrrrrrrrr.',
    ]
    rows = [list(r) for r in rows]
    apply_variant(rows, variant, 'left')
    return [''.join(r) for r in rows]


def apply_variant(rows, variant, facing):
    """What makes each Keeper somebody: a taller hood, a broader frame, a satchel strap, a lantern."""
    def put(x, y, c):
        if 0 <= x < W and 0 <= y < H: rows[y][x] = c

    def widen(y0, y1):
        for y in range(y0, y1 + 1):
            xs = [x for x in range(W) if rows[y][x] != '.']
            if not xs: continue
            lo, hi = min(xs), max(xs)
            if lo > 0: rows[y][lo - 1] = 'h' if rows[y][lo] in 'hR' else 'r'
            if hi < W - 1: rows[y][hi + 1] = 'r'

    if variant == 'odalys':
        # a pale stole down the front, and a hood a row taller
        if facing == 'down':
            for y in range(12, 31):
                put(7, y, 'p' if y not in (19, 20, 21) else rows[y][7]); put(8, y, 'p' if y not in (19, 20, 21) else rows[y][8])
        # the hood's tip
        put(7, 1, 'h'); put(8, 1, 'R')
    elif variant == 'tilda':
        widen(14, 29)                                    # rounder
        if facing in ('down', 'left'):
            put(4 if facing == 'down' else 5, 21, 'o')   # a small brass key at the cord (it shines)
    elif variant == 'marcus':
        widen(12, 18)                                    # broad shoulders
        widen(12, 18)
        if facing == 'down':
            for y in (21, 22, 23): put(3, y, 'g')         # a hammer shaft at the belt
            put(2, 20, 's'); put(3, 20, 's'); put(4, 20, 's')
    elif variant == 'dorian':
        # a satchel strap across the chest and a pouch at the hip
        if facing in ('down', 'up'):
            for i in range(0, 8):
                put(4 + i, 13 + i, 'g')
            for (x, y) in [(10, 22), (11, 22), (10, 23), (11, 23), (10, 24), (11, 24)]: put(x, y, 'g')
        else:
            for i in range(0, 6): put(5 + i, 13 + i, 'g')
    elif variant == 'wren':
        # a lantern held low, glowing
        if facing in ('down', 'left'):
            lx = 12 if facing == 'down' else 3
            for (dx, dy) in [(0, 0), (1, 0), (0, 1), (1, 1), (0, 2), (1, 2)]: put(lx + dx, 21 + dy, 'o')
            put(lx, 20, 'k'); put(lx + 1, 20, 'k')


def render(rows, palette):
    img = Image.new('RGBA', (W, H), (0, 0, 0, 0))
    px = img.load()
    for y, row in enumerate(rows):
        assert len(row) == W, (y, len(row), row)
        for x, c in enumerate(row):
            if c == '.': continue
            color = palette.get(c)
            if color is None: raise SystemExit(f'unknown code {c!r} at {x},{y}')
            px[x, y] = tuple(int(color[i:i + 2], 16) for i in (1, 3, 5)) + (255,)
    return img


def palette_for(keeper):
    r = ROBES[keeper]
    p = dict(COMMON)
    p.update({'h': r['h'], 'R': r['R'], 'r': r['r'], 's': r['s'], 't': r['trim'], 'T': r['trimShade'], 'k': r['cord'], 'K': r['trim']})
    return p


def main():
    os.makedirs(OUT, exist_ok=True)
    for keeper in ROBES:
        pal = palette_for(keeper)
        sets = {'down': front(keeper), 'up': back(keeper), 'left': left(keeper)}
        sets['right'] = [r[::-1] for r in sets['left']]
        for facing, rows in sets.items():
            img = render(rows, pal)
            img.save(os.path.join(OUT, f'npc_robe_{keeper}_idle_{facing}.png'))
    print('wrote', len(ROBES) * 4, 'robes to', OUT)
    # a contact sheet for a look
    sheet = Image.new('RGBA', (W * 4 * 6 * len(ROBES) // len(ROBES) * 1, 1), (0, 0, 0, 0))
    scale = 6
    sheet = Image.new('RGBA', (W * scale * 4, H * scale * len(ROBES)), (94, 140, 80, 255))
    for i, keeper in enumerate(ROBES):
        for j, facing in enumerate(('down', 'up', 'left', 'right')):
            im = Image.open(os.path.join(OUT, f'npc_robe_{keeper}_idle_{facing}.png')).resize((W * scale, H * scale), Image.NEAREST)
            sheet.alpha_composite(im, (j * W * scale, i * H * scale))
    sheet.save(os.path.join(ROOT, 'Builds', 'robes_sheet.png'))
    return 0


if __name__ == '__main__':
    sys.exit(main())
