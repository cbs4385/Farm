# Localisation readiness (T-139)

Status: **built**. English is the only launch language (ADR 0005 D4); this keeps adding a language cheap. The string table is `Resources/Localization/en.json` (flat key to text); every user-facing string already goes through `L.Get(key)`. Keys are stable ids (never renamed once shipped).

## For a translator

Run **Farm > Localization > Export CSV** (headless: `-executeMethod Farm.Editor.LocalizationTools.ExportAndExit`) to get `Builds/localization/en.csv`. One row per key:

| Column | Meaning |
|---|---|
| `key` | the stable id; never change it |
| `text` | the English source |
| `context` | where it appears and who says it ("Dialogue wren.spring.1, node n0, spoken by wren", "A speech bubble over the villager's head", "Title of a replayable scene, on a small button") |
| `speaker` | the villager (or `player` for a choice); keep that person's voice |
| `max_words` / `max_chars` | the budget the layout can take (barks 12 words, talk lines 28, gift lines 20, choices 9, event lines 40, letters 80, memory titles 22 characters); translations may run about a third longer than English, so write short |
| `tokens` | what must survive untouched |

**Tokens and markup that must be copied exactly:** `[player]`, `[farm]`, `[season]`, `[weekday]`, `[npc:id]`; `{0}`, `{1}` placeholders; inline markup `{pause=0.4}`, `{speed=slow}`, `{a|b|c}` (you may translate the words inside, keep the bars) and `{if condition}...{else}...{/if}` (translate the text, keep the condition); `<b>` and `<i>` tags.

## Writing rules that make translation possible

- **No joined sentences.** Never build a sentence from pieces in code; one string is one sentence or one thought, so word order can change.
- **Plurals.** Avoid strings that depend on a number ("one egg", "two eggs") by wording around the count; where a count is unavoidable use two keys (`.one`, `.other`), never an `s` appended in code.
- **The player has no gender.** Villagers never say sir, ma'am, lad, lass, handsome or similar to the player; use their name or "friend". Avoid he or she for the player; villagers' own pronouns are fixed by the bible and may be used for villagers.
- **Humour.** Puns and rhymes (Piper's verse, Tilda's slate, Juno's labels) are marked by their `context`; a translator should recreate the effect, not translate word by word. A rhyme that does not survive may be replaced.
- **Names.** Villager and place names are proper nouns and stay.

## Checking a language before it is translated

`pseudoloc` in the developer console (development builds) turns every string into an accented, 35% longer, bracketed lookalike: `⟦Hélló, [player]! ~~~~⟧`. Tokens and markup are left alone, so every screen still works. Look for text that does not fit, anything that stays plain English (a string that bypasses the table), and joined sentences.

Tests: `LocalizationReadinessTests` (every string keeps its tokens when pseudo-localised and still passes the markup check; the export has one row per key with context and budgets; the CSV round-trips the whole table).

## Not done

An import step from a translated CSV into `<lang>.json`, a language selector in Options, and a font check for non-Latin scripts (a Unity Localization swap is only needed if languages beyond English are added).
