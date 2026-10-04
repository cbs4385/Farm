"""T-131: pose sprites for a villager (wave, sit, shrug, point), 24 x 32, facing the viewer. One 4 x 1 sheet per villager is generated with
Gemini 2.5 Flash Image through OpenRouter (the villager's portrait is the reference image so the outfit and colours stay), then sliced with
the shared keying, scaling and palette steps of slice_sheets.py into Art/Placeholders/npc_<id>_pose_<name>.png.
Usage: python tools/art/generate_poses.py hazel [more ids] [--slice-only]
Key: OPENROUTER_API_KEY in the environment or tools/art/.env (git-ignored). Provenance: Art/Generated/pose_<id>.png and pose_<id>_prompt.txt."""
import base64, io, json, os, sys, urllib.request, urllib.error
import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(__file__))
import slice_sheets as S
from generate_expressions import load_key, data_uri, STYLE, URL, GEN, PH, ROOT, LOOK

POSES = ["wave", "sit", "shrug", "point"]
DESC = {
    "wave": "standing and waving hello with one hand raised high, a friendly smile",
    "sit": "sitting on the ground with knees up, relaxed, seen from the front, a calm smile",
    "shrug": "standing with both shoulders raised and both palms turned up in a shrug, a sheepish expression",
    "point": "standing and pointing off to the side with one straight arm, mouth slightly open as if saying 'over there'",
}


def prompt_for(npc):
    cells = "; ".join(f"column {i + 1}: {DESC[p]}" for i, p in enumerate(POSES))
    return (STYLE + "\n\nThe attached image is the reference portrait of the character: keep the same face, hair, glasses and clothes colours. "
            "On a perfectly flat solid pure magenta (#FF00FF) background, draw 4 separate sprites arranged in a single row of 4 columns. Each sprite is "
            "centred in its own equal-sized cell with a wide empty magenta margin around it and never touches another sprite or the image edge. All "
            "sprites share the same scale and style. Chibi proportions (head about 40 percent of the height), small full-body character. The one "
            "single character, drawn four times in four poses (same outfit, same colours, same proportions): " + LOOK[npc] + ". The poses: " + cells + ".\n"
            "Absolutely no text, letters, numbers, labels, captions, borders, frames, boxes or outlines around the sprites, watermarks or ground "
            "shadows: the sprites float on plain magenta. No photorealism, no 3D, no black outlines, no gore, no scary faces.")


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
    open(os.path.join(GEN, f"pose_{npc}.png"), "wb").write(raw)
    open(os.path.join(GEN, f"pose_{npc}_prompt.txt"), "w", encoding="utf-8").write(f"model: {model}\n\n{prompt}\n")
    print(f"{npc}: sheet written ({Image.open(io.BytesIO(raw)).size})")


def slice_sheet(npc):
    img = Image.open(os.path.join(GEN, f"pose_{npc}.png")).convert("RGB")
    rgb, fg = S.key_background(img)
    r, g, b = rgb[:, :, 0], rgb[:, :, 1], rgb[:, :, 2]
    fg = fg & ~((r - g > 50) & (b - g > 35) & (abs(r - b) < 90))      # the darker magenta of the ground shadow and the fringe
    fg = S.clean_mask(fg)
    rows = S.segment(fg, 1, dilate=7)
    boxes = [b for r in rows for b in r]
    os.makedirs(os.path.join(ROOT, "Builds"), exist_ok=True)
    if len(boxes) != len(POSES):
        S.index_preview(img, rows, os.path.join(ROOT, "Builds", f"idx_pose_{npc}.png"))
        sys.exit(f"{npc}: found {len(boxes)} sprites, expected {len(POSES)} (see Builds/idx_pose_{npc}.png)")
    boxes.sort(key=lambda b: b[0])
    tw, th, pad = 24, 32, 1      # a little wider than the 16 px idle sprites: arms out need room
    crops = {p: (rgb[y0:y1, x0:x1], fg[y0:y1, x0:x1]) for p, (x0, y0, x1, y1) in zip(POSES, boxes)}
    # One scale for every pose, taken from the standing ones, so the character does not change size when it gestures.
    standing = [crops[p][0].shape for p in POSES if p != "sit"]
    scale = min(min((tw - 2 * pad) / w, (th - 2 * pad) / h) for h, w, _ in standing)
    sprites = []
    for p in POSES:
        c, m = crops[p]
        nw, nh = max(1, round(c.shape[1] * scale)), max(1, round(c.shape[0] * scale))
        nw, nh = min(nw, tw - 2 * pad), min(nh, th - 2 * pad)
        sp = S.resize_masked(c, m, (nw, nh))
        canvas = np.zeros((th, tw, 4), np.uint8)
        canvas[th - pad - nh:th - pad, (tw - nw) // 2:(tw - nw) // 2 + nw] = sp
        sprites.append(canvas)
    out = S.quantize(sprites)
    pv = Image.new("RGBA", (len(POSES) * (tw * 6 + 8), th * 6 + 8), (60, 50, 60, 255))
    for i, (p, a) in enumerate(zip(POSES, out)):
        im = Image.fromarray(a, "RGBA")
        im.save(os.path.join(PH, f"npc_{npc}_pose_{p}.png"))
        big = im.resize((tw * 6, th * 6), Image.NEAREST)
        pv.paste(big, (i * (tw * 6 + 8) + 4, 4), big)
    pv.save(os.path.join(ROOT, "Builds", f"preview_pose_{npc}.png"))
    print(f"{npc}: wrote " + ", ".join(f"npc_{npc}_pose_{p}" for p in POSES))


if __name__ == "__main__":
    ids = [a for a in sys.argv[1:] if not a.startswith("--")]
    if not ids or any(i not in LOOK for i in ids): sys.exit("Usage: generate_poses.py hazel [more ids] [--slice-only]")
    key = None if "--slice-only" in sys.argv else load_key()
    for npc in ids:
        if key: generate(npc, key)
        slice_sheet(npc)
