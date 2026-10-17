#!/usr/bin/env python3
"""Compiles the Round 3 horror variants into the Mythos overlay.

    python3 build_overlay.py [--dry]

Reads drafts/scene_pass/r3/*.json (variants, adds, cuts). Writes
  Resources/Mythos/mythos_scenes.json     overlay copies of every scene that has a horror treatment (+ dialogue copies)
  Resources/Localization/en.json          the overlay strings, key mythos.v.<original key>
  Resources/Story/*.json                  cozy originals get `&& !flag:event.<id>` appended to their condition (once-only scenes only)
Nothing in the core story data names horror: the overlay is data under Resources/Mythos/, which MythosContent loads, so a game
without the layer (or at horror off) never evaluates it. Idempotent: re-running rebuilds the same file.

Overlay semantics: id `<id>__m`, priority original + 1, condition `(<original>) && horror:1 [&& !flag:event.<id>]`, the same steps
with the spoken lines replaced by `{if <when>}variant{else}...cozy{/if}` chains (most specific first), added lines carrying their own
`condition`, cut lines carrying `!(<when>)`, and dialogues copied as `<dialogue>__m` when any of their lines has a variant.
"""
from __future__ import annotations

import argparse
import copy
import json
import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from common import MYTHOS, STORY, WORK, load_en, load_story, write_en  # noqa: E402
from apply_round import read_json_file, write_json_file  # noqa: E402

R3 = WORK / "r3"
OUT = MYTHOS / "mythos_scenes.json"
META_GUID = "7c2d4b1e5a9f4e08b36d1f0a8c5e2b97"


def chain(variants: list[dict], cozy: str) -> str:
    out = cozy
    for v in reversed(variants):
        out = "{if " + v["when"].strip() + "}" + v["text"] + "{else}" + out + "{/if}"
    return out


def load_r3():
    variants: dict[str, list] = {}
    adds, cuts = [], []
    for f in sorted(R3.glob("*.json")):
        if f.name.endswith(".lint.json"):
            continue
        d = json.loads(f.read_text(encoding="utf-8"))
        for k, vs in d.get("variants", {}).items():
            variants[k] = vs
        for a in d.get("adds", []):
            adds.append(dict(a, villager=d["villager"]))
        for c in d.get("cuts", []):
            cuts.append(dict(c, villager=d["villager"]))
    return variants, adds, cuts


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--dry", action="store_true")
    a = ap.parse_args()

    variants, adds, cuts = load_r3()
    en = load_en()
    S = load_story()
    own = json.loads((WORK / "ownership.json").read_text(encoding="utf-8"))

    new_strings: dict[str, str] = {}

    def vkey(k):
        return "mythos.v." + k

    def make_string(k):
        new_strings[vkey(k)] = chain(variants[k], en[k])
        return vkey(k)

    # which dialogues need a copy
    dlg_keys: dict[str, set] = {}
    for k in variants:
        o = own.get(k)
        if o and o.get("dialogue"):
            dlg_keys.setdefault(o["dialogue"], set()).add(k)
    dialogues_out = []
    for did, ks in sorted(dlg_keys.items()):
        d = copy.deepcopy(S["dialogues"][did])
        d["id"] = did + "__m"
        for n in d["nodes"]:
            if n.get("text") in ks:
                n["text"] = make_string(n["text"])
            for c in n.get("choices", []):
                if c.get("text") in ks:
                    c["text"] = make_string(c["text"])
        dialogues_out.append(d)

    # which scenes need an overlay
    scene_adds: dict[str, list] = {}
    for x in adds:
        scene_adds.setdefault(x["scene"], []).append(x)
    scene_cuts: dict[str, list] = {}
    for x in cuts:
        scene_cuts.setdefault(x["scene"], []).append(x)

    events_out = []
    guards: dict[str, str] = {}          # scene id -> original condition (for the cozy guard)
    for eid, ev in S["events"].items():
        steps = ev["steps"]
        says = {s["text"] for s in steps if s["type"] == "say"}
        dls = {s["dialogue"] for s in steps if s["type"] == "dialogue"}
        has_line = bool(says & set(variants))
        has_dlg = bool(dls & set(dlg_keys))
        if not (has_line or has_dlg or eid in scene_adds or eid in scene_cuts):
            continue
        o = copy.deepcopy({k: v for k, v in ev.items() if not k.startswith("_")})
        o["id"] = eid + "__m"
        o["priority"] = int(ev.get("priority", 0)) + 1
        once = ev.get("once", True)
        cond = ev.get("condition") or "true"
        o["condition"] = f"({cond}) && horror:1" + (f" && !flag:event.{eid}" if once else "")
        new_steps = []
        cut_map = {c["key"]: c for c in scene_cuts.get(eid, [])}
        adds_after: dict[str, list] = {}
        for ad in scene_adds.get(eid, []):
            adds_after.setdefault(ad["after"], []).append(ad)
        counter = 0

        def add_steps(anchor):
            nonlocal counter
            out = []
            for ad in adds_after.get(anchor, []):
                counter += 1
                key = f"mythos.scene.{eid}.{counter}"
                new_strings[key] = ad["text"]
                st = {"type": "say", "speaker": ad["villager"], "text": key, "condition": ad["when"]}
                if ad.get("emote"):
                    st["emote"] = ad["emote"]
                if ad.get("expression"):
                    st["expression"] = ad["expression"]
                out.append(st)
            return out

        new_steps += add_steps("START")
        for s in steps:
            s = copy.deepcopy(s)
            if s["type"] == "say":
                k = s["text"]
                if k in variants:
                    s["text"] = make_string(k)
                if k in cut_map:
                    w = cut_map[k]["when"].strip()
                    s["condition"] = f"({s['condition']}) && !({w})" if s.get("condition") else f"!({w})"
                new_steps.append(s)
                new_steps += add_steps(k)
                continue
            if s["type"] == "dialogue" and s["dialogue"] in dlg_keys:
                s["dialogue"] += "__m"
            new_steps.append(s)
        o["steps"] = new_steps
        events_out.append(o)
        if once and f"!flag:event.{eid}" not in cond:
            guards[eid] = cond

    # --- cozy guards
    guarded = 0
    edits: dict[str, tuple] = {}
    for eid, cond in guards.items():
        fname = S["source"]["event:" + eid]
        edits.setdefault(fname, []).append((eid, cond))
    for fname, items in edits.items():
        path = STORY / fname
        data, ind, nl = read_json_file(path)
        explicit = {e["id"]: e for e in data.get("events", [])}
        inst = {i["id"]: i for i in data.get("eventsFromTemplates", [])}
        for eid, cond in items:
            new = f"({cond}) && !flag:event.{eid}"
            if eid in explicit:
                explicit[eid]["condition"] = new
            elif eid in inst:
                inst[eid].setdefault("event", {})["condition"] = new
            else:
                raise SystemExit(f"{eid}: not found in {fname}")
            guarded += 1
        if not a.dry:
            write_json_file(path, data, ind, nl)

    out = {"dialogues": dialogues_out, "events": events_out}
    if not a.dry:
        OUT.write_text(json.dumps(out, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
        meta = OUT.with_name(OUT.name + ".meta")
        if not meta.exists():
            meta.write_text((MYTHOS / "mythos_intro.json.meta").read_text().replace("12b7e03e9668c5d4792a4c91f4eaae53", META_GUID), encoding="utf-8")
        for k in [k for k in en if k.startswith("mythos.v.") or k.startswith("mythos.scene.")]:
            del en[k]
        en.update(new_strings)
        write_en(en)
    print(f"{len(events_out)} overlay scenes, {len(dialogues_out)} dialogue copies, {len(new_strings)} strings, {guarded} cozy conditions guarded"
          f"{' (dry run)' if a.dry else ''}")


if __name__ == "__main__":
    main()
