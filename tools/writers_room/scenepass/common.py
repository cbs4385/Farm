"""Shared loading and mapping code for the all-scenes pass (docs/narrative/SCENE_PASS_2026-10-17.md).

Everything here is read-only against the repo except `write_en`, which round-trips en.json byte for byte
(indent=2, ensure_ascii=False, trailing newline), and the fscript editor in apply.py.
"""
from __future__ import annotations

import copy
import glob
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
STORY = ROOT / "Assets" / "_Project" / "Resources" / "Story"
MYTHOS = ROOT / "Assets" / "_Project" / "Resources" / "Mythos"
NARR = ROOT / "Assets" / "_Project" / "Narrative"
EN = ROOT / "Assets" / "_Project" / "Resources" / "Localization" / "en.json"
WORK = ROOT / "tools" / "writers_room" / "drafts" / "scene_pass"

VILLAGERS = ["wren", "hazel", "bram", "tilda", "juno", "piper", "marcus", "odalys", "felix", "dorian", "elara", "ione"]


# ------------------------------------------------------------------ strings

def load_en(path: Path = EN) -> dict[str, str]:
    return json.loads(path.read_text(encoding="utf-8"))


def write_en(table: dict[str, str], path: Path = EN) -> None:
    path.write_text(json.dumps(table, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


# ------------------------------------------------------------------ story json

def story_files() -> list[Path]:
    return sorted(STORY.glob("*.json"), key=lambda p: p.name)  # StoryContent loads in ordinal name order


def load_story() -> dict:
    """Merge every Story/*.json the way StoryContent does, remembering each item's source file."""
    out = {"dialogues": {}, "events": {}, "templates": {}, "instances": [], "source": {}}
    for f in story_files():
        d = json.loads(f.read_text(encoding="utf-8"))
        for t in d.get("eventTemplates", []):
            out["templates"][t["id"]] = t
        for dl in d.get("dialogues", []):
            out["dialogues"].setdefault(dl["id"], dl)
            out["source"].setdefault("dialogue:" + dl["id"], f.name)
        for e in d.get("events", []):
            out["events"].setdefault(e["id"], e)
            out["source"].setdefault("event:" + e["id"], f.name)
        for inst in d.get("eventsFromTemplates", []):
            out["instances"].append((f.name, inst))
    for fname, inst in out["instances"]:
        ev = expand(inst, out["templates"])
        out["events"].setdefault(ev["id"], ev)
        out["source"].setdefault("event:" + ev["id"], fname)
        out["source"]["instance:" + ev["id"]] = fname
    return out


# ------------------------------------------------------------------ template expansion (EventTemplates.cs)

_WHOLE = re.compile(r"^\$\{(\w+)\}$")
_PART = re.compile(r"\$\{(\w+)\}")


def _subst(v, args):
    if isinstance(v, str):
        m = _WHOLE.match(v)
        if m:
            return copy.deepcopy(args[m.group(1)])
        return _PART.sub(lambda m: str(args[m.group(1)]), v)
    if isinstance(v, list):
        out = []
        for item in v:
            if isinstance(item, str) and _WHOLE.match(item) and isinstance(args.get(_WHOLE.match(item).group(1)), list):
                out.extend(copy.deepcopy(args[_WHOLE.match(item).group(1)]))
            else:
                out.append(_subst(item, args))
        return out
    if isinstance(v, dict):
        return {k: _subst(x, args) for k, x in v.items()}
    return v


def expand(inst: dict, templates: dict) -> dict:
    t = templates[inst["template"]]
    args = dict(t.get("defaults", {}))
    args.update(inst.get("args", {}))
    args["id"] = inst["id"]
    ev = _subst(t["event"], args)
    ev["id"] = inst["id"]
    for k, v in inst.get("event", {}).items():
        ev[k] = v
    ev["_template"] = inst["template"]
    ev["_args"] = inst.get("args", {})
    return ev


# ------------------------------------------------------------------ fscript key map (FScript.cs conventions, docs/narrative/FSCRIPT.md)

_NODE = re.compile(r"^(\S+) (\S+): (.*)$")
_CHOICE = re.compile(r"^  \? (.*?) => (\S+)(?: \| (.*))?$")
_KEY = re.compile(r"^  key (\S+)\s*$")


def _node_key(did: str, nid: str) -> str:
    m = re.fullmatch(r"n(\d+)", nid)
    return f"dlg.{did}.{m.group(1) if m else nid}"


def parse_fscript(path: Path) -> list[dict]:
    """Return [{'dialogue': id, 'kind': 'node'|'choice', 'key': k, 'line': lineno(0-based), 'speaker', 'text'}]."""
    lines = path.read_text(encoding="utf-8").split("\n")
    out = []
    did = None
    choice_idx = 0
    last = None   # the last node/choice record, for a following `key` override
    for i, raw in enumerate(lines):
        if raw.startswith("dialogue "):
            did = raw.split()[1]
            choice_idx = 0
            last = None
            continue
        if did is None:
            continue
        if raw.startswith(("set ", "topic ", "social ", "addto ", "#")) or raw == "":
            if raw.startswith(("set ", "topic ", "social ", "addto ")):
                did = None
            continue
        km = _KEY.match(raw)
        if km and last is not None:
            last["key"] = km.group(1)
            continue
        cm = _CHOICE.match(raw)
        if cm:
            rec = {"dialogue": did, "kind": "choice", "key": f"dlg.{did}.c{choice_idx}", "line": i, "text": cm.group(1), "speaker": None}
            choice_idx += 1
            out.append(rec)
            last = rec
            continue
        nm = _NODE.match(raw)
        if nm and not raw.startswith(" "):
            rec = {"dialogue": did, "kind": "node", "key": _node_key(did, nm.group(1)), "line": i, "speaker": nm.group(2), "text": nm.group(3), "node": nm.group(1)}
            out.append(rec)
            last = rec
    return out


def fs_unescape(s: str) -> str:
    """FScript text -> the string stored in en.json (`\\n` is a line break, `\\\\` a backslash)."""
    out = []
    i = 0
    while i < len(s):
        if s[i] == "\\" and i + 1 < len(s):
            out.append({"n": "\n", "\\": "\\"}.get(s[i + 1], s[i + 1]))
            i += 2
        else:
            out.append(s[i])
            i += 1
    return "".join(out)


def fs_escape(s: str) -> str:
    return s.replace("\\", "\\\\").replace("\n", "\\n")


def fscript_index() -> dict[str, tuple[Path, dict]]:
    """key -> (fscript file, record) for every dialogue text defined in Narrative/*.fscript."""
    idx = {}
    for f in sorted(NARR.glob("*.fscript")):
        for rec in parse_fscript(f):
            idx[rec["key"]] = (f, rec)
    return idx
