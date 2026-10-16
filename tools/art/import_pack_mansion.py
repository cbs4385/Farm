"""Backpack icons for the home furniture, taken from the purchased "Fancy Mansion Furniture" pack (0-mem0ry, AssetPacks/FancyMansion_Furniture/). Replaces the pictures of
item_machine_{armchair,bench,chair,clock,dining_table,lamp,painting,rug,vase} by name (so the .meta files and the item data stay as they are), 16 x 16. The bookshelf and wardrobe (68 and 71 pixels tall) lose too much at 16 pixels and keep their older icons.
The pieces are 16 to 78 pixels tall on a 16-pixel grid, so each is fitted to 16 pixels by "mode pooling": every output pixel takes the most common colour of the
source box it covers (no blending, so the palette and the dark outline of the pack survive), and is transparent when most of the box is.
Run from the repository root: py tools/art/import_pack_mansion.py   (add --preview <folder> to also write an enlarged contact sheet there)
Licence: see docs/ASSET_LICENSES.md (the pack's own Info.txt gives no terms; the owner keeps the itch.io page)."""
import os
import sys
from collections import Counter
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
SHEET = os.path.join(ROOT, "AssetPacks", "FancyMansion_Furniture", "FancyMansion_Furniture", "fancy_mansion_furnitureset.png")
OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")

# name -> (x, y, w, h) on the sheet
PIECES = {
    "item_machine_armchair": (97, 22, 31, 42),
    "item_machine_bench": (384, 39, 32, 22),
    "item_machine_chair": (128, 124, 16, 36),
    "item_machine_clock": (6, 327, 20, 56),
    "item_machine_dining_table": (0, 120, 48, 40),
    "item_machine_lamp": (70, 72, 20, 24),
    "item_machine_painting": (226, 66, 30, 28),
    "item_machine_rug": (288, 391, 64, 80),
    "item_machine_vase": (321, 226, 14, 21),
}


def fit(piece, size=16):
    w, h = piece.size
    scale = max(w, h) / size
    if scale <= 1:
        out = Image.new("RGBA", (size, size), (0, 0, 0, 0))
        out.paste(piece, ((size - w) // 2, size - h), piece)
        return out
    ow, oh = max(1, round(w / scale)), max(1, round(h / scale))
    px = piece.load()
    out = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    op = out.load()
    for oy in range(oh):
        for ox in range(ow):
            x0, x1 = int(ox * scale), max(int(ox * scale) + 1, int((ox + 1) * scale))
            y0, y1 = int(oy * scale), max(int(oy * scale) + 1, int((oy + 1) * scale))
            colours, total = Counter(), 0
            for y in range(y0, min(y1, h)):
                for x in range(x0, min(x1, w)):
                    total += 1
                    c = px[x, y]
                    if c[3] > 0:
                        colours[c] += 1
            if total and sum(colours.values()) * 2 > total:
                # the pack outlines everything in near-black: keep an outline pixel when a third of an edge box is one, or the pieces lose their edge
                dark = [(c, n) for c, n in colours.items() if sum(c[:3]) < 150]
                best = max(colours.items(), key=lambda kv: kv[1])[0]
                edge = sum(colours.values()) < total
                if edge and dark and sum(n for _, n in dark) * 3 >= total: best = max(dark, key=lambda kv: kv[1])[0]
                op[ox, oy] = best
    # centre horizontally, rest on the floor of the 16 x 16 cell
    box = out.getbbox()
    shifted = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    shifted.paste(out.crop(box), ((size - (box[2] - box[0])) // 2, size - (box[3] - box[1])))
    return shifted


def main():
    sheet = Image.open(SHEET).convert("RGBA")
    made = []
    for name, (x, y, w, h) in PIECES.items():
        icon = fit(sheet.crop((x, y, x + w, y + h)))
        icon.save(os.path.join(OUT, name + ".png"))
        made.append(icon)
        print("wrote", name)
    if "--preview" in sys.argv:
        folder = sys.argv[sys.argv.index("--preview") + 1]
        os.makedirs(folder, exist_ok=True)
        k = 8
        sheet_out = Image.new("RGBA", (len(made) * (16 * k + 8), 16 * k + 8), (110, 140, 100, 255))
        for i, icon in enumerate(made):
            sheet_out.alpha_composite(icon.resize((16 * k, 16 * k), Image.NEAREST), (i * (16 * k + 8) + 4, 4))
        sheet_out.save(os.path.join(folder, "mansion_icons.png"))


if __name__ == "__main__":
    main()
