#!/usr/bin/env python3
"""Lints a writers'-room scene draft before it is compiled into game data.

The checks mirror `NarrativeLint.cs` (length budgets, per-voice sentence
length, exclamation marks, banned phrases, British spelling, off-tone words,
gendered address, priority bands, the set-piece checklist) and add the
stage checks the C# validator does later: every speaker is in the cast,
every anim / emote / expression / facing / step type is one the engine has,
every condition atom and effect verb is registered.

    python3 tools/writers_room/lint_scene.py drafts/hall_dance.scene.json
    python3 tools/writers_room/lint_scene.py --strict draft.json   # warnings fail too

Exit code 1 on any error (or any warning with --strict). No third-party packages.
"""
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]

BUDGET = {"bark": 12, "talk": 28, "event": 40, "choice": 9, "gift": 20, "letter": 80}
MYTHOS_BAND = (8, 10)
FIRST_MEETING = 100

# Copied from NarrativeLint.cs so the two agree. Update both together.
BRITISH = re.compile(
    r"\b(neighbour\w*|colour\w*|cosy|cosier|cosiest|favourites?|fibres?|grey\w*|practis(?:e|es|ed|ing)|sunburnt|catalogues?|autumn\w*|"
    r"centres?|metres?|litres?|theatres?|defence|offence|licence|organis\w+|realis\w+|recognis\w+|apologis\w+|jewellery|moulds?|ploughs?|programmes?|tyres?|storeys?|whilst|amongst|"
    r"learnt|dreamt|burnt|cheques?|pyjamas|mum|mummy|aluminium|ageing|judgement|honour\w*|humour\w*|rumour\w*|labour\w*|flavour\w*|harbour\w*|savour\w*|vapour\w*|behaviour\w*|"
    r"endeavour\w*|armour\w*|odour\w*|vigour\w*|fulfil|enrol|skilful|travell\w+|cancell\w+|labell\w+|modell\w+|levell\w+|marvellous|woollen|kerb|gaol|draught|sceptic\w*|artefacts?|"
    r"manoeuvr\w+|pretence|analyse[sd]?|paralyse[sd]?)\b", re.I)
GENDERED = re.compile(r"\b(sir|ma'am|madam|milord|milady|lad|lass|handsome|gentleman|gentlemen|lady|ladies|young man|young lady|sonny)\b", re.I)
OFF_TONE = re.compile(r"\b(lol|omg|bro|dude|vibes?|literally|cringe|yeet|poggers|sus|selfie|hashtag|emoji|wifi|app|google|facebook|twitter|tiktok|youtube|twitch|damn|hell|crap|bloody|sucks?)\b", re.I)
TOKEN = re.compile(r"\[(player|farm|season|weekday|pet|crop|npc:[a-z]+|neighbor:[a-z]+)\]")


def words(text: str) -> int:
    return len([w for w in re.split(r"[ \n\t]+", text or "") if w])


def plain(text: str) -> str:
    """Strip inline markup the way RichText.Plain does, keeping the longest {if} branch."""
    text = re.sub(r"\{(?:pause|speed)=[^}]*\}", "", text)
    text = re.sub(r"</?[bi]>", "", text)
    # {if c}A{else}B{/if} -> the longer branch (worst case for the budget)
    def longest(m):
        branches = re.split(r"\{else\}", m.group(1))
        return max(branches, key=len)
    for _ in range(4):
        text = re.sub(r"\{if [^}]*\}(.*?)\{/if\}", longest, text, flags=re.S)
    text = re.sub(r"\{([^{}|]+(?:\|[^{}|]*)+)\}", lambda m: max(m.group(1).split("|"), key=len), text)
    return text.replace("\\n", " ").strip()


class Lint:
    def __init__(self, draft: dict, stage: dict, rules: dict):
        self.d = draft
        self.stage = stage
        self.rules = rules
        self.issues: list[tuple[str, str, str]] = []   # severity, where, message
        self.cast = set((draft.get("cast") or {}).keys())
        self.villagers = set(rules.get("locked", [])) | set(rules.get("voices", {}).keys())

    def add(self, sev: str, where: str, msg: str, villager: str | None = None):
        if sev == "warning" and villager and villager in self.rules.get("locked", []):
            sev = "error"
        self.issues.append((sev, where, msg))

    # ---- helpers
    def voice(self, v: str) -> dict:
        out = dict(self.rules.get("defaults", {}))
        out.update(self.rules.get("voices", {}).get(v, {}))
        return out

    def check_name(self, kind: str, name: str | None, where: str):
        if name in (None, "", "none"):
            return
        allowed = self.stage.get(kind, [])
        if allowed and name not in allowed:
            self.add("error", where, f"{kind} '{name}' is not one the engine has (allowed: {', '.join(allowed)})")

    def check_text(self, text: str, kind: str, speaker: str | None, where: str):
        t = plain(text)
        n = words(t)
        if n > BUDGET[kind]:
            self.add("warning", where, f"{n} words; the {kind} budget is {BUDGET[kind]}", speaker)
        if m := BRITISH.search(t):
            self.add("error", where, f"'{m.group(0)}' is a British spelling; the game uses American English")
        if m := OFF_TONE.search(t):
            self.add("warning", where, f"'{m.group(0)}' does not fit the village (slang, brand or swearing)", speaker)
        if m := GENDERED.search(t):
            self.add("warning", where, f"'{m.group(0)}': the player has no gender, so villagers do not address them this way", speaker)
        if text.count("{") != text.count("}"):
            self.add("error", where, "unbalanced braces in inline markup")
        for tok in re.findall(r"\[[^\]]+\]", t):
            if not TOKEN.fullmatch(tok):
                self.add("error", where, f"unknown token {tok}")
        if speaker in self.villagers and kind != "choice":
            v = self.voice(speaker)
            if not v.get("allowExclamation", True) and "!" in t:
                self.add("warning", where, "exclamation mark in a voice that does not use them", speaker)
            for sentence in re.split(r"(?<=[.!?])\s+", t):
                if words(sentence) > v.get("maxSentenceWords", 22):
                    self.add("warning", where, f"a sentence of {words(sentence)} words; this voice stops at {v['maxSentenceWords']}", speaker)
            for banned in v.get("banned", []):
                if banned.lower() in t.lower():
                    self.add("warning", where, f"uses '{banned}', which this villager never says", speaker)

    def check_condition(self, cond: str, where: str):
        if not cond or cond == "true":
            return
        atoms = self.stage.get("conditionAtoms", [])
        if not atoms:
            return
        for atom in re.split(r"\s*(?:&&|\|\|)\s*", cond):
            atom = atom.strip().lstrip("!").strip("() ")
            if not atom:
                continue
            head = re.split(r"[:<>=!]", atom, 1)[0]
            if head not in atoms:
                self.add("error", where, f"condition atom '{head}' is not registered (from '{atom}')")

    def check_effects(self, effects, where: str):
        verbs = self.stage.get("effectVerbs", [])
        for e in effects or []:
            verb = e.split(":", 1)[0]
            if verbs and verb not in verbs:
                self.add("error", where, f"effect verb '{verb}' is not registered (from '{e}')")

    # ---- the walk
    def run(self):
        d = self.d
        sid = d.get("id", "?")
        if not re.fullmatch(r"[a-z][a-z0-9_]*", sid):
            self.add("error", "id", "scene id must be lower_snake_case")
        if not d.get("title"):
            self.add("warning", sid, "a tagged scene needs a title so players can replay it")
        if d.get("map") and self.stage.get("maps") and d["map"] not in self.stage["maps"]:
            self.add("error", sid, f"map '{d['map']}' is not a known map id")
        prio = d.get("priority", 6)
        mythos = sid.startswith("mythos")
        if MYTHOS_BAND[0] <= prio <= MYTHOS_BAND[1] and not mythos:
            self.add("warning", sid, f"priority {prio} is in the mythos band (8-10); heart events use 6-7")
        if prio > MYTHOS_BAND[1] and prio != FIRST_MEETING:
            self.add("error", sid, f"priority {prio} is outside the bands")
        self.check_condition(d.get("condition", ""), f"{sid} condition")
        self.check_effects(d.get("effects"), f"{sid} effects")
        for actor, pos in (d.get("cast") or {}).items():
            if actor != "player" and actor not in self.villagers:
                self.add("error", f"cast {actor}", "not a villager id")
            self.check_name("facing", pos.get("facing"), f"cast {actor}")

        spoken_last = False
        visual = False
        seconds = 0.0
        player_lines = 0
        total_lines = 0
        flags_set = [e for e in d.get("effects", []) if e.startswith("flag:")]
        for i, beat in enumerate(d.get("beats", [])):
            where = f"{sid} beat {i}"
            kind = next((k for k in ("say", "narrate", "stage", "choice", "music", "sfx", "wait", "lighting", "camera", "effects") if k in beat), None)
            if kind is None:
                self.add("error", where, f"unknown beat {list(beat)}")
                continue
            if kind == "say":
                total_lines += 1
                sp = beat["say"]
                if sp not in self.cast:
                    self.add("error", where, f"speaker '{sp}' is not in the cast")
                self.check_text(beat.get("text", ""), "event", sp, where)
                if "[player]" in beat.get("text", ""):
                    player_lines += 1
                for k, kk in (("anim", "anims"), ("emote", "emotes"), ("expr", "expressions"), ("face", "facing")):
                    if beat.get(k):
                        self.check_name(kk, beat[k], where)
                        if k != "face":
                            visual = True
                spoken_last = True
                seconds += 3.0
            elif kind == "narrate":
                self.check_text(beat["narrate"], "event", None, where)
                spoken_last = True
                seconds += 3.0
            elif kind == "stage":
                for step in beat["stage"]:
                    st = step.get("type")
                    if st not in self.stage.get("stepTypes", [st]):
                        self.add("error", where, f"step type '{st}' is not one EventRunner handles")
                    if step.get("actor") and step["actor"] not in self.cast:
                        self.add("error", where, f"actor '{step['actor']}' is not in the cast")
                    if st == "anim": self.check_name("anims", step.get("name"), where)
                    if st == "emote": self.check_name("emotes", step.get("name"), where)
                    if st == "expression": self.check_name("expressions", step.get("name"), where)
                    if st == "face": self.check_name("facing", step.get("facing"), where)
                    if st in ("anim", "emote", "hop", "sway", "jiggle", "shake", "dance", "nod", "pan", "focus", "lighting", "letterbox"):
                        visual = True
                    if not step.get("async"):
                        seconds += float(step.get("seconds", 0.5) or 0)
                spoken_last = False
            elif kind == "choice":
                c = beat["choice"]
                sp = c.get("say")
                if sp and sp not in self.cast:
                    self.add("error", where, f"speaker '{sp}' is not in the cast")
                if sp and c.get("text"):
                    total_lines += 1
                    self.check_text(c["text"], "event", sp, where)
                opts = c.get("options", [])
                if not 2 <= len(opts) <= 4:
                    self.add("error", where, f"a choice has 2 to 4 options, not {len(opts)}")
                for j, o in enumerate(opts):
                    self.check_text(o.get("text", ""), "choice", None, f"{where} option {j}")
                    if o.get("tone") and o["tone"] not in self.stage.get("tones", [o["tone"]]):
                        self.add("error", where, f"tone '{o['tone']}' is not one of {self.stage.get('tones')}")
                    if o.get("flag") and not re.fullmatch(r"choice\.[a-z0-9_]+\.[a-z0-9_]+", o["flag"]):
                        self.add("warning", where, f"choice flag '{o['flag']}' should be choice.<villager or scene>.<name>")
                    if o.get("flag"):
                        flags_set.append("flag:" + o["flag"])
                    reply = o.get("reply")
                    if reply:
                        total_lines += 1
                        if reply.get("say") not in self.cast:
                            self.add("error", where, f"reply speaker '{reply.get('say')}' is not in the cast")
                        self.check_text(reply.get("text", ""), "event", reply.get("say"), f"{where} option {j} reply")
                if not any(o.get("flag") for o in opts):
                    self.add("warning", where, "a choice that matters sets a flag; none of these options does")
                spoken_last = True
                seconds += 6.0
            elif kind == "music":
                self.check_name("music", beat["music"], where)
                visual = True
                spoken_last = False
            elif kind == "sfx":
                visual = True
                spoken_last = False
            elif kind == "lighting":
                self.check_name("lighting", beat["lighting"], where)
                visual = True
                spoken_last = False
            elif kind == "camera":
                self.check_name("camera", beat["camera"], where)
                visual = True
                spoken_last = False
            elif kind == "wait":
                seconds += float(beat["wait"])
                spoken_last = False
            elif kind == "effects":
                self.check_effects(beat["effects"], where)
        # set-piece checklist (STYLE.md 8a and 8)
        if d.get("tag"):
            if not spoken_last:
                self.add("warning", sid, "a tagged scene ends on a spoken line (the payoff), not a stage beat")
            if not visual:
                self.add("warning", sid, "a tagged scene needs at least one beat the eye or ear can catch (emote, anim, music, light)")
            if seconds > 150:
                self.add("warning", sid, f"about {seconds:.0f}s long; a set piece stays under 150s")
            if d.get("tag") not in ("funny", "wholesome", "surprise", "mystery"):
                self.add("error", sid, f"tag '{d['tag']}' is not funny, wholesome, surprise or mystery")
        if d.get("kind", "scene") == "heart" and not 25 <= total_lines <= 45:
            self.add("warning", sid, f"{total_lines} spoken lines; a heart event or set piece runs 25 to 45")
        if total_lines and player_lines / total_lines > 0.25:
            self.add("warning", sid, f"[player] in {player_lines} of {total_lines} lines (limit one in four)")
        if not any(f == f"flag:event.{sid}" for f in flags_set):
            self.add("warning", sid, f"no effect sets flag:event.{sid}; later lines cannot refer to the scene")
        return self.issues


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("draft", type=Path)
    ap.add_argument("--strict", action="store_true")
    ap.add_argument("--stage", type=Path, default=HERE / "canon" / "stage.json")
    ap.add_argument("--rules", type=Path, default=ROOT / "Assets" / "_Project" / "Narrative" / "narrative_rules.json")
    a = ap.parse_args()
    draft = json.loads(a.draft.read_text(encoding="utf-8"))
    stage = json.loads(a.stage.read_text(encoding="utf-8")) if a.stage.exists() else {}
    rules = json.loads(a.rules.read_text(encoding="utf-8"))
    issues = Lint(draft, stage, rules).run()
    errors = sum(1 for s, _, _ in issues if s == "error")
    warnings = len(issues) - errors
    for sev, where, msg in sorted(issues, key=lambda x: (x[0] != "error", x[1])):
        print(f"{sev:7} {where}: {msg}")
    print(f"{errors} errors, {warnings} warnings")
    return 1 if errors or (a.strict and warnings) else 0


if __name__ == "__main__":
    sys.exit(main())
