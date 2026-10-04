"""T-130: slices the expression sheets (Art/Generated/expr_<npc>.png, 3 x 3, see generate_expressions.py) into 32 x 32 portraits named
ui_portrait_<npc>_<expression>.png in Art/Placeholders, quantised to one 48-colour palette per villager (like the base portraits),
and lists them in final_art.txt so placeholder regeneration never overwrites them. The neutral cell is not used: the base portrait
is the neutral face (NpcDefinition.PortraitFor falls back to it).
Usage: python tools/art/slice_expressions.py wren hazel bram [--preview] [--base] [--inset PIXELS (trim a drawn frame)]
(--base: also replace the base portrait with the sheet's neutral cell)"""
import os, sys
import numpy as np
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
GEN = os.path.join(ROOT, "Assets", "_Project", "Art", "Generated")
PH = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")
CELLS = ["neutral", "happy", "sad", "surprised", "embarrassed", "thinking", "angry", "laughing", "sleepy"]
USED = ["happy", "sad", "surprised", "embarrassed", "thinking"]


INSET = 4


def trim_frame(cell):
    """Removes a near-white frame or margin around a cell, then crops the largest centred square."""
    a = np.asarray(cell.convert("RGB")).astype(np.int32)
    white = (a.min(axis=2) > 235)
    rows = np.where(white.mean(axis=1) < 0.5)[0]
    cols = np.where(white.mean(axis=0) < 0.5)[0]
    if len(rows) and len(cols):
        cell = cell.crop((cols[0], rows[0], cols[-1] + 1, rows[-1] + 1))
    inset = INSET
    w, h = cell.size
    side = min(w, h) - 2 * inset
    x0 = (w - side) // 2
    y0 = (h - side) // 2
    return cell.crop((x0, y0, x0 + side, y0 + side))


def quantize(arrs, colors=48):
    px = np.concatenate([a.reshape(-1, 3) for a in arrs])
    sample = Image.fromarray(px[np.random.default_rng(1).permutation(len(px))[:20000]].reshape(1, -1, 3).astype(np.uint8))
    pal = sample.quantize(colors=colors, method=Image.MEDIANCUT, dither=Image.NONE)
    out = []
    for a in arrs:
        out.append(np.asarray(Image.fromarray(a).quantize(palette=pal, dither=Image.NONE).convert("RGB")))
    return out


def slice_npc(npc, preview, base=False):
    path = os.path.join(GEN, f"expr_{npc}.png")
    if not os.path.exists(path):
        print(f"{npc}: no sheet"); return []
    img = Image.open(path).convert("RGB")
    W, H = img.size
    cw, ch = W / 3, H / 3
    sprites = []
    names_to_cut = (["neutral"] if base else []) + USED
    for name in names_to_cut:
        i = CELLS.index(name)
        r, c = divmod(i, 3)
        cell = img.crop((int(c * cw), int(r * ch), int((c + 1) * cw), int((r + 1) * ch)))
        sprites.append(np.asarray(trim_frame(cell).resize((32, 32), Image.BOX).convert("RGB")))
    written = []
    for name, arr in zip(names_to_cut, quantize(sprites)):
        out = os.path.join(PH, f"ui_portrait_{npc}.png" if name == "neutral" else f"ui_portrait_{npc}_{name}.png")
        Image.fromarray(arr).convert("RGBA").save(out)
        written.append(f"ui_portrait_{npc}" if name == "neutral" else f"ui_portrait_{npc}_{name}")
    if preview:
        sheet = Image.new("RGBA", (32 * 6 * 4, 32 * 4), (0, 0, 0, 0))
        base = Image.open(os.path.join(PH, f"ui_portrait_{npc}.png")).convert("RGBA")
        for k, n in enumerate(["base"] + USED):
            im = base if n == "base" else Image.open(os.path.join(PH, f"ui_portrait_{npc}_{n}.png")).convert("RGBA")
            sheet.paste(im.resize((128, 128), Image.NEAREST), (k * 128, 0))
        os.makedirs(os.path.join(ROOT, "Builds"), exist_ok=True)
        sheet.crop((0, 0, 128 * 6, 128)).save(os.path.join(ROOT, "Builds", f"expr_preview_{npc}.png"))
    print(f"{npc}: wrote {len(written)} portraits")
    return written


if __name__ == "__main__":
    if "--inset" in sys.argv: INSET = int(sys.argv[sys.argv.index("--inset") + 1])
    npcs = [a for a in sys.argv[1:] if a.isalpha() and not a.startswith("--")] or ["wren", "hazel", "bram"]
    names = []
    for n in npcs: names += slice_npc(n, "--preview" in sys.argv, "--base" in sys.argv)
    listing = os.path.join(PH, "final_art.txt")
    have = set(open(listing, encoding="utf-8").read().split()) if os.path.exists(listing) else set()
    new = [n for n in names if n not in have]
    if new:
        with open(listing, "a", encoding="utf-8", newline="") as f:
            f.write("\n".join(new) + "\n")
    print(f"final_art.txt: added {len(new)}")
