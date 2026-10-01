# Status

Last updated: 2026-10-01. Tags: `m0`, `m1`. Tests: 138 EditMode + 15 PlayMode pass. Next: Milestone 2 (start with T-043, T-030, T-031, T-034). The horror layer ships with 1.0 (Milestone 3b).

Format: `T-xxx | status (todo/in-progress/done/blocked) | agent/date | notes`. "Done (partial)" lists what was not verified.

## Milestone 0 - Foundation (done)
T-001 | done | claude/2026-10-01 | repo initialized, LFS attributes, gitignore
T-002 | done | claude/2026-10-01 | `ProjectConfigurator`; Mono for dev, IL2CPP via build flag
T-003 | done | claude/2026-10-01 | asmdefs and test assemblies
T-004 | done | claude/2026-10-01 | template cruft removed; Localization, Newtonsoft, Cinemachine added
T-005 | done (partial) | claude/2026-10-01 | Win + Linux Mono builds verified. NOT verified: IL2CPP builds (Linux module not installed), CI workflow (no remote)
T-006 | done | claude/2026-10-01 | Bootstrap, ServiceLocator, EventBus, SceneLoader, Log
T-007 | done | claude/2026-10-01 | PixelPerfectCamera + `PixelSnapCamera`; verified at 1x-4x on Windows and Linux (WSLg); constant-speed pan confirmed smooth by the owner. Re-check judder with real movement; if seen, add sub-pixel smoothing (render at reference res, offset the upscale)
T-008 | done (partial) | claude/2026-10-01 | placeholder sprites as individual PNGs; atlases deferred to T-060
T-009 | done (partial) | claude/2026-10-01 | both builds boot; Linux only under WSLg, not on a real GPU

## Milestone 1 - Vertical slice (done)
T-010 | done (partial) | claude/2026-10-01 | generated input asset, rebinding persisted. NOT verified: gamepad on hardware, rebinding persistence by a person
T-011 | done | claude/2026-10-01 | clock/calendar/pause/pass-out; debug overlay moved to T-043
T-012 | done | claude/2026-10-01 | items, crops, database, inventory
T-013 | done | claude/2026-10-01 | player, camera follow + bounds + pixel snap (custom camera, ADR 0001)
T-014 | done | claude/2026-10-01 | Farm + FarmHouse, warps, spawn points
T-015 | done (partial) | claude/2026-10-01 | hoe, watering can, seeds, hotbar, energy; axe/pickaxe/scythe have no targets (T-032)
T-016 | done | claude/2026-10-01 | growth/regrow/season death, 6 spring crops
T-017 | done | claude/2026-10-01 | sleep, summary, pass-out, lighting, sunny/rain weather
T-018 | done | claude/2026-10-01 | HUD, backpack, tooltips, toasts (click-to-move)
T-019 | done | claude/2026-10-01 | shipping bin, selling, temporary shop stall
T-020 | done | claude/2026-10-01 | 3 slots, atomic writes, `.bak`, migrations, autosave
T-021 | done | claude/2026-10-01 | main menu, options, settings
T-022 | done | claude/2026-10-01 | `L.Get` + string table + lint tests (ADR 0001)
T-023 | done (partial) | claude/2026-10-01 | logical buses + placeholder blips; no mixer/music
T-024 | done (partial) | claude/2026-10-01 | M1 gate. NOT verified: IL2CPP, Steam Deck, real Linux GPU

## Extension points (ADR 0002, done)
Conditions language, moon phase, flags/vars/module data (saved), day-cycle hooks, weather modifiers, atmosphere layers, text filters/extra tables, content packs, map-loaded event, `Warp.Condition`, `ConditionalObject`, HUD widgets, module system, inert `Farm.Mythos`, `HorrorLevel` setting. Tested in EditMode and PlayMode (including a test module using every hook in the real game).
Not built yet: hooks for NPCs/dialogue/events/weather definitions/journal (come with M2, requirements in the plan and ADR 0002); HorrorLevel control in Options (X-009).

## Milestone 2 - World and living village (next)
T-043 | todo | | developer/QA tools (dev builds only)
T-030 | todo | | weather as data
T-031 | todo | | village, forest, beach, interiors
T-032 | todo | |
T-033 | todo | |
T-034 | todo | | dialogue (ADR first)
T-035 | todo | |
T-036 | todo | |
T-037 | todo | |
T-038 | todo | |
T-039 | todo | |
T-040 | todo | | data validator incl. conditions
T-041 | todo | |
T-044 | todo | | hook conformance / HorrorLevel-0 equivalence
T-042 | todo | | M2 gate

## Human playtest results (Windows, 2026-10-01)
Confirmed by hand: options scrolling, avatar/cursor alignment, sleep + day summary, till/water/plant/grow/harvest/ship over 6 days, buying seeds, energy bar and exhaustion message, save and load from the main menu and after a full restart, seeds planting with clearer feedback.
Bugs found by playing and fixed (each has a test): options not scrolling, avatar drawn a tile below its logical position (sprite import mode), sleep ending on a black screen (fade drawn over the summary), seedling nearly invisible and silent failed actions; earlier, a scene that crashed player builds (two MonoBehaviours in one file).
Still untested by a human: gamepad, passing out at 2 AM (in progress), rain days, key rebinding persistence, season change (spring -> summer crop death), real Linux hardware/Steam Deck, IL2CPP builds, CI.

## Milestone 3b - Mythos layer (required for 1.0; not started)
X-000..X-011 | todo | | see the plan. X-000 (lore bible) needs open questions G-K answered; the draft already holds the decided names and numbers

## Decisions and open questions (owner)
Decided 2026-10-01 (GDD section 9, 1-8 and A-E): the horror layer ships with 1.0; the Community Hall is a cozy arc the cult quietly uses; Nharoth, a sleeping cosmic entity, is kept asleep by the Keepers of the Covenant's rituals, and full awakening ends the world; wakefulness rises 25% per season and each ritual lowers it 30-40%, rituals each new moon in Harrow Wood, world changes every 5%; the cult looks menacing but protects the world; resisting risks waking the god unless another way is found; about a third of NPCs are Keepers (romance allowed, never exploitative); dread (mild, optional, never blocking) affects luck, dialogue options, seasonal events, weather and NPC attitude decay, gates crops, and ordinary crops may mutate; the main shop does not sell horror seeds; default intensity full; New England village Bellweather.
Hooks and code added because of the answers: crop grow conditions, shop opt-in and conditions (`ShopCatalog`), luck modifiers (`ILuckModifier`, `GameSession.Luck`), `WakefulnessModel` (the owner's numbers as tested pure functions, not yet driving the game), reserved ids and names (`village.name`, `mythos.*`, map id `HarrowWood`). 138 EditMode + 15 PlayMode tests pass.
Still open (GDD section 9): F scope protection (needed before M3), G ritual cadence on the 28-day lunar cycle, H what an unsuccessful ritual does and what decides success, I what mild removes, J wakefulness visibility, K spelling of "Bellweather".
