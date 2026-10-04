"""Story props (T-122/T-123/T-143): generates one 4 x 2 sheet of small story props with Gemini 2.5 Flash Image through OpenRouter, then slices
it into 16 x 16 item icons (Art/Placeholders/item_prop_<name>.png) with the shared keying, scaling and palette steps of slice_sheets.py.
Scenes show a prop through an item icon (the `spawn` step), so each prop is also a non-sellable Misc item (see ContentGenerator).
Usage: python tools/art/generate_props.py [--slice-only] [--only name,name]
  --only  redo just these props: the sheet is still 8 cells, but only the named ones are written (the others stay as approved); the raw sheet goes to Art/Generated/props_redo.png
Key: OPENROUTER_API_KEY in the environment or tools/art/.env (git-ignored). Provenance: Art/Generated/props.png and props_prompt.txt."""
import base64, io, json, os, sys, urllib.request, urllib.error
import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(__file__))
import slice_sheets as S
from generate_expressions import load_key, STYLE, URL, GEN, PH, ROOT

PROPS = [
    ("cat", "a small cool slate-gray cat (blue-gray fur, definitely not brown or orange) sitting upright, seen from the front, tail curled around its feet, tiny white chest patch"),
    ("umbrella", "a large bright cherry-red umbrella (pure red, not orange), open, seen from slightly above, a curved wooden handle"),
    ("note", "a small folded cream paper note tied with a thin red ribbon"),
    ("lantern", "a round paper lantern glowing warm yellow-orange with a little wooden top and a short string"),
    ("trophy", "a tiny iron anvil trophy on a small wooden base, with a small fish shape on top"),
    ("pumpkin", "a huge round orange prize pumpkin with a blue ribbon rosette on its side and a green curly stem, sitting on nothing (no post, no stand)"),
    ("scarecrow", "a cheerful straw scarecrow on a wooden post wearing a floppy patched brown hat, arms out"),
    ("shell", "a pretty pink-and-cream spiral seashell"),
]


REDO = {
    "trophy": "a tiny grey iron anvil on a small wooden base, with a small orange-red fish shape lying on top of the anvil",
    "scarecrow": "a cheerful straw scarecrow standing upright on a wooden post, seen from the front, floppy patched brown hat, straw arms held out to the sides",
}


def prompt():
    names = ", ".join(f"{i + 1}. {REDO.get(n, d) if ONLY and n in ONLY else d}" for i, (n, d) in enumerate(PROPS))
    return (STYLE + "\n\nOn a perfectly flat solid pure magenta (#FF00FF) background, draw 8 separate small props arranged in a grid of 4 columns "
            "and 2 rows. Each prop is centred in its own equal-sized cell with a wide empty magenta margin around it and never touches another "
            "prop or the image edge. All props share the same scale, viewpoint (slightly from above) and style, and read clearly at a tiny size: "
            "bold simple shapes, few colours. Left to right, top to bottom: " + names + ".\n"
            "Absolutely no text, letters, numbers, labels, captions, borders, frames, boxes or outlines around the props, watermarks or ground "
            "shadows: the props float on plain magenta. No photorealism, no 3D, no black outlines, no gore, no scary faces.")


ONLY = None
SHEET = "props.png"


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
    open(os.path.join(GEN, "props_prompt.txt"), "w", encoding="utf-8").write(f"model: {model}\n\n{prompt()}\n")
    print("sheet written", Image.open(io.BytesIO(raw)).size)


def slice_sheet():
    img = Image.open(os.path.join(GEN, SHEET)).convert("RGB")
    rgb, fg = S.key_background(img)
    fg = S.clean_mask(fg)
    rows = S.segment(fg, 2, dilate=7)
    boxes = [b for r in rows for b in r]
    os.makedirs(os.path.join(ROOT, "Builds"), exist_ok=True)
    if len(boxes) != len(PROPS):
        S.index_preview(img, rows, os.path.join(ROOT, "Builds", "idx_props.png"))
        sys.exit(f"found {len(boxes)} props, expected {len(PROPS)} (see Builds/idx_props.png)")
    tw = th = 16
    sprites = []
    for (x0, y0, x1, y1) in boxes:
        c, m = rgb[y0:y1, x0:x1], fg[y0:y1, x0:x1]
        scale = min((tw - 2) / c.shape[1], (th - 2) / c.shape[0])
        nw, nh = max(1, round(c.shape[1] * scale)), max(1, round(c.shape[0] * scale))
        sp = S.resize_masked(c, m, (nw, nh))
        canvas = np.zeros((th, tw, 4), np.uint8)
        canvas[(th - nh) // 2:(th - nh) // 2 + nh, (tw - nw) // 2:(tw - nw) // 2 + nw] = sp
        sprites.append(canvas)
    out = S.quantize(sprites)
    pv = Image.new("RGBA", (len(PROPS) * (tw * 8 + 8), th * 8 + 8), (60, 50, 60, 255))
    for i, ((name, _), a) in enumerate(zip(PROPS, out)):
        im = Image.fromarray(a, "RGBA")
        if not ONLY or name in ONLY: im.save(os.path.join(PH, f"item_prop_{name}.png"))
        big = im.resize((tw * 8, th * 8), Image.NEAREST)
        pv.paste(big, (i * (tw * 8 + 8) + 4, 4), big)
    pv.save(os.path.join(ROOT, "Builds", "preview_props.png"))
    print("wrote", ", ".join(f"item_prop_{n}" for n, _ in PROPS))


if __name__ == "__main__":
    for a in sys.argv[1:]:
        if a.startswith("--only"):
            ONLY = (a.split("=", 1)[1] if "=" in a else sys.argv[sys.argv.index(a) + 1]).split(",")
    if ONLY: SHEET = "props_redo.png"
    os.makedirs(GEN, exist_ok=True)
    if "--slice-only" not in sys.argv: generate(load_key())
    slice_sheet()
