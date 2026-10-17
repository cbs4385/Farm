#!/usr/bin/env python3
"""Applies a round of villager rewrites to the repo.

    python3 apply_round.py r1/*.json [--dry]

For every `lines` entry: sets en.json[key]; if the key is a dialogue text defined in Narrative/*.fscript, edits that fscript line too
(the zz_fs_*.json holds only keys, so it stays as it is). For every `adds` entry: inserts a `say` step into the scene's hand-written
JSON, with a new key event.<scene>.<n> (n = highest existing + 1). For every `cuts` entry: removes the say step and its string.
Structure the brief forbids touching (ids, keys, order, effects) is never edited. Run lint_lines.py first.
"""
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from common import STORY, fs_escape, fscript_index, load_en, load_story, write_en  # noqa: E402


def read_json_file(path: Path):
    raw = path.read_text(encoding="utf-8")
    data = json.loads(raw)
    nl = raw.endswith("\n")
    for ind in (2, 4):
        if json.dumps(data, indent=ind, ensure_ascii=False) + ("\n" if nl else "") == raw:
            return data, ind, nl
    raise SystemExit(f"{path.name}: formatting does not round-trip; refusing to rewrite it")


def write_json_file(path: Path, data, ind, nl):
    path.write_text(json.dumps(data, indent=ind, ensure_ascii=False) + ("\n" if nl else ""), encoding="utf-8")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("files", nargs="+", type=Path)
    ap.add_argument("--dry", action="store_true")
    a = ap.parse_args()

    en = load_en()
    fs = fscript_index()
    S = load_story()
    own = json.loads((Path(__file__).resolve().parents[1] / "drafts/scene_pass/ownership.json").read_text(encoding="utf-8"))

    edits_fs: dict[Path, dict[int, str]] = {}
    changed = adds = cuts = 0
    pending_story: dict[str, list] = {}   # file name -> [(scene id, op, args)]

    for f in a.files:
        d = json.loads(f.read_text(encoding="utf-8"))
        who = d["villager"]
        for key, rec in d.get("lines", {}).items():
            if key not in own:
                raise SystemExit(f"{f.name}: unknown key {key}")
            if en.get(key) != rec["text"]:
                en[key] = rec["text"]
                changed += 1
            if key in fs:
                path, r = fs[key]
                raw = rec["text"]
                if r["kind"] == "node":
                    edits_fs.setdefault(path, {})[r["line"]] = ("node", fs_escape(raw))
                else:
                    edits_fs.setdefault(path, {})[r["line"]] = ("choice", fs_escape(raw))
        for ad in d.get("adds", []):
            sc = S["events"][ad["scene"]]
            if sc.get("_template"):
                raise SystemExit(f"{f.name}: cannot add beats to template scene {ad['scene']}")
            src = S["source"]["event:" + ad["scene"]]
            if src.startswith("zz_fs_"):
                raise SystemExit(f"{f.name}: cannot add beats to generated file scene {ad['scene']}")
            pending_story.setdefault(src, []).append((ad["scene"], "add", dict(ad, villager=who)))
            adds += 1
        for c in d.get("cuts", []):
            sc_ids = own[c["key"]]["scenes"]
            src = S["source"]["event:" + sc_ids[0]]
            pending_story.setdefault(src, []).append((sc_ids[0], "cut", c))
            cuts += 1

    # --- story JSON edits
    for fname, ops in pending_story.items():
        path = STORY / fname
        data, ind, nl = read_json_file(path)
        events = {e["id"]: e for e in data.get("events", [])}
        for scene, op, args in ops:
            ev = events[scene]
            steps = ev["steps"]
            if op == "add":
                used = [int(m.group(1)) for k in en for m in [re.fullmatch(rf"event\.{re.escape(scene)}\.(\d+)", k)] if m]
                n = max(used, default=-1) + 1
                key = f"event.{scene}.{n}"
                step = {"type": "say", "speaker": args["villager"], "text": key}
                if args.get("emote"):
                    step["emote"] = args["emote"]
                if args.get("expression"):
                    step["expression"] = args["expression"]
                if args["after"] == "START":
                    idx = 0
                else:
                    idx = next(i for i, s in enumerate(steps) if s.get("text") == args["after"]) + 1
                steps.insert(idx, step)
                en[key] = args["text"]
            else:
                idx = next(i for i, s in enumerate(steps) if s.get("text") == args["key"])
                steps.pop(idx)
                en.pop(args["key"], None)
        if not a.dry:
            write_json_file(path, data, ind, nl)

    # --- fscript edits
    for path, edits in edits_fs.items():
        lines = path.read_text(encoding="utf-8").split("\n")
        for i, (kind, text) in edits.items():
            if kind == "node":
                m = re.match(r"^(\S+ \S+: )(.*)$", lines[i])
                lines[i] = m.group(1) + text
            else:
                m = re.match(r"^(  \? )(.*?)( => .*)$", lines[i])
                lines[i] = m.group(1) + text + m.group(3)
        if not a.dry:
            path.write_text("\n".join(lines), encoding="utf-8")

    if not a.dry:
        write_en(en)
    print(f"{changed} strings changed, {adds} lines added, {cuts} cut, {sum(len(v) for v in edits_fs.values())} fscript lines edited{' (dry run)' if a.dry else ''}")


if __name__ == "__main__":
    main()
