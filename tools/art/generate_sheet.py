"""UI kit, HUD icons and effects (T-060): generates one sheet per set with Gemini 2.5 Flash Image through OpenRouter, then slices it into
sprites with the shared keying, scaling and palette steps of slice_sheets.py. New names only: a sprite that already exists in
Art/Placeholders is never overwritten (the UI kit replaces the placeholder frames by name only with --replace-ui).
Usage: python tools/art/generate_sheet.py <ui|hud|fx> [--slice-only] [--replace-ui]
Key: OPENROUTER_API_KEY in the environment or tools/art/.env (git-ignored). Provenance: Art/Generated/sheet_<set>.png and sheet_<set>_prompt.txt."""
import base64, io, json, os, sys, urllib.request, urllib.error
import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(__file__))
import slice_sheets as S
from generate_expressions import load_key, STYLE, URL, GEN, PH, ROOT

# name, what it looks like, target size
SETS = {
    "ui": dict(cols=5, rows=2, what="small user-interface pieces for a farming game, each a flat front-on square or rectangle (no perspective)",
               bottom=False, items=[
        ("ui_frame_panel", "a parchment-coloured panel with a rounded warm-brown wooden border and tiny corner rivets, square", (24, 24)),
        ("ui_frame_panel_dark", "the same panel but in dark brown wood with a darker parchment centre", (24, 24)),
        ("ui_slot", "an inventory slot: a small square with a tan inset and a brown wooden border", (18, 18)),
        ("ui_slot_selected", "the same inventory slot with a glowing golden border", (18, 18)),
        ("ui_slot_disabled", "the same inventory slot greyed out and faded", (18, 18)),
        ("ui_tooltip", "a small parchment tooltip bubble with a brown border", (24, 24)),
        ("ui_button_normal", "a wooden button, flat tan with a brown border", (24, 24)),
        ("ui_button_hover", "the same wooden button, lighter and warmer", (24, 24)),
        ("ui_button_pressed", "the same wooden button, darker and pressed in", (24, 24)),
        ("ui_button_disabled", "the same wooden button, greyed out", (24, 24)),
    ]),
    "hud": dict(cols=5, rows=5, what="small friendly icons with thick outlines, one clear symbol each", bottom=False, items=[
        ("hud_weather_sunny", "a smiling yellow sun", (16, 16)),
        ("hud_weather_rain", "a grey-blue rain cloud with three raindrops", (16, 16)),
        ("hud_weather_storm", "a dark cloud with a yellow lightning bolt", (16, 16)),
        ("hud_weather_snow", "a pale cloud with a white snowflake", (16, 16)),
        ("hud_weather_wind", "three curling teal wind swirls", (16, 16)),
        ("hud_weather_fog", "a soft grey fog bank", (16, 16)),
        ("hud_weather_festival", "a string of colourful bunting flags", (16, 16)),
        ("hud_season_spring", "a pink blossom with a green leaf", (16, 16)),
        ("hud_season_summer", "a bright orange sun with rays", (16, 16)),
        ("hud_season_fall", "an orange-red maple leaf", (16, 16)),
        ("hud_season_winter", "a blue-white snowflake", (16, 16)),
        ("hud_moon_new", "a dark round moon with a thin outline", (16, 16)),
        ("hud_moon_waxing", "a yellow crescent moon open to the left", (16, 16)),
        ("hud_moon_full", "a bright full yellow moon", (16, 16)),
        ("hud_moon_waning", "a yellow crescent moon open to the right", (16, 16)),
        ("hud_gold", "a shiny gold coin", (16, 16)),
        ("hud_clock_face", "a round cream pocket-watch face with two hands", (16, 16)),
        ("hud_energy_icon", "a green lightning bolt", (16, 16)),
        ("hud_health_icon", "a red heart with a white cross-shaped highlight", (16, 16)),
        ("hud_fatigue_icon", "a purple crescent moon with a small z", (16, 16)),
        ("hud_heart_empty", "an empty heart, outline only with a pale fill", (16, 16)),
        ("hud_heart_half", "a heart that is half red and half pale", (16, 16)),
        ("hud_heart_full", "a full red heart", (16, 16)),
        ("hud_gift", "a wrapped gift box with a ribbon bow", (16, 16)),
        ("hud_quest", "a rolled quest scroll tied with a ribbon", (16, 16)),
    ]),
    "fx": dict(cols=5, rows=4, what="small effect sprites, each a few simple shapes", bottom=False, items=[
        ("fx_rain_drop", "one thin blue-grey rain streak, diagonal", (16, 16)),
        ("fx_rain_splash", "a tiny blue ring splash of a raindrop", (16, 16)),
        ("fx_snowflake", "a small white six-armed snowflake", (16, 16)),
        ("fx_wind_leaf", "a small green leaf blowing in the wind", (16, 16)),
        ("fx_petal", "a single pink blossom petal", (16, 16)),
        ("fx_autumn_leaf", "a single orange autumn leaf", (16, 16)),
        ("fx_dust", "a small puff of tan dust", (16, 16)),
        ("fx_sparkle", "a small four-pointed yellow-white sparkle", (16, 16)),
        ("fx_puff_0", "a tiny soft white smoke puff", (16, 16)),
        ("fx_puff_1", "a medium soft white-grey smoke puff", (16, 16)),
        ("fx_puff_2", "a large faint grey smoke puff", (16, 16)),
        ("fx_dig_dirt", "three brown dirt clumps flying up", (16, 16)),
        ("fx_water_drop", "three blue water droplets flying out", (16, 16)),
        ("fx_chop_chip", "three small wood chips flying out", (16, 16)),
        ("fx_ore_spark", "a burst of orange-yellow sparks", (16, 16)),
        ("fx_harvest_pop", "a burst of small green leaves and yellow dots", (16, 16)),
        ("fx_level_up", "a golden star burst with a few sparkles", (16, 16)),
        ("fx_heart_pop", "a small pink heart", (16, 16)),
        ("fx_coin_fly", "a spinning gold coin seen edge-on, slightly tilted", (16, 16)),
        ("fx_hit_star", "a white-yellow impact star with pointed spikes", (16, 16)),
    ]),
    "keep": dict(cols=4, rows=2, what="small keepsake items that a friend gives as a gift, one object each, seen slightly from above", bottom=False, items=[
        ("item_prop_sock", "a single knitted wool sock in warm red and cream stripes", (16, 16)),
        ("item_prop_hat", "a small knitted wool hat in teal with a cream bobble on top", (16, 16)),
        ("item_prop_carving", "a small hand-carved wooden bird figurine in honey-brown wood", (16, 16)),
        ("item_prop_charm", "a lucky charm: a small gold four-leaf clover pendant on a short brown cord", (16, 16)),
        ("item_prop_feather", "a bundle of a grey-brown feather, a short stick and a bit of twine tied together", (16, 16)),
        ("item_prop_ribbon", "a blue prize rosette ribbon with two hanging tails and a gold centre", (16, 16)),
        ("item_prop_almanac", "a thick farming almanac book with a green cover and a small golden sprout on the front", (16, 16)),
        ("item_prop_fieldguide", "a slim field guide book with a brown cover and a small red mushroom on the front", (16, 16)),
    ]),
    "gifts": dict(cols=4, rows=2, what="small handmade gifts that villagers promise to make for a friend, one object each, seen slightly from above", bottom=False, items=[
        ("item_prop_anvil", "a tiny iron anvil, a desk ornament, dark grey with a lighter top", (16, 16)),
        ("item_prop_clasp", "a small hand-forged iron clasp, a curled hook-and-loop fastener in dark grey", (16, 16)),
        ("item_prop_horseshoe", "a small hand-forged iron horseshoe tied with a short red cord", (16, 16)),
        ("item_prop_pinecone", "a single brown pinecone with layered scales", (16, 16)),
        ("item_prop_drawing", "a small torn notebook page with a tiny pencil sketch of a green mushroom, cream paper", (16, 16)),
        ("item_prop_salve", "a small round tin of salve with a cream lid and a dab of green balm", (16, 16)),
        ("item_prop_scarf", "a folded knitted scarf in many coloured stripes, with fringe at the end", (16, 16)),
        ("item_prop_longbook", "a very long and thick old book with a dusty blue cover and a gold clasp", (16, 16)),
    ]),
    "places": dict(cols=3, rows=1, what="small cozy furnishings for a village interior and shore, one object each, front view slightly from above", bottom=False, items=[
        ("obj_fishing_rock", "a smooth flat grey boulder worn comfortable to sit on, with a folded cream blanket on top", (16, 16)),
        ("obj_cat_door", "a very small arched wooden cat door set in a tiny wooden frame, with a little brass key hanging beside it", (16, 16)),
        ("obj_couch", "a small cosy teal couch with a patchwork blanket draped over one arm", (16, 16)),
    ]),
    "blight": dict(cols=3, rows=1, what="blighted, withered garden plants seen from the front, each a single plant growing from a small mound of soil", bottom=False, items=[
        ("crop_blight_0", "a tiny shrivelled seedling, two limp grey-brown leaves drooping from a thin bent stem", (16, 16)),
        ("crop_blight_1", "a half-grown plant, wilted and sickly: grey-green leaves curled and drooping, a dark purple-black stain spreading on the stem and leaves", (16, 16)),
        ("crop_blight_2", "a tall dead plant: a bare dry brown stalk bent over, a few black curled leaves and one shrivelled grey pod hanging, purple-black rot at its base", (16, 16)),
    ]),
    "wide": dict(cols=3, rows=1, what="larger shop furniture for a cozy farming village, each seen from the front and slightly above, filling its frame", bottom=False, items=[
        ("obj_counter_wide", "a long wooden shop counter, twice as wide as it is tall: a warm honey-brown wooden front with a lighter cream countertop, a small brass bell and a little scale on top", (32, 16)),
        ("obj_shipping_bin", "a big wooden shipping bin as wide as it is tall: a sturdy open-topped crate with slatted sides, iron corners, a hinged lid propped open and a few vegetables peeking out", (32, 32)),
        ("obj_stall_wide", "a market stall twice as wide as it is tall: a wooden table with a striped red-and-cream awning, baskets of fish and goods on the front", (32, 16)),
    ]),
    "mallet": dict(cols=1, rows=1, what="a single tool for a cozy farming game", bottom=False, items=[
        ("item_tool_hammer", "a builder's wooden mallet: a chunky honey-brown wooden head with two dark iron bands on a short brown handle, slightly tilted", (16, 16)),
    ]),
    "dull": dict(cols=1, rows=1, what="a single book for a cozy village library", bottom=False, items=[
        ("item_prop_dullbook", "a plain, thick, very dull book with a grey-brown cloth cover, a faded label and a bent corner, slightly tilted", (16, 16)),
    ]),
    "extra": dict(cols=5, rows=3, what="small game sprites of different shapes, one clear object each", bottom=False, items=[
        ("fx_puff_2", "a large faint grey-white smoke cloud puff", (16, 16)),
        ("fx_ore_spark", "a burst of bright orange and yellow sparks flying out from a centre point", (16, 16)),
        ("fx_petal", "a single pink blossom petal, a simple teardrop shape with one soft fold", (16, 16)),
        ("hud_weather_festival", "a string of colourful bunting: four triangular flags (red, yellow, green, blue) hanging in a shallow curve from a string, drawn large and filling the cell", (16, 16)),
        ("ui_dialogue_box", "a wide parchment dialogue box with a warm brown wooden border and a ribbon edge, wider than tall", (64, 24)),
        ("ui_speaker_plate", "a small wide name plate: a cream label with a brown wooden border and a tiny scroll end on each side", (32, 12)),
        ("hud_bar_energy_frame", "an empty horizontal bar frame: a long thin brown wooden rounded rectangle with a dark empty inside", (48, 16)),
        ("hud_bar_energy_fill", "a long thin horizontal bar filled solid bright green with a lighter top highlight, rounded ends", (48, 16)),
        ("hud_bar_health_frame", "an empty horizontal bar frame: a long thin brown wooden rounded rectangle with a dark empty inside", (48, 16)),
        ("hud_bar_health_fill", "a long thin horizontal bar filled solid red with a lighter top highlight, rounded ends", (48, 16)),
        ("hud_bar_fatigue_frame", "an empty horizontal bar frame: a long thin brown wooden rounded rectangle with a dark empty inside", (48, 16)),
        ("hud_bar_fatigue_fill", "a long thin horizontal bar filled solid purple with a lighter top highlight, rounded ends", (48, 16)),
    ]),
}

# The model does not always draw the sprites in the order asked for, so a sheet that was looked at gets a hand-checked map: name -> (detected
# sprite number counted row by row from 1, which part of it: whole/top/bottom). Sheets without a map are sliced in the order asked for.
ASSIGN = {
    "hud": {
        "hud_weather_sunny": (1, "whole"), "hud_weather_rain": (2, "whole"), "hud_weather_storm": (3, "whole"), "hud_weather_snow": (4, "whole"),
        "hud_weather_wind": (5, "whole"), "hud_weather_festival": (6, "top"), "hud_weather_fog": (6, "bottom"),
        "hud_season_spring": (7, "whole"), "hud_season_summer": (8, "whole"), "hud_season_fall": (9, "whole"), "hud_season_winter": (10, "whole"),
        "hud_moon_new": (11, "whole"), "hud_moon_waning": (12, "whole"), "hud_moon_full": (13, "whole"), "hud_moon_waxing": (14, "whole"),
        "hud_gold": (16, "whole"), "hud_clock_face": (17, "whole"), "hud_energy_icon": (18, "whole"), "hud_health_icon": (19, "whole"),
        "hud_fatigue_icon": (20, "whole"), "hud_heart_empty": (21, "whole"), "hud_heart_half": (22, "whole"),
        "hud_gift": (24, "whole"), "hud_quest": (25, "whole"),
    },
}

ASSIGN["ui"] = {
    "ui_frame_panel": (1, "whole"), "ui_frame_panel_dark": (2, "whole"), "ui_slot": (3, "whole"), "ui_slot_selected": (4, "whole"),
    "ui_slot_disabled": (11, "whole"), "ui_tooltip": (8, "whole"), "ui_button_normal": (12, "whole"), "ui_button_hover": (10, "whole"),
    "ui_button_pressed": (13, "whole"), "ui_button_disabled": (14, "whole"),
}
ASSIGN["fx"] = {
    "fx_rain_drop": (2, "whole"), "fx_rain_splash": (4, "whole"), "fx_snowflake": (5, "whole"), "fx_wind_leaf": (6, "whole"),
    "fx_petal": (7, "whole"), "fx_autumn_leaf": (8, "whole"), "fx_dust": (9, "whole"), "fx_sparkle": (11, "whole"), "fx_puff_0": (12, "whole"),
    "fx_puff_1": (10, "whole"), "fx_water_drop": (13, "whole"), "fx_dig_dirt": (15, "whole"), "fx_chop_chip": (16, "whole"),
    "fx_harvest_pop": (23, "whole"), "fx_level_up": (27, "whole"), "fx_heart_pop": (25, "whole"), "fx_coin_fly": (26, "whole"), "fx_hit_star": (28, "whole"),
}

ASSIGN["extra"] = {
    "fx_puff_2": (1, "whole"), "fx_ore_spark": (2, "whole"), "hud_weather_festival": (4, "whole"), "ui_dialogue_box": (7, "whole"),
    "ui_speaker_plate": (5, "whole"), "hud_bar_energy_frame": (8, "whole"), "hud_bar_energy_fill": (9, "whole"), "hud_bar_health_frame": (10, "whole"),
    "hud_bar_health_fill": (12, "whole"), "hud_bar_fatigue_frame": (11, "whole"), "hud_bar_fatigue_fill": (13, "whole"),
}

THIN = {"fx_ore_spark"}
REPLACE_UI = False
OVERWRITE = {"hud_weather_festival"} if "extra" in sys.argv else set()      # the weak first try is replaced


def prompt(spec):
    items = spec["items"]
    names = ", ".join(f"{i + 1}. {d}" for i, (_, d, _) in enumerate(items))
    return (STYLE + f"\n\nOn a perfectly flat solid pure magenta (#FF00FF) background, draw {len(items)} separate {spec['what']}, arranged in a grid of "
            f"{spec['cols']} columns and {spec['rows']} rows. Each is centred in its own equal-sized cell with a wide empty magenta margin around it and "
            "never touches another or the image edge. All share the same scale, viewpoint and style and read clearly at a tiny size: bold simple "
            "shapes, few colours. Left to right, top to bottom: " + names + ".\n"
            "Absolutely no text, letters, numbers, labels, captions, borders or boxes around the cells, watermarks or ground shadows: they float on "
            "plain magenta. No photorealism, no 3D, no black outlines, no gore, no scary faces.")


def generate(key, spec, sheet, model="google/gemini-2.5-flash-image"):
    body = {"model": model, "modalities": ["image", "text"], "messages": [{"role": "user", "content": [{"type": "text", "text": prompt(spec)}]}]}
    req = urllib.request.Request(URL, json.dumps(body).encode(), {"Authorization": "Bearer " + key, "Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(req, timeout=240) as r: resp = json.load(r)
    except urllib.error.HTTPError as e:
        sys.exit(f"HTTP {e.code}: {e.read().decode(errors='replace')[:400]}")
    images = resp["choices"][0]["message"].get("images") or []
    if not images: sys.exit("no image in the response")
    raw = base64.b64decode(images[0]["image_url"]["url"].split(",", 1)[1])
    open(os.path.join(GEN, sheet + ".png"), "wb").write(raw)
    open(os.path.join(GEN, sheet + "_prompt.txt"), "w", encoding="utf-8").write(f"model: {model}\n\n{prompt(spec)}\n")
    print("sheet written", Image.open(io.BytesIO(raw)).size)


def full_heart(half):
    """A full heart from the half heart: the pale half takes the red of the other half (the outline stays)."""
    a = half.copy()
    opaque = a[:, :, 3] > 0
    reds = a[opaque & (a[:, :, 0] > a[:, :, 2] + 60) & (a[:, :, 1] < 110)]
    if len(reds) == 0: return a
    red = np.median(reds[:, :3], axis=0).astype(np.uint8)
    pale = opaque & (a[:, :, 0] > 200) & (a[:, :, 1] > 170) & (a[:, :, 2] > 120)
    a[pale, :3] = red
    return a


def slice_sheet(spec, sheet, tag):
    items = spec["items"]
    img = Image.open(os.path.join(GEN, sheet + ".png")).convert("RGB")
    rgb, fg = S.key_background(img)
    r, g, b = rgb[:, :, 0], rgb[:, :, 1], rgb[:, :, 2]
    fg = fg & ~((r - g > 50) & (b - g > 35) & (abs(r - b) < 90))
    raw_fg = fg
    fg = S.clean_mask(fg)
    rows = S.segment(fg, spec["rows"], dilate=7)
    boxes = [bx for row in rows for bx in row]
    os.makedirs(os.path.join(ROOT, "Builds"), exist_ok=True)
    S.index_preview(img, rows, os.path.join(ROOT, "Builds", f"idx_{tag}.png"))
    if len(boxes) != len(items) and tag not in ASSIGN:
        sys.exit(f"found {len(boxes)} sprites, expected {len(items)} (see Builds/idx_{tag}.png)")
    assign = ASSIGN.get(tag)
    wanted = [(n, sz, *assign[n]) for n, _, sz in items if n in assign] if assign else [(n, sz, i + 1, "whole") for i, (n, _, sz) in enumerate(items)]
    names = [w[0] for w in wanted]
    sprites = []
    for name, (tw, th), number, part in wanted:
        x0, y0, x1, y1 = boxes[number - 1]
        if part == "top": y1 = y0 + (y1 - y0) // 2 - 4
        elif part == "bottom": y0 = y0 + (y1 - y0) // 2 + 4
        c, m = rgb[y0:y1, x0:x1], (raw_fg if name in THIN else fg)[y0:y1, x0:x1]       # thin rays do not survive the speck filter
        ys, xs = np.where(m)
        if len(xs):                                                      # trim to what is drawn (a half may have empty margin)
            c, m = c[ys.min():ys.max() + 1, xs.min():xs.max() + 1], m[ys.min():ys.max() + 1, xs.min():xs.max() + 1]
        if name.startswith(("ui_", "hud_bar")):                          # frames fill their cell so that they can be 9-sliced
            nw, nh = tw, th
        else:
            scale = min((tw - 2) / c.shape[1], (th - 2) / c.shape[0])
            nw, nh = max(1, round(c.shape[1] * scale)), max(1, round(c.shape[0] * scale))
        sp = S.resize_masked(c, m, (nw, nh))
        canvas = np.zeros((th, tw, 4), np.uint8)
        canvas[(th - nh) // 2:(th - nh) // 2 + nh, (tw - nw) // 2:(tw - nw) // 2 + nw] = sp
        sprites.append(canvas)
    out = S.quantize(sprites)
    cell = 8
    pv = Image.new("RGBA", (spec["cols"] * (24 * cell + 8), spec["rows"] * (24 * cell + 8)), (70, 60, 70, 255))
    wrote = []
    sizes = {n: sz for n, _, sz in items}
    out = list(out)
    if tag == "hud" and "hud_heart_half" in names:
        out.append(full_heart(out[names.index("hud_heart_half")])); names.append("hud_heart_full")
    for i, (name, a) in enumerate(zip(names, out)):
        tw, th = sizes.get(name, (16, 16))
        im = Image.fromarray(a, "RGBA")
        path = os.path.join(PH, name + ".png")
        existing = os.path.exists(path)
        if not existing or (tag == "ui" and REPLACE_UI) or name in OVERWRITE:
            im.save(path)
            wrote.append(name)
        big = im.resize((tw * cell, th * cell), Image.NEAREST)
        pv.paste(big, ((i % spec["cols"]) * (24 * cell + 8) + 4, (i // spec["cols"]) * (24 * cell + 8) + 4), big)
    pv.save(os.path.join(ROOT, "Builds", f"preview_{tag}.png"))
    print("wrote", ", ".join(wrote) if wrote else "nothing (all names exist)")


if __name__ == "__main__":
    tag = sys.argv[1]
    REPLACE_UI = "--replace-ui" in sys.argv
    sheet = "sheet_" + tag
    os.makedirs(GEN, exist_ok=True)
    if "--slice-only" not in sys.argv: generate(load_key(), SETS[tag], sheet)
    slice_sheet(SETS[tag], sheet, tag)
