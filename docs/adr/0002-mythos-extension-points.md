# ADR 0002: Extension points for the planned cosmic-horror ("mythos") layer

Status: accepted (2026-10-01).

## Context
The game will eventually carry a Lovecraftian layer: the remote village hides a cult serving an Elder God that lives in the neighbouring woods. The base game must stay a complete, shippable cozy farming game, and the horror must be addable, tunable and removable without rewriting core code. This ADR records the hooks built now (before NPCs, dialogue, events and more maps exist) and the requirements they place on later milestones.

## Decision
1. **Everything horror-specific lives outside core code**, in its own assembly `Farm.Mythos` (`Assets/_Project/Scripts/Mythos`) plus data. Core code may know about *generic* hooks, never about cults or gods. `MythosModule` exists today but is inert (it only registers English names for the reserved weather ids).
2. **The player controls the intensity.** `SettingsData.HorrorLevel` (0 off, 1 mild, 2 full; default 2) is in the settings file and in `ModuleContext.HorrorLevel`. Every module and hook must respect it. 0 means the plain farming game. (An Options screen control is a to-do: X-009.)
3. **A hook that fails must never break the game.** Day-cycle hooks, weather modifiers, map-loaded handlers and module initialization are wrapped: the error is logged and the rest continues.

## Extension points (implemented and tested)

| Need | Hook | API |
|---|---|---|
| Register a layer of content | Module system | `IGameModule`, `GameModules.Register` (from a `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`), `Initialize(ModuleContext)` gives bus, session, hooks, database, settings |
| Story state (suspicion, dread, knowledge, ritual progress) | Flags and variables | `GameSession.SetFlag/HasFlag`, `SetVar/GetVar/AddVar(min,max)`; events `FlagChanged`, `VarChanged`; saved in `GameState.Flags/Vars` |
| A module's own persistent data | Module data | `GameSession.GetModuleData<T>(id)` / `SetModuleData`; saved as JSON in `GameState.ModuleData`; older saves load fine |
| "Only when..." in data (NPC schedules, dialogue, doors, events, shop stock) | Conditions | `Conditions.Evaluate("flag:x && var:dread>=3 && moon:full && !weather:rain && hour>=20", session.World)`. Atoms: `flag var season weather moon map hour day year`; add more with `Conditions.Register`. `Conditions.Validate` is for the data validator |
| Moon and night rhythms | Calendar | `GameDateTime.MoonPhase` (8 phases, full moon days 15-18 each season) |
| Overnight events: dreams, blight, offerings, sleepwalking | Day-cycle hooks | `IDayCycleHook` / `DayCycleHook`: `OnNightFalls` (after shipping, before crops grow) and `OnDawn` (after the new day is set). `DayCycleContext` exposes state, grids, items, crops, world query, `Note(key, args)` for extra summary lines, and `WakeMap/WakeSpawn` |
| New weather (fog, blood moon) | Weather modifiers | `IWeatherModifier` chain over the deterministic roll; new ids need `weather.<id>` strings (`L.AddTable`) |
| Mood: tint, dread | Atmosphere | `AtmosphereService.Stack.Set(id, tint, strength, priority)`; the day/night light blends it in, indoors too |
| Distorted or secret text | Localization | `L.AddFilter((key, text) => ...)` post-processes every string (keys let you target `npc.*`); `L.AddTable` adds strings |
| Extra items and crops (offerings, strange seeds) | Content packs | `ContentPack` assets under `Resources/Packs` merge into `GameDatabase`; id clashes with core are rejected |
| Reacting to a map | Map hook | `GameHooks.MapLoaded` (spawn objects, push atmosphere, show/hide things) |
| Gated paths and hidden objects | Scene components | `Warp.Condition` (+ blocked message), `ConditionalObject` (shows children while a condition holds; updates on flags, vars, hour, new day) |
| Meters and indicators | HUD widgets | `GameHooks.AddHudWidget(() => IHudWidget)`; hosted top-left by the UI layer |

Reserved names (flags, vars, weather, atmosphere layers, map ids) are in `MythosIds`. They are placeholders and can be renamed freely.

## Requirements for upcoming milestones (so the hooks stay useful)
Not built yet; each must be designed to use the hooks above.

- **T-030 weather:** weather is data (id, name key, tint, particles, effects) rather than special cases in lighting code, so modules can add weather.
- **T-031 maps:** the Forest map carries a condition-gated warp slot for a future `Woods` map (closed in the base game; the Woods scene itself ships with the mythos content, X-002). Village exits use `Warp.Condition` wherever a door might later be locked. Reserve the map ids.
- **T-034 dialogue and T-041 events:** lines, choices, events and cutscene steps accept a `Condition` and can set flags and variables. Text goes through `L.Get` so filters apply.
- **T-035 NPCs:** `NpcDefinition` gets an allegiance/secret field; schedule entries, dialogue sets and gift reactions accept a `Condition`, so a cultist can have a hidden night schedule and different lines once a flag is set.
- **T-036/T-039 UI and quests:** the journal supports extra pages (a lore page); quest availability accepts a `Condition`.
- **T-040 data validator:** validates every `Condition` string with `Conditions.Validate`, and every `weather.*` / hook-supplied string key.
- **T-061 audio:** music and ambience layers can be switched by condition or atmosphere layer; leave room for an audio filter.
- **T-063 accessibility and T-021 options:** add the HorrorLevel control (X-009), plus content notes on the store page.

- **T-043 developer tools:** can set flags/variables and skip time, so story logic is testable in seconds.
- **T-044 conformance tests:** a scripted playthrough with test modules registered, versus none, and at `HorrorLevel` 0 must produce identical game state; hooks stay exercised in PlayMode.

## What the intensity levels mean
- **0 (off):** the layer is inert: no hooks registered, no content visible, no foreshadowing. The game is the plain farming sim.
- **1 (mild):** unease without the most disturbing text and imagery: dread tinting, fog, hints, odd NPC behaviour; no explicit ritual content, no distortion of text.
- **2 (full):** everything.
Modules decide per feature which level it needs and check `ModuleContext.HorrorLevel`. The level can change between sessions; saved story state must remain valid at any level (a flag set at level 2 must not break a level 0 session).

## Verification
Unit and PlayMode tests cover each hook (ordering, failure isolation, saving of flags/vars/module data, older saves loading), the condition language, and a test module that uses every extension point inside the real game. The inert `Farm.Mythos` module is tested to add no hooks. T-044 extends this to full playthroughs.

## Related
GDD section 7 (content guidelines), section 8 (the layer), section 9 (open questions); Tech Design section 3.18; `docs/mythos/LORE.md`; plan Milestone 6.

## Rules for contributors
- No cult, god or horror content in core assemblies (`Farm.Core/Data/Gameplay/UI`): only generic hooks.
- New story state goes in flags/vars/module data, never new `GameState` fields (those are for core systems and need migrations if they change shape).
- Never rename a shipped flag, variable, weather id or map id (saves depend on them).
- Respect `HorrorLevel` in every module hook.
- One MonoBehaviour per file, named like the file (see ADR 0001).
