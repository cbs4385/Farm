# The writers' room

A way to write villager scenes and lines with the villagers in the room. Each of the twelve villagers (and the late uncle, Edmund) is a Claude Code subagent built from the narrative canon; the main session directs them; a lint and a compiler turn what they improvise into the story formats the game already compiles (ADR 0003, `docs/narrative/FSCRIPT.md`).

Why it exists: the cast's wants and relationships are written down in detail, but every scene so far was drafted by one writer playing all twelve at once, which is how one joke template ended up in eight mouths (`docs/NPC_DESIGN_REVIEW_2026-10-16.md` section 2.6). With a persona per villager that only knows its own bible, its own matrix row and its own shipped lines, a Wren line has to come from Wren, and a line Bram would not say gets a NOTE from Bram.

## Parts

| Path | What |
|---|---|
| `.claude/agents/npc-<villager>.md` | one persona per villager, **generated** by `build_personas.py` from `docs/narrative/BIBLE*.md`, `RELATIONSHIPS.md`, `Assets/_Project/Narrative/narrative_rules.json` and the villager's shipped lines in `en.json` |
| `.claude/agents/npc-edmund.md` | the dead uncle: memories, letters and an audit of what each villager could know |
| `.claude/skills/writers-room/SKILL.md` | the Director's procedure (`/writers-room <brief>`) |
| `canon/stage.md`, `canon/stage.json` | what an NPC can do on screen: event step types, anim / emote / expression / facing names, maps, condition atoms, effect verbs. Hand-maintained from the code; the lint reads the JSON |
| `canon/knowledge.md`, `canon/protocol.md` | shared text inlined into every persona: village facts, what a villager knows, the turn format |
| `lint_scene.py` | the checks of `NarrativeLint.cs` plus stage checks, on a draft |
| `compile_scene.py` | draft → `events` JSON + `event.<id>.<n>` strings + choice dialogues in FScript |
| `drafts/` | scene drafts and their compiled output; nothing here ships until merged by hand |

## Commands

```
python3 tools/writers_room/build_personas.py            # rebuild the persona agents (run after any bible change)
python3 tools/writers_room/build_personas.py --check    # CI: fail if the agents are stale
python3 tools/writers_room/lint_scene.py drafts/x.scene.json [--strict]
python3 tools/writers_room/compile_scene.py drafts/x.scene.json
```

In Claude Code: `/writers-room improvise hall_dance_v2: the Hall Dance at horror 0 with choice.chorus.home and choice.wren.bag.go true; close Wren's full house, Piper's chorus, Juno's bell and Marcus's roof`, or `/writers-room table-read hall_dance`.

## The draft format (`*.scene.json`)

```json
{
  "id": "hall_dance_v2",
  "title": "Opening Night",
  "kind": "heart",
  "map": "CommunityHall",
  "trigger": "map",
  "priority": 7,
  "tag": "wholesome",
  "condition": "flag:hall.restored && flag:hall.roof && hour>=18 && hour<23",
  "frame": { "letterbox": true, "lighting": "warm" },
  "cast": {
    "marcus": { "x": 8, "y": 6, "facing": "up" },
    "player": { "x": 7, "y": 2, "facing": "up" }
  },
  "beats": [
    { "say": "marcus", "text": "I checked the roof once more. Then twice. It is finished.", "anim": "nod", "emote": "sparkle", "expr": "happy", "face": "down" },
    { "narrate": "The whole hall sings the chorus." },
    { "stage": [ { "type": "anim", "actor": "marcus", "name": "dance", "seconds": 4.0, "async": true } ] },
    { "music": "piper_theme" },
    { "wait": 2.5 },
    { "choice": {
        "say": "wren", "text": "Well? First at the bar. What'll it be?",
        "options": [
          { "text": "To the roof.", "tone": "kind", "flag": "choice.hall_dance.roof", "reply": { "say": "wren", "text": "To the roof. Marcus, don't you dare cry." } },
          { "text": "To the porch.", "tone": "playful", "flag": "choice.hall_dance.porch" }
        ] } },
    { "effects": [ "friend:marcus,40" ] }
  ],
  "effects": [ "friend:marcus,40", "friend:wren,40" ]
}
```

- `say` fields `anim`, `emote`, `expr`, `face` are optional and must use names from `canon/stage.json`.
- `narrate` is a `say` with no speaker (italics, no portrait).
- `stage` passes raw event steps through unchanged, for anything the short forms do not cover.
- A `choice` becomes an FScript dialogue `<id>.choice<n>` and a `dialogue` step; its option flags are what later lines refer to with `choice:`.
- `kind: "heart"` turns on the 25-to-45-line check; `kind: "pool"` is a list of talk lines (`beats` of `say` only, no cast) for a villager's set.
- The compiler always appends `flag:event.<id>` to the effects.

## What it does not do

It does not merge into `Assets/`, run Unity, or decide canon. Villagers' NOTEs and the lint's warnings are the output as much as the lines are; the owner decides. Nothing it writes has been played until `docs/STATUS.md` says so.
