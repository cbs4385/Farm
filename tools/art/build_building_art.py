"""Draws the village building exteriors so each building looks like what it is (playtest request, 2026-10-05: "each building looks the same").

For each building: bld_<id>_wall.png and bld_<id>_roof.png (16x16 tiles), bld_<id>_window.png, bld_<id>_sign.png (a board with a picture of
what the shop does, over the door) and bld_<id>_roof_top.png (an ornament on the roof: chimney, clock, banner...). Project-made pixel art,
written to Assets/_Project/Art/Placeholders. Run: python tools/art/build_building_art.py
"""
import os
from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(__file__), '..', '..', 'Assets', '_Project', 'Art', 'Placeholders')

def hexc(h, a=255):
    h = h.lstrip('#')
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)

def shade(c, f):
    return (max(0, min(255, int(c[0] * f))), max(0, min(255, int(c[1] * f))), max(0, min(255, int(c[2] * f))), c[3])

def canvas(fill=None):
    im = Image.new('RGBA', (16, 16), fill or (0, 0, 0, 0))
    return im, ImageDraw.Draw(im)

# ---- walls -------------------------------------------------------------------------------------------------------------------

def wall_vertical_planks(base, seam):
    im, d = canvas(hexc(base))
    for x in range(0, 16, 4):
        d.line([(x, 0), (x, 15)], fill=hexc(seam))
    for x, y in ((2, 3), (6, 9), (10, 5), (14, 12)):
        d.point((x, y), fill=shade(hexc(base), 0.85))
    d.line([(0, 15), (15, 15)], fill=shade(hexc(base), 0.7))
    return im

def wall_horizontal_planks(base, seam):
    im, d = canvas(hexc(base))
    for y in range(3, 16, 4):
        d.line([(0, y), (15, y)], fill=hexc(seam))
    for x, y in ((3, 1), (9, 5), (13, 9), (5, 13)):
        d.point((x, y), fill=shade(hexc(base), 1.12))
    return im

def wall_blocks(base, mortar, rows=4, offset=True):
    im, d = canvas(hexc(base))
    h = 16 // rows
    for r in range(rows):
        y = r * h
        d.line([(0, y), (15, y)], fill=hexc(mortar))
        shift = (4 if r % 2 and offset else 0)
        for x in range(shift, 16 + 8, 8):
            if 0 <= x < 16:
                d.line([(x, y), (x, y + h - 1)], fill=hexc(mortar))
        d.point((2 + (r * 5) % 11, y + 1), fill=shade(hexc(base), 0.88))
    return im

def wall_plaster(base, trim):
    im, d = canvas(hexc(base))
    for x, y in ((2, 2), (7, 5), (12, 3), (4, 9), (10, 11), (14, 8)):
        d.point((x, y), fill=shade(hexc(base), 0.93))
    d.rectangle([0, 14, 15, 15], fill=hexc(trim))
    return im

# ---- roofs -------------------------------------------------------------------------------------------------------------------

def roof(base, trim=None, round_tiles=False):
    c = hexc(base)
    im, d = canvas(c)
    for r in range(4):
        y = r * 4
        d.line([(0, y + 3), (15, y + 3)], fill=shade(c, 0.7))
        shift = 4 if r % 2 else 0
        for x in range(shift, 16, 8):
            d.line([(x, y), (x, y + 2)], fill=shade(c, 0.78))
        d.line([(0, y), (15, y)], fill=shade(c, 1.12))
    if trim:
        d.line([(0, 15), (15, 15)], fill=hexc(trim))
        d.line([(0, 0), (15, 0)], fill=hexc(trim))
    return im

# ---- windows -----------------------------------------------------------------------------------------------------------------

def window(frame, glass, arch=False, shutters=None, glow=False):
    im, d = canvas()
    f, g = hexc(frame), hexc(glass)
    top = 3 if not arch else 2
    d.rectangle([4, top, 11, 12], fill=f)
    d.rectangle([5, top + 1, 10, 11], fill=g)
    if arch:
        d.point((4, top), fill=(0, 0, 0, 0)); d.point((11, top), fill=(0, 0, 0, 0))
    d.line([(7, top + 1), (7, 11)], fill=f)
    d.line([(8, top + 1), (8, 11)], fill=f)
    d.line([(5, 7), (10, 7)], fill=f)
    d.point((5, top + 1), fill=shade(g, 1.35))
    if glow:
        d.rectangle([5, 8, 10, 11], fill=hexc('#f0a030'))
        d.line([(7, 8), (7, 11)], fill=f); d.line([(8, 8), (8, 11)], fill=f)
    d.line([(3, 13), (12, 13)], fill=shade(f, 0.75))
    if shutters:
        s = hexc(shutters)
        d.rectangle([1, 3, 3, 12], fill=s); d.rectangle([12, 3, 14, 12], fill=s)
        d.line([(2, 4), (2, 11)], fill=shade(s, 0.7)); d.line([(13, 4), (13, 11)], fill=shade(s, 0.7))
    return im

# ---- signs: a board with a picture ------------------------------------------------------------------------------------------

def board(bg='#7a5230', border='#3a2414'):
    im, d = canvas()
    d.rectangle([1, 3, 14, 12], fill=hexc(border))
    d.rectangle([2, 4, 13, 11], fill=hexc(bg))
    d.point((3, 2), fill=hexc(border)); d.point((12, 2), fill=hexc(border))
    return im, d

def sign_sack():
    im, d = board('#d9c9a0')
    d.ellipse([5, 5, 10, 11], fill=hexc('#b89a62'))
    d.rectangle([6, 4, 9, 5], fill=hexc('#b89a62'))
    d.line([(6, 5), (9, 5)], fill=hexc('#5a3a1a'))
    d.point((7, 8), fill=hexc('#8a6a3a'))
    return im

def sign_anvil():
    im, d = board('#4a4a56', '#20202a')
    a = hexc('#c9ccd6')
    d.rectangle([4, 5, 11, 6], fill=a)
    d.point((3, 5), fill=a); d.point((12, 6), fill=a)
    d.rectangle([6, 7, 9, 8], fill=shade(a, 0.8))
    d.rectangle([5, 9, 10, 10], fill=shade(a, 0.65))
    d.point((10, 3), fill=hexc('#ff9a30')); d.point((11, 4), fill=hexc('#ffd060'))
    return im

def sign_saw():
    im, d = board('#d2b27a')
    blade = hexc('#c9ccd6')
    d.rectangle([4, 6, 12, 8], fill=blade)
    for x in range(4, 13, 2):
        d.point((x, 9), fill=blade)
    d.rectangle([3, 5, 4, 9], fill=hexc('#6a3a1a'))
    return im

def sign_book():
    im, d = board('#2f6f7a', '#16363c')
    d.rectangle([3, 5, 7, 10], fill=hexc('#f0e6c8'))
    d.rectangle([8, 5, 12, 10], fill=hexc('#e6dab8'))
    d.line([(7, 5), (7, 10)], fill=hexc('#7a5a3a'))
    for y in (6, 8):
        d.line([(4, y), (6, y)], fill=hexc('#a89a78')); d.line([(9, y), (11, y)], fill=hexc('#a89a78'))
    return im

def sign_mug():
    im, d = board('#4a2a1a', '#241208')
    d.rectangle([4, 5, 9, 10], fill=hexc('#e0a83a'))
    d.rectangle([4, 5, 9, 6], fill=hexc('#fff3c8'))
    d.rectangle([10, 6, 12, 9], outline=hexc('#e0a83a'))
    d.point((11, 7), fill=(0, 0, 0, 0)); d.point((11, 8), fill=(0, 0, 0, 0))
    return im

def sign_cross():
    im, d = board('#f4f0e8', '#8a8a92')
    d.rectangle([7, 4, 8, 11], fill=hexc('#c0453f'))
    d.rectangle([4, 7, 11, 8], fill=hexc('#c0453f'))
    return im

def sign_nameplate():
    # A small plaque by a villager's front door.
    im, d = canvas()
    d.rectangle([4, 5, 11, 10], fill=hexc('#3a2414'))
    d.rectangle([5, 6, 10, 9], fill=hexc('#d9c9a0'))
    d.line([(6, 7), (9, 7)], fill=hexc('#7a5230'))
    d.line([(6, 8), (8, 8)], fill=hexc('#7a5230'))
    return im

def sign_flag():
    im, d = board('#d9c48a', '#6a4a22')
    d.line([(5, 4), (5, 11)], fill=hexc('#5a3a1a'))
    d.polygon([(6, 4), (12, 6), (6, 8)], fill=hexc('#6a4a8a'))
    d.point((9, 6), fill=hexc('#e0c050'))
    return im

# ---- roof ornaments ---------------------------------------------------------------------------------------------------------

def top_chimney(smoke=False):
    im, d = canvas()
    d.rectangle([5, 2, 10, 15], fill=hexc('#7a4a3a'))
    d.rectangle([4, 1, 11, 3], fill=hexc('#5a3228'))
    for y in (6, 10, 14):
        d.line([(5, y), (10, y)], fill=hexc('#5a3228'))
    if smoke:
        for (x, y, r) in ((6, 0, 1),):
            d.ellipse([x - 1, y - 1, x + 2, y + 1], fill=hexc('#c8c8d0', 180))
    return im

def top_clock():
    im, d = canvas()
    d.ellipse([3, 3, 12, 12], fill=hexc('#e8dcc0'), outline=hexc('#3a2a1a'))
    d.line([(7, 7), (7, 4)], fill=hexc('#3a2a1a')); d.line([(7, 7), (10, 8)], fill=hexc('#3a2a1a'))
    return im

def top_banner(color, frame=0):
    im, d = canvas()
    d.line([(7, 0), (7, 15)], fill=hexc('#5a3a1a'))
    w = 13 if frame == 0 else 12
    d.rectangle([8, 1, w, 6 if frame == 0 else 5], fill=hexc(color))
    d.polygon([(8, 7 if frame == 0 else 6), (w, 7 if frame == 0 else 6), (w, 9 if frame == 0 else 8), (11, 8 if frame == 0 else 7)], fill=hexc(color))
    d.point((10, 3), fill=hexc('#e0c050'))
    return im

def top_vane(frame=0):
    im, d = canvas()
    d.line([(8, 6), (8, 15)], fill=hexc('#3a3a44'))
    d.line([(4, 5), (12, 5)], fill=hexc('#3a3a44'))
    if frame == 0:
        d.polygon([(12, 5), (10, 3), (10, 7)], fill=hexc('#3a3a44'))      # the arrow swings round in the wind
        d.polygon([(4, 5), (6, 3), (6, 7)], fill=hexc('#3a3a44'))
    else:
        d.polygon([(4, 5), (6, 3), (6, 7)], fill=hexc('#3a3a44'))
        d.polygon([(12, 5), (11, 4), (11, 6)], fill=hexc('#3a3a44'))
    d.point((8, 4), fill=hexc('#e0a83a'))
    return im

def top_pennant(color, frame=0):
    im, d = canvas()
    d.line([(6, 2), (6, 15)], fill=hexc('#5a3a1a'))
    if frame == 0:
        d.polygon([(7, 2), (13, 4), (7, 6)], fill=hexc(color))
    else:
        d.polygon([(7, 2), (12, 3), (11, 4), (12, 5), (7, 6)], fill=hexc(color))
    return im

# ---- door tags: green when the business is open, red when it is closed ---------------------------------------------------------

def tag(fill, mark):
    im, d = canvas()
    d.line([(8, 2), (8, 4)], fill=hexc('#3a2414'))
    d.rectangle([3, 4, 12, 12], fill=hexc('#3a2414'))
    d.rectangle([4, 5, 11, 11], fill=hexc(fill))
    mark(d)
    return im

def mark_open(d):
    for p in ((5, 8), (6, 9), (7, 10), (8, 9), (9, 8), (10, 7)):
        d.point(p, fill=hexc('#ffffff'))

def mark_closed(d):
    d.rectangle([5, 7, 10, 9], fill=hexc('#ffffff'))

# ---- the buildings -----------------------------------------------------------------------------------------------------------

BUILDINGS = {
    'general':   dict(wall=lambda: wall_vertical_planks('#cfa46a', '#a8814c'), roof=lambda: roof('#3f7a4a', '#2c5a35'),
                      window=lambda: window('#f0e6c8', '#9ad0e0', shutters='#3f7a4a'), sign=sign_sack, top=lambda: top_pennant('#3f7a4a'), top2=lambda: top_pennant('#3f7a4a', 1)),
    'blacksmith': dict(wall=lambda: wall_blocks('#6e6e78', '#43434c'), roof=lambda: roof('#3a3f4d', '#23262f'),
                       window=lambda: window('#2a2a30', '#5a4a40', glow=True), sign=sign_anvil, top=lambda: top_chimney(True)),
    'carpenter': dict(wall=lambda: wall_horizontal_planks('#dcbd80', '#b8955a'), roof=lambda: roof('#b5532f', '#7a3418'),
                      window=lambda: window('#8a5a2e', '#a8d8e8'), sign=sign_saw, top=top_vane, top2=lambda: top_vane(1)),
    'library':   dict(wall=lambda: wall_blocks('#a8473a', '#d8c8b8', rows=4), roof=lambda: roof('#2f6f7a', '#1c4850'),
                      window=lambda: window('#e8dcc0', '#a8d0e8', arch=True), sign=sign_book, top=top_clock),
    'saloon':    dict(wall=lambda: wall_vertical_planks('#6e3a2a', '#4a2418'), roof=lambda: roof('#5a3a24', '#3a2414'),
                      window=lambda: window('#3a2414', '#f0c860'), sign=sign_mug, top=lambda: top_chimney(False)),
    'clinic':    dict(wall=lambda: wall_plaster('#ece8e0', '#9ab4cc'), roof=lambda: roof('#5a86b0', '#f4f0e8'),
                      window=lambda: window('#ffffff', '#b8dcf0'), sign=sign_cross, top=lambda: top_pennant('#c0453f'), top2=lambda: top_pennant('#c0453f', 1)),
    'hall':      dict(wall=lambda: wall_blocks('#cdbd94', '#9a8a68', rows=4, offset=False), roof=lambda: roof('#6a4a8a', '#e0c050'),
                      window=lambda: window('#9a8a68', '#f0d890', arch=True), sign=sign_flag, top=lambda: top_banner('#6a4a8a'), top2=lambda: top_banner('#6a4a8a', 1)),
}

# The villagers' cottages (playtest 2026-10-07: they should live in homes): four colourings of the same small house.
COTTAGES = {
    'cottage1': dict(wall=lambda: wall_plaster('#efe4cc', '#b8864f'), roof=lambda: roof('#b5532f', '#7a3418'),
                     window=lambda: window('#8a5a2e', '#a8d8e8', shutters='#3f7a4a'), sign=sign_nameplate, top=lambda: top_chimney(True)),
    'cottage2': dict(wall=lambda: wall_horizontal_planks('#d9b98a', '#b08a58'), roof=lambda: roof('#4a6a8a', '#2c4258'),
                     window=lambda: window('#f0e6c8', '#9ad0e0', shutters='#b5532f'), sign=sign_nameplate, top=lambda: top_chimney(True)),
    'cottage3': dict(wall=lambda: wall_blocks('#b8a888', '#8a7a5c'), roof=lambda: roof('#3f7a4a', '#2c5a35'),
                     window=lambda: window('#e8dcc0', '#a8d0e8', shutters='#6a4a8a'), sign=sign_nameplate, top=lambda: top_chimney(True)),
    'cottage4': dict(wall=lambda: wall_vertical_planks('#9a6a4a', '#74482e'), roof=lambda: roof('#8a6a3a', '#5a4220'),
                     window=lambda: window('#3a2414', '#f0c860', shutters='#c8a040'), sign=sign_nameplate, top=lambda: top_chimney(True)),
}
BUILDINGS.update(COTTAGES)

def main():
    os.makedirs(OUT, exist_ok=True)
    count = 0
    for key, b in BUILDINGS.items():
        for part in ('wall', 'roof', 'window', 'sign', 'top'):
            name = f'bld_{key}_{"roof_top" if part == "top" else part}.png'
            b[part]().save(os.path.join(OUT, name))
            count += 1
        if 'top2' in b:
            b['top2']().save(os.path.join(OUT, f'bld_{key}_roof_top2.png'))
            count += 1
    tag('#3f9a4f', mark_open).save(os.path.join(OUT, 'bld_tag_open.png'))
    tag('#c0453f', mark_closed).save(os.path.join(OUT, 'bld_tag_closed.png'))
    count += 2
    print(f'wrote {count} sprites to {os.path.abspath(OUT)}')

    # A contact sheet for a person to look at: each building as it will be placed (roof row, wall rows, windows, sign).
    sheet = Image.new('RGBA', (8 + len(BUILDINGS) * 80, 16 * 5 + 16), (110, 170, 90, 255))
    for i, (key, b) in enumerate(BUILDINGS.items()):
        ox, oy = 8 + i * 80, 8
        roof_t, wall_t = b['roof'](), b['wall']()
        for x in range(4):
            sheet.paste(roof_t, (ox + x * 16, oy))
        sheet.alpha_composite(b['top'](), (ox + 2 * 16, oy))
        for y in range(1, 5):
            for x in range(4):
                sheet.paste(wall_t, (ox + x * 16, oy + y * 16))
        sheet.alpha_composite(b['window'](), (ox, oy + 2 * 16))
        sheet.alpha_composite(b['window'](), (ox + 3 * 16, oy + 2 * 16))
        sheet.alpha_composite(b['sign'](), (ox + 16, oy + 3 * 16))
    path = os.path.join(os.path.dirname(__file__), '..', '..', 'Builds', 'building_sheet.png')
    os.makedirs(os.path.dirname(path), exist_ok=True)
    sheet.resize((sheet.width * 3, sheet.height * 3), Image.NEAREST).save(path)

if __name__ == '__main__':
    main()
