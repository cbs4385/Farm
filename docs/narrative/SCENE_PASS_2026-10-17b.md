# Scene pass, second full run (2026-10-17, redo on the new writers' room)

Branch `writers-room`. This run replaces the text of the first scene pass (documented in `SCENE_PASS_2026-10-17.md`), re-running the whole pipeline with the finished scripts feeding the teams. **Nothing here has been compiled in Unity, tested in Unity or played.** All checks are static (`tools/writers_room/scenepass/verify.py`).

## What was asked
Redo the scene pass with the updated scripts feeding the writing teams: every story scene rewritten by its villagers, accepted as written, with variants, additions and subtractions for the dread levels and the three horror settings.

## What was done
Scope: 177 story scenes, 74 scene dialogues, ~1,125 owned text keys. The first pass's text was the starting draft; the first pass's drafts are archived in `tools/writers_room/drafts/scene_pass_v1/`.

1. **Round 1, cozy rewrite** (commit `Round 1 cozy rewrite`): 263 strings changed, 11 beats added, 64 fscript lines edited. Each villager rewrote only lines they could beat. New canon from this round: Tilda's shop cat is **Ledger** everywhere; Juno's maker's mark is **a little anvil with a J hiding in it**; her fifty-one anonymous notes are in pencil; Dorian's notebook holds **four hundred and twelve** drawings; Hazel's almanac is eighty-one years old (page two hundred and four); Ione's set-aside slips carry a date and nothing else; Dr. Penn keeps a list of everyone who eats standing up; Bram's heart 4 hoe is **Edmund's** (split socket, never explained).
2. **Round 2, coherence read** (commit `Round 2 coherence read`): 61 strings. Tic thinning (Bram's "Hm." to signature uses — he declined the 18-word allowance; Juno's triple-okay from five openers to two; Tilda's "Oh"; Wren's "Don't"; Marcus's measure-tic audit), repeated-opener fixes across every overheard chain, two calendar-safe date fixes, Dorian's "nine falls" → eight (no false echo of Ione's nine years), hall_dance narration now shows two separate dances two steps apart.
3. **Round 3, horror layer** (commit `Round 3 horror variants`): **237 variants, 25 added lines, 11 conditional cuts**, by horror setting, dread band and lore tier. This room was deliberately more selective than the first run: many scenes are left wholly plain as the control group the shadowed scenes read against (all pumpkin branches, the bench friendship, most romances, Bram's overheards).

## Round 3 design, as written by the room
- **Keepers get gentler as dread rises, never colder**: Tilda feeds and walks you home ("I'm not asking, I'm stocking you"), Marcus sets a nobody-walks-home-alone rule, Dr. Penn closes the clinic "for you as much as me", Wren holds the stool, Dorian promises safety before proposing.
- **The unaware notice without explaining**: blue forge fire, cold iron off the rack, a flat sea with no smell, gulls skipping crumbs, the cat facing the wood, lanterns drifting toward the trees, "the quiet has a tune of its own".
- **Edmund threads at clue tier** (lore≥3, hearts≥4): his page was "tidy, at the end" and Penn **held the lamp** (revelation); the cellar hinge was an alarm he wanted to hear open; the hoe left at Bram's door the week before he died, no note; the four hummed notes somebody hummed first; the uncollected set-aside book; his first pressed leaf; he counted to forty-one.
- **The Keeper calendar shows at hint tier**: the fourths of the season (early closes, the village emptying, Bram's birthday), new-moon customs (nail orders, Wren's hours, the glazing deadline), Penn crossed Harrow Wood off Elara's list hard enough to dent three pages.
- **Distortion at dread≥60, full only**: short repeated fragments ("It will hold. It will hold.", "They were in my hands. In my hands already."), jokes that fall flat, cuts paired with comfort adds.

## How it attaches
Unchanged mechanism, rebuilt data: `Resources/Mythos/mythos_scenes.json` now holds **125 overlay scenes and 26 dialogue copies** (first run: 170/58 — the difference is deliberate selectivity, not loss); **213 `mythos.*` strings** in en.json; the 146 once-only cozy guards from the first run still stand and needed no changes. No C# changed; nothing in core data names horror. Not overlaid, by design: the pumpkin scenes, most overheards between unaware pairs, the romances' second asks, festivals except winter lanterns.

## Verified (static only)
`verify.py` passes end to end after every round: fscript/en sync (1,047 texts), structure and conditions for 302 events and all dialogues, per-branch text lint for every owner, horror isolation (0 horror tokens in core story JSON), overlay priorities and guards, no test pins replaced text. All 12 Round 1 and Round 2 files passed `lint_lines.py`, all 12 Round 3 files passed `lint_variants.py`, with zero errors. `render_levels.py <scene> --horror N --dread D --lore L --hearts v=N` prints any scene at any setting.

## Not verified. Do these first
1. Open the project: **Farm > Compile Story**, then EditMode tests (`FScriptSourcesInSyncTests`, `NarrativeLintTests`, `StoryValidator`, Mythos and Memories suites, `SpecScenesTests`). Check the compile keeps the 213 `mythos.*` strings in `en.json`.
2. Play a heart event at horror off, mild and full (e.g. tilda_heart5 or odalys_heart5 — both have strong variants).
3. Read the new lines; only the agents have.

## Needs your decision (carried and new)
- **Hall Dance**: shipped structure kept; `hall_dance_v2` still unmerged.
- **Felix heart 8 map**: event plays on Beach, shipped dialogue says pond; text now placeless — settle the map.
- **Wren heart 4**: written to the build (she sings her own worst song); the bible says she volunteers the player to introduce Piper.
- **Hazel's heart 10 lantern payoff**: her conditional festival branch was vetoed by the structure lint; needs a Director slot if wanted.
- **Juno's brass key**: echoed at hint tier in a horror add; the cozy daily-talk/letter slot for it is still owed for horror-off players.
- **New canon to bless**: Edmund's hoe left at Bram's door the week before he died; Penn "held the lamp"; Marcus's "four nights a year the work stops"; Wren's "something here needed keeping" (eight years); Tilda's mother's unwritten inheritance; "the wood keeps what it catches."
- **Engine items** (from the room, not text): prop mismatches (bram_heart10 goldbar/heirloom, story_umbrella prop ids, next_door copperbar-as-hinge), `(sfx click)` not in vocabulary, a bell sfx for the winter lanterns, Dr. Penn's birthday line living in two homes, heart-2 beat keys playing out of numeric order (felix, elara), Marcus's whistler add anchoring after the scene's last line.
- **Memories page**: counts run slightly high at mild/full (each overlaid scene has two versions).

## Revert and re-run
Each round is one commit on this branch (`redo prep`, `Round 1`, `Round 2`, `Round 3 + overlay`). Deleting `Resources/Mythos/mythos_scenes.json` (+ `.meta`) switches off all horror variation. The pipeline: `corpus.py` → agents → `lint_lines.py` / `lint_variants.py` → `apply_round.py` → `build_overlay.py` → `verify.py`; drafts, briefs and notes in `tools/writers_room/drafts/scene_pass/` (first run archived in `scene_pass_v1/`).
