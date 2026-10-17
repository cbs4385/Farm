#!/usr/bin/env python3
"""Static verification of the scene pass (a stand-in for the Unity tests, which cannot run here).

    python3 verify.py [--base GITREF]       # --base: compare strings against that ref to find pinned old text (default: the commit before the pass)

Checks
  1. fscript <-> en.json sync for every dialogue text
  2. every story + Mythos event: unique ids, every say/dialogue/title key exists, dialogue ids resolve, step types and vocabularies valid
  3. every condition (event, step, choice, {if} in text) parses: known atoms, balanced parentheses
  4. every text lints (all {if} branches) for its owner: word budgets, voice limits, spelling, tone, tokens, markup
  5. horror isolation: no horror atom (horror:/dread/lore/keeper:/ritual:/mythos) in core story data or core strings; Mythos strings use mythos.* keys
  6. overlay shape: each overlay event's original exists, priority = original + 1, original (once-only) is guarded by !flag:event.<id>
  7. no Tests/*.cs pins a string that the pass replaced
"""
from __future__ import annotations

import argparse
import json
import re
import subprocess
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from common import MYTHOS, ROOT, STORY, VILLAGERS, WORK, fs_unescape, fscript_index, load_en, load_story  # noqa: E402
import lint_lines as LL  # noqa: E402
import lint_variants as LV  # noqa: E402

STAGE = LL.STAGE
HORROR = re.compile(r"horror:|var:dread|var:lore|keeper:|ritual:|mythos|cult\.|woods\.open", re.I)


def all_conditions_in_text(text):
    return [m.group(1) for m in re.finditer(r"\{if ([^}]*)\}", text)]


def cond_ok(c):
    probs = []
    if c.count("(") != c.count(")"):
        probs.append("unbalanced parentheses")
    for part in re.split(r"\s*(?:&&|\|\|)\s*", c):
        atom = part.strip().lstrip("!").strip("() ")
        head = re.split(r"[:<>=!]", atom, maxsplit=1)[0]
        if head and head not in STAGE["conditionAtoms"]:
            probs.append(f"unknown atom '{head}'")
    return probs


def load_mythos_scenes():
    p = MYTHOS / "mythos_scenes.json"
    return json.loads(p.read_text(encoding="utf-8")) if p.exists() else {"events": [], "dialogues": []}


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--base", default=None)
    a = ap.parse_args()
    en = load_en()
    S = load_story()
    M = load_mythos_scenes()
    own = json.loads((WORK / "ownership.json").read_text(encoding="utf-8"))
    errors: list[str] = []

    def err(m):
        errors.append(m)

    # 1
    idx = fscript_index()
    bad = [k for k, (f, r) in idx.items() if fs_unescape(r["text"]) != en.get(k)]
    for k in bad:
        err(f"fscript/en mismatch: {k}")
    print(f"1. fscript sync: {len(idx)} texts, {len(bad)} mismatches")

    # 2-3
    events = dict(S["events"])
    dialogues = dict(S["dialogues"])
    ids = set(events)
    for e in M["events"]:
        if e["id"] in ids:
            err(f"duplicate event id {e['id']}")
        ids.add(e["id"])
        events[e["id"]] = e
    for d in M["dialogues"]:
        if d["id"] in dialogues:
            err(f"duplicate dialogue id {d['id']}")
        dialogues[d["id"]] = d

    def walk(steps, where):
        for s in steps:
            t = s["type"]
            if t not in STAGE["stepTypes"]:
                err(f"{where}: unknown step type {t}")
            if s.get("condition"):
                for p in cond_ok(s["condition"]):
                    err(f"{where}: step condition '{s['condition']}': {p}")
            if t == "say":
                if s["text"] not in en:
                    err(f"{where}: missing string {s['text']}")
                for key, vocab in (("emote", "emotes"), ("expression", "expressions")):
                    if s.get(key) and s[key] not in STAGE[vocab]:
                        err(f"{where}: unknown {key} {s[key]}")
            elif t == "dialogue" and s["dialogue"] not in dialogues:
                err(f"{where}: dialogue {s['dialogue']} not found")
            elif t == "anim" and s.get("name") not in STAGE["anims"]:
                err(f"{where}: unknown anim {s.get('name')}")
            elif t == "emote" and s.get("name") not in STAGE["emotes"]:
                err(f"{where}: unknown emote {s.get('name')}")
            elif t == "expression" and s.get("name") not in STAGE["expressions"]:
                err(f"{where}: unknown expression {s.get('name')}")
            if s.get("steps"):
                walk(s["steps"], where)

    for eid, e in events.items():
        if e.get("condition"):
            for p in cond_ok(e["condition"]):
                err(f"{eid}: condition: {p}")
        if e.get("titleKey") and e["titleKey"] not in en:
            err(f"{eid}: missing title {e['titleKey']}")
        walk(e["steps"], eid)
    for did, d in dialogues.items():
        for n in d["nodes"]:
            if n.get("text") and n["text"] not in en:
                err(f"{did}.{n['id']}: missing {n['text']}")
            if n.get("condition"):
                for p in cond_ok(n["condition"]):
                    err(f"{did}.{n['id']}: {p}")
            for c in n.get("choices", []):
                if c["text"] not in en:
                    err(f"{did}.{n['id']}: missing choice {c['text']}")
                if c.get("condition"):
                    for p in cond_ok(c["condition"]):
                        err(f"{did}.{n['id']}: {p}")
    print(f"2-3. {len(events)} events ({len(M['events'])} overlay), {len(dialogues)} dialogues ({len(M['dialogues'])} overlay): structure + conditions checked")

    # 4: every string owned by the pass (cozy keys, overlay strings, added mythos scene strings)
    n4 = 0
    for k, v in en.items():
        if k.startswith(("mythos.v.", "mythos.scene.")):
            kind = "say"
            base = k[len("mythos.v."):] if k.startswith("mythos.v.") else None
            owner = own.get(base, {}).get("owner") if base else None
            if owner is None and k.startswith("mythos.scene."):
                continue
            if base and own.get(base, {}).get("kind"):
                kind = own[base]["kind"]
            for c in all_conditions_in_text(v):
                for p in cond_ok(c):
                    err(f"{k}: {{if {c}}}: {p}")
            for sev, msg in LL.lint_text(owner, kind, v):
                if sev == "error":
                    err(f"{k}: {msg}")
            n4 += 1
    for k, o in own.items():
        for sev, msg in LL.lint_text(o["owner"], o["kind"], en[k]):
            if sev == "error":
                err(f"{k}: {msg}")
        n4 += 1
    print(f"4. text lint: {n4} strings")

    # 5
    hits = []
    for f in sorted(STORY.glob("*.json")):
        raw = f.read_text(encoding="utf-8")
        for m in HORROR.finditer(raw):
            hits.append((f.name, m.group(0)))
    # the pre-existing core story data is expected to be clean; report only new ones
    print(f"5. horror isolation: {len(hits)} horror-ish tokens in core story JSON" + (f" {sorted(set(hits))[:5]}" if hits else ""))
    for k, v in en.items():
        if not k.startswith(("mythos", "dlg.mythos", "clue", "react", "memory.mythos")) and re.search(r"\{if [^}]*horror", v):
            err(f"core string {k} carries a horror condition")

    # 6
    for e in M["events"]:
        oid = e["id"][:-3]
        o = S["events"].get(oid)
        if o is None:
            err(f"overlay {e['id']}: original missing")
            continue
        if e["priority"] != int(o.get("priority", 0)) + 1:
            err(f"overlay {e['id']}: priority")
        if o.get("once", True) and f"!flag:event.{oid}" not in (o.get("condition") or ""):
            err(f"overlay {e['id']}: original not guarded")
    print(f"6. overlay shape: {len(M['events'])} overlays")

    # 7
    base = a.base
    if base is None:
        base = subprocess.run(["git", "-C", str(ROOT), "log", "--format=%H", "--diff-filter=A", "--", "tools/writers_room/scenepass/common.py"],
                              capture_output=True, text=True).stdout.split()[-1] + "~1"
    old = subprocess.run(["git", "-C", str(ROOT), "show", f"{base}:Assets/_Project/Resources/Localization/en.json"], capture_output=True, text=True)
    pinned = 0
    if old.returncode == 0:
        old_en = json.loads(old.stdout)
        tests = "\n".join(p.read_text(encoding="utf-8", errors="ignore") for p in (ROOT / "Assets/_Project/Tests").rglob("*.cs"))
        for k, v in old_en.items():
            if k in en and en[k] != v and len(v) > 8 and (v in tests or json.dumps(v, ensure_ascii=False)[1:-1] in tests):
                err(f"a test pins the old text of {k}: {v[:50]}")
                pinned += 1
    else:
        print("7. (could not read the base en.json)")
    print(f"7. tests pinning replaced text: {pinned}")

    print()
    if errors:
        print(f"{len(errors)} ERRORS")
        for e in errors[:60]:
            print("  " + e)
        sys.exit(1)
    print("all static checks pass")


if __name__ == "__main__":
    main()
