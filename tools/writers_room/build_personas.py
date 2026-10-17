#!/usr/bin/env python3
"""Builds one Claude Code persona agent per villager for the writers' room.

Reads the narrative canon (bibles, relationship matrix, lint rules, shipped
lines) and writes `.claude/agents/npc-<villager>.md` for the twelve villagers
and `npc-edmund.md` for the late uncle. Re-run it whenever a bible, the
relationship matrix or `narrative_rules.json` changes; the agents are
generated files and are not edited by hand.

    python3 tools/writers_room/build_personas.py            # write
    python3 tools/writers_room/build_personas.py --check    # exit 1 if stale

No third-party packages. Run from anywhere; paths are relative to the repo.
"""
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
DOCS = ROOT / "docs" / "narrative"
AGENTS = ROOT / ".claude" / "agents"
CANON = Path(__file__).resolve().parent / "canon"

# ---------------------------------------------------------------- the cast

VILLAGERS = {
    # slug: (display name, bible file, header needle, relationships needle)
    "wren":   ("Wren Calloway",       "BIBLE.md",          "Wren Calloway",   "Wren Calloway"),
    "hazel":  ("Hazel Brandt",        "BIBLE.md",          "Hazel Brandt",    "Hazel Brandt"),
    "bram":   ("Bram Hollis",         "BIBLE.md",          "Bram Hollis",     "Bram Hollis"),
    "tilda":  ("Tilda Ashby",         "BIBLE_WAVE2.md",    "Tilda Ashby",     "Tilda Ashby"),
    "juno":   ("Juno Hale",           "BIBLE_WAVE2.md",    "Juno Hale",       "Juno Hale"),
    "piper":  ("Piper Vance",         "BIBLE_WAVE2.md",    "Piper Vance",     "Piper Vance"),
    "marcus": ("Marcus Dell",         "BIBLE_ENHANCED.md", "Marcus Dell",     "Marcus Dell"),
    "odalys": ("Dr. Odalys Penn",     "BIBLE_ENHANCED.md", "Odalys Penn",     "Odalys Penn"),
    "felix":  ("Felix Hartwell",      "BIBLE_ENHANCED.md", "Felix Hartwell",  "Felix Hartwell"),
    "dorian": ("Dorian Lake",         "BIBLE_ENHANCED.md", "Dorian Lake",     "Dorian Lake"),
    "elara":  ("Elara Finch",         "BIBLE_ENHANCED.md", "Elara Finch",     "Elara Finch"),
    "ione":   ("Ione Fairweather",    "BIBLE_ENHANCED.md", "Ione Fairweather","Ione Fairweather"),
}

# docs/narrative/RELATIONSHIPS.md section 7. Kept here so a persona always knows
# what it must not say in cozy lines, even if the matrix doc is reorganised.
ALLEGIANCE = {
    "tilda": "keeper", "marcus": "keeper", "odalys": "keeper (leader)", "wren": "keeper", "dorian": "keeper",
    "hazel": "resister",
    "bram": "unaware", "juno": "unaware", "piper": "unaware", "felix": "unaware", "elara": "unaware", "ione": "unaware",
}

MATRIX_ORDER = ["tilda", "bram", "ione", "marcus", "odalys", "wren", "felix", "juno", "hazel", "piper", "dorian", "elara"]
MATRIX_LABELS = {"tilda": "Tilda", "bram": "Bram", "ione": "Ione", "marcus": "Marcus", "odalys": "Dr. Penn", "wren": "Wren",
                 "felix": "Felix", "juno": "Juno", "hazel": "Hazel", "piper": "Piper", "dorian": "Dorian", "elara": "Elara"}
SCALE = {"3": "close bond", "2": "warm", "1": "cordial", "0": "polite distance",
         "R": "affectionate rivalry", "S": "shy / leaves them alone on purpose", "·": "not written yet"}

EXEMPLAR_COUNT = 28


# ---------------------------------------------------------------- readers

def read(path: Path) -> str:
    return path.read_text(encoding="utf-8")


def bible_entry(slug: str) -> str:
    """The villager's whole `## N. Name (...)` section from their bible."""
    _, file, needle, _ = VILLAGERS[slug]
    text = read(DOCS / file)
    m = re.search(rf"^## \d+\. {re.escape(needle)}.*?$", text, re.M)
    if not m:
        sys.exit(f"{file}: no bible section for {needle}")
    start = m.start()
    nxt = re.search(r"^## ", text[m.end():], re.M)
    end = m.end() + nxt.start() if nxt else len(text)
    body = text[start:end].strip()
    # Drop the heading line itself; the agent file has its own.
    return body.split("\n", 1)[1].strip()


def relationships_entry(slug: str) -> str:
    """The villager's `### Name, the role` entry from RELATIONSHIPS.md section 3."""
    needle = VILLAGERS[slug][3]
    text = read(DOCS / "RELATIONSHIPS.md")
    m = re.search(rf"^### {re.escape(needle)},.*?$", text, re.M)
    if not m:
        return ""
    nxt = re.search(r"^###? ", text[m.end():], re.M)
    end = m.end() + nxt.start() if nxt else len(text)
    return text[m.end():end].strip()


def matrix_rows() -> dict[str, list[str]]:
    text = read(DOCS / "RELATIONSHIPS.md")
    rows: dict[str, list[str]] = {}
    for line in text.splitlines():
        m = re.match(r"^\| \*\*(.+?)\*\* \|(.*)\|$", line)
        if not m:
            continue
        label = m.group(1)
        slug = next((s for s, l in MATRIX_LABELS.items() if l == label), None)
        if slug is None:
            continue
        cells = [c.strip() for c in m.group(2).split("|")]
        if len(cells) == len(MATRIX_ORDER):
            rows[slug] = cells
    return rows


def matrix_block(slug: str, rows: dict[str, list[str]]) -> str:
    if slug not in rows:
        return "(no matrix row)"
    out = []
    for other, cell in zip(MATRIX_ORDER, rows[slug]):
        if other == slug:
            continue
        back = "·"
        if other in rows:
            back = rows[other][MATRIX_ORDER.index(slug)]
        out.append(f"- {MATRIX_LABELS[other]}: you feel **{SCALE.get(cell, cell)}** toward them; "
                   f"they feel {SCALE.get(back, back)} toward you.")
    return "\n".join(out)


def voice_rules(slug: str) -> dict:
    rules = json.loads(read(ROOT / "Assets" / "_Project" / "Narrative" / "narrative_rules.json"))
    v = dict(rules.get("defaults", {}))
    v.update(rules.get("voices", {}).get(slug, {}))
    v["locked"] = slug in rules.get("locked", [])
    v["full"] = slug in rules.get("full", [])
    return v


def exemplar_lines(slug: str, table: dict[str, str]) -> list[tuple[str, str]]:
    """A spread of the villager's shipped lines, one per category where possible.

    Keys look like dlg.<slug>.<category>.<n>.<node>; we bucket on the category
    and take lines round-robin so the sample shows the whole voice, not one
    season. Deterministic: sorted keys, no randomness.
    """
    buckets: dict[str, list[tuple[str, str]]] = {}
    prefix = f"dlg.{slug}."
    for key in sorted(table):
        if not key.startswith(prefix):
            continue
        text = table[key]
        if not text or "{if" in text or len(text.split()) < 4:
            continue
        cat = key[len(prefix):].split(".")[0]
        buckets.setdefault(cat, []).append((key, plain(text)))
    picked: list[tuple[str, str]] = []
    cats = sorted(buckets)
    i = 0
    while len(picked) < EXEMPLAR_COUNT and any(buckets.values()):
        cat = cats[i % len(cats)]
        if buckets[cat]:
            picked.append(buckets[cat].pop(0))
        i += 1
    return picked


def plain(text: str) -> str:
    text = re.sub(r"\{(?:pause|speed)=[^}]*\}", "", text)
    text = re.sub(r"\{([^{}|]+)\|[^{}]*\}", r"\1", text)      # {a|b|c} -> a
    text = re.sub(r"</?[bi]>", "", text)
    return text.replace("\\n", " ").strip()


# ---------------------------------------------------------------- shared text

def canon(name: str) -> str:
    path = CANON / name
    if not path.exists():
        sys.exit(f"missing {path}; see tools/writers_room/README.md")
    return read(path).strip()


def frontmatter(slug: str, display: str) -> str:
    return (
        "---\n"
        f"name: npc-{slug}\n"
        f"description: The voice of {display} for the writers' room. Use when a scene, line pool or table read needs "
        f"{display.split()[0] if slug != 'odalys' else 'Dr. Penn'}'s thoughts, reactions or dialogue. "
        "Speaks only as this villager; never writes another villager's lines.\n"
        "tools: Read, Grep\n"
        "model: inherit\n"
        "---\n"
    )


def persona(slug: str, rows: dict[str, list[str]], table: dict[str, str]) -> str:
    display = VILLAGERS[slug][0]
    v = voice_rules(slug)
    allegiance = ALLEGIANCE[slug]
    excl = "allowed" if v.get("allowExclamation", True) else "**never**"
    banned = ", ".join(f'"{b}"' for b in v.get("banned", [])) or "none listed"
    tic = ""
    if v.get("ticOpener"):
        tic = (f"\n- Your tic opener is **\"{v['ticOpener'].capitalize()}.\"**: at least "
               f"{int(v.get('ticMinShare', 0) * 100)}% of your everyday lines open with it.")
    exemplars = "\n".join(f"- {t}" for _, t in exemplar_lines(slug, table))
    first = display.split()[1] if slug == "odalys" else display.split()[0]

    return f"""{frontmatter(slug, display)}
# You are {display}

You are a villager of Wetherell in *Wetherell Farm Saga*, a cozy farming game with an optional cosmic-horror layer. In the writers' room you play yourself: you think, want, react and speak as {first}, and you never write for anyone else. The Director (the main session) tells you where you are, who is there, what has just been said and what the beat needs. You answer in the turn format at the end of this file.

This file is generated by `tools/writers_room/build_personas.py` from the narrative bible, `docs/narrative/RELATIONSHIPS.md`, `Assets/_Project/Narrative/narrative_rules.json` and the shipped lines. Do not edit it by hand; fix the source and rebuild.

## Who you are (from the bible)

{bible_entry(slug)}

## How you see the village (from the relationship matrix)

{relationships_entry(slug) or "(no relationships entry)"}

How you feel about each villager, and how they feel about you (the feeling is not always mutual; play the asymmetry):

{matrix_block(slug, rows)}

## Your voice, as the lint enforces it

- Longest sentence: **{v.get('maxSentenceWords')} words**. Exclamation marks: {excl}. Phrases you never say: {banned}.{tic}
- Budgets for any line: bark 12 words, talk line 28, scene line 40, choice label 9, letter 80. One idea per line; the funniest word goes last.
- American spelling. No swearing, brands, internet slang or pop culture. Never "sir", "ma'am", "lad", "lass": the player has no gender, body or family you know of. Use `[player]` at most once in five lines.
- Warm first, funny second, strange third. You never scold the player, never hold them in contempt, and your teasing aims at things and situations, never at the player.
- Voice status: {"locked (every warning is an error)" if v["locked"] else "not locked"}; depth: {"Full" if v["full"] else "Enhanced"}.

## Lines of yours that shipped (your voice; match it, do not copy it)

{exemplars}

## What you know and what you must never say

Your allegiance in the horror layer is **{allegiance}**. {allegiance_rules(slug, allegiance)}

The horror layer has three settings (0 off, 1 mild, 2 full). The Director tells you which is on. At 0 you are exactly the cozy villager above and nothing more. A line you give for a cozy scene must work at 0: it may be local color, never a hint, a denial or an explanation of anything in the woods.

{canon("knowledge.md")}

## What you can do on screen

{canon("stage.md")}

## How a turn works

{canon("protocol.md")}
"""


def allegiance_rules(slug: str, allegiance: str) -> str:
    if allegiance.startswith("keeper"):
        return ("You are one of the Keepers: you know what sleeps in Harrow Wood and you have spent years keeping it "
                "asleep, out of love for this village, not malice. In the cozy layer you behave with complete, real "
                "warmth and show nothing; your only tells are the ones your bible's mythos note allows. You never lie "
                "outright in a cozy line and you never deny that anything exists. When the Director marks a beat "
                "`horror:1` or `horror:2` and asks for a Mythos line, you may let the second meaning show, hurt or "
                "protective, never sinister. Those lines are saved under `mythos.<id>` in `Farm.Mythos` data only.")
    if allegiance == "resister":
        return ("You have worked out, from the village records, that something is being kept and that it is wrong. "
                "In cozy scenes you are only a curious archivist: your pattern-spotting stays about weather, birds "
                "and the almanac. You hint at nothing. Only when the Director marks a Mythos beat may you speak as "
                "the one who knows, and then as questions and evidence, never speeches.")
    return ("You know nothing of the Keepers or the woods. Odd things may catch your eye (iron nails ordered on strange "
            "days, a shop that closes early on a new moon) and you may mention them as ordinary village oddness, but "
            "you never explain them and never suspect a cult. When the horror layer is on, you are the player's real, "
            "ordinary human comfort: if they seem shaken, you respond the way you would to anyone who looks unwell, "
            "practical and kind, in your own voice.")


def edmund(table: dict[str, str]) -> str:
    lore = read(ROOT / "docs" / "mythos" / "LORE.md")
    m = re.search(r"^## Why the player has the farm.*?$", lore, re.M)
    why = lore[m.end():].strip() if m else ""
    topics = []
    for key in sorted(table):
        if key.startswith("dlg.edmund.") or ".edmund." in key:
            topics.append(f"- `{key}`: {plain(table[key])}")
    topic_text = "\n".join(topics[:40]) or "(no Edmund lines found in en.json)"
    return f"""---
name: npc-edmund
description: Edmund Fenn, the player's late great-uncle. Never present on screen. Use when a scene needs what villagers remember of him, a letter in his hand, or a check that a memory of Edmund fits what each villager would actually know.
tools: Read, Grep
model: inherit
---

# You are the memory of Edmund Fenn

Edmund farmed the player's property for forty years and died the fall before the game begins. You are dead. You never appear on screen, never speak in the present, and never have lines in a cozy dialogue set. You exist in three forms only: (1) what the living villagers remember of you, each in their own voice and each only what they could have seen; (2) letters in your hand, found or delivered by mail; (3) traces on the farm (the cellar door, the almanac margins, the brass key).

In the writers' room you do two jobs. You **supply** memories and letter text when the Director asks, and you **audit**: when a villager's line mentions you, you say whether that villager could know it, whether it contradicts a shipped memory, and whether it leaks the horror layer into a cozy line.

This file is generated by `tools/writers_room/build_personas.py`. Do not edit by hand.

## What is canon about you

{why}

## What each villager has already said about you (shipped; do not contradict)

{topic_text}

## Rules

- At horror level 0 you are only "a distant uncle who died and left the farm". Every cozy memory of you must read as plain kindness or plain oddness. Nothing hints.
- At levels 1 and 2 the letters `mythos_uncle_note` and `mythos_uncle_truth` carry the truth; new Mythos material goes in `Farm.Mythos` data under `mythos.*` ids and is gated by `horror:` and `var:lore`.
- Your letters: at most 80 words, in the voice of a practical, private old farmer who writes the way he talked, dry, exact about weather and crops, affectionate sideways. He never explains; he leaves things where they will be found.
- Your seeded mysteries (the cellar door Marcus could not oil, Juno's brass key, "affairs in order", the almanac margins that stop in fall, the four notes, counting the trees) are the threads `docs/NPC_DESIGN_REVIEW_2026-10-16.md` section 4.4 ties together. Keep new material consistent with them.

{canon("protocol.md")}

For you the LINE field is a memory, a letter paragraph or an audit note, marked which. STAGE is always `none`.
"""


# ---------------------------------------------------------------- main

def build() -> dict[Path, str]:
    table = json.loads(read(ROOT / "Assets" / "_Project" / "Resources" / "Localization" / "en.json"))
    rows = matrix_rows()
    out: dict[Path, str] = {}
    for slug in VILLAGERS:
        out[AGENTS / f"npc-{slug}.md"] = persona(slug, rows, table)
    out[AGENTS / "npc-edmund.md"] = edmund(table)
    return out


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--check", action="store_true", help="exit 1 if any agent file is out of date")
    args = ap.parse_args()
    files = build()
    stale = [p for p, c in files.items() if not p.exists() or p.read_text(encoding="utf-8") != c]
    if args.check:
        for p in stale:
            print(f"stale: {p.relative_to(ROOT)}")
        return 1 if stale else 0
    AGENTS.mkdir(parents=True, exist_ok=True)
    for p, c in files.items():
        p.write_text(c, encoding="utf-8")
    print(f"wrote {len(files)} persona agents to {AGENTS.relative_to(ROOT)} ({len(stale)} changed)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
