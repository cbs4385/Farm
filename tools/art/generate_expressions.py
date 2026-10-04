"""T-130: generates expression portrait sheets with Gemini 2.5 Flash Image through OpenRouter, using the villager's existing
portrait as a reference image so the character stays recognisable.

Usage: python tools/art/generate_expressions.py wren hazel bram [--model google/gemini-2.5-flash-image] [--no-ref]
(--no-ref: no reference image, for a new base look; slice it with slice_expressions.py --base)
Key: OPENROUTER_API_KEY in the environment or in tools/art/.env (git-ignored; never commit it). The key is never printed.
Output: Assets/_Project/Art/Generated/expr_<npc>.png (the raw sheet: 3 x 3 grid) and expr_<npc>_prompt.txt (the prompt, as provenance).
Slice the sheets with tools/art/slice_expressions.py."""
import base64, io, json, os, re, sys, urllib.request, urllib.error
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
GEN = os.path.join(ROOT, "Assets", "_Project", "Art", "Generated")
PH = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")
URL = "https://openrouter.ai/api/v1/chat/completions"

# Row-major cells of the 3 x 3 sheet. The first six are the game's expression names; the last three are spare.
CELLS = ["neutral", "happy", "sad", "surprised", "embarrassed", "thinking", "angry", "laughing", "sleepy"]
LOOK = {
    "wren": "Wren Calloway, the saloon owner: warm, quick-witted woman with red hair in a loose bun, a rust-red apron over a cream blouse, freckles, a bar towel on one shoulder",
    "hazel": "Hazel Brandt, the library assistant: slight, shy young woman, round glasses, dark bob haircut, a mustard cardigan over a teal blouse, a pencil behind one ear",
    "tilda": "Tilda Ashby, the general store owner: warm motherly woman, green apron over a cream blouse, chestnut hair in a bun, round spectacles",
    "juno": "Juno Hale, the blacksmith's apprentice: teenage girl, oversized apron, auburn hair in a short ponytail, grimy goggles on her forehead",
    "piper": "Piper Vance, the saloon musician: lanky young woman, mustard coat, black curly hair, a fiddle on her back",
    "marcus": "Marcus Dell, the carpenter: sturdy man, brown work vest, flat cap, a pencil on his ear, tool belt",
    "odalys": "Dr. Odalys Penn, the village doctor: tall woman, long white coat, dark hair pinned up, small round pendant; kind but unreadable",
    "felix": "Felix Hartwell, the fishmonger: easygoing man, blue oilskin jacket, yellow sou'wester hat, fishing net on his back",
    "dorian": "Dorian Lake, the forager: quiet young man, moss-green coat with a hood down, grey scarf, a small basket of mushrooms on his arm",
    "elara": "Elara Finch, the clinic nurse: gentle woman, pale blue uniform and cap, dark purple hair in twin tails, a small satchel of herbs",
    "ione": "Ione Fairweather, the librarian: slender woman, lavender cardigan, golden hair in a braid, a pencil behind her ear, holding a book",
    "bram": "Bram Hollis, the blacksmith: broad, bearded man, grey work apron, dark hair, rolled sleeves, a soot smudge on one cheek",
}
FEEL = {
    "neutral": "neutral, calm, a small closed-mouth smile (match the reference image exactly)",
    "happy": "happy, a warm open smile, eyes crinkled",
    "sad": "sad, eyes lowered, mouth turned down, eyebrows tilted up in the middle",
    "surprised": "surprised, eyes wide, eyebrows raised high, small round open mouth",
    "embarrassed": "embarrassed, a blush across the cheeks, eyes glancing aside, a small crooked smile, one hand not shown",
    "thinking": "thinking, eyes looking up and to the side, mouth pursed a little, one eyebrow raised",
    "angry": "annoyed or angry, eyebrows pulled down, mouth a firm line",
    "laughing": "laughing, eyes shut, mouth wide open in a big laugh",
    "sleepy": "sleepy, heavy half-closed eyelids, a small yawn",
}
STYLE = ("Cozy hand-crafted pixel art for a warm countryside farming game, storybook look, soft friendly rounded chunky shapes, "
         "crisp hard pixel edges, no anti-aliasing, no gradients, no blur. Warm palette only: cream, butter yellow, honey, terracotta, "
         "rosewood, moss green, leaf green, deep pine, sky teal, dusk blue, lavender, bark brown, soil brown, warm stone grey; "
         "one-pixel warm dark brown outlines (never black); light from the upper left; two soft tones per colour.")


def load_key():
    key = os.environ.get("OPENROUTER_API_KEY")
    path = os.path.join(os.path.dirname(__file__), ".env")
    if not key and os.path.exists(path):
        for line in open(path, encoding="utf-8"):
            m = re.match(r"\s*OPENROUTER_API_KEY\s*=\s*(\S+)", line)
            if m: key = m.group(1)
    if not key: sys.exit("No OPENROUTER_API_KEY (environment or tools/art/.env).")
    return key


def prompt_for(npc, use_ref=True):
    lines = [f"{i + 1}. {c}: {FEEL[c]}" for i, c in enumerate(CELLS)]
    return (STYLE + "\n\nThe attached image is the reference portrait of the character. Draw nine head-and-shoulders portraits of the SAME character "
            f"({LOOK[npc]}) in a perfect 3 by 3 grid that fills the entire image edge to edge. Every portrait is a square cell touching its "
            "neighbours directly with NO gaps, NO borders and NO frames. Same character, same face shape, hair, clothes, colours and slight "
            "three-quarter angle and framing in every cell; only the expression changes. Each cell has the same plain soft warm colour "
            "backdrop as the reference, filling the whole cell. ABSOLUTELY NO frame lines, outlines, borders, gutters or white margins anywhere: the backdrops of the nine cells meet each other and the image edge directly. Left to right, top to bottom:\n" + "\n".join(lines) +
            "\nNo text, no labels, no watermark. NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, "
            "neon colours, scary faces, extra limbs, different art style, different character.")


def data_uri(path):
    img = Image.open(path).convert("RGBA")
    img = img.resize((img.width * 8, img.height * 8), Image.NEAREST)
    buf = io.BytesIO(); img.save(buf, "PNG")
    return "data:image/png;base64," + base64.b64encode(buf.getvalue()).decode()


def generate(npc, model, key, use_ref=True):
    ref = os.path.join(PH, f"ui_portrait_{npc}.png")
    prompt = prompt_for(npc, use_ref)
    content = [{"type": "text", "text": prompt}]
    if use_ref: content.append({"type": "image_url", "image_url": {"url": data_uri(ref)}})
    body = {"model": model, "modalities": ["image", "text"], "messages": [{"role": "user", "content": content}]}
    req = urllib.request.Request(URL, json.dumps(body).encode(), {"Authorization": "Bearer " + key, "Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(req, timeout=240) as r: resp = json.load(r)
    except urllib.error.HTTPError as e:
        sys.exit(f"{npc}: HTTP {e.code}: {e.read().decode(errors='replace')[:400]}")
    msg = resp["choices"][0]["message"]
    images = msg.get("images") or []
    if not images: sys.exit(f"{npc}: no image in the response: {str(msg)[:300]}")
    url = images[0]["image_url"]["url"]
    raw = base64.b64decode(url.split(",", 1)[1])
    out = os.path.join(GEN, f"expr_{npc}.png")
    open(out, "wb").write(raw)
    open(os.path.join(GEN, f"expr_{npc}_prompt.txt"), "w", encoding="utf-8").write(f"model: {model}\n\n{prompt}\n")
    im = Image.open(io.BytesIO(raw))
    print(f"{npc}: wrote {os.path.relpath(out, ROOT)} ({im.width}x{im.height})")


if __name__ == "__main__":
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    model = "google/gemini-2.5-flash-image"
    if "--model" in sys.argv: model = sys.argv[sys.argv.index("--model") + 1]
    key = load_key()
    for npc in args or ["wren", "hazel", "bram"]:
        generate(npc, model, key, use_ref="--no-ref" not in sys.argv)
