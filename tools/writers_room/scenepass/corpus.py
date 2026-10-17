#!/usr/bin/env python3
"""Builds the scene corpus for the all-scenes pass.

Writes, under tools/writers_room/drafts/scene_pass/:
  scenes.json      every story event (expanded), its beats, the dialogues it plays, who owns each text key
  ownership.json   key -> {owner, kind, scenes, dialogue}
  cards/<who>.md   the scene cards one villager reads: their own lines marked, everyone else's for context

Ownership: a say step or dialogue node belongs to its speaker; a choice label belongs to the speaker of the node that offers it;
a line with no speaker is the narrator's (the Director's).

    python3 tools/writers_room/scenepass/corpus.py [--en PATH]     # --en reads a working copy of en.json instead of the repo's
"""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from common import VILLAGERS, WORK, load_en, load_story  # noqa: E402


def stage_summary(step: dict) -> str | None:
    t = step["type"]
    a = step.get("actor")
    if t in ("place", "move"):
        return f"{t} {a} to ({step.get('x')},{step.get('y')})"
    if t == "face":
        return f"{a} faces {step.get('facing')}"
    if t == "anim":
        return f"{a} {step.get('name')}" + (" (background)" if step.get("async") else "")
    if t == "emote":
        return f"{a} emote {step.get('name')}"
    if t == "expression":
        return f"{a} expression {step.get('name')}"
    if t in ("music", "sfx", "lighting", "letterbox"):
        return f"{t} {step.get('name')}"
    if t == "camera":
        return f"camera {step.get('name')}" + (f" on {a}" if a else "")
    if t == "wait":
        return f"wait {step.get('seconds')}s"
    if t == "spawn":
        return f"spawn prop {step.get('name')} at ({step.get('x')},{step.get('y')})"
    if t == "despawn":
        return f"remove prop {step.get('id')}"
    if t in ("fadeout", "fadein"):
        return t
    if t == "parallel":
        return "together: " + "; ".join(filter(None, (stage_summary(c) for c in step.get("steps", []))))
    return None


def build(en: dict[str, str]) -> dict:
    S = load_story()
    scenes = {}
    own: dict[str, dict] = {}

    def claim(key, owner, kind, scene, dialogue=None):
        o = own.setdefault(key, {"owner": owner, "kind": kind, "scenes": [], "dialogue": dialogue})
        if scene not in o["scenes"]:
            o["scenes"].append(scene)

    def who(speaker):
        return speaker if speaker in VILLAGERS else "narrator"

    for eid, ev in S["events"].items():
        beats = []
        cast = {}
        dialogues = []
        for i, st in enumerate(ev["steps"]):
            t = st["type"]
            if t in ("place", "move") and st.get("actor"):
                cast.setdefault(st["actor"], (st.get("x"), st.get("y")))
            if t == "say":
                sp = st.get("speaker")
                claim(st["text"], who(sp), "say", eid)
                beats.append({"i": i, "type": "say", "speaker": sp or "-", "key": st["text"], "emote": st.get("emote"), "expression": st.get("expression"),
                              "condition": st.get("condition")})
            elif t == "dialogue":
                dialogues.append(st["dialogue"])
                beats.append({"i": i, "type": "dialogue", "dialogue": st["dialogue"]})
            else:
                s = stage_summary(st)
                if s:
                    beats.append({"i": i, "type": "stage", "text": s})
                if t == "effects":
                    beats.append({"i": i, "type": "effects", "effects": st["effects"]})
        scenes[eid] = {
            "id": eid, "file": S["source"].get("event:" + eid), "template": ev.get("_template"), "map": ev.get("map"), "trigger": ev.get("trigger"),
            "priority": ev.get("priority"), "condition": ev.get("condition"), "titleKey": ev.get("titleKey"), "tag": ev.get("tag"),
            "title": en.get(ev.get("titleKey") or "", None), "once": ev.get("once", True), "recheck": ev.get("recheck", False),
            "cast": cast, "beats": beats, "dialogues": dialogues,
        }

    dialogues = {}
    for eid, sc in scenes.items():
        for did in sc["dialogues"]:
            d = S["dialogues"].get(did)
            if d is None:
                continue
            nodes = []
            for n in d["nodes"]:
                owner = who(n.get("speaker"))
                if n.get("text"):
                    claim(n["text"], owner, "node", eid, did)
                chs = []
                for c in n.get("choices", []):
                    claim(c["text"], owner, "choice", eid, did)
                    chs.append({"key": c["text"], "next": c.get("next"), "effects": c.get("effects", []), "tone": c.get("tone"),
                                "condition": c.get("condition"), "default": c.get("default")})
                nodes.append({"id": n["id"], "speaker": n.get("speaker"), "key": n.get("text"), "effects": n.get("effects", []), "next": n.get("next"),
                              "condition": n.get("condition"), "expression": n.get("expression"), "emote": n.get("emote"), "choices": chs})
            dialogues[did] = {"id": did, "start": d.get("start"), "nodes": nodes, "scene": eid, "file": S["source"].get("dialogue:" + did)}
    # The narrator's lines (no speaker) go to the villager who speaks most in the scene, so someone in the room writes them.
    for eid, sc in scenes.items():
        tally = {}
        for b in sc["beats"]:
            if b["type"] == "say" and b["speaker"] in VILLAGERS:
                tally[b["speaker"]] = tally.get(b["speaker"], 0) + 1
        for did in sc["dialogues"]:
            for n in dialogues.get(did, {}).get("nodes", []):
                if n["speaker"] in VILLAGERS:
                    tally[n["speaker"]] = tally.get(n["speaker"], 0) + 1
        lead = max(tally, key=lambda k: (tally[k], -VILLAGERS.index(k))) if tally else None
        sc["lead"] = lead
        # beats may be added or cut only in hand-edited explicit events (never template scenes, never generated zz_fs files)
        sc["editable"] = sc["template"] is None and not (sc["file"] or "").startswith("zz_fs_")
    for key, o in own.items():
        if o["owner"] == "narrator":
            lead = scenes[o["scenes"][0]].get("lead")
            if lead:
                o["owner"] = lead
                o["narration"] = True
    return {"scenes": scenes, "dialogues": dialogues, "ownership": own}


def line(en, key, owned, label):
    text = en.get(key, "<MISSING>")
    mark = f"  ✏ `{key}`" if owned else ""
    return f"{label}: \"{text}\"{mark}"


def card(sc, data, en, me=None) -> str:
    own = data["ownership"]
    out = [f"### {sc['id']}" + (f" · \"{sc['title']}\"" if sc["title"] else "") + (f" · {sc['tag']}" if sc["tag"] else "")]
    kind = sc["template"] or "event"
    out.append(f"{kind} · {sc['file']} · map {sc['map']} · priority {sc['priority']}" + ("" if sc["once"] else " · repeatable"))
    out.append(f"plays when: `{sc['condition']}`")
    out.append("beats may be ADDED or CUT in this scene" if sc.get("editable") else "text-only scene: rewrite lines, do not add or cut beats")
    if sc["cast"]:
        out.append("placed: " + ", ".join(f"{a}{p}" for a, p in sc["cast"].items()))
    for b in sc["beats"]:
        if b["type"] == "stage":
            out.append(f"  · ({b['text']})")
        elif b["type"] == "effects":
            out.append(f"  · effects: {', '.join(b['effects'])}")
        elif b["type"] == "say":
            o = own[b["key"]]["owner"] == me if me else False
            narr = own[b["key"]].get("narration")
            cues = ", ".join(x for x in (b.get("emote") and f"emote {b['emote']}", b.get("expression") and f"expr {b['expression']}") if x)
            cond = f" [only if {b['condition']}]" if b.get("condition") else ""
            out.append("  " + line(en, b["key"], o, "[narration]" if narr else f"[{b['speaker']}]") + (f"  ({cues})" if cues else "") + cond)
        elif b["type"] == "dialogue":
            d = data["dialogues"].get(b["dialogue"])
            out.append(f"  ▶ DIALOGUE `{b['dialogue']}` (player-facing graph, plays here):")
            if not d:
                out.append("    (not found)")
                continue
            for n in d["nodes"]:
                o = n["key"] and own.get(n["key"], {}).get("owner") == me if me else False
                head = f"    {n['id']}" + (f" [{n['speaker']}]" if n["speaker"] else " [narration]")
                tail = []
                if n["effects"]:
                    tail.append("effects: " + ", ".join(n["effects"]))
                if n["next"]:
                    tail.append("→ " + n["next"])
                if n["condition"]:
                    tail.append(f"only if {n['condition']}")
                if n["expression"]:
                    tail.append("expr " + n["expression"])
                if n["emote"]:
                    tail.append("emote " + n["emote"])
                if n["key"]:
                    if own.get(n["key"], {}).get("narration"):
                        head = f"    {n['id']} [narration]"
                    out.append(head + ": " + line(en, n["key"], o, "").lstrip(": ").strip() + (f"   ({'; '.join(tail)})" if tail else ""))
                else:
                    out.append(head + (f"   ({'; '.join(tail)})" if tail else ""))
                for c in n["choices"]:
                    co = own.get(c["key"], {}).get("owner") == me if me else False
                    t = f" [{c['tone']}]" if c["tone"] else ""
                    ef = f"  effects: {', '.join(c['effects'])}" if c["effects"] else ""
                    out.append(f"       ? \"{en.get(c['key'], '<MISSING>')}\"{t} → {c['next']}{ef}" + (f"  ✏ `{c['key']}`" if co else ""))
    return "\n".join(out)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--en", type=Path)
    a = ap.parse_args()
    en = load_en(a.en) if a.en else load_en()
    data = build(en)
    WORK.mkdir(parents=True, exist_ok=True)
    (WORK / "cards").mkdir(exist_ok=True)
    (WORK / "scenes.json").write_text(json.dumps(data, indent=1, ensure_ascii=False) + "\n", encoding="utf-8")
    (WORK / "ownership.json").write_text(json.dumps(data["ownership"], indent=1, ensure_ascii=False) + "\n", encoding="utf-8")
    counts = {}
    for key, o in data["ownership"].items():
        counts.setdefault(o["owner"], {"say": 0, "node": 0, "choice": 0})[o["kind"]] += 1
    for v in VILLAGERS:
        mine = [sc for sc in data["scenes"].values() if any(data["ownership"][b["key"]]["owner"] == v for b in sc["beats"] if b["type"] == "say")
                or any(any(n["key"] and data["ownership"].get(n["key"], {}).get("owner") == v or any(data["ownership"].get(c["key"], {}).get("owner") == v for c in n["choices"]) for n in data["dialogues"][d]["nodes"])
                       for d in sc["dialogues"] if d in data["dialogues"])]
        c = counts.get(v, {})
        head = (f"# Scene cards for {v}\n\n{len(mine)} scenes. Your lines are marked ✏ with their text key; everything else is context. "
                f"You own {c.get('say', 0)} spoken lines in scenes, {c.get('node', 0)} dialogue lines and {c.get('choice', 0)} player choice labels.\n\n")
        (WORK / "cards" / f"{v}.md").write_text(head + "\n\n".join(card(sc, data, en, v) for sc in mine) + "\n", encoding="utf-8")
        print(f"{v:8} {len(mine):3} scenes  say {c.get('say', 0):3}  node {c.get('node', 0):3}  choice {c.get('choice', 0):3}")
    print("narrator", counts.get("narrator"))
    print(len(data["scenes"]), "scenes,", len(data["dialogues"]), "scene dialogues,", len(data["ownership"]), "owned keys")


if __name__ == "__main__":
    main()
