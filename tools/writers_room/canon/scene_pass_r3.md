# The scene pass, Round 3: the horror layer (variants, additions, subtractions)

The cozy text is final and accepted. This round writes what a scene becomes when the horror layer is on, for **your own lines only**. Read `canon/mythos_sheet.md` first (the truth, the three levels, dread bands, information tiers, your allegiance). The cozy text of every line stays the answer at horror off and for a calm player; you write what changes **around** it.

## The goal
The player who turns the horror on should feel the village is slightly wrong in a way they cannot yet name, and should want to find out. Every scene should leave at least one thing a curious player would chase: a person to ask, a night to be awake, a place to go, an object. A player who never chases it still reads the scene as warm, a little shadowed.

## The three levels and the dread bands, concretely
Variants are conditional replacements written for the line they replace. For each line you choose, write up to four variants, **ordered most specific first**; the cozy text is automatically the last fallback.

| Name | `when` pattern | Use |
|---|---|---|
| frightened, full | `horror:2 && var:dread>=60` | strained scene; distortion allowed (short fragment only) |
| uneasy, full | `horror:2 && var:dread>=30` | small noticing; a Keeper's too-exact care |
| uneasy, mild | `horror:1 && var:dread>=30` | the same noticing without ritual imagery (no robe, chant, altar, offering, sacrifice, ritual, Nharoth, blood) |
| clue/hint (any dread) | `horror:1 && var:lore>=1` (hint), `horror:1 && var:lore>=3 && hearts:<you>>=4` (clue), `horror:2 && var:lore>=5 && hearts:<you>>=6` (revelation) | what you let slip |
You may combine (`horror:2 && var:dread>=30 && var:lore>=3`). `horror:1` is true at full too, so the more specific (full) variant must come **before** the mild one. A variant must be written to be read **in place of** the cozy line: same speaker, same job in the scene, same flag meaning, within the **same word budget** (40 spoken, 9 choice label, 20 overheard). No `{if}` markup inside your variant text; the compiler builds the chain.

## What to produce for each scene of yours
1. **Variants** (`variants`): choose the lines where the horror changes the meaning. Aim for about one line per scene in the uneasy band, one per important scene in the frightened band, and one clue-tier line in every scene where your allegiance gives you something to slip. Leave most lines alone: contrast needs plain lines.
2. **Additions** (`adds`): a short extra spoken line (or a pure beat: a line that is only an ellipsis-and-glance with an `emote`) inserted into the scene only when the condition holds. Use it for the thing a Keeper does not quite say, the unaware villager's puzzled aside, Hazel's quiet warning. At most 2 additions per scene, only in scenes the card marks "may be ADDED or CUT".
3. **Subtractions** (`cuts`): a spoken line the scene loses when the condition holds (a joke that would be cruel at `var:dread>=60`, a cheerful sign-off). Only in editable scenes, only for a plain spoken line (not a dialogue node), and never the scene's last line or the line that carries its flag. A cut line often pairs with an addition that fills the silence.
4. **A shadow of the thread** (`threads`): for each scene, in one sentence, the question the horror version leaves the player with.

## Allegiance (from the sheet)
Keepers: warm; the tell is exactness. Hazel: the resister; dry, afraid, pattern-spotting. Unaware: human, local, never explanatory. If the line is yours and you are a Keeper, the player's dread makes you **gentler**, never colder. Do not reveal what you may not at your hearts/lore gate.

## Mechanics you must keep
Keys exactly as on the card (✏ lines only). Never change speaker, order of the other beats, flags, effects, items, or what a choice means. A choice label variant keeps its meaning. `[player]`/`[farm]` tokens may be added in variants. American English, no gendered address, no gore, nothing cruel to animals. Your voice limits and the word budgets still apply to every variant.

## What to hand back
`drafts/scene_pass/r3/<you>.json`:
```json
{
  "villager": "bram",
  "variants": {
    "event.bram_heart2.1": [
      { "when": "horror:2 && var:dread>=60", "text": "…", "why": "what it does" },
      { "when": "horror:1 && var:dread>=30", "text": "…", "why": "…" }
    ]
  },
  "adds": [ { "scene": "bram_heart2", "after": "event.bram_heart2.1", "when": "horror:1 && var:lore>=3 && hearts:bram>=4",
              "text": "…", "emote": null, "expression": null, "why": "…" } ],
  "cuts": [ { "scene": "bram_heart2", "key": "event.bram_heart2.2", "when": "horror:2 && var:dread>=60", "why": "…" } ],
  "threads": { "bram_heart2": "one sentence" },
  "notes": [ "anything the Director must know" ]
}
```
Run `python3 /home/claude/farm/tools/writers_room/scenepass/lint_variants.py <file>` and fix every ERROR. After every ~10 scenes save the file and send one line to "main" via SendMessage: "<you>: N/total scenes, V variants, A adds, C cuts, E lint errors". Final reply at most 120 words.
