# Code review, 2026-10-11 (version 0.4.5)

A read-through by grep and spot reading of `Assets/_Project/Scripts` (about 360 files) plus the release process. Nothing here is a crash or data-loss bug; the game's tests pass (1559 EditMode, 358 PlayMode). The items are ordered by what is worth doing first. "Mine" marks problems introduced in the last few days of work.

## Worth fixing soon

1. **The village-damage feature breaks two of the project's own rules (mine).** CLAUDE.md says NPC, dialogue and map work must be data-driven and use conditions and hooks, not hard-coded checks. `VillageWatch` hard-codes the village map, builds the telling-off dialogue in C# (`ScoldGraph`), and keeps its mutters as plain string keys, so none of it goes through the story JSON, the narrative lint (voice, length, duplicates) or conditions. `VillageTrees` has grown from "trees" into trees, rocks, soil and the daily restore, and should be renamed (`VillageGreen`). Move the lines into `Resources/Story/*.json` as dialogue sets with a condition (for example `flag:village.damaged.today`) and keep only the walking and restoring in code.
2. **Restoring the village depends on physics state at load time (mine).** `VillageTrees.Rocks` decides where rocks belong by asking `HasTillableGround`, which does `Physics2D.OverlapPointAll`. It must give the same answer on every load for "the original state" to mean anything, and it only does while no villager has spawned yet (the call is ordered before `NpcManager.Init`). A cleverly placed object (a chest, a sapling, a villager) would move or drop a rock. Store the original rock cells in the save (or derive them from the tile map alone) instead.
3. **`string.GetHashCode()` used as a stable seed (mine).** `VillageWatch.MutterFor/ScoldFor` pick lines with `npcId.GetHashCode()`. That is stable in Unity's Mono and IL2CPP today but is explicitly not guaranteed (it is randomised per process on modern .NET). The project already has deterministic hashes (`WoodLayout.Random01`, `WeatherRoller.Unit`); use those.
4. **Scene files are rewritten in full on every ship.** `SceneSetup` regenerates all ~40 scene files, and each regeneration changes every line (`Barn.unity`: 1242 lines added and 1242 removed in each of the last three commits, with no real change). That buries real scene changes in git history, makes merges and `git blame` useless for scenes, and bloats the repository. Find the nondeterminism (file ids or ordering in the generator), or stop committing scenes that did not change.
5. **The release process lives outside the repository.** The test, commit, build and upload flow runs from throw-away scripts in a temporary folder (`ship63.sh` ...`ship66.sh`). Three times in this period a script was wrong or empty and nothing ran for a long while. Check one canonical script into `tools/` (with the guards: stop on failure, run `InputAssetGenerator` and `SceneSetup`, commit, build three platforms, then upload and push only on request).
6. **A full PlayMode run takes about 50 minutes.** That is the main cost of every change (three restarts this week), and the "timing-sensitive family" in docs/QA.md is a standing source of retries. Options: run a fast smoke subset for most commits and the full suite nightly or before a release; replace real-time waits (`WaitForSeconds`, 0.4 s polling) with a fake clock where the code under test allows; split the suite across two Unity instances.
7. **No protection against binding two actions to one key (mine).** The new Options rows let the player assign Interact and Rotate to the same key, or Pause to a hotbar key. Detect a clash on completion and either swap the two bindings or refuse with a message. Generic pads have the same issue.

## Structure and design

8. **Service locator everywhere.** 123 calls to `ServiceLocator.Get/TryGet` outside the editor, and 12 `public static ... Current` singletons (`NpcManager`, `PlacedObjectsView`, `MineController`, `AnimalManager`, `HoverInspector` ...). Dependencies are invisible in constructors, and every static needs a `TestResets` entry or a test leaks into the next one (this broke tests twice this week: `ControlPrompts.Kind`, then `ControlPrompts.Rebound`). New code should take its collaborators in `Init(...)`, as `VillageWatch` and `BarkDirector` do, and the statics should be retired gradually.
9. **Large multi-purpose classes.** `PlayerActions` (671 lines) handles input dispatch, every tool, placing, animals, foraging, villager interaction, the hammer and the mouse aim. `HudView` (647) builds and updates a dozen widgets. `GameSession` (partial, 458+) is the state, the clock, the backpack, flags and vars, grids, nodes, the event bus (it now also has `Publish/Subscribe/Unsubscribe` wrappers, mine) and the save entry point. `UiService` (514) lazily creates every screen with `??=`. Splitting by responsibility (tool handlers per tool, a HUD widget per class) would make changes like this week's (a new tool key, villager picking) much smaller.
10. **Long methods.** `FScript.ParseLine` 159 lines, `StorySimulation.Run` 147, `OptionsScreen.Build` 144 (and growing: it now lists about 60 rows), `GameDatabase.Merge` 121, `MapSceneController.Start` 96 (a long list of map-specific `if (_map.MapId == ...)` branches, three more added this week: forest, village, torch). A per-map "map hook" list (the extension point ADR 0002 already describes) would replace those branches.
11. **Two near-copies of the 2D light setup.** `FlickerLight` (candles, lamps) and `TorchLight` (mine) both create a `Light2D`, set the same sorting layers and falloff, and flicker with sums of sines. Let `TorchLight` configure a `FlickerLight` (colour source, radius, on/off) rather than carry its own.
12. **Magic strings.** About 18 raw flag and variable names in code (`GetVar("...")`); button names parsed with `Split('_')` in tests; the `"Key_"`/`"Pad_"` prefixes shared between `OptionsScreen` and its tests; dialogue and key names composed by concatenation (`"village.scold." + kind + ...`). Constants (or small builder functions, as `MutterKey/ScoldKey` now are) avoid a typo failing silently at run time.
13. **Per-frame work that could be cached.** `HoverInspector.Update` does four `ServiceLocator` lookups and several `Camera.main` reads each frame and has a long boolean condition; `HudView.Tick`, `NpcManager.Update` (every villager every frame, with LINQ and string building in the spawn branch) and `CameraFollow.LateUpdate` (`GetComponent`) are the other hot paths found. None is a measured problem; they conflict with the "no per-frame allocations" convention in CLAUDE.md, so fix them when next touching those files.
14. **Time sources.** 52 uses of `Time.time`/`Time.deltaTime` in gameplay and UI, while the game pauses by stopping its own clock, not `Time.timeScale`. That is consistent, but easy to get wrong in new code (a villager walking to the player keeps moving while a menu is open). Worth stating in Tech Design section 4 which timer each kind of code should use.

## Smaller points

15. **Click targeting is generous (mine).** `PlayerActions.PointedVillager` accepts a villager up to two cells away so that a counter does not block a click, but it does not check that the cell between is a counter; a wall would not stop it either.
16. **Hidden global coupling for hints (mine).** `ControlPrompts.Rebound` is a static delegate assigned by whichever `InputService` was constructed last. It works with one service, but a second `InputService` (as the tests create) silently steals it. Pass the resolver to `L.Get`/`ControlPrompts.Expand` explicitly, or have `InputService` own registration and unregistration.
17. **Seven catch blocks without logging.** Most are deliberate and commented (`CrashLog`, `BugReport` tidy-up), but `SaveService` returns the message without logging it and `StoryContent` converts exceptions into a `Fail` string; make sure each reaches `Log` so that a player's bug-report zip contains it.
18. **HUD overlays decide for themselves.** The first-days tip card, the pad hint line, the hotbar label and the item help box each choose independently whether to show, which is how they came to overlap (fixed in 0.4.3). A single "what the HUD may show now" decision would stop a repeat.
19. **Untracked audio.** `audio/*.mp3` (12 files) have shown as untracked in git status in every session. Either commit them (with LFS if large), add them to `.gitignore`, or move them out of the working tree.
20. **Localization is healthy.** 5931 keys, no duplicates, no literal keys missing from the table. The one `LocalizationTests` rule worth keeping in mind: keys built by concatenation (the new `action.Move*`, `village.mutter.*`) are only checked by tests that name them, so each new family needs its own test (the village keys have one).

## What was done about it (0.4.6)

| Item | Result |
| --- | --- |
| 1 Village rules | Partly. `VillageTrees` is now `VillageGreen` (it does trees, rocks, soil and the daily restore). The mutter and telling-off lines are still built in code: the story JSON has no way to give a line to "whoever saw it" (a dialogue node needs a fixed speaker, and the lint judges each speaker's voice), so moving them needs a small story feature first. Left open on purpose. |
| 2 Rocks | Done. Each rock is remembered in the save as a flag (`village.rock:x,y,type`), chosen once; saves from 0.4.5 pick theirs at the next visit. |
| 3 Hash | Done. `NpcInteractions.StableHash` replaces `GetHashCode()`. |
| 4 Scene churn | Looked into, not changed. The whole-file diffs are Unity renumbering object ids and reordering objects each time the scenes are regenerated, which the generator cannot control. The real fix is a policy decision: treat `Assets/_Project/Scenes/*.unity` as generated (git-ignore them, have `tools/ship.sh` and a fresh checkout run `SceneSetup`). Needs the owner's say. |
| 5 Ship script | Done: `tools/ship.sh` (see docs/BUILD.md). |
| 6 PlayMode time | Not changed. A smoke subset needs `[Category]` attributes on the tests, and which tests are the smoke set is a judgement for the owner. `SKIP_PLAYMODE=1` gives a fast EditMode check meanwhile. |
| 7 Key clashes | Done for keys and pad buttons: moving one onto another action's control swaps the two, with a notice. |
| 8 Service locator | Done 2026-10-12 (ADR 0006, phases 0 to 5): `ServiceLocator` and the map singletons are gone; services come through `GameContext` and `MapContext`. |
| 9, 10 Large classes, long methods | `PlayerActions` was split (2026-10-12) into aim, item, placement, fishing and hand-interaction classes. The rest is not changed: each is best done when a feature next needs that file. |
| 11 Light duplication | Done: `TorchLight` now drives a `FlickerLight`. |
| 12 Magic strings | Partly: the Options button-name prefixes are constants used by the screen and the tests. |
| 13 Per-frame work | Done for `HoverInspector` (services and camera looked up once). `CameraFollow` already cached its component (the review was wrong); the other hot spots are untouched. |
| 14 Time sources | Done: a "Which clock" convention in docs/02-TechnicalDesign.md. |
| 15 Click reach | Done: two cells away only across a counter, in a straight or diagonal line. |
| 16 `ControlPrompts.Rebound` | Not changed (works with one input service; the tests that make several reset it). |
| 17 Catch blocks | `SaveService` now logs. The review was wrong about `StoryContent`: its failures are already logged. |
| 18 HUD overlays | Not changed. |
| 19 Untracked audio | Not changed: committing, ignoring or moving 12 files is the owner's call. |

## What looks good

- Assembly separation is respected: nothing depends on `Farm.Mythos`, and the horror layer is data and hooks.
- One MonoBehaviour per file is followed (including this week's additions).
- Saves have a version and a migration chain; the farm flip shipped with a tested migration.
- Tests are broad, and each bug report this month got a test; the build guard keeps developer tools out of release builds.
- No async-void, no `TODO`/`FIXME` markers, no empty catch blocks.
