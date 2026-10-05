"""Furniture and decorations for the farmhouse and the farm (playtest request, 2026-10-04): generates one 4 x 3 sheet of twelve pieces with
Gemini 2.5 Flash Image through OpenRouter, then slices it into 16 x 16 sprites with the shared keying, scaling and palette steps of
slice_sheets.py. Each piece is written twice: Art/Placeholders/obj_<id>.png (the thing in the world) and item_machine_<id>.png (its icon).
Usage: python tools/art/generate_furniture.py [--slice-only] [--only id,id]
  --only  redo just these pieces (the sheet still has all twelve; only the named ones are written, the rest stay as approved)
Key: OPENROUTER_API_KEY in the environment or tools/art/.env (git-ignored). Provenance: Art/Generated/furniture.png and furniture_prompt.txt."""
import base64, io, json, os, sys, urllib.request, urllib.error
import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(__file__))
import slice_sheets as S
from generate_expressions import load_key, STYLE, URL, GEN, PH, ROOT

PIECES = [
    ("rug", "an oval woven rug seen from above, red and cream stripes with a tassel edge"),
    ("dining_table", "a small round wooden table with a cream tablecloth and a tiny candle"),
    ("chair", "a simple wooden chair with a straw seat, seen from the front"),
    ("bookshelf", "a tall wooden bookshelf full of colourful books"),
    ("plant", "a leafy green houseplant in a terracotta pot"),
    ("lamp", "a standing floor lamp with a warm glowing yellow shade"),
    ("painting", "a framed landscape painting (green hills and a blue sky) on a small wooden easel"),
    ("clock", "a tall wooden grandfather clock with a round white face and a brass pendulum"),
    ("bench", "a wooden bench with a green cushion"),
    ("wardrobe", "a wooden wardrobe with two doors and brass handles"),
    ("vase", "a blue ceramic vase holding yellow flowers"),
    ("armchair", "a cozy teal armchair with a cream cushion"),
]
COLS, ROWS = 4, 3
ONLY = None
SHEET = "furniture.png"


def prompt():
    names = ", ".join(f"{i + 1}. {d}" for i, (_, d) in enumerate(PIECES))
    return (STYLE + "\n\nOn a perfectly flat solid pure magenta (#FF00FF) background, draw 12 separate pieces of cozy home furniture arranged in a grid of "
            "4 columns and 3 rows. Each piece is centred in its own equal-sized cell with a wide empty magenta margin around it and never touches "
            "another piece or the image edge. All pieces share the same scale, viewpoint (front view, slightly from above) and style, and read "
            "clearly at a tiny size: bold simple shapes, few colours, warm wood tones. Left to right, top to bottom: " + names + ".\n"
            "Absolutely no text, letters, numbers, labels, captions, borders, frames around the cells, watermarks or ground shadows: the pieces float on "
            "plain magenta. No photorealism, no 3D, no black outlines.")


def generate(key, model="google/gemini-2.5-flash-image"):
    body = {"model": model, "modalities": ["image", "text"], "messages": [{"role": "user", "content": [{"type": "text", "text": prompt()}]}]}
    req = urllib.request.Request(URL, json.dumps(body).encode(), {"Authorization": "Bearer " + key, "Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(req, timeout=240) as r: resp = json.load(r)
    except urllib.error.HTTPError as e:
        sys.exit(f"HTTP {e.code}: {e.read().decode(errors='replace')[:400]}")
    images = resp["choices"][0]["message"].get("images") or []
    if not images: sys.exit("no image in the response")
    raw = base64.b64decode(images[0]["image_url"]["url"].split(",", 1)[1])
    open(os.path.join(GEN, SHEET), "wb").write(raw)
    open(os.path.join(GEN, SHEET.replace(".png", "_prompt.txt")), "w", encoding="utf-8").write(f"model: {model}\n\n{prompt()}\n")
    print("sheet written", Image.open(io.BytesIO(raw)).size)


def slice_sheet():
    img = Image.open(os.path.join(GEN, SHEET)).convert("RGB")
    rgb, fg = S.key_background(img)
    r, g, b = rgb[:, :, 0], rgb[:, :, 1], rgb[:, :, 2]
    fg = fg & ~((r - g > 50) & (b - g > 35) & (abs(r - b) < 90))
    fg = S.clean_mask(fg)
    rows = S.segment(fg, ROWS, dilate=7)
    boxes = [bx for row in rows for bx in row]
    os.makedirs(os.path.join(ROOT, "Builds"), exist_ok=True)
    if len(boxes) != len(PIECES):
        S.index_preview(img, rows, os.path.join(ROOT, "Builds", "idx_furniture.png"))
        sys.exit(f"found {len(boxes)} pieces, expected {len(PIECES)} (see Builds/idx_furniture.png)")
    tw = th = 16
    sprites = []
    for (x0, y0, x1, y1) in boxes:
        c, m = rgb[y0:y1, x0:x1], fg[y0:y1, x0:x1]
        scale = min((tw - 2) / c.shape[1], (th - 2) / c.shape[0])
        nw, nh = max(1, round(c.shape[1] * scale)), max(1, round(c.shape[0] * scale))
        sp = S.resize_masked(c, m, (nw, nh))
        canvas = np.zeros((th, tw, 4), np.uint8)
        canvas[th - 1 - nh:th - 1, (tw - nw) // 2:(tw - nw) // 2 + nw] = sp         # standing on the bottom edge of the tile
        sprites.append(canvas)
    out = S.quantize(sprites)
    pv = Image.new("RGBA", (len(PIECES) * (tw * 6 + 8), th * 6 + 8), (60, 50, 60, 255))
    for i, ((name, _), a) in enumerate(zip(PIECES, out)):
        im = Image.fromarray(a, "RGBA")
        if not ONLY or name in ONLY:
            existing = os.path.join(PH, f"obj_{name}.png")
            if os.path.exists(existing) and not os.path.exists(os.path.join(PH, f"item_machine_{name}.png")):
                sys.exit(f"obj_{name}.png already exists and was not made by this tool: rename the piece instead of overwriting it")
            im.save(existing)
            im.save(os.path.join(PH, f"item_machine_{name}.png"))
        big = im.resize((tw * 6, th * 6), Image.NEAREST)
        pv.paste(big, (i * (tw * 6 + 8) + 4, 4), big)
    pv.save(os.path.join(ROOT, "Builds", "preview_furniture.png"))
    print("wrote", ", ".join(n for n, _ in PIECES if not ONLY or n in ONLY))


if __name__ == "__main__":
    for i, a in enumerate(sys.argv[1:]):
        if a.startswith("--only"):
            ONLY = (a.split("=", 1)[1] if "=" in a else sys.argv[i + 2]).split(",")
    if ONLY: SHEET = "furniture_redo.png"
    os.makedirs(GEN, exist_ok=True)
    if "--slice-only" not in sys.argv: generate(load_key())
    slice_sheet()
