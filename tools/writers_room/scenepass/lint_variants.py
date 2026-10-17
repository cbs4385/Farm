#!/usr/bin/env python3
"""Lints a Round 3 horror-variant file (variants / adds / cuts).

    python3 lint_variants.py r3/bram.json [--json]

Checks: ownership and scene membership; every `when` parses (known atoms), contains horror:1 or horror:2, and uses var:dread>=60
only together with horror:2; variants ordered so a horror:2 variant is never shadowed by an earlier horror:1 one with the same or
broader condition; the same word budgets, voice limits and spelling rules as the cozy lint; no {if} markup in variant text;
mild-only variants (horror:1 without horror:2) carry no ritual imagery; adds/cuts only in editable scenes.
"""
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
sys.path.insert(0, str(HERE.parent))
from common import VILLAGERS, WORK, load_en  # noqa: E402
import lint_lines as LL  # noqa: E402

STAGE = LL.STAGE
MILD_BANNED = re.compile(r"\b(robes?|chant(?:s|ed|ing)?|altar|offerings?|sacrific\w*|rituals?|nharoth|blood|cult|covenant)\b", re.I)
ATOM = re.compile(r"[A-Za-z_.]+")


def cond_problems(cond: str) -> list[str]:
    probs = []
    if not cond or not cond.strip():
        return ["empty condition"]
    if cond.count("(") != cond.count(")"):
        probs.append("unbalanced parentheses")
    for part in re.split(r"\s*(?:&&|\|\|)\s*", cond):
        atom = part.strip().lstrip("!").strip("() ")
        head = re.split(r"[:<>=!]", atom, maxsplit=1)[0]
        if head and head not in STAGE["conditionAtoms"]:
            probs.append(f"unknown condition atom '{head}'")
    if not re.search(r"horror:[12]", cond):
        probs.append("a horror variant must require horror:1 or horror:2")
    if re.search(r"\|\|", cond):
        probs.append("avoid '||' in a variant condition (write two variants instead)")
    if re.search(r"var:dread>=([6-9]\d|100)", cond) and "horror:2" not in cond:
        probs.append("var:dread>=60 belongs to full intensity only (needs horror:2)")
    if re.search(r"!\s*horror", cond):
        probs.append("do not negate horror in a variant condition")
    return probs


def mild_only(cond: str) -> bool:
    return "horror:1" in cond and "horror:2" not in cond


def lint_file(path: Path, en: dict, own: dict, scenes: dict) -> dict:
    d = json.loads(path.read_text(encoding="utf-8"))
    who = d.get("villager")
    out = {"villager": who, "errors": [], "warnings": [], "variants": 0, "adds": 0, "cuts": 0}

    def err(where, msg):
        out["errors"].append((where, msg))

    def check_text(where, kind, text, cond):
        if re.search(r"\{(if|else|/if)\b", text):
            err(where, "no {if} markup inside a variant (the compiler builds the chain)")
        for sev, msg in LL.lint_text(who, kind, text):
            (err if sev == "error" else out["warnings"].append)(where, msg) if sev == "error" else out["warnings"].append((where, msg))
        if mild_only(cond) and MILD_BANNED.search(text):
            err(where, f"mild-only variant uses ritual/cult imagery: '{MILD_BANNED.search(text).group(0)}'")

    for key, vs in d.get("variants", {}).items():
        o = own.get(key)
        if not o:
            err(key, "unknown key")
            continue
        if o["owner"] != who:
            err(key, f"owned by {o['owner']}, not {who}")
            continue
        if not 1 <= len(vs) <= 4:
            err(key, f"{len(vs)} variants (1 to 4 allowed)")
        seen_h1 = None
        for i, v in enumerate(vs):
            where = f"{key} #{i}"
            out["variants"] += 1
            for p in cond_problems(v.get("when", "")):
                err(where, p)
            check_text(where, o["kind"], v.get("text", ""), v.get("when", ""))
            if v.get("text", "").strip() == en.get(key, "").strip():
                out["warnings"].append((where, "variant equals the cozy text"))
            c = v.get("when", "")
            if seen_h1 and "horror:2" in c and c.replace("horror:2", "horror:1") == seen_h1:
                err(where, "a horror:2 variant after the same condition at horror:1 can never run; put it first")
            if mild_only(c) and seen_h1 is None:
                seen_h1 = c
    for a in d.get("adds", []):
        where = f"add in {a.get('scene')}"
        out["adds"] += 1
        sc = scenes.get(a.get("scene"))
        if not sc:
            err(where, "unknown scene")
            continue
        if not sc["editable"]:
            err(where, "scene is text-only: adds not allowed")
        if a.get("after") != "START" and not any(b.get("key") == a.get("after") for b in sc["beats"] if b["type"] == "say"):
            err(where, f"after '{a.get('after')}' is not a spoken line of this scene")
        for p in cond_problems(a.get("when", "")):
            err(where, p)
        check_text(where, "say", a.get("text", ""), a.get("when", ""))
        if a.get("emote") and a["emote"] not in STAGE["emotes"]:
            err(where, f"unknown emote {a['emote']}")
        if a.get("expression") and a["expression"] not in STAGE["expressions"]:
            err(where, f"unknown expression {a['expression']}")
    per_scene = {}
    for a in d.get("adds", []):
        per_scene[a.get("scene")] = per_scene.get(a.get("scene"), 0) + 1
    for s, n in per_scene.items():
        if n > 2:
            err(f"adds in {s}", f"{n} additions (2 allowed)")
    for c in d.get("cuts", []):
        where = f"cut in {c.get('scene')}"
        out["cuts"] += 1
        sc = scenes.get(c.get("scene"))
        if not sc:
            err(where, "unknown scene")
            continue
        if not sc["editable"]:
            err(where, "scene is text-only: cuts not allowed")
        says = [b for b in sc["beats"] if b["type"] == "say"]
        idx = next((i for i, b in enumerate(says) if b["key"] == c.get("key")), None)
        if idx is None:
            err(where, f"'{c.get('key')}' is not a spoken line of this scene")
        elif idx == len(says) - 1:
            err(where, "never cut the scene's last spoken line")
        elif own.get(c["key"], {}).get("owner") != who:
            err(where, "can only cut your own line")
        for p in cond_problems(c.get("when", "")):
            err(where, p)
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("files", nargs="+", type=Path)
    a = ap.parse_args()
    en = load_en()
    own = json.loads((WORK / "ownership.json").read_text(encoding="utf-8"))
    scenes = json.loads((WORK / "scenes.json").read_text(encoding="utf-8"))["scenes"]
    bad = 0
    for f in a.files:
        r = lint_file(f, en, own, scenes)
        print(f"{f.name}: {r['variants']} variants, {r['adds']} adds, {r['cuts']} cuts; {len(r['errors'])} errors, {len(r['warnings'])} warnings")
        for k, m in r["errors"]:
            print(f"  ERROR {k}: {m}")
        for k, m in r["warnings"][:12]:
            print(f"  warn  {k}: {m}")
        bad += len(r["errors"])
    sys.exit(1 if bad else 0)


if __name__ == "__main__":
    main()
