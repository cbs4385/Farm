"""Builds tools/art/manifest.json: one entry per sprite sheet to generate (prompt, grid, target size, cell names).
Reuses the descriptive tables of the prompt generator (docs/art_prompts) so wording stays in one place.
Run: python tools/art/build_manifest.py"""
import json, os, glob, re

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
PH = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")
names = sorted(os.path.basename(p)[:-4] for p in glob.glob(os.path.join(PH, "*.png")))
def pick(pfx): return [n for n in names if n.startswith(pfx)]

# Reuse the description tables from the prompt generator without letting it rewrite the docs.
src = open(os.path.join(ROOT, "..", "..") if False else os.path.join(os.path.dirname(__file__), "prompt_tables.py"), encoding="utf-8").read()
ns = {}
exec(src, ns)
tile_desc, cropd, trees, fd, fish, fo, ar, pr, rs, ms, tooldesc, objd, npc, an, mon = (ns[k] for k in
    ["tile_desc","cropd","trees","fd","fish","fo","ar","pr","rs","ms","tooldesc","objd","npc","an","mon"])

PALETTE = ("Warm palette only: cream, butter yellow, honey, terracotta, rosewood, moss green, leaf green, deep pine, sky teal, dusk blue, lavender, "
           "bark brown, soil brown, warm stone grey; one-pixel warm dark brown outlines (never black); light from the upper left; two soft tones per colour.")
STYLE = ("Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter storybook view, soft friendly rounded chunky shapes, "
         "cheerful calm nostalgic mood, crisp hard pixel edges, no anti-aliasing, no gradients, no blur. " + PALETTE)
AVOID = ("Absolutely no text, letters, numbers, labels, captions, borders, frames, watermarks or ground shadows. No photorealism, no 3D, no black outlines, no gore, no scary faces.")

def sprite_prompt(cols, rows, cells, extra=""):
    n = sum(1 for c in cells if c)
    return (f"{STYLE}\n\nOn a perfectly flat solid pure magenta (#FF00FF) background, draw {n} separate sprites arranged in a perfect grid of {cols} columns by {rows} rows "
            f"(read left to right, top to bottom). Each sprite is centred in its own equal-sized cell with a wide empty magenta margin around it and never touches another sprite or the image edge. "
            f"All sprites share the same scale, viewpoint and style. {extra}\n\nContent:\n" + "\n".join(f"{i+1}. {c}" for i, c in enumerate(cells) if c) + f"\n\n{AVOID}")

def tile_prompt(cols, rows, cells, extra=""):
    return (f"{STYLE}\n\nDraw exactly {cols*rows} square flat top-down seamlessly tileable textures in a perfect {cols} by {rows} grid that fills the entire image edge to edge. "
            f"Every tile is the same size and touches its neighbours directly with NO gaps, NO borders, NO outlines around tiles. {extra}\nLeft to right, top to bottom:\n" +
            "\n".join(f"{i+1}. {c}" for i, c in enumerate(cells)) + f"\n\n{AVOID}")

sheets = []
def add(sid, kind, cols, rows, target, cells_names, cells_desc, anchor="center", group="none", extra="", outdir="Placeholders"):
    p = tile_prompt(cols, rows, cells_desc, extra) if kind == "tile" else sprite_prompt(cols, rows, cells_desc, extra)
    sheets.append(dict(id=sid, kind=kind, cols=cols, rows=rows, target=target, cells=cells_names, anchor=anchor, group=group, prompt=p, outdir=outdir))

# ---- tiles (16 distinct per sheet; the three extra are seasonal grass for later use) -----------------------------
tl = pick("tile_")
order = ["tile_grass","tile_dirt","tile_path","tile_sand","tile_cobble","tile_forest","tile_tilled","tile_tilled_watered","tile_wall","tile_roof","tile_floor_wood","tile_water","tile_door"]
tl = [t for t in tl if t in order]
extra_t = [("tile_grass_summer","deep lush summer grass with tiny yellow flowers"),("tile_grass_fall","autumn grass in rust and gold with fallen orange leaves"),("tile_grass_winter","winter grass under soft snow with a few green tufts")]
add("tiles", "tile", 4, 4, (16,16), order + [e[0] for e in extra_t],
    [f"{t.replace('tile_','').replace('_',' ')}: {tile_desc[t]}" for t in order] + [f"{n.replace('tile_','').replace('_',' ')}: {d}" for n,d in extra_t],
    extra="Make every tile clearly different from every other (path is pale cream-beige, sand is golden yellow, dirt is brown, tilled soil is dark brown furrows, watered soil is darker and wet).")

# ---- characters: 4 characters per sheet, row = character, columns = down, up, left, right ------------------------
chars = [("player","the player farmer: round straw hat with a blue band, rolled-sleeve blue work shirt, brown trousers, sturdy boots, friendly face"),
         ("npc_generic","a generic villager in an orange tunic and brown trousers, friendly face")] + \
        [(f"npc_{k}", f"{k.capitalize()}, the {role}: {look}") for k,(role,look,note) in npc.items()]
for i in range(0, len(chars), 2):
    grp = chars[i:i+2]
    cells = []; desc = []
    for cid, d in grp:
        for dirn, text in zip(("down","up","left","right"), ("facing the viewer","seen from behind, facing away","in left-facing side profile","in right-facing side profile")):
            cells.append(f"{cid}_idle_{dirn}")
            desc.append(f"{d.split(':')[0]} {text}")
    rowtxt = "\n".join(f"Row {r+1} is one single character, drawn four times (same outfit, same colours, same proportions): {d}" for r,(cid,d) in enumerate(grp))
    add(f"chr_{i//2+1}", "sprite", 4, len(grp), (16,32), cells, desc, anchor="bottom", group="row",
        extra=f"Chibi proportions (head about 40 percent of the height), standing idle, small full-body characters taller than wide. Columns are: facing the viewer, facing away, left profile, right profile.\n{rowtxt}")

# ---- portraits (32x32, backdrop included) ----------------------------------------------------------------------------
pk = list(npc)
add("portraits", "tile", 4, 3, (32,32), [f"ui_portrait_{k}" for k in pk],
    [f"{k.capitalize()}, the {npc[k][0]}: {npc[k][1]}; head and shoulders, three-quarter view, soft warm plain colour backdrop" for k in pk],
    extra="Each tile is one portrait filling its square completely with a plain soft colour backdrop (no frame).")

# ---- crops: 4 crops per sheet, 6 stage columns --------------------------------------------------------------------
allcrops = sorted({re.match(r"crop_(.+)_\d+$", n).group(1) for n in pick("crop_")})
stages = {c: sorted(int(re.match(r".*_(\d+)$", n).group(1)) for n in pick(f"crop_{c}_")) for c in allcrops}
horror_desc = {"nightbloom":"a plant with pale violet flowers that close at night, faintly glowing","hollowroot":"grey-green leaves over a pale knobbly root with a dark hollow",
 "ashfruit":"a soft ash-grey bush with glowing orange cinder fruit","mutant_root":"an ordinary root vegetable with a second tiny curly root and oddly pink skin",
 "mutant_gourd":"a gourd with swirling stripes and an extra bump","mutant_leaf":"a leafy plant whose leaves are two different colours"}
def cdesc(c):
    if c in cropd: return cropd[c]
    if c.startswith("tree_"): return f"a small {c[5:]} tree with " + trees[c[5:]]
    return horror_desc[c]
for i in range(0, len(allcrops), 2):
    grp = allcrops[i:i+2]
    cells = []; desc = []
    for c in grp:
        k = len(stages[c])
        for s in range(6):
            if s < k:
                cells.append(f"crop_{c}_{s}"); desc.append(f"{c.replace('_',' ')} stage {s+1} of {k}")
            else:
                cells.append(None); desc.append(None)
    rowtxt = "\n".join(f"Row {r+1}: {c.replace('_',' ')}: {cdesc(c)}. Exactly 6 growth stages left to right, one clear sprite per stage with wide gaps between them: the first is a freshly planted mound with a tiny sprout, then taller and fuller, the last is fully grown, ripe and ready to harvest." for r,c in enumerate(grp))
    add(f"crp_{i//2+1}", "sprite", 6, len(grp), (16,16), cells, [d for d in desc if d] and [d if d else None for d in desc], anchor="bottom", group="row",
        extra=f"Each row is the same plant growing over time; each sprite includes its small mound of tilled soil at the bottom. Later stages are larger than earlier ones in the same row.\n{rowtxt}")

# ---- items (icons) ---------------------------------------------------------------------------------------------------
def hum(n, pfx): return n[len(pfx):].replace("_", " ")
def icons(sid, items, descf, cols=5):
    for k in range(0, len(items), 20):
        chunk = items[k:k+20]
        rows = (len(chunk)+cols-1)//cols
        cells = chunk + [None]*(rows*cols-len(chunk))
        d = [descf(n) for n in chunk] + [None]*(rows*cols-len(chunk))
        add(f"{sid}{k//20+1}", "sprite", cols, rows, (16,16), cells, d, anchor="center",
            extra="Each sprite is one small inventory icon: a single object viewed slightly from above, chunky simple readable silhouette, centred.")
seeds = pick("item_seed_"); icons("seeds", seeds, lambda n: ("a small paper seed packet with a cream label showing a tiny picture of the " + hum(n,'item_seed_').replace('tree ','') + " plant" if not n.endswith("generic") else "a plain paper seed packet"))
crs = pick("item_crop_"); icons("harvest", crs, lambda n: ("the freshly harvested " + hum(n,'item_crop_').replace('tree ','') + ", plump and bright") if not n.endswith("generic") else "a generic vegetable")
fam = [("forage","item_forage_",fd),("fish","item_fish_",fish),("food","item_food_",fo),("artisan","item_artisan_",ar),("product","item_product_",pr),("resource","item_resource_",rs),("machine","item_machine_",ms),("tool","item_tool_",tooldesc)]
pool = []
for _, pfx, dd in fam:
    for n in pick(pfx): pool.append((n, dd.get(hum(n,pfx), hum(n,pfx)) + (" (fish drawn flat in left-facing side view)" if pfx=="item_fish_" else "")))
for n in pick("item_animal_"): pool.append((n, f"a small cute {hum(n,'item_animal_')} portrait icon"))
for n in pick("item_fertilizer_"): pool.append((n, "a small cloth sack of " + hum(n,'item_fertilizer_').replace("quality","quality-boosting").replace("speed","growth-speeding") + " fertilizer tied with string"))
mythos = {"item_mythos_relic_seal":"an ash-grey stone seal disc with a carved knot","item_mythos_relic_bell":"a dull bronze handbell","item_mythos_relic_thread":"a coil of green thread"}
for n in pick("item_mythos_"): pool.append((n, mythos[n]))
pool_names = [p[0] for p in pool]; pdesc = dict(pool)
icons("goods", pool_names, lambda n: pdesc[n])

# ---- world objects ----------------------------------------------------------------------------------------------------
objs = pick("obj_")
od = dict(objd); od.update({f"obj_{k}": v for k,v in fd.items()})
od.update({"obj_altar":"a low flat altar of old grey stone carved with a knot symbol, with a faint gold glow","obj_stone":"a standing carved stone with moss","obj_relic":"a small relic (ash disc, bell and thread) lying on mossy ground"})
for k in range(0, len(objs), 16):
    chunk = objs[k:k+16]
    rows = (len(chunk)+3)//4
    cells = chunk + [None]*(rows*4-len(chunk))
    d = [od.get(n, n[4:].replace("_"," ")) for n in chunk] + [None]*(rows*4-len(chunk))
    add(f"objects{k//16+1}", "sprite", 4, rows, (16,16), cells, d, anchor="bottom",
        extra="Each sprite is one small world object sitting on the ground, drawn with its base at the bottom of the sprite, charming and readable.")

add("extras", "sprite", 3, 1, (16,16), ["item_crop_cucumber","item_crop_radish","obj_counter"],
    ["the freshly harvested cucumber, a long glossy dark green cucumber with a few small bumps", "the freshly harvested radish, a round pink-red radish with a white tip and green leaves", "a wooden shop counter segment with a cloth runner on top"],
    anchor="center", extra="Three separate objects with wide gaps; each is one chunky readable icon.")
json.dump(sheets, open(os.path.join(os.path.dirname(__file__), "manifest.json"), "w", encoding="utf-8"), indent=1)
print(len(sheets), "sheets;", sum(sum(1 for c in s["cells"] if c) for s in sheets), "sprites")
for s in sheets: print(s["id"], s["cols"], "x", s["rows"])
