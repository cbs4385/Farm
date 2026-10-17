#!/usr/bin/env python3
"""Lints rewritten scene lines the way NarrativeLint.cs does, but on every {if} branch, and checks that a rewrite kept its structure.

    python3 lint_lines.py r1/bram.json            # one round file (lines + adds)
    python3 lint_lines.py r1/*.json --json out    # also write <file>.lint.json beside each input

A line is an error when it would fail the C# lint for a locked villager (all twelve are locked), when its markup is broken,
or when a rewrite changed the set of {if ...} conditions / [tokens] / {pause}/{speed} cues of the line it replaces.
"""
from __future__ import annotations

import argparse
import json
import re
import sys
from collections import Counter
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE.parent))
sys.path.insert(0, str(HERE))
from common import ROOT, VILLAGERS, WORK, load_en  # noqa: E402
import lint_scene as ls  # noqa: E402

RULES = json.loads((ROOT / "Assets/_Project/Narrative/narrative_rules.json").read_text(encoding="utf-8"))
STAGE = json.loads((ROOT / "tools/writers_room/canon/stage.json").read_text(encoding="utf-8"))
BUDGET = {"say": 40, "node": 40, "choice": 9}
TOKEN = re.compile(r"\[[^\]]+\]")
CUE = re.compile(r"\{(?:pause|speed)=[^}]*\}")
IFTAG = re.compile(r"\{if ([^}]*)\}")


def voice(v: str) -> dict:
    out = dict(RULES.get("defaults", {}))
    out.update(RULES.get("voices", {}).get(v, {}))
    return out


def branches(text: str, cap: int = 64) -> list[str]:
    """Every string the line can resolve to. `{a|b}` options count as separate branches too."""
    # tokenise into text, {if c}, {else}, {/if}
    parts = re.split(r"(\{if [^}]*\}|\{else\}|\{/if\})", text)

    def parse(i, stop):
        """returns (list of alternative strings, next index)"""
        alts = [""]
        while i < len(parts):
            p = parts[i]
            if p == "{else}" or p == "{/if}":
                if stop:
                    return alts, i
                i += 1
                continue
            if p.startswith("{if "):
                a, j = parse(i + 1, True)
                b = [""]
                if j < len(parts) and parts[j] == "{else}":
                    b, j = parse(j + 1, True)
                i = j + 1
                alts = [x + y for x in alts for y in (a + b)][:cap]
            else:
                alts = [x + p for x in alts]
                i += 1
        return alts, i

    out, _ = parse(0, False)
    res = []
    for o in out:
        m = re.search(r"\{([^{}|]+(?:\|[^{}|]*)+)\}", o)
        if m:
            for opt in m.group(1).split("|"):
                res.append(o.replace(m.group(0), opt, 1))
        else:
            res.append(o)
    return res[:cap]


def markup_problems(text: str) -> list[str]:
    probs = []
    depth = 0
    seen_else = []
    i = 0
    while i < len(text):
        c = text[i]
        if c == "}":
            probs.append("a '}' with no '{'")
        elif c == "{":
            j = text.find("}", i + 1)
            if j < 0:
                probs.append("a '{' is never closed")
                break
            inner = text[i + 1:j]
            if inner.startswith("if "):
                depth += 1
                seen_else.append(False)
                cond = inner[3:].strip()
                for atom in re.split(r"\s*(?:&&|\|\|)\s*", cond):
                    atom = atom.strip().lstrip("!").strip("() ")
                    head = re.split(r"[:<>=!]", atom, maxsplit=1)[0]
                    if head and head not in STAGE["conditionAtoms"]:
                        probs.append(f"unknown condition atom '{head}' in {{if {cond}}}")
                if cond.count("(") != cond.count(")"):
                    probs.append(f"unbalanced parentheses in {{if {cond}}}")
            elif inner == "else":
                if depth == 0:
                    probs.append("{else} outside an {if}")
                elif seen_else[depth - 1]:
                    probs.append("two {else} in one {if}")
                else:
                    seen_else[depth - 1] = True
            elif inner == "/if":
                if depth == 0:
                    probs.append("{/if} with no {if}")
                else:
                    depth -= 1
                    seen_else.pop()
            elif inner.startswith(("pause=", "speed=")):
                pass
            elif "|" in inner:
                pass
            else:
                probs.append(f"unknown markup {{{inner}}}")
            i = j
        i += 1
    if depth:
        probs.append(f"{depth} {{if}} never closed")
    return probs


TOKEN_OK = {"event.bram_heart10.thanks"}           # the player-name moment: [player] may be added
SENTENCE_CAP = {"dlg.bram.long.0": 18}               # Bram's longest sentence ever


def lint_text(owner: str, kind: str, text: str, old: str | None = None, speaker_check: bool = True, key: str | None = None) -> list[tuple[str, str]]:
    """Returns [(severity, message)]."""
    issues: list[tuple[str, str]] = []
    if not text or not text.strip():
        return [("error", "empty text")]
    for p in markup_problems(text):
        issues.append(("error", "markup: " + p))
    if text != text.strip():
        issues.append(("warning", "leading or trailing space"))
    bs = branches(text)
    v = voice(owner) if owner in VILLAGERS else dict(RULES.get("defaults", {}))
    for b in bs:
        plain = re.sub(r"</?[bi]>", "", CUE.sub("", b)).replace("\n", " ").strip()
        n = ls.words(plain)
        if n > BUDGET[kind]:
            issues.append(("error", f"{n} words > {BUDGET[kind]} in branch: {plain[:60]}…"))
        if m := ls.BRITISH.search(plain):
            issues.append(("error", f"British spelling '{m.group(0)}'"))
        if m := ls.OFF_TONE.search(plain):
            issues.append(("error", f"off-tone word '{m.group(0)}'"))
        if m := ls.GENDERED.search(plain):
            issues.append(("error", f"gendered address '{m.group(0)}'"))
        if kind != "choice" and owner in VILLAGERS:
            if not v.get("allowExclamation", True) and "!" in plain:
                issues.append(("error", f"exclamation mark in {owner}'s voice"))
            for sentence in re.split(r"(?<=[.!?])\s+", plain):
                w = ls.words(sentence)
                if w > SENTENCE_CAP.get(key, v.get("maxSentenceWords", 22)):
                    issues.append(("error", f"sentence of {w} words > {owner}'s {v['maxSentenceWords']}: {sentence[:50]}…"))
            for banned in v.get("banned", []):
                if banned.lower() in plain.lower():
                    issues.append(("error", f"{owner} never says '{banned}'"))
            for other, ov in RULES.get("voices", {}).items():
                tic = ov.get("ticOpener")
                if tic and other != owner and re.search(rf"(?:^|[.!?]\s+)\.*{re.escape(tic)}\b", plain, re.I):
                    issues.append(("error", f"opens a sentence with {tic.capitalize()}, which is {other.capitalize()}'s tic"))
        if "  " in plain:
            issues.append(("warning", "double space"))
        if re.search(r"\s[—–-]\s?[—–-]", plain):
            issues.append(("warning", "stacked dashes"))
        if kind == "choice" and (" => " in b or " | " in b):
            issues.append(("error", "a choice label cannot contain ' => ' or ' | '"))
    for tok in TOKEN.findall(text):
        if not ls.TOKEN.fullmatch(tok):
            issues.append(("error", f"unknown token {tok}"))
    if old is not None:
        oc = Counter(c.strip() for c in IFTAG.findall(old))
        nc = Counter(c.strip() for c in IFTAG.findall(text))
        if oc != nc:
            issues.append(("error", f"the {{if}} conditions changed: was {sorted(oc.elements())}, now {sorted(nc.elements())}"))
        ot = Counter(TOKEN.findall(old))
        nt = Counter(TOKEN.findall(text))
        if ot != nt and key not in TOKEN_OK:
            issues.append(("error", f"tokens changed: was {sorted(ot.elements())}, now {sorted(nt.elements())}"))
        if Counter(CUE.findall(old)).keys() and not CUE.findall(text) and len(CUE.findall(old)) > 0:
            issues.append(("warning", "the line lost its {pause}/{speed} cues"))
    return issues


def lint_round(path: Path, en: dict, own: dict) -> dict:
    d = json.loads(path.read_text(encoding="utf-8"))
    who = d.get("villager")
    out = {"villager": who, "errors": [], "warnings": [], "checked": 0}
    for key, rec in d.get("lines", {}).items():
        o = own.get(key)
        if o is None:
            out["errors"].append((key, "not a known text key"))
            continue
        if o["owner"] != who:
            out["errors"].append((key, f"owned by {o['owner']}, not {who}"))
            continue
        out["checked"] += 1
        for sev, msg in lint_text(who, o["kind"], rec["text"], old=en.get(key), key=key):
            out["errors" if sev == "error" else "warnings"].append((key, msg))
    for a in d.get("adds", []):
        out["checked"] += 1
        for sev, msg in lint_text(who, "say", a["text"]):
            out["errors" if sev == "error" else "warnings"].append((f"add after {a.get('after')} in {a.get('scene')}", msg))
        if a.get("emote") and a["emote"] not in STAGE["emotes"]:
            out["errors"].append((f"add in {a.get('scene')}", f"unknown emote {a['emote']}"))
        if a.get("expression") and a["expression"] not in STAGE["expressions"]:
            out["errors"].append((f"add in {a.get('scene')}", f"unknown expression {a['expression']}"))
    # share of new lines using [player]
    texts = [r["text"] for r in d.get("lines", {}).values()] + [a["text"] for a in d.get("adds", [])]
    if texts:
        share = sum(1 for t in texts if "[player]" in t) / len(texts)
        if share > 0.25:
            out["warnings"].append(("all lines", f"[player] in {share:.0%} of rewritten lines (limit about 25%)"))
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("files", nargs="+", type=Path)
    ap.add_argument("--en", type=Path)
    ap.add_argument("--json", action="store_true")
    a = ap.parse_args()
    en = load_en(a.en) if a.en else load_en()
    own = json.loads((WORK / "ownership.json").read_text(encoding="utf-8"))
    bad = 0
    for f in a.files:
        r = lint_round(f, en, own)
        print(f"{f.name}: {r['checked']} checked, {len(r['errors'])} errors, {len(r['warnings'])} warnings")
        for k, m in r["errors"]:
            print(f"  ERROR {k}: {m}")
        for k, m in r["warnings"][:15]:
            print(f"  warn  {k}: {m}")
        bad += len(r["errors"])
        if a.json:
            f.with_suffix(".lint.json").write_text(json.dumps(r, indent=1, ensure_ascii=False) + "\n", encoding="utf-8")
    sys.exit(1 if bad else 0)


if __name__ == "__main__":
    main()
