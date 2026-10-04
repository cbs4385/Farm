# FScript: writing dialogue as plain text

Status: built (T-135). Code: `Assets/_Project/Scripts/Gameplay/Story/FScript.cs`; editor tool: `Farm/Compile Story`. Tests: `FScriptTests`.

FScript is a screenplay-style text format that compiles to the story JSON (ADR 0003) and the English string table. Writers do not type text keys, JSON or ids twice.

## Where files go

- Source: `Assets/_Project/Narrative/**/*.fscript` (create the folder when you write the first file).
- Run **Farm > Compile Story** (or headless: `-executeMethod Farm.Editor.FScriptCompiler.CompileAndExit`).
- Output: `Assets/_Project/Resources/Story/fs_<filename>.json` (generated; do not hand-edit) and new or changed entries merged into `Assets/_Project/Resources/Localization/en.json`.
- Nothing is written if any file has an error. Errors name the file and line.
- Dialogue ids must be unique across files and across the existing hand-written JSON (the loader rejects duplicates).

## Syntax

```
# A comment. Whole lines only.

dialogue wren.first
n0 wren: Well, if it isn't the new farmer! Wren. I run the saloon.
  + flag:met.wren
  next n1
n1 wren: Cider, stories, and a seat by the fire.
  ? I'll stop by, Wren. => r0 | friend:wren,10
  ? Any gossip? => r1
r0 wren: You'd better. I save the good stool for regulars.
r1 wren: Oh, sit down. Where do I start...
  next r1b
r1b wren: Tilda hides the best jam. Between us.

set npc.wren.talk
  100 !flag:met.wren => wren.first
  1 weather:rain => wren.rain1
  0 season:fall && hearts:wren>=3 => wren.sea_fall rarity=uncommon tag=funny cat=seasonal
```

**Dialogue block.** `dialogue <id> [start=<node>]`. The first node is the start unless `start=` says otherwise.

**Node.** `<node id> <speaker>: <text>`. Use `-` as the speaker for narration (the text box shows it in italics with no portrait). A node with no text is allowed (it only runs effects or moves on). Lines under a node:

| Line | Meaning |
|---|---|
| `+ <effect>` | an effect run when the node is shown, such as `flag:met.wren` or `friend:wren,10` |
| `if <condition>` | the node is skipped (to its `next`) when the condition is false |
| `next <node id>` | the following node; leave out to end the conversation |
| `? <text> => <node id> [if <cond>] [| <effect> | <effect>]` | a choice; `-` as the target ends the conversation |
| `key <text key>` | override the generated text key for the node or the choice just above it |
| `expr <name>` | portrait expression: neutral, happy, sad, surprised, embarrassed, thinking |
| `emote <name>` | bubble over the speaker: heart, note, sweat, exclaim, question, ellipsis, sparkle, zzz |
| `sfx <id>`, `voice <id>`, `camera <speaker|player|wide>`, `tag <funny|wholesome|surprise|mystery>` | optional presentation and moment tag for the node |
| `tone <name>` | under a choice: kind, honest, playful, shy, curt (a small icon, never a hidden penalty) |

A node has either choices or `next`, not both. Targets must exist in the same dialogue.

**Set.** `set <id>`, then entries `<priority> <condition or ~> => <dialogue> [options]`. Options: `rarity=` (common, uncommon, rare, legendary), `weight=` (multiplier, default 1), `cooldown=` (days; default 14, and none for priority 4 and above), `tag=` (funny, wholesome, surprise, mystery), `cat=` (category label for reports).

**Topic block.** `topic <id>`, then `npc <villager>`, `label <menu text>`, `plays <dialogue id>`, `if <condition>`, `priority <n>` (higher is offered first), `cooldown <days>` (default 1: once a day) and `once`. The label becomes the string `topic.<id>`. At most two topics are offered after a chat.

**Social block.** `social <villager>`, then `loves`, `likes` and `dislikes` with a list of `joke`, `compliment`, `advice`, `tease`. Reactions are ordinary dialogues named `social.<villager>.<action>.<outcome>` (outcomes: flop, meh, good, great); any missing one falls back to `social.<action>.<outcome>`, where the speaker `*` means the villager being talked to.

**Inline markup in text.** `{pause=0.4}` waits, `{speed=slow}` (slow, normal, fast or a number) changes the typing speed, `{Hello|Hi|Hey}` picks one option (the same one all day), `{if flag:x}...{else}...{/if}` depends on a condition, and `<b>` `<i>` pass through. Names: `[player]`, `[farm]`, `[season]`, `[weekday]`, `[npc:wren]`. Mistakes are reported as `markup` errors by the narrative report.

**Text.** One line per text. `\n` is a line break, `\\` is a backslash. Tokens such as `[player]` and `[farm]` are written as they appear. Choice labels must not contain ` => ` or ` | `.

## Generated text keys

- Node text: `dlg.<dialogue id>.<n>` where `<n>` is the number for node ids like `n0`, `n12`, or the node id itself (`r0_0`).
- Choice text: `dlg.<dialogue id>.c<running index>`, counting choices in the dialogue in order from 0.
- Reuse of a key with a different text is an error. Use `key` to share or to keep an existing key.
- All 299 shipped dialogues follow this convention, which is why the exporter can round-trip them.

## Round-trip and migration

`Farm > Export Story As FScript` writes every shipped dialogue and set as FScript to `Builds/narrative_export/shipped.fscript` for reading or for migrating a villager into `Narrative/`. `FScriptTests.EveryShippedDialogueAndSet_RoundTripsThroughFScript` proves the export compiles back to identical data. To migrate a villager, move their dialogues out of the JSON file and into a `.fscript` file in the same change, so the id appears once.

## Not yet in the format

Reactions (T-091, today written in story JSON), topics (T-097) and social actions (T-141) will get their own blocks. New features are added as optional lines so existing files keep compiling.
