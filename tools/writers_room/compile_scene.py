#!/usr/bin/env python3
"""Compiles a writers'-room scene draft into the game's story formats.

Input: a `*.scene.json` draft (see tools/writers_room/README.md for the shape;
`lint_scene.py` checks it). Output, next to the draft unless --out says otherwise:

    <id>.events.json   an `events` array in the shape of Resources/Story/spec_scenes.json
    <id>.strings.json  the `event.<id>.<n>` strings to merge into Localization/en.json
    <id>.fscript       the choice dialogues (FScript; Farm > Compile Story makes their keys)

Nothing is written into Assets/ by this script. Merging is a deliberate step
(see the README), because shipped ids are save-compatibility promises.

    python3 tools/writers_room/compile_scene.py drafts/hall_dance.scene.json
"""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path


def compile_draft(d: dict) -> tuple[dict, dict, str]:
    sid = d["id"]
    steps: list[dict] = []
    strings: dict[str, str] = {}
    fscript: list[str] = [f"# Generated from {sid}.scene.json by tools/writers_room/compile_scene.py; edit the draft, not this.", ""]
    n = 0

    def key(text: str) -> str:
        nonlocal n
        k = f"event.{sid}.{n}"
        strings[k] = text
        n += 1
        return k

    frame = d.get("frame", {})
    if frame.get("letterbox"):
        steps.append({"type": "letterbox", "name": "on", "seconds": 0.6})
    if frame.get("lighting"):
        steps.append({"type": "lighting", "name": frame["lighting"], "seconds": 1.0})

    for actor, pos in (d.get("cast") or {}).items():
        if actor == "player":
            steps.append({"type": "move", "actor": "player", "x": pos["x"], "y": pos["y"]})
        else:
            steps.append({"type": "place", "actor": actor, "x": pos["x"], "y": pos["y"]})
        if pos.get("facing"):
            steps.append({"type": "face", "actor": actor, "facing": pos["facing"]})

    choice_no = 0
    for beat in d.get("beats", []):
        if "say" in beat:
            sp = beat["say"]
            if beat.get("face"):
                steps.append({"type": "face", "actor": sp, "facing": beat["face"]})
            if beat.get("anim"):
                steps.append({"type": "anim", "actor": sp, "name": beat["anim"], "seconds": beat.get("seconds", 1.2), "async": True})
            step = {"type": "say", "speaker": sp, "text": key(beat["text"])}
            if beat.get("emote"):
                step["emote"] = beat["emote"]
            if beat.get("expr"):
                step["expression"] = beat["expr"]
            steps.append(step)
        elif "narrate" in beat:
            steps.append({"type": "say", "text": key(beat["narrate"])})
        elif "stage" in beat:
            steps.extend(beat["stage"])
        elif "choice" in beat:
            c = beat["choice"]
            choice_no += 1
            did = f"{sid}.choice{choice_no}"
            sp = c.get("say", "-")
            fscript.append(f"dialogue {did}")
            if c.get("text"):
                fscript.append(f"n0 {sp}: {c['text']}")
            else:
                fscript.append("n0 -:")
            for j, o in enumerate(c.get("options", [])):
                tail = f" | flag:{o['flag']}" if o.get("flag") else ""
                fscript.append(f"  ? {o['text']} => r{j}{tail}")
                if o.get("tone"):
                    fscript.append(f"  tone {o['tone']}")
            for j, o in enumerate(c.get("options", [])):
                r = o.get("reply")
                if r:
                    fscript.append(f"r{j} {r['say']}: {r['text']}")
                    if r.get("expr"):
                        fscript.append(f"  expr {r['expr']}")
                    if r.get("emote"):
                        fscript.append(f"  emote {r['emote']}")
                else:
                    fscript.append(f"r{j} -:")
            fscript.append("")
            steps.append({"type": "dialogue", "dialogue": did})
        elif "music" in beat:
            steps.append({"type": "music", "name": beat["music"]})
        elif "sfx" in beat:
            steps.append({"type": "sfx", "name": beat["sfx"]})
        elif "lighting" in beat:
            steps.append({"type": "lighting", "name": beat["lighting"], "seconds": beat.get("seconds", 1.0)})
        elif "camera" in beat:
            steps.append({"type": "camera", "target": beat["camera"]})
        elif "wait" in beat:
            steps.append({"type": "wait", "seconds": beat["wait"]})
        elif "effects" in beat:
            steps.append({"type": "effects", "effects": beat["effects"]})

    if frame.get("lighting"):
        steps.append({"type": "lighting", "name": "reset", "seconds": 1.0})
    if frame.get("letterbox"):
        steps.append({"type": "letterbox", "name": "off", "seconds": 0.6})
    effects = [f"flag:event.{sid}"] + [e for e in d.get("effects", []) if e != f"flag:event.{sid}"]
    steps.append({"type": "effects", "effects": effects})

    event = {
        "id": sid,
        "trigger": d.get("trigger", "map"),
        "map": d["map"],
        "priority": d.get("priority", 6),
        "recheck": True,
        "condition": d.get("condition", "true"),
        "titleKey": f"memory.{sid}",
        "steps": steps,
    }
    if d.get("tag"):
        event["tag"] = d["tag"]
    strings[f"memory.{sid}"] = d.get("title", sid)
    return {"events": [event]}, strings, "\n".join(fscript).rstrip() + "\n"


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("draft", type=Path)
    ap.add_argument("--out", type=Path)
    a = ap.parse_args()
    d = json.loads(a.draft.read_text(encoding="utf-8"))
    out = a.out or a.draft.parent
    out.mkdir(parents=True, exist_ok=True)
    events, strings, fs = compile_draft(d)
    (out / f"{d['id']}.events.json").write_text(json.dumps(events, indent=2) + "\n", encoding="utf-8")
    (out / f"{d['id']}.strings.json").write_text(json.dumps(strings, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    (out / f"{d['id']}.fscript").write_text(fs, encoding="utf-8")
    print(f"{d['id']}: {len(events['events'][0]['steps'])} steps, {len(strings)} strings, {fs.count('dialogue ')} choice dialogues -> {out}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
