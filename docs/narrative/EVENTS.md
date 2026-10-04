# Writing scenes (events)

Status: built (T-100, T-101, T-102). Scenes are story JSON `events` (heart events, festivals, the hall ending). Code: `EventDirector` (plays them), `EventStage` (the effects), `EventFlow` (control flow), `EventSteps` (the vocabulary and the checks), `EventStaging` (is it physically possible), `Memories` (replays). Tests: `EventFlowTests`, `EventStagingTests`, `EventStageTests`, `MemoriesTests`, `MemoriesFlowTests`.

An event has an `id`, a `trigger` (`map`, `dawn` or `manual`), a `map`, a `condition`, `steps`, optional `skipEffects`, and for replays a `titleKey` (festivals use their `calendar` name). Escape skips a scene: the effects it still owed run (following its branches), `skipEffects` run, and the stage is cleared.

## Steps

| Step | Fields | What it does |
|---|---|---|
| `say` | `speaker`, `text`, `expression`, `emote` | one line in the dialogue box |
| `dialogue` | `dialogue` | a whole dialogue graph (this is how a scene gets choices) |
| `move` | `actor`, `x`, `y`, `async` | walk to a cell along a path |
| `place` | `actor`, `x`, `y` | put an actor on a cell |
| `face` | `actor`, `facing` | turn (up, down, left, right) |
| `wait` | `seconds` | pause |
| `advance` | `minutes` | move the game clock on |
| `fadeout`, `fadein` | `seconds` | fade the screen |
| `effects` | `effects` | run effects (flags, friendship, items) |
| `emote` | `actor`, `name`, `seconds`, `async` | a bubble over the actor: heart, note, sweat, exclaim, question, ellipsis, sparkle, zzz |
| `expression` | `actor`, `name` | the speaker's portrait expression for their following lines (`neutral` clears it) |
| `anim` | `actor`, `name`, `seconds`, `async` | a small procedural gesture: hop, jiggle, nod, sway, look, dance (real frames come with T-131) |
| `camera` | `name`, `actor` or `x`,`y`, `seconds`, `value`, `async` | `focus` (follow an actor), `pan` (to a point), `shake` (`value` is the strength), `reset` (back to the player). Always clamped to the map |
| `sfx` | `name` | a sound: click, hoe, water, plant, harvest, coin, error |
| `music` | `name` | a music cue (`stop` ends it); published for the audio pass to play (T-061, T-133) |
| `lighting` | `name`, `seconds`, `async` | a mood over the normal light: day, dawn, dusk, night, warm, dim, `reset` |
| `letterbox` | `name` (`on` or `off`), `seconds` | cinematic bars |
| `spawn`, `despawn` | `id`, `name` (an item id), `x`, `y` | a prop (an item icon) on a cell, for example the gift being handed over |
| `label` | `label` | a place to jump to |
| `branch` | `target`, `condition` | jump to a label when the condition holds (always, if there is none); a jump backward needs a condition |
| `parallel` | `steps` | child steps (move, face, place, wait, emote, anim, camera, sfx, lighting, spawn, despawn, expression) run together; the group ends when all are done |
| `waitFor` | `actor` (optional) | wait for the `async` steps of that actor, or all of them |

Any step may carry a `condition`: it only runs while it holds. A step marked `async` starts and the scene carries on (only move, emote, anim, camera and lighting can be async).

## Templates

Most heart events are one of eight patterns, so a writer fills in a villager, a stage cell and some text keys instead of writing steps. Templates live in `Resources/Story/event_templates.json`; an instance goes in any story file:

```json
{ "eventsFromTemplates": [
  { "id": "wren_heart8", "template": "heirloom",
    "args": { "npc": "wren", "map": "Saloon", "px": 6, "py": 5, "nx": 6, "ny": 6, "item": "artisan.wine",
              "intro": "event.wren_heart8.0", "give": "event.wren_heart8.1", "thanks": "event.wren_heart8.2",
              "when": "hearts:wren>=8 && !weekday:tue", "reward": [ "friend:wren,60" ] },
    "event": { "titleKey": "memory.wren_heart8", "priority": 7 } } ] }
```

| Template | The scene | Needs (besides npc, map, px, py) |
|---|---|---|
| `shared_activity` | the villager shows the player something they do; gesture and emote | `intro`, `doing`, `wrap`; optional `gesture`, `emote` |
| `confession` | bars, dim light, the camera on the villager, and a conversation (a dialogue with choices) | `intro`, `talk` (a dialogue id), `after` |
| `heirloom` | the villager hands over an item that appears on screen, heart emote, item given | `nx`, `ny` (the villager's cell), `item`, `intro`, `give`, `thanks` |
| `shared_meal` | a dish appears, both react, a little energy returns | `nx`, `ny`, `dish`, `intro`, `bite`, `wrap` |
| `helping_scene` | asks for help; succeeds if `need` (a condition) holds and is then spent, otherwise says to come back and is offered again | `need`, `ask`, `success`, `fallback` |
| `prank` | a harmless joke: jiggle, sound, camera shake, a surprised player, then a conversation | `setup`, `prank`, `talk`, `reaction` |
| `performance` | warm light, bars, the camera and a music cue, a dance with notes, applause | `intro`, `cue`, `outro` |

Every template also takes `when` (the condition, default `true`) and `reward` (a list of effects, default none), sets `flag:event.<id>` when it completes (the helping scene only when it succeeds), and makes the event's id available as `${id}`. `px`, `py` is where the player is moved to; the villager is expected one cell below, as in the shipped heart events. A parameter the template does not declare, a missing one, or a leftover `${...}` is a load error. The expanded event is a normal event: add a `titleKey` to make it a memory.

## Rules that are checked

- The validator (`Farm > Validate Data`, `EventSteps.Problems`) rejects unknown step types, actors, words, labels, bad branches, nested parallels, a prop removed before it was spawned, and so on.
- `EventStaging` (tested against the real map scenes) checks that everyone is placed and walks to free floor, a scripted walk has a path, props and camera targets are on the map, and two actors never share a cell.
- Whatever a scene changes (camera, lighting, bars, props, bubbles, gestures) is undone when it ends or is skipped.

## Memories

A scene with a title (or a festival) appears in the **Memories** tab once it has been seen. Replaying it plays the scene on its own map with every effect off (no friendship, items, flags, clock changes, no `EventFinished`), brings in villagers who are elsewhere, then returns the player to where they were. Titles are `memory.<event id>` in `en.json` and must be 22 characters or fewer.
