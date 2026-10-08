"""Redraws one villager's four idle sprites (npc_<id>_idle_down/up/left/right, 16 x 32) from their portrait: generates a 4 x 1 sheet on a
magenta background with Gemini 2.5 Flash Image via OpenRouter (the villager's base portrait is the reference image), then slices it with the
same keying, scaling and palette steps as slice_sheets.py.
Usage: python tools/art/redo_character.py hazel [--slice-only] [--swap] [--sides-only]
  --slice-only  reuse Art/Generated/chr_<id>.png without calling the API
  --sides-only  write only the left and right pictures and keep the existing front and back ones (used when only the side views were wrong)
  --swap        the generated 'left' view faces right (check by eye): swap the pair
Key: OPENROUTER_API_KEY in the environment or tools/art/.env (git-ignored). Provenance: Art/Generated/chr_<id>.png and chr_<id>_prompt.txt."""
import base64, io, json, os, sys, urllib.request, urllib.error
import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(__file__))
import slice_sheets as S                      # keying, segmentation, scaling, palette
from generate_expressions import load_key, data_uri, STYLE, URL, GEN, PH, ROOT

LOOK = {
    "dorian": "Dorian Lake, the forager: quiet young man, moss-green coat, grey scarf, a small basket of mushrooms on his arm, dark trousers and boots",
    "piper": "Piper Vance, the saloon musician: lanky young woman, mustard coat, black curly hair, a fiddle on her back, dark trousers and boots",
    "bram": "Bram Hollis, the blacksmith: broad, gruff man with a full dark beard and short dark hair, a grey leather smith's apron over a cream shirt with rolled sleeves, brown work gloves, dark trousers and boots",
    "marcus": "Marcus Dell, the carpenter: middle-aged man in a flat brown cap with a short brown beard, a brown waistcoat over a cream shirt, a tool belt, dark trousers and boots",
    "wren": "Wren Calloway, the saloon keeper: young woman with wavy copper-red hair, a red waistcoat over a cream blouse with rolled sleeves, dark red-brown trousers and boots",
    "felix": "Felix Hartwell, the fish seller: man in a wide straw hat with short stubble, a blue fisherman's jacket with a teal scarf, dark trousers and rubber boots",
    "juno": "Juno Hale, the blacksmith's apprentice: young woman with red-orange hair in two short tails and goggles pushed up on her forehead, a tan leather apron over a cream shirt, dark trousers and boots",
    "elara": "Elara Finch, the clinic nurse: young woman with long dark purple hair in a braid, a light blue cap and a light blue dress, a small brown satchel on a strap, brown boots",
    "hazel": "Hazel Brandt, the library assistant: slight, shy young woman, round glasses, dark bob haircut, a mustard cardigan over a teal blouse, dark trousers and boots",
}


def prompt_for(npc):
    return (STYLE + "\n\nThe attached image is the reference portrait of the character: keep the same face, hair, glasses and clothes colours. "
            "On a perfectly flat solid pure magenta (#FF00FF) background, draw 4 separate sprites arranged in a single row of 4 columns. Each sprite is "
            "centred in its own equal-sized cell with a wide empty magenta margin around it and never touches another sprite or the image edge. All "
            "sprites share the same scale, viewpoint and style. Chibi proportions (head about 40 percent of the height), standing idle, small full-body "
            "character taller than wide. The one single character, drawn four times (same outfit, same colours, same proportions): " + LOOK[npc] +
            ". Columns are: facing the viewer, facing away, left profile, right profile.\n"
            "Absolutely no text, letters, numbers, labels, captions, borders, frames, boxes or outlines around the sprites, watermarks or ground shadows: the sprites float on plain magenta. No photorealism, no 3D, no black "
            "outlines, no gore, no scary faces.")


def generate(npc, key, model="google/gemini-2.5-flash-image"):
    prompt = prompt_for(npc)
    ref = os.path.join(PH, f"ui_portrait_{npc}.png")
    body = {"model": model, "modalities": ["image", "text"],
            "messages": [{"role": "user", "content": [{"type": "text", "text": prompt}, {"type": "image_url", "image_url": {"url": data_uri(ref)}}]}]}
    req = urllib.request.Request(URL, json.dumps(body).encode(), {"Authorization": "Bearer " + key, "Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(req, timeout=240) as r: resp = json.load(r)
    except urllib.error.HTTPError as e:
        sys.exit(f"{npc}: HTTP {e.code}: {e.read().decode(errors='replace')[:400]}")
    images = resp["choices"][0]["message"].get("images") or []
    if not images: sys.exit(f"{npc}: no image in the response")
    raw = base64.b64decode(images[0]["image_url"]["url"].split(",", 1)[1])
    open(os.path.join(GEN, f"chr_{npc}.png"), "wb").write(raw)
    open(os.path.join(GEN, f"chr_{npc}_prompt.txt"), "w", encoding="utf-8").write(f"model: {model}\n\n{prompt}\n")
    print(f"{npc}: sheet written ({Image.open(io.BytesIO(raw)).size})")


def slice_sheet(npc, swap, sides_only=False):
    img = Image.open(os.path.join(GEN, f"chr_{npc}.png")).convert("RGB")
    rgb, fg = S.key_background(img)
    fg = S.clean_mask(fg)
    rows = S.segment(fg, 1, dilate=7)
    boxes = [b for r in rows for b in r]
    if len(boxes) != 4:
        os.makedirs(os.path.join(ROOT, "Builds"), exist_ok=True)
        S.index_preview(img, rows, os.path.join(ROOT, "Builds", f"idx_chr_{npc}.png"))
        sys.exit(f"{npc}: found {len(boxes)} sprites, expected 4 (see Builds/idx_chr_{npc}.png)")
    boxes.sort(key=lambda b: b[0])
    names = ["down", "up", "left", "right"]
    tw, th, pad = 16, 32, 1
    box_w, box_h = tw - 2 * pad, th - 2 * pad
    crops = {n: (rgb[y0:y1, x0:x1], fg[y0:y1, x0:x1]) for n, (x0, y0, x1, y1) in zip(names, boxes)}
    scale = min(min(box_w / c[0].shape[1], box_h / c[0].shape[0]) for c in crops.values())      # one shared scale
    sprites = {}
    for n, (c, m) in crops.items():
        nw, nh = max(1, round(c.shape[1] * scale)), max(1, round(c.shape[0] * scale))
        sp = S.resize_masked(c, m, (nw, nh))
        canvas = np.zeros((th, tw, 4), np.uint8)
        canvas[th - pad - nh:th - pad, (tw - nw) // 2:(tw - nw) // 2 + nw] = sp
        sprites[n] = canvas
    sprites["right"] = np.ascontiguousarray(sprites["left"][:, ::-1])          # the generator rarely draws a true right profile
    if swap: sprites["left"], sprites["right"] = sprites["right"], sprites["left"]
    keys = [k for k in sprites if not sides_only or k in ("left", "right")]
    out = S.quantize([sprites[k] for k in keys])
    for arr in out:                                                      # a leftover pixel of the magenta background (pink: red and blue well above green) is not part of the character
        pink = (arr[:, :, 3] > 0) & (arr[:, :, 0].astype(int) - arr[:, :, 1] >= 50) & (arr[:, :, 2].astype(int) - arr[:, :, 1] >= 25)
        arr[pink] = 0
    names_out = []
    for k, a in zip(keys, out):
        name = f"npc_{npc}_idle_{k}"
        Image.fromarray(a, "RGBA").save(os.path.join(PH, name + ".png"))
        names_out.append(name)
    pv = Image.new("RGBA", (4 * (tw * 6 + 8), th * 6 + 8), (60, 50, 60, 255))
    for i, name in enumerate(names_out):
        s = Image.open(os.path.join(PH, name + ".png")).convert("RGBA").resize((tw * 6, th * 6), Image.NEAREST)
        pv.paste(s, (i * (tw * 6 + 8) + 4, 4), s)
    pv.save(os.path.join(ROOT, "Builds", f"preview_chr_{npc}.png"))
    print(f"{npc}: wrote {', '.join(names_out)}")


if __name__ == "__main__":
    npc = next((a for a in sys.argv[1:] if not a.startswith("--")), None)
    if npc not in LOOK: sys.exit("Usage: redo_character.py hazel [--slice-only] [--swap]")
    os.makedirs(os.path.join(ROOT, "Builds"), exist_ok=True)
    if "--slice-only" not in sys.argv: generate(npc, load_key())
    slice_sheet(npc, "--swap" in sys.argv, "--sides-only" in sys.argv)
