# ADR 0006: Retire the service locator (plan)

Status: **done** (2026-10-12; phases 0 to 5 built, see section 8). Written as a plan for the owner to approve; sections 1 to 7 are the plan as approved. It answers item 8 of `docs/CODE_REVIEW_2026-10-11.md` (service locator everywhere, twelve `Current` singletons).

## 1. What is there today (measured 2026-10-12)

- `Farm.Core.ServiceLocator` is a static `Dictionary<Type, object>` with `Register/Get/TryGet/Unregister/Clear`.
- **138 uses in the game code** (86 `TryGet`, 38 `Get`, 11 `Register`, 3 `Clear`): Gameplay 94, UI 28, Mythos 10, Core 6. **281 more in the tests.**
- Eleven service types are registered, by use: `IUiService` 27, `GameSession` 22, `SettingsStore` 14, `SceneLoader` 13, `AudioService` 9, `InputService` 9, `EventBus` 7, `SaveService` 4, `AtmosphereService` 2, `UiService` 2 (the same object as `IUiService`).
- The files that lean on it most: `PlayerActions` (13), `GameServices` (8, the registrations), `MapSceneController` (8), `MainMenu` (7), `Bootstrapper` (6), `EventDirector` (6), `UiService` (6), `InputDeviceWatcher` (5), `GameSession` (5).
- **Twelve static "current instance" singletons** do the same job for things that live in a map scene: `NpcManager`, `PlacedObjectsView`, `AnimalManager`, `EnemyManager`, `MineController`, `EventDirector`, `MusicDirector`, `UiAccess` (a static front for the UI), `HoverInspector.Current` (a label), `NpcOutfits`, `PlatformServices`, `Sway`.
- **22 `TestResets.Add` registrations** exist only so that one test's statics do not leak into the next. Four bugs this month were a leaked static (`ControlPrompts.Kind`, `ControlPrompts.Rebound`, the cursor and pointer actions, a stale `InputService`).

## 2. Why bother

1. **Hidden dependencies.** A class that asks for `GameSession` inside `Update` cannot be read, tested or reused without knowing the whole game's startup. `HoverInspector` does four lookups per frame to find out what it needs.
2. **Order and lifetime bugs.** `Get` throws if a service is not registered yet; `TryGet` silently does nothing, which turns a wiring mistake into a feature that quietly does not work (a missing HUD toast, a skipped announcement). Both have happened.
3. **Test friction.** Tests must build the real service root, or register a half set, and then clean up statics by hand (`ResetForTests`, `TestResets`).
4. **Obstacle to refactoring.** The large classes (`PlayerActions`, `HudView`, `GameSession`) cannot be split cleanly while any piece can reach any service from anywhere.

Not a reason: performance. The lookup is cheap. Do not do this for speed.

## 3. Principles

- **A composition root builds the graph once; classes receive what they use.** No class below the root asks a static for a service.
- **Two lifetimes, two containers, both plain objects.**
  - `GameContext`: lives for the whole run (created by the `Bootstrapper`): `EventBus`, `SettingsStore`, `SaveService`, `GameSession`, `InputService`, `AudioService`, `AtmosphereService`, `SceneLoader`, the UI (`IUiService`), platform services.
  - `MapContext`: lives for one loaded map (created by `MapSceneController`): the `GameContext` plus the map's own things (`FarmMap`, `FarmMapView`, player, `NpcManager`, `AnimalManager`, `PlacedObjectsView`, `EnemyManager`, `MineController`, `EventDirector`, `HoverInspector`). It replaces the twelve `Current` singletons.
- **Pass narrow interfaces, not the whole context, where a class needs one or two things.** `IUiService` already exists and is the model; add small ones (`ISettings`, `IAudio`, `IMapTravel`) only where a class is worth testing on its own.
- **Unity `MonoBehaviour`s get their collaborators through an `Init(...)` call from their creator** (the pattern `VillageWatch` and `BarkDirector` already use), never through a constructor Unity cannot call, and never by `FindObjectOfType`.
- **Keep statics that are not services**: the localization table (`L`), `Conditions`, `Log`, pure helpers. They are configuration and pure functions, not collaborators. `ControlPrompts` is the exception: its mutable `Kind` and `Rebound` move into `GameContext` (an `InputHints` object owned by the input service).

## 4. Target shape (illustrative)

```csharp
public sealed class GameContext            // built once by Bootstrapper
{
    public EventBus Bus; public SettingsStore Settings; public SaveService Saves;
    public GameSession Session; public InputService Input; public AudioService Audio;
    public AtmosphereService Atmosphere; public SceneLoader Scenes; public IUiService Ui; public IPlatformServices Platform;
}

public sealed class MapContext             // built by MapSceneController for each loaded map
{
    public GameContext Game; public FarmMap Map; public FarmMapView View; public PlayerController Player;
    public NpcManager Npcs; public AnimalManager Animals; public PlacedObjectsView Placed; public EnemyManager Enemies; ...
}
```

Map-scene components receive a `MapContext` in `Init`. Persistent services receive what they need in their constructors. `Bootstrapper` is the only place that builds a `GameContext`; `MapSceneController` the only place that builds a `MapContext`. Tests build either by hand (`new GameContext { Bus = new EventBus(), ... }`) with fakes for what they do not exercise, which also ends the need for `TestResets` for these types.

## 5. How to get there without a big bang

The game has to stay green and shippable after every step. The old `ServiceLocator` stays as a thin, obsolete shim until the last phase and is fed from the same `GameContext`, so converted and unconverted code run side by side.

**Phase 0: stop the growth (half a day).**
- Add `GameContext`, `MapContext` and the shim: `Bootstrapper` builds the `GameContext` and also registers its members in `ServiceLocator` as today.
- Add a **ratchet test** `ServiceLocatorUseTests` that counts `ServiceLocator.` uses per file against a committed baseline file (`tools/servicelocator-baseline.txt`) and fails when any file goes up or a new file appears. The number can only fall. New code must take its collaborators in `Init`. Mark `ServiceLocator` `[Obsolete]` for game code (warnings, not errors) once the count is low enough to read.

**Phase 1: the leaf services (1 to 2 days).** `SettingsStore` (14), `SaveService` (4), `AtmosphereService` (2), `AudioService` (9), `SceneLoader` (13). They have few dependants, mostly menu and map code. Convert callers to receive them through the `MapContext` or the constructor of their creator. `UiService` (2) goes with the UI in phase 3. Exit: those five types have no `ServiceLocator.Get/TryGet` outside `Bootstrapper` and the shim.

**Phase 2: `EventBus`, `InputService`, `GameSession` (3 to 4 days).** The core. `EventBus` (7) and `InputService` (9) first. `GameSession` (22) last, because most of its callers are the large classes. Convert `PlayerActions` (13 uses) as part of splitting it (item 9 of the review): its tool handlers receive `MapContext`, and the split falls out of the cleanup. Also move `ControlPrompts.Kind/Rebound` into an `InputHints` owned by `InputService`.

**Phase 3: the UI (2 to 3 days).** `IUiService` (27) and `UiAccess` (the static front). Gameplay classes already hold `IUiService`; give each its reference through `MapContext.Game.Ui`. `UiAccess.Run(...)` goes last: it exists so code that has no UI reference can still pop a message. Replace its callers with the reference they now have; keep a single `INotifier` (toasts, messages) if a few places truly need fire-and-forget.

**Phase 4: the twelve `Current` singletons (2 to 3 days).** One at a time, each becomes a field of `MapContext` set by `MapSceneController`; delete the static and its `TestResets.Add`. Order by number of users: `NpcManager`, `PlacedObjectsView`, `MineController`, `AnimalManager`, `EnemyManager`, `EventDirector`, `MusicDirector`, then the rest (`HoverInspector.Current` and `Sway` and `NpcOutfits` are not services and can become plain static helpers or fields of what owns them).

**Phase 5: delete the locator (half a day).** When the baseline is zero outside `Bootstrapper`, remove `ServiceLocator`, the `[Obsolete]` shim and every remaining `TestResets.Add` that guarded a service; keep `TestResets` only for true configuration statics (`L`, `Conditions`).

**Tests, throughout.** The 281 test uses are converted per phase with the code they touch: a helper `TestContext.Create(...)` builds a `GameContext` with real or fake parts, so PlayMode fixtures stop calling `Bootstrapper.InitializeServices()` and stop sharing global state. This also makes it possible to run PlayMode tests in parallel later (review item 6), which is the real payoff.

Total: roughly **9 to 13 working days**, each phase shippable by itself, none blocking feature work.

## 6. Risks and how they are handled

- **A forgotten wiring is silent today and a null reference tomorrow.** That is the trade: the failure moves to startup, where `Bootstrapper` and `MapSceneController` check that every field of the context is set (one assertion each) and the ratchet and the PlayMode suite catch the rest.
- **Scene objects created in the editor (`MapBuilder`) cannot take constructor arguments.** They get an `Init(MapContext)` call from `MapSceneController`, as `VillageWatch`, `BarkDirector` and `HoverInspector` do now. Scenes do not change (and need no hand-edited YAML).
- **Mythos and other optional modules** register hooks, not services; their ten locator uses are mostly `GameSession` and `EventBus` and follow phase 2. Nothing in the core may depend on Mythos (ADR 0002), so `GameContext` exposes hook registries, not Mythos types.
- **Merge friction** with feature work: each phase touches the same large files. Do phase 2 together with the `PlayerActions` split rather than twice.
- **Rollback.** Because the shim is kept until phase 5, any phase can be reverted on its own.

## 7. Decision needed

- Approve the plan (or the order), and say whether to start with phase 0 now: it is small, changes no behavior, and makes every later phase safer.
- Say whether `INotifier` (fire-and-forget toasts and messages) is acceptable as the one remaining narrow, injected "global-ish" service, or whether every caller must hold `IUiService`.

## 8. What was built (2026-10-12)

- Phases 0 to 5 as planned, one commit each (phase 2 with the `PlayerActions` split and `InputHints`). `ServiceLocator`, its tests, the ratchet test and `tools/servicelocator_baseline.py` are deleted.
- Scene components that need services implement `IMapBound` (`Bind(MapContext)`), and `MapSceneController` binds every one in the scene once the map is built; components made in code are bound by their creator. This replaced the lazy `TryGet` in `Update` loops.
- The twelve singletons: seven (`NpcManager`, `PlacedObjectsView`, `AnimalManager`, `EnemyManager`, `MineController`, `EventDirector`, `MusicDirector`) became fields of `MapContext`/`GameContext`. `HoverInspector.Current` is a diagnostic label, `NpcOutfits` and `Sway` take the session as an argument, and `PlatformServices` stays as the platform adapter.
- What is still static on purpose: `GameContext.Current` (the entry anchor), the localization table, and `ControlPrompts.Current` (the current `InputHints`, so the language table can expand `[[Action]]`). `SingletonRatchetTests` fails if another `static Current` appears.
- Not done: PlayMode fixtures still call `Bootstrapper.InitializeServices()` and share the one run, so they cannot run in parallel yet (no `TestContext` helper was built).
