"""Ending illustrations (M3b / X-010): one wide pixel-art picture per ending (awakened, sealed, joined, ignored), generated with
Gemini 2.5 Flash Image through OpenRouter, then cropped to 16:9, scaled down to 320 x 180 and reduced to a small palette so that they sit
with the rest of the art. The pictures are shown full screen by the closing screen (IUiService.ShowIllustration).
Usage: python tools/art/generate_endings.py [awakened sealed joined ignored] [--slice-only]
Key: OPENROUTER_API_KEY in the environment or tools/art/.env (git-ignored). Provenance: Art/Generated/ending_<id>.png and ending_<id>_prompt.txt.
Output: Assets/_Project/Resources/Endings/ending_<id>.png (loaded as a Texture2D, so no sprite import is needed).
All four are free of gore and of explicit ritual imagery, so they are fine at the mild intensity as well."""
import base64, io, json, os, sys, urllib.request, urllib.error
from PIL import Image

sys.path.insert(0, os.path.dirname(__file__))
from generate_expressions import load_key, URL, GEN, ROOT

OUT = os.path.join(ROOT, "Assets", "_Project", "Resources", "Endings")
SIZE = (320, 180)
COLORS = 28

STYLE = ("Cozy hand-crafted pixel art for a warm countryside farming game with a quiet cosmic-horror undertone, storybook look, chunky shapes, "
         "crisp hard pixel edges, no anti-aliasing, no blur, limited palette of about 24 colours, dithering allowed sparingly. One wide cinematic "
         "scene that fills the whole frame edge to edge, no border, no text, no letters, no numbers, no watermark, no UI. The village is called "
         "Wetherell: a small New England farming village of timber houses with steep roofs, a farmhouse, a barn, stone walls and a dark pine wood "
         "(Harrow Wood) behind it. No gore, no blood, no corpses, no skulls, no scary faces.")

ENDINGS = {
    "awakened": ("A fiery doom over the village at dusk: the sky is blood red and orange, glowing cracks of firelight run through the dark pine wood "
                 "at the horizon, and a colossal vague shape of tentacled darkness with one great pale glowing eye rises behind the trees, "
                 "taller than the clouds. The small timber houses of Wetherell in the foreground are in silhouette, with tiny lit windows, "
                 "and a few tiny villagers stand in the road looking up. Dramatic and awe-struck, not gory."),
    "sealed": ("A calm, hopeful dawn in the sacred clearing of Harrow Wood: a stone altar in the centre with three small glowing relics "
               "(green, gold, blue) arranged on it, soft green light flowing from them into the ground and sealing a great carved stone circle "
               "set in the earth, pale mist lifting between the pines, golden sunrise light through the branches, birds in the sky, "
               "a lone farmer in a straw hat standing at the edge looking on peacefully."),
    "joined": ("A solemn moonlit gathering in the clearing of Harrow Wood: a ring of figures in deep green hooded robes holding small lanterns "
               "stand in a circle around the stone altar, a big pale moon above the pines, violet and teal night colours, "
               "and at the centre of the circle one figure with a straw hat on their back has put on a green hood and stands among them, "
               "serene and chosen. Mysterious and quiet, nothing violent."),
    "ignored": ("A warm, cozy farm at golden sunset in the third year: the farmhouse with a glowing window, a neat vegetable field, a scarecrow "
                "and a cat on the fence, a farmer in a straw hat sitting on the porch with a cup of tea, and far behind, the dark edge of "
                "Harrow Wood where a faint strange light pulses between the trees, which no one in the picture notices. Peaceful and sweet "
                "with a very faint uneasy hint."),
}


def prompt(name):
    return STYLE + "\n\n" + ENDINGS[name]


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
    open(os.path.join(GEN, f"ending_{name}.png"), "wb").write(raw)
    open(os.path.join(GEN, f"ending_{name}_prompt.txt"), "w", encoding="utf-8").write(f"model: {model}\n\n{prompt(name)}\n")
    print(name, "generated", Image.open(io.BytesIO(raw)).size)


def process(name):
    img = Image.open(os.path.join(GEN, f"ending_{name}.png")).convert("RGB")
    w, h = img.size
    want = SIZE[0] / SIZE[1]
    if w / h > want:                                   # too wide: trim the sides
        nw = int(h * want); img = img.crop(((w - nw) // 2, 0, (w - nw) // 2 + nw, h))
    else:                                              # too tall: trim top and bottom, a little more from the top (skies hold less)
        nh = int(w / want); top = (h - nh) // 3; img = img.crop((0, top, w, top + nh))
    img = img.resize(SIZE, Image.BOX).quantize(colors=COLORS, method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE).convert("RGB")
    os.makedirs(OUT, exist_ok=True)
    img.save(os.path.join(OUT, f"ending_{name}.png"))
    print(name, "written", SIZE)


if __name__ == "__main__":
    names = [a for a in sys.argv[1:] if not a.startswith("--")] or list(ENDINGS)
    os.makedirs(GEN, exist_ok=True)
    key = None if "--slice-only" in sys.argv else load_key()
    for n in names:
        if key: generate(key, n)
        process(n)
