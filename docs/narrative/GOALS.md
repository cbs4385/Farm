# Villager goals, choices that come back, and villagers talking to each other

Status: **draft for owner review** (2026-10-11). Data: `Narrative/goals.fscript`, `Narrative/callbacks.fscript`, 15 new scenes in `Resources/Story/wave6_overheard.json`. Every line was checked against the voice rules in `narrative_rules.json` (sentence length, exclamation marks, banned words, spelling, tone) by an offline copy of the lint; the Editor lint and tests have not run yet.

## 1. Why

The bibles give every villager a want and a fear, but in play they only showed up in one heart event. Talk lines were mostly weather and season. Half of the 164 choice flags the player sets were never mentioned again: all of Bram's and Hazel's heart choices, Wren's open mic and kitchen choices (the bible promises the open-mic choice comes back), and the anonymous notes, missing pumpkin and mystery whistler storylines. The villager-to-villager scenes paired only best friends, and Wren, Hazel and Bram (who the bible says must meet) never shared one.

## 2. How a goal moves (goals.fscript)

Each villager gets three stages of talk lines, using only state the game already keeps:

| Stage | When | Priority |
|---|---|---|
| hint | from 2 to 3 hearts until the heart event that reveals the want | 0 (mixes into the normal pool) |
| after | once that event is seen; the words depend on the choice made there | 1 (surfaces within a few visits) |
| resolve | after the last event of the arc, or when the world moves (the hall restored, a quest done, another villager's event) | 1 |

Where the words depend on a choice, **one entry carries every option** with `{if choice:..}{else}..{/if}`. That keeps the talk sets small (the coverage guard in `StorySimulationTests` counts entries) and every save hears the line.

| Villager | Want (bible) | Revealed by | Moves with | Resolves when |
|---|---|---|---|---|
| Wren | the sea from somewhere else; not to be "the one who stayed" | heart 8 (the bag) | go / stay / listen | heart 10: walks to the point, frames the map, or unpacks half |
| Hazel | to be heard once, properly | heart 6 (hiding) | hiding choice, then rehearsal choice | heart 10: plans a reading night with Ione; a poem on a winter lantern |
| Bram | Juno's first piece; one word from his old teacher | heart 6 (the long sentence) | long-sentence choice, then Juno choice | sends the letter after heart 8; after heart 10 the teacher answers "Good work." |
| Tilda | a village that stays full | heart 6 (after hours) | reassure / plan / joke | the hall: a lamp lit (1 room), the street louder (restored); heart 10 sign |
| Juno | something original, without leaving Bram alone | heart 6 (first strike) | strike choice, then horseshoe choice | heart 10: her mark; with Bram's heart 10, "Hm. Next door." |
| Piper | a song the village sings without her | heart 6 (stage fright) | fright choice, then chorus word | heart 10: Bram hums it; someone sings it at the fish stall |
| Marcus | the hall roof finished | heart 8 (the roof) | quest planks, roof choice | the hall restored: "Not nearly. Fixed." |
| Odalys | a village that rests, herself included | heart 8 (office hours) | insist / soup / prescription | at 9 hearts Elara runs the clinic for a morning |
| Felix | someone to share the dawn | heart 8 (the big one) | net / release / measure | "Mornin', partner"; offers Elara his boat |
| Dorian | a friend who doesn't need him to talk | heart 8 (the notebook) | praise / ask / draw | shows Hazel the notebook |
| Elara | the sea from a boat | heart 8 (the list) | pick / add / promise | Felix offers to row her past the point |
| Ione | more readers | heart 8 (the set-aside shelf) | take / read / note | the reading night with Hazel; the beach in the first snow |

## 3. Choices that come back (callbacks.fscript)

Heart events: Wren's open mic and kitchen, Hazel's swap, Bram's repair, Juno's eaten label, Piper's parsnip request, Tilda's slate promise. Storylines: the notes writer, the pumpkin culprit and the whistler each remember how the player took it, gated on the scene (`flag:event.story_<storyline>_<villager>`).

Already covered, so not repeated: birthdays, partner evenings, the pie, the umbrella, the scarecrows and the band. Their scenes answer every option with `{if}`/`{else}`.

## 4. Villagers talking to each other (wave6_overheard.json)

There are 15 new scenes in five new pairings, three each, using the same template, place and rules as the first 40. Later scenes in a pairing need a heart event, so they reflect what has happened:

| Pairing | Thread | Gate on scene 3 |
|---|---|---|
| Wren and Bram | the fourth-word bet; his unsent letter; "Don't go far." | Wren's heart 8 |
| Wren and Hazel | Hazel as Wren's notebook; the corner stool; "Ask me again when we're friends" pays off | Hazel's heart 8 |
| Hazel and Piper | the pencil fixing Piper's rhymes; the hidden verse | Piper's heart 8 |
| Marcus and Odalys | his back; the clinic bench nobody sits on; "take your own advice" | none |
| Tilda and Juno | paying the slate in clasps; the flour sack; a forge next door | Juno's heart 6 |

## 5. New canon to approve

- Bram's old teacher writes back after heart 10: two words, "Good work."
- Felix owns a small boat ("leaks politely") and offers to row Elara past the point.
- Hazel and Ione plan a reading night at the library after Hazel's heart 10.
- Wren walks to the point to see the sea (Piper minds the bar) if the player told her to go.
- Marcus keeps a bench outside the clinic in repair; Dr. Penn never sits on it until he asks.

## 6. Fixes made in the same change

- Overheard scenes with the speakers swapped: Tilda and Felix 2 to 4, Hazel and Ione 2 and 3.
- Square-and-daytime scenes no longer talk about "the room tonight", "the lamp" or "the window here".
- Bram's "Hm." removed from 47 lines spoken by other villagers (lines that quote him are kept). Felix no longer labels things (Juno's bit), and Juno no longer says Wren's "Between us".
- Tilda's "dear" cut by 20, back under the bible's one-in-twenty.
- "A dear. Five parsnips." is now "A deal."
- Wren's bag is under the bar (the bible and her talk lines) in heart 8 and the partners evening, not under the bed.
- Copy-paste lines replaced in each villager's voice: the six identical quest accept/decline replies, the twelve identical birthday prompts ("What do you say?") and the eight identical partner-evening prompts.

## 7. Open

- Overheard scenes always play in the village square by day. Playing them in the forge, library or saloon needs a `map` parameter on the template and interior coordinates checked by `EventStaging`.
- The three missing-pumpkin culprits share the line "...It might be." Felix and Marcus deserve their own.
- `zz_fs_wake_reactions.json` and `zz_fs_elara_garden.json` have no source `.fscript` in `Narrative/`.
