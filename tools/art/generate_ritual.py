"""Illustrations for the witnessed ritual (2026-10-10): the Keepers on the road, the moment an offering dissolves into light, and the moment a ritual fails and the ground stirs.
Same pipeline as the endings (generate_endings.py): Gemini 2.5 Flash Image through OpenRouter, cropped to 16:9, scaled to 320 x 180 and reduced to a small palette.
Usage: python tools/art/generate_ritual.py [procession dissolve stir] [--slice-only]
Key: OPENROUTER_API_KEY in the environment or tools/art/.env (git-ignored). Provenance: Art/Generated/ritual_<id>.png and ritual_<id>_prompt.txt.
Output: Assets/_Project/Resources/Ritual/ritual_<id>.png (a Texture2D under Resources, shown by the `illustration` step of a scene).
Full intensity only (the scene is not shown at mild). No gore, no blood, no corpses, no harm to anyone or anything; the offerings turn to light."""
import base64, io, json, os, sys, urllib.request, urllib.error
from PIL import Image

sys.path.insert(0, os.path.dirname(__file__))
from generate_expressions import load_key, URL, GEN, ROOT
from generate_endings import STYLE, SIZE, COLORS

OUT = os.path.join(ROOT, "Assets", "_Project", "Resources", "Ritual")

SCENES = {
    "procession": ("A new-moon night on a country road: a single line of five hooded figures in deep green robes walks away from the viewer toward a dark pine wood, "
                   "each carrying a small warm lantern, the lights making a dotted line along the road, low fog over the fields, a few tiny lit windows of the village "
                   "far behind, no moon, stars, a stone wall on one side. Quiet and ominous, nothing violent."),
    "dissolve": ("The clearing of a dark pine wood on a new-moon night: a stone altar at the centre, a ring of eight small candles around it, five hooded figures in deep green robes "
                 "standing in a circle with their hoods up and small lanterns in their hands, and above the altar a single offering (a small round loaf) dissolving upward into a column "
                 "of golden light and drifting motes, the light spilling warm gold over the figures and the trunks. Awe and calm, a sacred feeling, no faces visible, nothing violent."),
    "stir": ("The same clearing in a dark pine wood at night, but wrong: the stone altar is bare, the candles are guttering, the hooded green figures have stepped back and lowered their lanterns, "
             "and the ground of the clearing glows faintly from beneath through hairline cracks, a pale green-gold light seeping up between the roots, fog curling low. "
             "A tense, hushed moment, something very large below has stirred. No monster visible, no gore, nothing violent."),
}


def prompt(name):
    return STYLE + "\n\n" + SCENES[name]


def generate(key, name, model="google/gemini-2.5-flash-image"):
    body = {"model": model, "modalities": ["image", "text"], "messages": [{"role": "user", "content": [{"type": "text", "text": prompt(name)}]}]}
    req = urllib.request.Request(URL, json.dumps(body).encode(), {"Authorization": "Bearer " + key, "Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(req, timeout=240) as r: resp = json.load(r)
    except urllib.error.HTTPError as e:
        sys.exit(f"HTTP {e.code}: {e.read().decode(errors='replace')[:400]}")
    images = resp["choices"][0]["message"].get("images") or []
    if not images: sys.exit("no image in the response for " + name)
    raw = base64.b64decode(images[0]["image_url"]["url"].split(",", 1)[1])
    open(os.path.join(GEN, f"ritual_{name}.png"), "wb").write(raw)
    open(os.path.join(GEN, f"ritual_{name}_prompt.txt"), "w", encoding="utf-8").write(f"model: {model}\n\n{prompt(name)}\n")
    print(name, "generated", Image.open(io.BytesIO(raw)).size)


def process(name):
    img = Image.open(os.path.join(GEN, f"ritual_{name}.png")).convert("RGB")
    w, h = img.size
    want = SIZE[0] / SIZE[1]
    if w / h > want:
        nw = int(h * want); img = img.crop(((w - nw) // 2, 0, (w - nw) // 2 + nw, h))
    else:
        nh = int(w / want); top = (h - nh) // 3; img = img.crop((0, top, w, top + nh))
    img = img.resize(SIZE, Image.BOX).quantize(colors=COLORS, method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE).convert("RGB")
    os.makedirs(OUT, exist_ok=True)
    img.save(os.path.join(OUT, f"ritual_{name}.png"))
    print(name, "written", SIZE)


if __name__ == "__main__":
    names = [a for a in sys.argv[1:] if not a.startswith("--")] or list(SCENES)
    os.makedirs(GEN, exist_ok=True)
    key = None if "--slice-only" in sys.argv else load_key()
    for n in names:
        if key: generate(key, n)
        process(n)
