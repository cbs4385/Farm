# Writing style guide

Status: **draft for owner review** (T-082). Read `BIBLE.md` first. This guide is about the page: length, rhythm, tags, rules. The bible is about the people.

## 1. Principles

1. **Write for the clip.** A line should land without context. If someone sees ten seconds of a stream, they should get the joke, the warmth or the question.
2. **Voice before wit.** A joke that any villager could say is not a Wren joke. Check the bible's *humour type* before keeping a line.
3. **Short beats long.** One idea per line. The funniest word goes last. Cut the setup if the line still works.
4. **Specific beats generic.** "The returns box is sticky again" beats "the library is old". Numbers, objects and names make lines quotable.
5. **Warm by default.** Teasing is affectionate and aimed at the situation, not the player. No villager is contemptuous of the player and none ever scolds.
6. **Make the player feel noticed.** Where a line can refer to the season, the weather, the farm, a past choice or a neighbor, it should.

## 2. Length budgets (enforced by the validator, T-136)

| Type | Words | Notes |
|---|---|---|
| Bark (speech bubble) | at most 12 | one sentence |
| Talk line | at most 28 | up to two sentences |
| Event / scene line | at most 40 | |
| Choice label | at most 9 | start with the verb or the feeling |
| Gift reaction | at most 20 | |
| Letter | at most 80 | |

No line may need more than three text-box lines at the default font. Split instead.

## 3. Language rules

- **Spelling:** American English (decided; see `BIBLE.md` section 7). The validator checks a list of British forms and Britishisms (neighbour, favourite, cosy, colour, autumn, biscuit, plaster, till for a shop counter, and others). Contractions are normal; no dialect spelling.
- **The player is "you".** Never assume the player's gender, body, name format or relationships. Use `[player]` sparingly (once in about five lines; more reads as a tic), `[farm]` where the farm is the point. Avoid gendered terms for the player (no "lad", "lass", "boy", "girl", "sir", "ma'am").
- **Words to avoid:** real brands, real places other than the generic New England feel, modern slang that will date, internet jokes, pop-culture quotes, insults aimed at groups or bodies, medical advice presented as fact.
- **Swearing:** none. Mild exclamations ("good grief", "for pity's sake") are fine.
- **Numbers:** write numbers under ten as words in dialogue; keep exact numbers as digits when the precision is the joke ("eleven times" is a word; "41 years" can be digits if the gag is the number). Pick one per gag and keep it.
- **Punctuation:** em dashes are allowed but sparingly; ellipses mark hesitation and are never used for a fade-out in more than one line in ten; one exclamation mark per line at most, and none for Hazel and Bram.
- **Tokens:** `[player]`, `[farm]`. More tokens arrive with T-094 (`[season]`, `[pet]`, `[crop]`, `[neighbor:<npc>]`).

## 4. Humour rules

- **Types:** raconteur (Wren), deadpan understatement (Hazel), deadpan minimalism (Bram), plus the types in the bible for the others. A villager keeps to theirs.
- **Punch sideways or up.** At the villager, at a thing, at a situation. Never at the player's looks, ability, gender, money, health or culture.
- **Callbacks are currency.** A running bit gets better each time. Every bit has an arc: introduce, repeat, twist, pay off.
- **Comic failures are kind.** When a social action or a gift goes badly it is funny and forgiving. The villager laughs it off or is endearingly stubborn, never hurt for long.
- **No cringe quotas.** If a line needs an exclamation mark to be funny, it is not.

## 5. Moment tags and rarity

Every line that is meant to be remembered carries a **moment tag**. The validator checks the quotas per villager (Full: at least 3 funny, 2 wholesome, 1 surprise, 1 quotable bit).

| Tag | Meaning |
|---|---|
| `funny` | lands as a joke without context |
| `wholesome` | a small kindness that makes a viewer smile |
| `surprise` | an unexpected turn or reveal |
| `mystery` | a hook the player will want to follow (cozy hooks first; horror hooks live in `Farm.Mythos`) |

(The quotable running bit counts toward `funny` or `wholesome` and is marked in the bible.) Tags go on set entries today (`tag=` in FScript). Node-level tags arrive with T-095.

**Rarity** (`rarity=` on set entries; the engine weights them 100 / 30 / 8 / 1):

| Rarity | Use for | Per Full villager |
|---|---|---|
| common | the everyday pool | most lines |
| uncommon | a good line that should not show every week | about 10% |
| rare | a standout line, a small treat | 3 |
| legendary | the line people will search for | 1 |

A rare or legendary line must work out of context and must not carry plot or a flag. Never make the only line that explains something rare.

## 6. Categories and quotas (for a Full villager)

| Category (`cat=`) | Count | Notes |
|---|---|---|
| greeting | 10 | first words of a visit; mood-aware once T-093 exists |
| seasonal | 40 | 10 per season, tied to `season:` |
| weekday | 14 | tied to `weekday:` and the villager's schedule |
| weather | 16 | 4 per weather type |
| place / time | 20 | tied to `hour` and `map` |
| tier | 30 | friendship stages, 10 each |
| reactive | 30 | farm, calendar, neighbors, past choices |
| idle / bark | 30 | speech bubbles |

Each line is **tagged with its category** so the validator can report gaps.

## 7. Choices

- A choice has two to four options, each under nine words, each a distinct *attitude* (kind, honest, playful, shy, curt), never a hidden right answer.
- A choice that matters sets a flag named `choice.<villager>.<event>` and is **referenced by at least one later line** (`choice:<flag>` atom once T-092 exists).
- Never punish. A "wrong" choice costs a laugh, not friendship.
- Keep the first option the warmest and the last the funniest.

## 7b. Topics and social actions

- A **topic** is 2 to 4 short nodes ending on a line that works alone. Open with the villager's voice, not with information. Gate it with `hearts:` so deeper topics reward friendship, and use `{if ...}` so it reacts to the player's farm.
- A **social reaction** is one line (at most 20 words). Write great, good and flop for each action; leave meh to the generic lines unless the villager's meh is a character moment. Flops are funny and forgiving, never hurtful. Match the profile: a villager who loves an action must never flop it.
- Reactions may use stage directions in parentheses for non-verbal beats, plus `expr` and `emote`.

## 8a. The clip checklist (checked by the narrative report)

A scene tagged `funny`, `wholesome`, `surprise` or `mystery` is a set piece, and is checked as one: **a title** (so players can replay it), **under 150 seconds**, **at least one beat the eye or ear can catch** (an emote, a gesture, a camera move, a light, a sound or a prop), and **it ends on a spoken line** (the payoff). A shareable moment also lands without context: someone who sees only its last ten seconds should still get it.

## 8. Heart events and set pieces

- Structure: **setup, turn, payoff** in 25 to 45 lines, with an emote or camera beat at the turn once T-100 exists.
- Open with an action, not an explanation. End on a line that works as a standalone clip.
- Each event: one flag set, one reference later, one remembered detail.
- Do not reveal lore in a cozy event. Hooks are questions, not answers.

## 9. Voice-lint (what the validator checks, T-136)

Run `Farm > Narrative Report` (or `-executeMethod Farm.Editor.NarrativeTools.ReportAndExit`) to get `Builds/narrative_report.md`. Rules live in `Assets/_Project/Narrative/narrative_rules.json`. Errors (spelling, priority bands) fail the data tests; everything else is a warning until a villager is listed under `locked`, when every warning becomes an error.


- Per villager: banned words, required tics (for example Bram's *Hm* frequency), maximum sentence length, exclamation marks, use of `[player]`.
- Global: length budgets, spelling list, duplicate text, forbidden brands and slurs, repeated openers (no more than three lines in a villager's pool starting with the same word).

## 10. Review and gates (T-083)

Per villager: **outline** (bible entry locked) → **draft** (agent) → **consistency edit** (check against the entry and neighbors) → **voice lint** (tooling) → **sensitivity read** (all-ages, humour targets, care topics) → **integration** (compile, validate, simulate) → **human read** (named reviewer; recorded in `STATUS.md`) → **lock**. A villager is **locked** when the reviewer signs off and the simulation targets pass. Locked lines change only through a ticket.

## 11. Mythos compatibility

- Cozy text never denies, explains or hints at the cult. Foreshadowing is ambiguous and works at horror level 0 as local color.
- Reserved priority bands: cozy 0-7, mythos 8-10, first meeting and tutorial 100.
- A cozy line that would be contradicted by a clue scene is rewritten, not patched in the mythos data.
