"""Slices generated sprite sheets (Assets/_Project/Art/Generated/<id>.png) into the named sprites listed in manifest.json.
Sprite sheets: magenta background is keyed out, each cell's content is cropped, scaled (nearest-friendly, area averaged then
palette-quantised) into the target size and written to Art/Placeholders (or --out). Tile sheets: each cell is resized whole.
Usage: python tools/art/slice_sheets.py [sheet ids...] [--out DIR] [--preview]"""
import json, os, sys
import numpy as np
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
GEN = os.path.join(ROOT, "Assets", "_Project", "Art", "Generated")
PH = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")
COLORS = 128
ASSIGN_PATH = os.path.join(os.path.dirname(__file__), 'assign.json')
DILATE = {'goods1': 2, 'crp_2': 2}
ASSIGN = json.load(open(ASSIGN_PATH, encoding='utf-8')) if os.path.exists(ASSIGN_PATH) else {}          # palette size per sheet

def key_background(img):
    a = np.asarray(img.convert("RGB")).astype(np.int32)
    corners = np.array([a[2, 2], a[2, -3], a[-3, 2], a[-3, -3]])
    bg = np.median(corners, axis=0)
    d = np.abs(a - bg).sum(axis=2)
    # also treat magenta-ish pixels as background (the model rarely hits exactly #FF00FF)
    mag = (a[:, :, 0] > 170) & (a[:, :, 2] > 170) & (a[:, :, 1] < 110)
    fg = (d > 90) & ~mag
    return a, fg

def clean_mask(fg):
    # drop isolated specks (anti-alias fringe, noise)
    from PIL import ImageFilter
    m = Image.fromarray((fg * 255).astype(np.uint8)).filter(ImageFilter.MedianFilter(5))
    return np.asarray(m) > 127

def bbox(mask):
    ys, xs = np.where(mask)
    if len(xs) == 0: return None
    return xs.min(), ys.min(), xs.max() + 1, ys.max() + 1

def resize_masked(rgb, mask, size):
    """Area-average a cropped sprite into `size` using only foreground pixels for colour; alpha from coverage."""
    w, h = size
    m = mask.astype(np.float32)
    chans = [Image.fromarray((rgb[:, :, c] * m).astype(np.float32), "F").resize((w, h), Image.BOX) for c in range(3)]
    cov = Image.fromarray(m, "F").resize((w, h), Image.BOX)
    cov = np.asarray(cov)
    out = np.zeros((h, w, 4), np.uint8)
    for c in range(3):
        ch = np.asarray(chans[c]) / np.maximum(cov, 1e-4)
        out[:, :, c] = np.clip(ch, 0, 255).astype(np.uint8)
    out[:, :, 3] = np.where(cov >= 0.5, 255, 0)
    return out

def quantize(arrs, colors=COLORS):
    """One shared adaptive palette for every sprite of the sheet (keeps the set consistent)."""
    px = np.concatenate([a[a[:, :, 3] > 0][:, :3] for a in arrs if (a[:, :, 3] > 0).any()])
    if len(px) == 0: return arrs
    sample = Image.fromarray(px[np.random.default_rng(1).permutation(len(px))[:20000]].reshape(1, -1, 3).astype(np.uint8))
    pal = sample.quantize(colors=colors, method=Image.MEDIANCUT, dither=Image.NONE)
    res = []
    for a in arrs:
        rgb = Image.fromarray(a[:, :, :3]).quantize(palette=pal, dither=Image.NONE).convert("RGB")
        o = np.asarray(rgb).copy()
        res.append(np.dstack([o, a[:, :, 3]]))
    return res


def segment(mask, rows_hint=None, dilate=7):
    """Connected-component sprite detection (a small dilation joins parts of one sprite). Returns rows of boxes."""
    from scipy import ndimage as ndi
    dil = ndi.binary_dilation(mask, iterations=dilate)
    lab, n = ndi.label(dil)
    boxes = []
    for i in range(1, n + 1):
        region = (lab == i) & mask
        if region.sum() < 350: continue
        ys, xs = np.where(region)
        boxes.append((xs.min(), ys.min(), xs.max() + 1, ys.max() + 1))
    if not boxes: return []
    boxes.sort(key=lambda b: (b[1] + b[3]) / 2)
    med = float(np.median([b[3] - b[1] for b in boxes]))
    rows, cur = [], [boxes[0]]
    for b in boxes[1:]:
        cy = (b[1] + b[3]) / 2
        if cy - np.mean([(c[1] + c[3]) / 2 for c in cur]) > med * 0.6:
            rows.append(cur); cur = [b]
        else:
            cur.append(b)
    rows.append(cur)
    return [sorted(r, key=lambda b: b[0]) for r in rows]

def index_preview(img, rowboxes, path):
    from PIL import ImageDraw
    im = img.copy(); d = ImageDraw.Draw(im); k = 0
    for r in rowboxes:
        for (x0, y0, x1, y1) in r:
            k += 1
            d.rectangle((x0, y0, x1, y1), outline=(255, 255, 0), width=2)
            d.rectangle((x0, y0, x0 + 34, y0 + 22), fill=(0, 0, 0))
            d.text((x0 + 3, y0 + 4), str(k), fill=(255, 255, 0))
    im.save(path)

def slice_sheet(sheet, out_dir, preview=False):
    path = os.path.join(GEN, sheet["id"] + ".png")
    if not os.path.exists(path): return None
    img = Image.open(path).convert("RGB")
    W, H = img.size
    cols, rows = sheet["cols"], sheet["rows"]
    cw, ch = W / cols, H / rows
    tw, th = sheet["target"]
    cells = sheet["cells"]
    result = {}

    if sheet["id"] == "tiles":
        _, fg_t = key_background(img)
        tboxes = [b for row in segment(fg_t, dilate=1) for b in row if b[2] - b[0] > 150 and b[3] - b[1] > 150]
        print("  tile frames found:", len(tboxes))
        if len(tboxes) != 16:
            return 0
        def cell(i, y0=0.0, y1=1.0):
            x0, ty0, x1, ty1 = tboxes[i]
            m = 14                                     # trim the drawn frame and its soft edge
            x0 += m; x1 -= m; ty0 += m; ty1 -= m
            h = ty1 - ty0
            return img.crop((x0, int(ty0 + h * y0), x1, int(ty0 + h * y1))).resize((tw, th), Image.BOX).convert("RGBA")
        picks = {"tile_grass": cell(0), "tile_dirt": cell(1), "tile_path": cell(2), "tile_sand": cell(3), "tile_cobble": cell(4),
                 "tile_forest": cell(5), "tile_tilled": cell(6), "tile_roof": cell(8, 0.02, 0.46), "tile_wall": cell(8, 0.56, 0.98),
                 "tile_floor_wood": cell(9), "tile_water": cell(10), "tile_door": cell(12), "tile_grass_summer": cell(13),
                 "tile_grass_fall": cell(14), "tile_grass_winter": cell(15)}
        wet = np.asarray(picks["tile_tilled"]).astype(np.float32).copy()
        wet[:, :, :3] = np.clip(wet[:, :, :3] * 0.62 + np.array([0, 4, 10]), 0, 255)
        picks["tile_tilled_watered"] = Image.fromarray(wet.astype(np.uint8), "RGBA")
        keys = list(picks)
        q = quantize([np.asarray(picks[k]) for k in keys])
        for k, a in zip(keys, q): result[k] = a
    elif sheet["id"] == "portraits":
        a_full, fg_full = key_background(img)
        rowboxes = segment(fg_full, dilate=1)
        flat = [b for row in rowboxes for b in row]
        print('  portrait frames found:', len(flat))
        arrs, names = [], []
        for name, (x0, y0, x1, y1) in zip(ASSIGN["portraits"], flat):
            if not name: continue
            inset = 5
            x0 += inset; y0 += inset; x1 -= inset; y1 -= inset
            side = min(x1 - x0, y1 - y0)
            t = img.crop((x0, y0, x0 + side, y0 + side)).resize((tw, th), Image.BOX).convert("RGBA")
            arrs.append(np.asarray(t)); names.append(name)
        for n, a in zip(names, quantize(arrs, 48)): result[n] = a
    elif sheet["kind"] == "tile":
        arrs = []
        for i, name in enumerate(cells):
            if not name: arrs.append(None); continue
            r, c = divmod(i, cols)
            inset = 0.03
            box = (int(c * cw + cw * inset), int(r * ch + ch * inset), int((c + 1) * cw - cw * inset), int((r + 1) * ch - ch * inset))
            t = img.crop(box).resize((tw, th), Image.BOX).convert("RGBA")
            arrs.append(np.asarray(t))
        keep = [a for a in arrs if a is not None]
        q = iter(quantize(keep))
        for name, a in zip(cells, arrs):
            if a is not None: result[name] = next(q)
    else:
        a_full, fg_full = key_background(img)
        fg_full = clean_mask(fg_full)
        rowboxes = segment(fg_full, rows, dilate=DILATE.get(sheet['id'], 3 if sheet['id'].startswith('crp_') else 7))
        os.makedirs(os.path.join(ROOT, "Builds"), exist_ok=True)
        index_preview(img, rowboxes, os.path.join(ROOT, "Builds", "idx_" + sheet["id"] + ".png"))
        flat = [(b, r) for r, row in enumerate(rowboxes) for b in row]
        crops = {}
        names_by_row = [[n for n in cells[r * cols:(r + 1) * cols] if n] for r in range(rows)]
        flat_expected = [n for row in names_by_row for n in row]
        assign = ASSIGN.get(sheet["id"])
        if isinstance(assign, dict):
            # {"crops": [...], "rows": [[1-based sprite numbers per crop]...]}: each growth row is resampled to the crop's stage names
            for crop, idxs in zip(assign["crops"], assign["rows"]):
                want = [n for n in cells if n and n.startswith(f"crop_{crop}_")]
                boxes = [flat[i - 1] for i in idxs]
                pick_i = [round(i * (len(boxes) - 1) / max(1, len(want) - 1)) for i in range(len(want))]
                for name, k in zip(want, pick_i):
                    (x0, y0, x1, y1), r = boxes[k]
                    crops[name] = (name, a_full[y0:y1, x0:x1], fg_full[y0:y1, x0:x1], crop)
        elif assign is not None:
            # hand-checked mapping: names in detected (row-major) order, null to skip a sprite
            for i, entry in enumerate(assign):
                if not entry or i >= len(flat): continue
                (x0, y0, x1, y1), r = flat[i]
                for name in (entry if isinstance(entry, list) else [entry]):
                    rgb, m = a_full[y0:y1, x0:x1], fg_full[y0:y1, x0:x1]
                    if name.startswith("~"):          # mirrored copy
                        name = name[1:]; rgb, m = rgb[:, ::-1], m[:, ::-1]
                    crops[name] = (name, np.ascontiguousarray(rgb), np.ascontiguousarray(m), r)
        elif sheet["group"] == "row" and rows > 0 and len(rowboxes) in (rows, rows * 2):
            if len(rowboxes) == rows * 2:      # the model drew every growth row twice: keep the first of each pair
                rowboxes = rowboxes[0::2]
            print("  rows:", [len(b) for b in rowboxes])
            for r, boxes in enumerate(rowboxes):
                want = names_by_row[r]
                if not want: continue
                if len(boxes) != len(want):
                    pick_i = [round(i * (len(boxes) - 1) / max(1, len(want) - 1)) for i in range(len(want))] if len(boxes) > 1 else [0] * len(want)
                    print(f"  note: row {r+1} of {sheet['id']} has {len(boxes)} sprites, expected {len(want)}")
                    boxes = [boxes[i] for i in pick_i]
                for name, (x0, y0, x1, y1) in zip(want, boxes):
                    crops[name] = (name, a_full[y0:y1, x0:x1], fg_full[y0:y1, x0:x1], r)
        else:
            if len(flat) != len(flat_expected):
                print(f"  WARNING: {sheet['id']}: found {len(flat)} sprites, expected {len(flat_expected)}; add an entry to assign.json")
                return 0
            for name, ((x0, y0, x1, y1), r) in zip(flat_expected, flat):
                crops[name] = (name, a_full[y0:y1, x0:x1], fg_full[y0:y1, x0:x1], r)
        # scale: per row (shared) or per sprite
        pad = 1
        box_w, box_h = (tw if th > tw else tw - 2 * pad), th - 2 * pad
        row_scale = {}
        if sheet["group"] == "row":
            for key, (name, rgb, m, r) in crops.items():
                s = min(box_w / rgb.shape[1], box_h / rgb.shape[0])
                row_scale[r] = min(row_scale.get(r, 1e9), s)
        sprites = {}
        for key, (name, rgb, m, r) in crops.items():
            s = row_scale[r] if sheet["group"] == "row" else min(box_w / rgb.shape[1], box_h / rgb.shape[0])
            fit = min(box_w / rgb.shape[1], box_h / rgb.shape[0])
            while True:
                nw, nh = max(1, round(rgb.shape[1] * s)), max(1, round(rgb.shape[0] * s))
                sp = resize_masked(rgb, m, (nw, nh))
                # a seedling must stay visible in game (tests require >= 24 opaque pixels): grow it a little if needed
                if (sp[:, :, 3] > 0).sum() >= 26 or s >= fit: break
                s = min(fit, s * 1.08)
            canvas = np.zeros((th, tw, 4), np.uint8)
            ox = (tw - nw) // 2
            oy = th - pad - nh if sheet["anchor"] == "bottom" else (th - nh) // 2
            canvas[oy:oy + nh, ox:ox + nw] = sp
            sprites[name] = canvas
        names = list(sprites)
        q = quantize([sprites[n] for n in names])
        for n, a in zip(names, q): result[n] = a

    if sheet["id"].startswith("chr_"):
        # the generator rarely draws a true right profile: mirror the left one so the pair always matches
        for n in [n for n in result if n.endswith("_idle_left")]:
            result[n.replace("_idle_left", "_idle_right")] = np.ascontiguousarray(result[n][:, ::-1])
    os.makedirs(out_dir, exist_ok=True)
    for name, a in result.items():
        Image.fromarray(a, "RGBA").save(os.path.join(out_dir, name + ".png"))
    if os.path.abspath(out_dir) == os.path.abspath(PH):
        lst = os.path.join(PH, "final_art.txt")
        have = set(open(lst).read().split()) if os.path.exists(lst) else set()
        open(lst, "w").write(chr(10).join(sorted(have | set(result))) + chr(10))
    if preview:
        pv = Image.new("RGBA", (cols * (tw * 6 + 8), rows * (th * 6 + 8)), (60, 50, 60, 255))
        for i, name in enumerate(cells):
            if name in result:
                r, c = divmod(i, cols)
                s = Image.fromarray(result[name], "RGBA").resize((tw * 6, th * 6), Image.NEAREST)
                pv.paste(s, (c * (tw * 6 + 8) + 4, r * (th * 6 + 8) + 4), s)
        pv.save(os.path.join(ROOT, "Builds", "preview_" + sheet["id"] + ".png"))
    return len(result)

if __name__ == "__main__":
    args = sys.argv[1:]
    out = PH
    preview = "--preview" in args
    if "--out" in args:
        out = args[args.index("--out") + 1]
    ids = [a for a in args if not a.startswith("--") and a != out]
    manifest = json.load(open(os.path.join(os.path.dirname(__file__), "manifest.json"), encoding="utf-8"))
    os.makedirs(os.path.join(ROOT, "Builds"), exist_ok=True)
    for s in manifest:
        if ids and s["id"] not in ids: continue
        n = slice_sheet(s, out, preview)
        print(s["id"], "->", "missing sheet" if n is None else f"{n} sprites")
